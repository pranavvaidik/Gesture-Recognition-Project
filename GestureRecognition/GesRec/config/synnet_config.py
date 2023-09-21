# define paths to images directory for left hand
IMAGES_PATH_LEFT = "../data/images/"

# path to training and validation image paths
DATA_PATHS = {"ASL_real" : "output/alphabet/alphabet_real/data_paths.pkl",
              "ASL_syn" : "output/alphabet/alphabet_syn/data_paths.pkl",
              "Numbers_real": "output/numbers/numbers_real/data_paths.pkl",
              "Numbers_syn": "output/numbers/numbers_syn/data_paths.pkl"}

# Assign a number of test and validation images to measure the metrics
NUM_CLASSES = {'ASL_real': 28,'ASL_syn': 28, 'Numbers_real':10,'Numbers_syn':10, 'hands':10}
#NUM_VAL_IMAGES = 1250*NUM_CLASSES # change this later
#NUM_TEST_IMAGES = 1250*NUM_CLASSES # change this later

# define path for output training, validation and testing HDF5 files

TRAIN_HDF5 = {"ASL_real" : "../data/hdf5/AmericanSignLanguage_Real/train.hdf5",
             "ASL_syn" : "../data/hdf5/AmericanSignLanguage/train.hdf5",
             "Numbers_real" : "../data/hdf5/SignLanguageForNumbers_Real/train.hdf5",
             "Numbers_syn" : "../data/hdf5/SignLanguageForNumbers/train.hdf5" }
VAL_HDF5 = {"ASL_real" : "../data/hdf5/AmericanSignLanguage_Real/test.hdf5",
             "ASL_syn" : "../data/hdf5/AmericanSignLanguage/test.hdf5",
             "Numbers_real" : "../data/hdf5/SignLanguageForNumbers_Real/test.hdf5",
             "Numbers_syn" : "../data/hdf5/SignLanguageForNumbers/test.hdf5" }
TEST_HDF5 = {"ASL_real" : "../data/hdf5/AmericanSignLanguage_Real/val.hdf5",
             "ASL_syn" : "../data/hdf5/AmericanSignLanguage/val.hdf5",
             "Numbers_real" : "../data/hdf5/SignLanguageForNumbers_Real/val.hdf5",
             "Numbers_syn" : "../data/hdf5/SignLanguageForNumbers/val.hdf5" }

# path to output model file
MODEL_PATH = {"ASL_real" : "output/alphabet/syn_net.h5",
              "ASL_syn" : "output/alphabet",
              "Numbers_real": "output/alphabet",
              "Numbers_syn": "output/alphabet"}

# define the path to dataset mean
DATASET_MEAN = {"ASL_real" : "output/alphabet/alphabet_real/mean.json",
              "ASL_syn" : "output/alphabet/alphabet_syn/mean.json",
              "Numbers_real": "output/numbers/numbers_real/mean.json",
              "Numbers_syn": "output/numbers/numbers_syn/mean.json"}

# path to output directory for storing plots, classification reports, etc.
OUTPUT_PATH = {"ASL_real" : "output/alphabet/alphabet_real/",
              "ASL_syn" : "output/alphabet/alphabet_syn/",
              "Numbers_real": "output/numbers/numbers_real/",
              "Numbers_syn": "output/numbers/numbers_syn/"}

# batch size
BATCH_SIZE = 128
