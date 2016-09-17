import theano
import numpy as np
from theano import tensor as T
from theano.ifelse import ifelse
from theano.tensor.shared_randomstreams import RandomStreams
from blocks import initialization
from blocks.bricks import Linear, Rectifier, cost
from blocks.bricks.parallel import Fork
from blocks.bricks.recurrent import GatedRecurrent, LSTM, SimpleRecurrent
from blocks.bricks.cost import AbsoluteError, SquaredError
from config import config
from blocks.bricks import (MLP, Logistic, Initializable, FeedforwardSequence, Tanh,
                           NDimensionalSoftmax)
from blocks.initialization import Constant, Uniform
import logging
locals().update(config)

def initialize(to_init):
    for bricks in to_init:
        bricks.weights_init = initialization.Uniform(width=0.08)
        bricks.biases_init = initialization.Constant(0)
        bricks.initialize()

def MDN_output_layer(x, h, y, in_size, out_size, hidden_size, pred, task):
    if connect_h_to_o:
        if separate_last_hidden:
            dedicated_last_h = h[-1][:,:,task*dedicated_last_h_size:(task+1)*dedicated_last_h_size]
            shared_last_h = h[-1][:,:,len(game_tasks)*dedicated_last_h_size:]
            shared_last_h_size = hidden_size - len(game_tasks) * dedicated_last_h_size
            hiddens = T.concatenate([hidden for hidden in h[0:-1] + [dedicated_last_h] + [shared_last_h]], axis=2)
            hidden_out_size = hidden_size * (len(h) - specialized_layer_num - 1) + specialized_hidden_size * specialized_layer_num + dedicated_last_h_size + shared_last_h_size
        else:
            hiddens = T.concatenate([hidden for hidden in h], axis=2)
            hidden_out_size = hidden_size * (len(h) - specialized_layer_num) + specialized_hidden_size * specialized_layer_num
    else:
        hiddens = h[-1]
        hidden_out_size = hidden_size

    mu_linear = Linear(name='mu_linear' + str(pred), input_dim=hidden_out_size, output_dim=out_size * components_size)
    sigma_linear = Linear(name='sigma_linear' + str(pred), input_dim=hidden_out_size, output_dim=components_size)
    mixing_linear = Linear(name='mixing_linear' + str(pred), input_dim=hidden_out_size, output_dim=components_size)
    initialize([mu_linear, sigma_linear, mixing_linear])
    mu = mu_linear.apply(hiddens)
    mu = mu.reshape((mu.shape[0], mu.shape[1], out_size, components_size))
    sigma = sigma_linear.apply(hiddens)
    sigma = T.nnet.softplus(sigma)
    mixing = mixing_linear.apply(hiddens)
    # apply softmax to mixing
    e_x = T.exp(mixing - mixing.max(axis=2, keepdims=True))
    mixing = e_x / e_x.sum(axis=2, keepdims=True)
    # calculate cost
    exponent = -0.5 * T.inv(sigma) * T.sum((y.dimshuffle(0, 1, 2, 'x') - mu) ** 2, axis=2)
    normalizer = (2 * np.pi * sigma)
    exponent = exponent + T.log(mixing) - (out_size * .5) * T.log(normalizer)
    # LogSumExp(x)
    max_exponent = T.max(exponent , axis=2, keepdims=True)
    mod_exponent = exponent - max_exponent
    gauss_mix = T.sum(T.exp(mod_exponent), axis=2, keepdims=True)
    log_gauss = T.log(gauss_mix) + max_exponent
    # multiply by the task ( 0 if the cost is not related to this task, 1 otherwise)
    if task_specialized:
        task_index = in_size - len(game_tasks) + task
        log_gauss = T.mul(log_gauss,T.sub(x[:,:,task_index:task_index+1],1))
    # mean over the batch, mean over sequence
    cost = -T.mean(log_gauss, axis=1).mean()

    # sampling
    srng = RandomStreams(seed=seed)
    component = srng.multinomial(pvals=mixing)
    component_mean = T.sum(mu * component.dimshuffle(0, 1, 'x', 2), axis=3)
    component_std = T.sum(sigma * component, axis=2, keepdims=True)
    linear_output = srng.normal(avg=component_mean, std=component_std)
    linear_output.name = 'linear_output'
    return linear_output, cost

def MSE_output_layer(x, h, y, in_size, out_size, hidden_size, pred, task):
    if connect_h_to_o:
        hiddens = T.concatenate([hidden for hidden in h], axis=2)
        hidden_out_size = hidden_size * (len(h) - specialized_layer_num) + specialized_hidden_size * specialized_layer_num
        hidden_to_output = Linear(name='hidden_to_output' + str(pred), input_dim=hidden_out_size,
                                output_dim=out_size)
    else:
        hidden_to_output = Linear(name='hidden_to_output' + str(pred), input_dim=hidden_size,
                                output_dim=out_size)
        hiddens = h[-1]
    initialize([hidden_to_output])
    linear_output = hidden_to_output.apply(hiddens)
    linear_output.name = 'linear_output'
    cost = T.sqr(y - linear_output).mean(axis=1).mean()  # + T.mul(T.sum(y[:,:,8:9],axis=1).mean(),2)
    return linear_output, cost

def softmax_output_layer(x, h, y, in_size, out_size, hidden_size, pred, task):
    if connect_h_to_o:
        hidden_to_output = Linear(name='hidden_to_output' + str(pred), input_dim=hidden_size * len(h),
                                output_dim=out_size)
        hiddens = T.concatenate([hidden for hidden in h], axis=2)
    else:
        hidden_to_output = Linear(name='hidden_to_output' + str(pred), input_dim=hidden_size,
                                output_dim=out_size)
        hiddens = h[-1]
    initialize([hidden_to_output])
    linear_output = hidden_to_output.apply(hiddens)
    linear_output.name = 'linear_output'
    softmax = NDimensionalSoftmax()
    extra_ndim = 1 if single_dim_out else 2
    y_hat = softmax.apply(linear_output, extra_ndim=extra_ndim)
    cost = softmax.categorical_cross_entropy(y, linear_output, extra_ndim=extra_ndim).mean()

    return y_hat, cost

def output_layer(x, h, specialized_h, y, in_size, out_size, hidden_size):
    costs = []
    linear_outputs = []
    for i in range(len(future_predictions)):
        linear_outputs.append([])
        output_count = len(game_tasks) if task_specialized else 1
        for task in range(output_count):
            hiddens = h + specialized_h[task] if i == len(future_predictions) - 1 else h[i:i + 1]
            if cost_mode == 'MDN':
                linear_output, cost = MDN_output_layer(x, hiddens, y[:, :, out_size * i:out_size * (i + 1)], in_size, out_size, hidden_size, str(i) + '-' + str(task), task)
            elif cost_mode == 'MSE':
                linear_output, cost = MSE_output_layer(x, hiddens, y[:, :, out_size * i:out_size * (i + 1)], in_size, out_size, hidden_size, str(i) + '-' + str(task), task)
            elif cost_mode == 'Softmax':
                linear_output, cost = softmax_output_layer(hiddens, y[:, :, out_size * i:out_size * (i + 1)], in_size, out_size, hidden_size, str(i) + '-' + str(task), task)
            linear_outputs[i].append(linear_output)
            costs.append(T.mul(cost, prediction_cost_weights[i]))
    cost_sum = T.sum(costs)
    cost_sum.name = 'cost'
    return linear_outputs[-1], cost_sum

def linear_layer(in_size, dim, x, h, n, task, first_layer=False):
    if first_layer:
        input = x
        linear = Linear(input_dim=in_size, output_dim=dim, name='feedforward' + str(n) + '-' + str(task))
    elif connect_x_to_h:
        input = T.concatenate([x] + [h[n - 1]], axis=1)
        linear = Linear(input_dim=in_size + dim, output_dim=dim, name='feedforward' + str(n) + '-' + str(task))
    else:
        input = h[n - 1]
        linear = Linear(input_dim=dim, output_dim=dim, name='feedforward' + str(n) + '-' + str(task))
    initialize([linear])
    return linear.apply(input)

def gru_layer(dim, h, n):
    fork = Fork(output_names=['linear' + str(n) + '-' + str(task), 'gates' + str(n) + '-' + str(task)],
                name='fork' + str(n) + '-' + str(task), input_dim=dim, output_dims=[dim, dim * 2])
    gru = GatedRecurrent(dim=dim, name='gru' + str(n) + '-' + str(task))
    initialize([fork, gru])
    linear, gates = fork.apply(h)
    return gru.apply(linear, gates)

def rnn_layer(in_size, dim, x, h, n, first_layer = False):
    if connect_h_to_h == 'all-previous':
        if first_layer:
            rnn_input = x
            linear = Linear(input_dim=in_size, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            rnn_input = T.concatenate([x] + [hidden for hidden in h], axis=2)
            linear = Linear(input_dim=in_size + dim * n, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        else:
            rnn_input = T.concatenate([hidden for hidden in h], axis=2)
            linear = Linear(input_dim=dim * n, output_dim=dim, name='linear' + str(n) + '-' + str(task))
    elif connect_h_to_h == 'two-previous':
        if first_layer:
            rnn_input = x
            linear = Linear(input_dim=in_size, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            rnn_input = T.concatenate([x] + h[max(0, n - 2):n], axis=2)
            linear = Linear(input_dim=in_size + dim * 2 if n > 1 else in_size + dim, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        else:
            rnn_input = T.concatenate(h[max(0, n - 2):n], axis=2)
            linear = Linear(input_dim=dim * 2 if n > 1 else dim, output_dim=dim, name='linear' + str(n) + '-' + str(task))
    elif connect_h_to_h == 'one-previous':
        if first_layer:
            rnn_input = x
            linear = Linear(input_dim=in_size, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            rnn_input = T.concatenate([x] + [h[n-1]], axis=2)
            linear = Linear(input_dim=in_size + dim, output_dim=dim, name='linear' + str(n) + '-' + str(task))
        else:
            rnn_input = h[n]
            linear = Linear(input_dim=dim, output_dim=dim, name='linear' + str(n) + '-' + str(task))
    rnn = SimpleRecurrent(dim=dim, activation=Tanh(), name=layer_models[n] + str(n) + '-' + str(task))
    initialize([linear, rnn])
    if layer_models[n] == 'rnn':
        return rnn.apply(linear.apply(rnn_input))
    elif layer_models[n] == 'mt_rnn':
        return rnn.apply(linear.apply(rnn_input), time_scale=layer_resolutions[n], time_offset=layer_execution_time_offset[n])

def lstm_layer(in_size, dim, x, h, n, task, first_layer = False):
    if connect_h_to_h == 'all-previous':
        if first_layer:
            lstm_input = x
            linear = Linear(input_dim=in_size, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            lstm_input = T.concatenate([x] + [hidden for hidden in h], axis=2)
            linear = Linear(input_dim=in_size + dim * (n), output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        else:
            lstm_input = T.concatenate([hidden for hidden in h], axis=2)
            linear = Linear(input_dim=dim * (n + 1), output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
    elif connect_h_to_h == 'two-previous':
        if first_layer:
            lstm_input = x
            linear = Linear(input_dim=in_size, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            lstm_input = T.concatenate([x] + h[max(0, n - 2):n], axis=2)
            linear = Linear(input_dim=in_size + dim * 2 if n > 1 else in_size + dim, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        else:
            lstm_input = T.concatenate(h[max(0, n - 2):n], axis=2)
            linear = Linear(input_dim=dim * 2 if n > 1 else dim, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
    elif connect_h_to_h == 'one-previous':
        if first_layer:
            lstm_input = x
            linear = Linear(input_dim=in_size, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        elif connect_x_to_h:
            lstm_input = T.concatenate([x] + [h[n-1]], axis=2)
            linear = Linear(input_dim=in_size + dim, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
        else:
            lstm_input = h[n-1]
            linear = Linear(input_dim=dim, output_dim=dim * 4, name='linear' + str(n) + '-' + str(task))
    lstm = LSTM(dim=dim , name=layer_models[n] + str(n) + '-' + str(task))
    initialize([linear, lstm])
    if layer_models[n] == 'lstm':
        return lstm.apply(linear.apply(lstm_input))
    elif layer_models[n] == 'mt_lstm':
        return lstm.apply(linear.apply(lstm_input), time_scale=layer_resolutions[n], time_offset=layer_execution_time_offset[n])

def add_layer(model, i, in_size, h_size, x, h, cells, task, first_layer = False):
    if model == 'rnn' or model == 'mt_rnn':
        h.append(rnn_layer(in_size, h_size, x, h, i, task, first_layer))
    if model == 'gru':
        h.append(gru_layer(h_size, h[i], i))
    if model == 'lstm' or model == 'mt_lstm':
        state, cell = lstm_layer(in_size, h_size, x, h, i, task, first_layer)
        h.append(state)
        cells.append(cell)
    if model == 'feedforward':
        h.append(linear_layer(in_size, h_size, x, h, i, task, first_layer))
    return h, cells
def nn_fprop(x, y, in_size, out_size, hidden_size, num_layers, model, training):
    if single_dim_out:
        out_size = 1
    if e2e_multi_timescale:
        return mt_rnn_fprop(x, y, in_size, out_size, hidden_size, num_layers, model, training)
    else:
        cells = []
        h = []
        for i in range(num_layers- specialized_layer_num):
            model = layer_models[i]
            h, cells = add_layer(model, i, in_size, hidden_size, x, h, cells, task=-1, first_layer = True if i == 0 else False)
        specialized_h = []
        shared_h = T.concatenate([hidden for hidden in h], axis=2)
        shared_h_size = (num_layers- specialized_layer_num)*hidden_size
        specialized_count = len(game_tasks) if task_specialized else 1
        for task in range(specialized_count):
            specialized_h.append([])
            for i in range(num_layers- specialized_layer_num,num_layers):
                model = layer_models[i]
                specialized_h[task], cells = add_layer(model, i, shared_h_size, specialized_hidden_size, shared_h, specialized_h[task], cells, task, first_layer = True if i == num_layers- specialized_layer_num else False)
        return output_layer(x, h, specialized_h, y, in_size, out_size, hidden_size) + (cells,)
