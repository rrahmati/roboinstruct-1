import sys
import h5py
import yaml
import numpy as np
from fuel.datasets import H5PYDataset
from fuel.streams import DataStream
from fuel.schemes import SequentialScheme, ShuffledScheme
from fuel.transformers import Mapping
from blocks.extensions import saveload, predicates
from blocks.extensions.training import TrackTheBest
from blocks import main_loop
from fuel.utils import do_not_pickle_attributes

#Define this class to skip serialization of extensions
@do_not_pickle_attributes('extensions')
class MainLoop(main_loop.MainLoop):

    def __init__(self, **kwargs):
        super(MainLoop, self).__init__(**kwargs)

    def load(self):
        self.extensions = []


def transpose_stream(data):
    return (np.swapaxes(data[0],0,1), np.swapaxes(data[1],0,1))


def track_best(channel, save_path):
    sys.setrecursionlimit(1500000)
    tracker = TrackTheBest(channel, choose_best=min)
    checkpoint = saveload.Checkpoint(
        save_path, after_training=False, use_cpickle=True)
    checkpoint.add_condition(["after_epoch"],
                             predicate=predicates.OnLogRecord('{0}_best_so_far'.format(channel)))
    return [tracker, checkpoint]

def get_metadata(hdf5_file):
    with h5py.File(hdf5_file) as f:
        ix_to_out = yaml.load(f['targets'].attrs['ix_to_out'])
        out_to_ix = yaml.load(f['targets'].attrs['out_to_ix'])
        out_size = len(ix_to_out)
    return ix_to_out, out_to_ix, out_size

def get_stream(hdf5_file, which_set, batch_size=None):
    dataset = H5PYDataset(
        hdf5_file, which_sets=(which_set,), load_in_memory=True)
    if batch_size == None:
        batch_size = dataset.num_examples
    stream = DataStream(dataset=dataset, iteration_scheme=ShuffledScheme(
        examples=dataset.num_examples, batch_size=batch_size))
    # Required because Recurrent bricks receive as input [sequence, batch,
    # features]

    return Mapping(stream, transpose_stream)
