# import the necessary packages
import os

# number of subjects
NUM_SUBJECTS = 5

# define the base path to the *original* input dataset and then use
# the base path to derive the image and annotations directories
#ORIG_IMAGES = os.path.sep.join([ORIG_BASE_PATH, "images"])
ORIG_IMAGES = "images"
ANNOT_BASE_PATH = "annotations"

# labels
LABELS = ['eight', 'five', 'four', 'horiz', 'nine', 'one', 'punch', 'seven', 'six', 'span', 'three', 'two']
#LABELS = ['eight', 'five', 'four', 'nine', 'one', 'punch', 'seven', 'six', 'span', 'three', 'two']
#LABELS = ['hand']

# define the base path to the *new* dataset after running our dataset
# builder scripts and then use the base path to derive the paths to
# our output class label directories
BASE_PATH = "dataset"

# define the number of max proposals used when running selective
# search for (1) gathering training data and (2) performing inference
MAX_PROPOSALS = 2000
MAX_PROPOSALS_INFER = 500

# define the maximum number of positive and negative images to be
# generated from each image
MAX_POSITIVE = 30
MAX_NEGATIVE = 10

# initialize the input dimensions to the network
INPUT_DIMS = (224, 224)

# define the path to the output model and label binarizer
MODEL_PATH = "hands_classifier.h5"
#MODEL_PATH = "oldclassifiers/vgg16_withoutnothing_realhands_classifier.h5"
ENCODER_PATH = "label_encoder.pickle"
RPN_PATH = "adjustedRCNN/RPN/faster_rcnn_resnet50_v1_640x640_coco17_tpu-8/saved_model"

# define the minimum probability required for a positive prediction
# (used to filter out false-positive predictions)
MIN_PROBA = 0.5

# retrain or use new model
RETRAIN = False