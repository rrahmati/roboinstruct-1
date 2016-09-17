import theano
import numpy as np
import sys
import codecs
import h5py
import yaml
from fuel.datasets import H5PYDataset
from config import config
import os
import pandas as pd
from coord_transform import cartesian_to_spherical, spherical_to_cartesian
from sample import load_models, predict_one_timestep, set_task_column_to_one_hot

# Load config parameters
locals().update(config)
np.random.seed(0)

def transformCoordinates(data):
#     data = dataframe_to_spherical(data)
    for col in list(dataCopy.columns.values):
        if "p_x" in col and not 'gripper_center' in col:
            data[col] -= data['gripper_center_p_x']
        if "p_y" in col and not 'gripper_center' in col:
            data[col] -= data['gripper_center_p_y']
        if "p_z" in col and not 'gripper_center' in col:
            data[col] -= data['gripper_center_p_z']
    return data

def gripper_difference(data):

    dataCopy = data.copy()
    dataCopy = dataCopy.shift(-1)
    data = data[:-1]
    dataCopy = dataCopy[:-1]
    for c in gripper_difference_columns:
        data[c] = dataCopy[c] - data[c]
        data[c] *= gripper_difference_upscale

    return data
def dataframe_to_spherical(data):
#     data['gripper_center_p_x'],data['gripper_center_p_y'],data['gripper_center_p_z'] = columns_to_spherical(data['gripper_center_p_x'],data['gripper_center_p_y'],data['gripper_center_p_z'])
#     data['box (1)_p_x'],data['box (1)_p_x'],data['box (1)_p_x'] = columns_to_spherical(data['box (1)_p_x'],data['box (1)_p_x'],data['box (1)_p_x'])
#     if game_task == 6 or game_task == 7:
#         data['big_box_goal_p_x'],data['big_box_goal_p_y'],data['big_box_goal_p_z'] = columns_to_spherical(data['big_box_goal_p_x'],data['big_box_goal_p_y'],data['big_box_goal_p_z'])
#         data['big_box_p_x'],data['big_box_p_y'],data['big_box_p_z'] = columns_to_spherical(data['big_box_p_x'],data['big_box_p_y'],data['big_box_p_z'])
    return data

def columns_to_spherical(x_col, y_col, z_col):
    x_out = np.empty(0)
    y_out = np.empty(0)
    z_out = np.empty(0)
    for i in range(len(x_col.index)):
        x, y, z = cartesian_to_spherical(np.array([x_col.iloc[i], y_col.iloc[i], z_col.iloc[i]]))
        x_out = np.append(x_out, x)
        y_out = np.append(y_out, y)
        z_out = np.append(z_out, z)
    return pd.Series(x_out), pd.Series(y_out), pd.Series(z_out)

def transformData(dataCopy):
    data = pd.DataFrame()
    for i in np.arange(z_shift_range[0],z_shift_range[1],z_shift_offset):
        for j in np.arange(x_shift_range[0],x_shift_range[1],x_shift_offset):
            for k in np.arange(y_shift_range[0],y_shift_range[1],y_shift_offset):
                dataTransformed = dataCopy.copy()
                for col in list(dataCopy.columns.values):
                    if "p_z" in col:
                        dataTransformed[col] += i
                    if "p_x" in col:
                        dataTransformed[col] += j
                    if "p_y" in col:
                        dataTransformed[col] += k
                data = data.append(dataTransformed)

    return data

def setDataResolution(dataCopy):
    data = pd.DataFrame()
    for start_index in range(0, random_resolution_range[0],1):
        indexes = []
        i = start_index
        while i < len(dataCopy.index):
            indexes.append(i)
            i += np.random.randint(random_resolution_range[0],random_resolution_range[1])
        data = data.append(pd.DataFrame(dataCopy.iloc[indexes]))
    data = data.reset_index()
    return data

def setGoalAndPrefs(data):

    task_executions = 0
    task_start = 0
    for i in range(len(data.index) - 1):
        if data['task'].iloc[i] == 2:
            if data['box (1)_p_x'].iloc[i] < 1 and data['box (1)_p_x'].iloc[i + 1] > 1.1 or i == len(data.index) - 2:
                task_executions += 1
                if user_prefs:
                    data['task_length'].iloc[task_start:i + 1] = (i - task_start) / 150.0
                    data['box_goal_z'].iloc[task_start:i + 1] = data['box (1)_p_z'].iloc[i]
                task_start = i
        else:
            if i + 1 < len(data.index) and data['try_in_task'].iloc[i] < data['try_in_task'].iloc[i + 1] :
                task_executions += 1
    print "Sample task executions: ", task_executions

    goal_col_series = []
    steps = np.empty(0)
    for c in range(len(goal_columns)):
        goal_col_series.append(np.empty(0))
    for i in range(0, len(data.index) - max_goal_difference, max_goal_difference):
        steps = np.append(steps, range(max_goal_difference, 0, -1))
        for c in range(len(goal_columns)):
            goal_col_series[c] = np.append(goal_col_series[c], [data[goal_columns[c]].iloc[i + max_goal_difference]] * max_goal_difference)
    for c in range(len(goal_columns)):
        data[goal_columns[c] + '_goal'] = pd.Series(goal_col_series[c])
    data['steps'] = pd.Series(steps)
    data = data[:-max_goal_difference]

    return data

def train_test_split():
    print("Loading data...")
    data = pd.DataFrame()
    counter = 0
    maxfiles = 2000
    task_data_loaded = [0] * len(trajs_paths)
    for i in range(len(trajs_paths)):
        for filename in os.listdir(trajs_paths[i]):
            counter += 1
            if(counter > maxfiles):
                break
            newData = pd.read_csv(os.path.join(trajs_paths[i], filename), header=2, sep=',', index_col=False)
            # newData = pd.DataFrame(newData.iloc[0:len(newData.index)-(len(newData.index)%(seq_length*2))])
            if task_data_loaded[i] + len(newData.index) > task_waypoints_to_load[i]:
                data_to_add_size = task_waypoints_to_load[i] - task_data_loaded[i]
                task_data_loaded[i] += data_to_add_size
                newData = pd.DataFrame(newData.iloc[0:data_to_add_size])
            else:
                task_data_loaded[i] += len(newData.index)
            size = len(newData.index)
#             data = data.append(newData)
            for j in range(task_weights[i]):
                data = pd.DataFrame(newData.iloc[waypoints_to_ignore:int(size * train_size)]).append(data)
                data = data.append(newData.iloc[int(size * train_size):size-waypoints_to_ignore])
    data = data.reset_index()
    data = set_task_column_to_one_hot(data)
    waypoints_size = len(data.index)
    print "Sample waypoints: ", waypoints_size
    data['box_goal_z'] = 0
    data['task_length'] = 0
    data['steps'] = 0
    for col in goal_columns:
        data[col + '_goal'] = 0
    data = pd.DataFrame(data[list(set(['task', 'try_in_task'] + hierarchy_input_columns[level_number_in_hierarchy] + list(set(hierarchy_output_columns[level_number_in_hierarchy]) - set(hierarchy_input_columns[level_number_in_hierarchy]))))])
    if multi_task_mode == 'ID':
        dataCopy = pd.DataFrame(data)
    data = setDataResolution(dataCopy.iloc[0:int(waypoints_size * train_size)])
    data = data.append(setDataResolution(dataCopy.iloc[int(waypoints_size * train_size):waypoints_size]))
    waypoints_size = len(data.index)
    dataCopy = data.copy()
    # data = setGoalAndPrefs(dataCopy.iloc[0:int(waypoints_size * train_size)])
    # data = data.append(setGoalAndPrefs(dataCopy.iloc[int(waypoints_size * train_size):waypoints_size]))

#     data = transformCoordinates(data)
    waypoints_size = len(data.index)

    dataCopy = data.copy()
    data = transformData(dataCopy.iloc[0:int(waypoints_size * train_size)])
    data = data.append(transformData(dataCopy.iloc[int(waypoints_size * train_size):waypoints_size]))
    data = pd.DataFrame(data[hierarchy_input_columns[level_number_in_hierarchy] + list(set(hierarchy_output_columns[level_number_in_hierarchy]) - set(hierarchy_input_columns[level_number_in_hierarchy]))])
#     data_max = data.max()
#     data_min = data.min()
#     data = (data - (data_max + data_min)/2.0) / (data_max - data_min)*2.0

    data += 1


    data_in = pd.DataFrame(data[hierarchy_input_columns[level_number_in_hierarchy]])
    data_out = pd.DataFrame(data[hierarchy_output_columns[level_number_in_hierarchy]])

    if output_displacement:
        data = data[:-1]
        data_in = data_in[:-1]
        data_out = gripper_difference(data_out)



#     print data_in.min()
#     print data_in.max()
#     print data_in.mean()
#     print data_out.min()
#     print data_out.max()
#     print data_out.mean()
#     print data_out.std()
#     print data_out.var()

    print "Any nulls? ", data.isnull().values.any()
    # print data_in.as_matrix()[5000:5010]
    # print data_out.as_matrix()[5000:5010]

    print "input columns: ", hierarchy_input_columns[level_number_in_hierarchy]
    print "output_columns: ", hierarchy_output_columns[level_number_in_hierarchy]
    print "Sample waypoints: ", waypoints_size
    print 'Data shape: ', np.shape(data)

    return data.as_matrix(), data_in.as_matrix(), data_out.as_matrix(), data.min(), data.max()

def getHiddens(data_in):
    in_size = helper_hidden_size * helper_num_layers
    predict_funcs, initials = load_models(models= helper_models, hidden_size= helper_hidden_size, num_layers=helper_num_layers, model=layer_models[0])
    hierarchy_values = [None] * (len(predict_funcs) + 1)
    hiddens = np.empty((len(data_in), in_size), dtype=theano.config.floatX)
    for i in range(len(data_in)):
        sys.stdout.write('\r' + str(i) + '/' + str(len(data_in)))
        sys.stdout.flush() # important
        x = np.array(data_in[i], dtype=theano.config.floatX)
        hierarchy_values, newinitials = predict_one_timestep(predict_funcs, initials, x, hierarchy_values, i)
        state_values = np.empty(0)
        for j in range(len(initials[0])):
            if "state" in initials[0][j].name:
                state_values = np.concatenate((state_values, newinitials[j].flatten()))
        hiddens[i] = state_values
    data_in = np.column_stack((hiddens, data_in))
    return data_in
def main():
    data, data_in, data_out, data_min, data_max = train_test_split()
    out_size = len(hierarchy_output_columns[level_number_in_hierarchy])
    if use_helper_model:
        in_size = helper_hidden_size * helper_num_layers + len(hierarchy_input_columns[level_number_in_hierarchy])
        data_in = getHiddens(data_in)
    else:
        in_size = len(hierarchy_input_columns[level_number_in_hierarchy])
    max_prediction = max(future_predictions)
    if len(data) % seq_length > 0:
        data = data[:len(data) - len(data) % seq_length + max_prediction]
    else:
        data = data[:len(data) - seq_length + max_prediction]
    nsamples = len(data) // seq_length

    if single_dim_out:
        inputs = np.empty((nsamples, seq_length * out_size, in_size), dtype=theano.config.floatX)
        outputs = np.empty((nsamples, seq_length * out_size, len(future_predictions)), dtype=theano.config.floatX)
        for i, p in enumerate(range(0, len(data) - max_prediction, seq_length)):
            for out_i in range(out_size):
                inputs[i,out_i*seq_length:(out_i+1)*seq_length] = np.array([d for d in data_in[p:p + seq_length]])
                for j in range(len(future_predictions)):
                    outputs[i, :, j * out_size:(j + 1) * out_size] = np.array([d for d in data_out[p + future_predictions[j]:p + seq_length + future_predictions[j]]])
    else:
        inputs = np.empty((nsamples, seq_length, in_size), dtype=theano.config.floatX)
        outputs = np.empty((nsamples, seq_length, len(future_predictions) * out_size), dtype=theano.config.floatX)
        for i, p in enumerate(range(0, len(data) - max_prediction, seq_length)):
            inputs[i] = np.array([d for d in data_in[p:p + seq_length]])
            for j in range(len(future_predictions)):
                outputs[i, :, j * out_size:(j + 1) * out_size] = np.array([d for d in data_out[p + future_predictions[j]:p + seq_length + future_predictions[j]]])

    if cost_mode == 'Softmax':
        outputs *= 10 ^ out_round_decimal
        outputs = outputs.astype('uint8')
        outs = list(set(outputs.flat))
        outs_size = len(outs)
        out_to_ix = {ch: i for i, ch in enumerate(outs)}
        ix_to_out = {i: ch for i, ch in enumerate(outs)}
        outputs = np.vectorize(out_to_ix.get)(outputs)
        print outputs[0:1]
        print "out size: ", outs_size

    nsamples = len(inputs)
    nsamples_train = int(nsamples * train_size)

    if noise > 0:
        noise_array = np.random.normal(0, noise, inputs.shape)
        inputs[0:nsamples_train] += noise_array[0:nsamples_train]
		
    print np.isnan(np.sum(inputs))
    print np.isnan(np.sum(outputs))
    inputs = np.nan_to_num(inputs)
    print len(data)
	
    f = h5py.File(hdf5_file, mode='w')
    features = f.create_dataset('features', inputs.shape, dtype=theano.config.floatX)
    if cost_mode == 'Softmax':
        targets = f.create_dataset('targets', outputs.shape, dtype='uint8')
        targets.attrs['out_to_ix'] = yaml.dump(out_to_ix)
        targets.attrs['ix_to_out'] = yaml.dump(ix_to_out)
    else:
        targets = f.create_dataset('targets', outputs.shape, dtype=theano.config.floatX)
    features[...] = inputs
    targets[...] = outputs
    features.dims[0].label = 'batch'
    features.dims[1].label = 'sequence'
    features.dims[2].label = 'input'
    targets.dims[0].label = 'batch'
    targets.dims[1].label = 'sequence'
    targets.dims[2].label = 'input'


    split_dict = {
        'train': {'features': (0, nsamples_train), 'targets': (0, nsamples_train)},
        'test': {'features': (nsamples_train, nsamples), 'targets': (nsamples_train, nsamples)}}

    f.attrs['split'] = H5PYDataset.create_split_array(split_dict)
    f.flush()
    f.close()

    print 'inputs shape:', inputs.shape
    print 'outputs shape:', outputs.shape

    print 'Data loaded.'

if __name__ == "__main__":
    main()
