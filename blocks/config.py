config = {}

config['batch_size'] = 10  # number of samples taken per each update. You might want to increase it to make the training faster, but you might not get the same result.
config['hidden_size'] = 50
config['learning_rate'] = .001
config['learning_rate_decay'] = 0.999  # set to 0 to not decay learning rate
config['decay_rate'] = 0.999  # decay rate for rmsprop
config['step_clipping'] = 1.0  # clip norm of gradients at this value
config['dropout'] = .0
config['nepochs'] = 1000  # number of full passes through the training data
config['seq_length'] = 50  # number of waypoints in the truncated sequence
config['hdf5_file'] = 'input.hdf5'  # hdf5 file with Fuel format
config['random_resolution_range'] = [8,9]
config['noise'] = .0
config['plot_hidden_states'] = False
config['layer_models'] = ['lstm', 'lstm', 'lstm'] # feedforward, lstm, rnn
config['num_layers'] = len(config['layer_models'])

# parameters of data augmentation
config['z_shift_range'] = [0,.5]
config['z_shift_offset'] = 2
config['x_shift_range'] = [0,.1]
config['x_shift_offset'] = 2
config['y_shift_range'] = [0,.3]
config['y_shift_offset'] = 2

# parameters about auxiliary predictions - The idea is to have some layers to predict another related prediction, for instance, predict the pose of object or the pose of gripper in next 4 timesteps
config['future_predictions'] = [1]
config['prediction_cost_weights'] = [1]

# parameters of multi-timescale learning : The idea is to enable different layers of LSTM or RNN to work at different time-scales
config['layer_resolutions'] = [1,1,1]
config['layer_execution_time_offset'] = [0,0,0]


# parameters of hierarchical model - The idea is to have a hierarchy of models, They are trained separately, and the input of the bottom model forms a part of its above modelconfig['hierarchy_resolutions'] = [ 8]
config['max_goal_difference'] = 1
config['level_name_in_hierarchy'] = 'bottom'  # top , middle , bottom
config['level_number_in_hierarchy'] = 0
config['hierarchy_resolutions'] = [8]

# parameters of multi-task learning : The idea is to train a network on data of multiple tasks. The ID if the task to be executed is given as an input in each time-step
config['multi_task_mode'] = 'ID'
config['game_tasks'] = [18]
config['trajs_paths'] = ['trajectories/18'] # 'trajectories/2', 'trajectories/18'
config['task_weights'] = [1]
config['task_waypoints_to_load'] = [400000]
config['waypoints_to_ignore'] = 0  # ignore number of waypoints from the beginning and the end of each file

# parameters of specialized part of the network - The idea is to specialize some parts of the network to a task and keep the rest of the network shared among the tasks
config['task_specialized'] = False
config['specialized_hidden_size'] = 3
config['specialized_layer_num'] = 0
config['separate_last_hidden'] = False
config['dedicated_last_h_size'] = 1

# parameters of helper model - The idea is to firt train a helper network and then train another network that uses the hidden states of the helper network to predict its own output
config['use_helper_model'] = False
config['helper_models'] = ['models/ID_False_2-11-12-13-14-15-16-17_8_MDN_20_one-previous_True_True_1-1_0.0_1_bottom_False_2_20_5_50_best.pkl']
config['helper_hidden_size'] = 20
config['helper_num_layers'] = 2
config['helper_game_tasks'] = [2,11,12,13,14,15,16,17,18]

# We make a stacked RNN with the following skip connections added if the corresponding parameter is True
config['connect_x_to_h'] = True
config['connect_h_to_h'] = 'one-previous'  # all-previous , two-previous, one-previous
config['connect_h_to_o'] = True

# We can make the network to predict the difference between the current gripper pose and the next waypoint instead of predictiing the next gripper pose
config['output_displacement'] = False
config['gripper_difference_columns'] = ['gripper_center_p_x', 'gripper_center_p_y', 'gripper_center_p_z', 'gripper_center_r_x', 'gripper_center_r_y', 'gripper_center_r_z', 'gripper_center_r_w']
config['gripper_difference_upscale'] = 1

# parameters of cost function
config['cost_mode'] = 'MDN'  # MDN, MSE, Softmax
config['out_round_decimal'] = 2

# parameters of MDN
config['components_size'] = 20
config['seed'] = 66478

# outputting one dimension at a time parameters - Predicting only one dimension of the output at a time. The sequence length would be multiplied by the output dimension. Seems slow!
config['single_dim_out'] = False
config['single_dim_out_mode'] = 'each_layer_outputs_one_dim' # 'each_layer_outputs_one_dim'

# configs related to Baxter control
config['control_baxter'] = False

config['user_prefs'] = False
config['state_columns'] = ['gripper', 'gripper_center_p_x', 'gripper_center_p_y', 'gripper_center_p_z', 'gripper_center_r_x', 'gripper_center_r_y', 'gripper_center_r_z', 'gripper_center_r_w']
if [i for i in [2,11,12,13,14,15,16,17,18] if i in config['game_tasks']]:
    config['state_columns'] += list(set(['box (1)_p_x', 'box (1)_p_y', 'box (1)_p_z', 'box (1)_r_x', 'box (1)_r_y', 'box (1)_r_z', 'box (1)_r_w']) - set(config['state_columns']))
if 6 in config['game_tasks']:
    config['state_columns'] += list(set(['big_box_p_x', 'big_box_p_y', 'big_box_p_z', 'big_box_r_x', 'big_box_r_y', 'big_box_r_z', 'big_box_r_w', 'big_box_goal_sq_p_x', 'big_box_goal_sq_p_y', 'big_box_goal_sq_p_z']) - set(config['state_columns']))
if 7 in config['game_tasks']:
    config['state_columns'] += list(set(['big_box_p_x', 'big_box_p_y', 'big_box_p_z', 'big_box_r_x', 'big_box_r_y', 'big_box_r_z', 'big_box_r_w']) - set(config['state_columns']))
config['goal_columns'] = ['gripper', 'gripper_center_p_x', 'gripper_center_p_y', 'gripper_center_p_z', 'gripper_center_r_x', 'gripper_center_r_y', 'gripper_center_r_z', 'gripper_center_r_w']
config['hierarchy_input_columns'] = []
config['hierarchy_output_columns'] = []
config['hierarchy_input_columns'].append(config['state_columns'])
if config['multi_task_mode'] == 'ID':
    for i in config['game_tasks']:
        config['hierarchy_input_columns'][0] += ['task' + str(i)]
if config['user_prefs']:
    config['hierarchy_input_columns'][0] += ['box_goal_z', 'task_length']
config['hierarchy_output_columns'].append(config['goal_columns'])
for i in range(len(config['hierarchy_resolutions']) - 2):
    config['hierarchy_input_columns'].append(config['state_columns'] + [s + '_goal' for s in config['goal_columns']] + ['steps'])
    config['hierarchy_output_columns'].append(config['goal_columns'])
config['hierarchy_input_columns'].append(config['state_columns'] + [s + '_goal' for s in config['goal_columns']] + ['steps'])
config['hierarchy_output_columns'].append(['gripper', 'gripper_center_p_x', 'gripper_center_p_y', 'gripper_center_p_z', 'gripper_center_r_x', 'gripper_center_r_y', 'gripper_center_r_z', 'gripper_center_r_w'])

config['train_size'] = 0.80  # fraction of data that goes into train set
# path to the best model file
config['save_path'] = 'models/{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}_{13}_{14}_{15}_{16}_{17}_{18}_{19}_{20}_best.pkl'.format(config['multi_task_mode'], config['use_helper_model'], '-'.join(str(x) for x in config['game_tasks']), config['task_specialized'], config['separate_last_hidden'], config['dedicated_last_h_size'], config['random_resolution_range'][0], config['cost_mode'], config['components_size'], config['connect_h_to_h'], config['connect_x_to_h'], config['connect_h_to_o'], '-'.join(str(x) for x in config['layer_resolutions']), config['noise'], config['max_goal_difference'], config['level_name_in_hierarchy'], config['user_prefs'], config['num_layers'], config['hidden_size'], config['batch_size'], config['seq_length'])
# path to save the model of the last epoch
config['last_path'] = 'models/{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}_{8}_{9}_{10}_{11}_{12}_{13}_{14}_{15}_{16}_{17}_{18}_{19}_{20}_last.pkl'.format(config['multi_task_mode'], config['use_helper_model'], '-'.join(str(x) for x in config['game_tasks']), config['task_specialized'], config['separate_last_hidden'], config['dedicated_last_h_size'], config['random_resolution_range'][0], config['cost_mode'], config['components_size'], config['connect_h_to_h'], config['connect_x_to_h'], config['connect_h_to_o'], '-'.join(str(x) for x in config['layer_resolutions']), config['noise'], config['max_goal_difference'], config['level_name_in_hierarchy'], config['user_prefs'], config['num_layers'], config['hidden_size'], config['batch_size'], config['seq_length'])
config['load_path'] = config['save_path']
config['hierarchy_models'] = [ config['save_path']]
