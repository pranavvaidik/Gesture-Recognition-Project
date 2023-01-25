# define paths to images directory for left hand
IMAGES_PATH_LEFT = "../data/images/"

# path to training and validation image paths
DATA_PATHS = "output/data_paths.pkl"

# Assign a number of test and validation images to measure the metrics
NUM_CLASSES = 28
NUM_VAL_IMAGES = 1250*NUM_CLASSES # change this later
NUM_TEST_IMAGES = 1250*NUM_CLASSES # change this later

# define path for output training, validation and testing HDF5 files
TRAIN_HDF5 = "../data/hdf5/train.hdf5"
VAL_HDF5 = "../data/hdf5/val.hdf5"
TEST_HDF5 = "../data/hdf5/test.hdf5"

# path to output model file
MODEL_PATH = "output/syn_net.h5"

# define the path to dataset mean
DATASET_MEAN = "output/mean.json"

# path to output directory for storing plots, classification reports, etc.
OUTPUT_PATH = "output"

# batch size
BATCH_SIZE = 64
