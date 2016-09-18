import numpy as np
import theano
from theano import tensor
from blocks import roles
from blocks.model import Model
from blocks.extensions import saveload
from blocks.filter import VariableFilter
from utils import MainLoop
from config import config
from model import nn_fprop
from utils import get_stream
import argparse
import sys
import os
import pandas as pd
import time
import signal
from pandas.parser import CParserError
import matplotlib.pyplot as plt
from coord_transform import spherical_to_cartesian

locals().update(config)
sceneStateFile = os.path.abspath("predictions/sceneState")
if control_baxter:
    try:
        sys.path.append(os.path.expanduser('~/catkin_ws/src/baxter_examples/scripts/'))
        from trajectory_ik import PoseToJoints
        poseToJoints = PoseToJoints(filename=sceneStateFile, limb='right')
        control_baxter = True
    except ImportError as ie:
        print sys.exc_info()[0]
        control_baxter = False

    def signal_handler(signal, frame):
        poseToJoints._end_thread = True
        sys.exit(0)
    signal.signal(signal.SIGINT, signal_handler)

def load_models(models = hierarchy_models, in_size = len(hierarchy_input_columns[level_number_in_hierarchy]),
                out_size = len(hierarchy_output_columns[level_number_in_hierarchy]),
                hidden_size = hidden_size, num_layers = num_layers, model = layer_models[0]):
    predict_funcs = []
    initials = []
    for hierarchy_index in range(len(models)):
        saved_model = models[hierarchy_index]
        print 'Loading model from {0}...'.format(models[hierarchy_index])
        x = tensor.tensor3('features', dtype=theano.config.floatX)
        y = tensor.tensor3('targets', dtype=theano.config.floatX)
        y_hat, cost, cells = nn_fprop(x, y, in_size, out_size, hidden_size, num_layers, model, training=False)
        main_loop = MainLoop(algorithm=None, data_stream=None, model=Model(cost),
                             extensions=[saveload.Load(saved_model)])
        for extension in main_loop.extensions:
            extension.main_loop = main_loop
        main_loop._run_extensions('before_training')
        bin_model = main_loop.model
        print 'Model loaded. Building prediction function...'
        hiddens = []
        initials.append([])
        for i in range(num_layers- specialized_layer_num):
            brick = [b for b in bin_model.get_top_bricks() if b.name == layer_models[i] + str(i) + '-' + str(-1)][0]
            hiddens.extend(VariableFilter(theano_name=brick.name + '_apply_states')(bin_model.variables))
            hiddens.extend(VariableFilter(theano_name=brick.name + '_apply_cells')(cells))
            initials[hierarchy_index].extend(VariableFilter(roles=[roles.INITIAL_STATE])(brick.parameters))
        specialized_count = len(game_tasks) if task_specialized else 0
        for task in range(specialized_count):
            for i in range(num_layers- specialized_layer_num,num_layers):
                brick = [b for b in bin_model.get_top_bricks() if b.name == layer_models[i] + str(i) + '-' + str(task)][0]
                hiddens.extend(VariableFilter(theano_name=brick.name + '_apply_states')(bin_model.variables))
                hiddens.extend(VariableFilter(theano_name=brick.name + '_apply_cells')(cells))
                initials[hierarchy_index].extend(VariableFilter(roles=[roles.INITIAL_STATE])(brick.parameters))
        output_count = len(game_tasks) if task_specialized else 1
        predict_funcs.append([])
        for task in range(output_count):
            predict_funcs[hierarchy_index].append(theano.function([x], hiddens + [y_hat[task]]))
    return predict_funcs, initials

def predict_one_timestep(predict_funcs, initials, x, hierarchy_values, task_index, iteration):
    hierarchy_values[0] = x
    for hierarchy_index in range(len(predict_funcs)):
        if iteration % (hierarchy_resolutions[hierarchy_index] / hierarchy_resolutions[-1]) == 0:
            hierarchy_values[hierarchy_index][0:len(x)] = x
            if hierarchy_index > 0:
                hierarchy_values[hierarchy_index][-1] = 1 + hierarchy_resolutions[hierarchy_index - 1] / hierarchy_resolutions[hierarchy_index] - iteration % (hierarchy_resolutions[hierarchy_index - 1] / hierarchy_resolutions[hierarchy_index])
            if single_dim_out:
                for out_i in range(out_size):
                    newinitials = predict_funcs[hierarchy_index][task_index]([[hierarchy_values[hierarchy_index]]])
                    predicted_values[out_i] = newinitials.pop()[-1, -1, -1].astype(theano.config.floatX)
                    layer = 0
                    for initial, newinitial in zip(initials[hierarchy_index], newinitials):
                        if iteration % layer_resolutions[layer // 2] == 0:
                            initial.set_value(newinitial[-1].flatten())
                        layer += (2 if layer_models[layer // 2] == 'mt_rnn' else 1)
                        layer = min([layer,len(layer_resolutions)])
            else:
                newinitials = predict_funcs[hierarchy_index][task_index]([[hierarchy_values[hierarchy_index]]])
                predicted_values = newinitials.pop()[-1, -1, :].astype(theano.config.floatX)
                layer = 0
                for initial, newinitial in zip(initials[hierarchy_index], newinitials):
                    if iteration % layer_resolutions[layer // 2] == 0:
                        initial.set_value(newinitial[-1].flatten())
                    layer += (2 if layer_models[layer // 2] == 'mt_rnn' else 1)
                    layer = min([layer,len(layer_resolutions)])
            if hierarchy_index == len(hierarchy_models) - 1:
                hierarchy_values[hierarchy_index + 1] = predicted_values
            else:
                hierarchy_values[hierarchy_index + 1] = np.concatenate((x, predicted_values, [0]))

    return hierarchy_values, newinitials
def set_task_column_to_one_hot(data):
    if config['multi_task_mode'] == 'ID':
        for i in config['helper_game_tasks']:
            data['task' + str(i)] = 0
            data.loc[data['task'] == i, 'task' + str(i)] = 1
    return data

def sample():
    np.random.seed(seed)
    if plot_hidden_states:
        plt.ion()
        plt.ylim([-2,+4])
        plt.show()
    if use_helper_model:
        in_size = helper_hidden_size * helper_num_layers + len(hierarchy_input_columns[level_number_in_hierarchy])
        predict_funcs, initials = load_models(in_size = in_size)
        helper_predict_funcs, helper_initials = load_models(models= helper_models, hidden_size= helper_hidden_size, num_layers=helper_num_layers, model=layer_models[0])
        helper_hierarchy_values = [None] * (len(helper_predict_funcs) + 1)
    else:
        predict_funcs, initials = load_models()
    hierarchy_values = [None] * (len(predict_funcs) + 1)
    print("Generating trajectory...")
    last_time = 0
    counter = 0
    out_size = len(hierarchy_output_columns[level_number_in_hierarchy])
    predicted_values = [None] * out_size
    last_speed_calc = time.time()
    predicted = np.array([2, 2.42, 2.316, 1.223, 1, 1, 1.7, 1.71])
    last_prediction = predicted.copy()
    for iteration in range(10000000):

        try:
            try:
                if control_baxter:
                    # execute the command on Baxter
                    poseToJoints.execute('right', np.array([x-1 for x in predicted[0:8]]))
                out = open('predictions/prediction', 'w')
                out.write(','.join([str(x - 1) for x in predicted]) + '\n')
                out.close()
            except IOError:
                print 'could not open the prediction file.'
            wait_counter = 0
            wait_time = 0.25
            while True:
                time.sleep(wait_time)
                if control_baxter:
                    # read gripper status, end-effector pose, and object pose
                    poseToJoints.write_env_state('right')
                    new_state = pd.read_csv(sceneStateFile, header=0, sep=',', index_col=False)
                    new_state = set_task_column_to_one_hot(new_state)
                else:
                    new_state = pd.read_csv(sceneStateFile, header=0, sep=',', index_col=False)
                    new_state = set_task_column_to_one_hot(new_state)
                if last_time == new_state['time'][0]:
                    time.sleep(.005)
                    continue
                gripper_pose_difference = np.linalg.norm(new_state[goal_columns].iloc[-1].as_matrix()[1:8] - [x-1 for x in predicted[1:8]])
                wait_counter += wait_time
                if gripper_pose_difference < .01 or wait_counter > 1:
                    break
            last_time = new_state['time'][0]
#             new_state = transformCoordinates(new_state)
            new_state[hierarchy_input_columns[level_number_in_hierarchy]] += 1
            task_index = game_tasks.index(int(new_state['task'][0])) if task_specialized else 0
            helper_x = np.array(new_state[hierarchy_input_columns[0]].iloc[0], dtype=theano.config.floatX)
            if use_helper_model:
                helper_hierarchy_values, helper_newinitials = predict_one_timestep(helper_predict_funcs, helper_initials, helper_x, helper_hierarchy_values, task_index, iteration)
                helper_state_values = np.empty(0)
                for j in range(len(helper_initials[0])):
                    if "state" in helper_initials[0][j].name:
                        helper_state_values = np.concatenate((helper_state_values, helper_newinitials[j].flatten()))
                x = np.concatenate((helper_state_values, helper_x))
            else:
                x = np.array(new_state[hierarchy_input_columns[0]].iloc[0], dtype=theano.config.floatX)

            hierarchy_values, newinitials = predict_one_timestep(predict_funcs, initials, x, hierarchy_values, task_index, iteration)

            predicted = hierarchy_values[-1]
            if output_displacement:
                predicted[1:8] /= gripper_difference_upscale
                predicted[1:8] += (np.array(new_state[gripper_difference_columns].iloc[0], dtype=theano.config.floatX))
#             predicted[1],predicted[2],predicted[3] = spherical_to_cartesian(np.array([predicted[1],predicted[2],predicted[3]]))
            if plot_hidden_states:
                plt.pause(0.0001)
                plt.clf()
                values = newinitials[0].flatten()
                plt.scatter(range(len(values)), values, color="r")
                values = newinitials[2].flatten()
                plt.scatter(range(len(values)), values, color="b")
                plt.scatter(range(len(predicted)), predicted, color="g")
                plt.ylim([-2,+4])
                plt.draw()
    #             plt.waitforbuttonpress()
        except (CParserError, RuntimeError, TypeError, NameError, ValueError, IndexError) as ie:
            print sys.exc_info()[0]

        counter += 1
        if(time.time() - last_speed_calc > 1):
#             print str(counter) + ' Hz'
            counter = 0
            last_speed_calc = time.time()

if __name__ == '__main__':
    # Load config parameters
    locals().update(config)
    float_formatter = lambda x: "%.5f" % x
    np.set_printoptions(formatter={'float_kind':float_formatter})
    parser = argparse.ArgumentParser(
        description='Generate the learned trajectory',
        formatter_class=argparse.ArgumentDefaultsHelpFormatter)
    args = parser.parse_args()

    sample_string = sample()
