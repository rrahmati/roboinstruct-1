import theano
import numpy as np
import sys
from theano import tensor
from blocks.model import Model
from blocks.graph import ComputationGraph, apply_dropout
from blocks.algorithms import StepClipping, GradientDescent, CompositeRule, RMSProp, Adam
from blocks.filter import VariableFilter
from blocks.extensions import FinishAfter, Timing, Printing, saveload, ProgressBar
from blocks.extensions.training import SharedVariableModifier
from blocks.extensions.monitoring import DataStreamMonitoring, TrainingDataMonitoring
from blocks.monitoring import aggregation
from utils import get_stream, track_best, MainLoop
from model import nn_fprop
from config import config

# Load config parameters
locals().update(config)
# DATA
train_stream = get_stream(hdf5_file, 'train', batch_size)
test_stream = get_stream(hdf5_file, 'test', batch_size)

# MODEL
x = tensor.tensor3('features', dtype='floatX')
y = tensor.tensor3('targets', dtype='uint8' if cost_mode == 'Softmax' else 'floatX')
out_size = len(hierarchy_output_columns[level_number_in_hierarchy])
if use_helper_model:
    in_size = helper_hidden_size * helper_num_layers + len(hierarchy_input_columns[level_number_in_hierarchy])
else:
    in_size = len(hierarchy_input_columns[level_number_in_hierarchy])
y_hat, cost, cells = nn_fprop(x, y, in_size, out_size, hidden_size, num_layers, layer_models[0], training=True)

# COST
cg = ComputationGraph(cost)

if dropout > 0:
    # Apply dropout only to the non-recurrent inputs (Zaremba et al. 2015)
    inputs = VariableFilter(theano_name_regex=r'.*apply_input.*')(cg.variables)
    print inputs
    cg = apply_dropout(cg, inputs, dropout)
    cost = cg.outputs[0]

# Learning algorithm
step_rules = [RMSProp(learning_rate=learning_rate, decay_rate=decay_rate),
              StepClipping(step_clipping)]
# step_rules = [Adam()]
algorithm = GradientDescent(cost=cost, parameters=cg.parameters,
                            step_rule=CompositeRule(step_rules))

# Extensions
gradient_norm = aggregation.mean(algorithm.total_gradient_norm)
step_norm = aggregation.mean(algorithm.total_step_norm)
monitored_vars = [cost, step_rules[0].learning_rate, gradient_norm, step_norm]

test_monitor = DataStreamMonitoring(variables=[cost], after_epoch=True,
                                   before_first_epoch=True, data_stream=test_stream, prefix="test")
train_monitor = TrainingDataMonitoring(variables=monitored_vars, after_epoch=True,
                                       before_first_epoch=True, prefix='train')

# plot = Plot('Plotting example', channels=[['cost']], after_batch=True, open_browser=True)
extensions = [test_monitor, train_monitor, Timing(), Printing(after_epoch=True),
              FinishAfter(after_n_epochs=nepochs),
              saveload.Load(load_path),
#               saveload.Checkpoint(last_path,after_epoch=True),
              ] + track_best('test_cost', save_path)

if learning_rate_decay not in (0, 1):
    extensions.append(SharedVariableModifier(step_rules[0].learning_rate,
                                             lambda n, lr: np.cast[theano.config.floatX](learning_rate_decay * lr), after_epoch=True, after_batch=False))

print 'number of parameters in the model: ' + str(tensor.sum([p.size for p in cg.parameters]).eval())
# Finally build the main loop and train the model
main_loop = MainLoop(data_stream=train_stream, algorithm=algorithm,
                     model=Model(cost), extensions=extensions)
main_loop.run()
