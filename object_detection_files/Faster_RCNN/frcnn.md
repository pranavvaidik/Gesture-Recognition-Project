This folder has all the code/scripts for the faster RCNN object detection.
This was done using Tensorflow's Object Detection API. 

The configs folder contains the configuration for the faster RCNN. This file is where 
changes to the model architecture, data input, labelmap, etc. is done. 

The eval folder contains example images and Tensorflow evaluation logs that
can be viewed using tensorboard (cd to directory, then 'tensorboard --logdir=directory')

the image_outputs folder contains some example results from the faster RCNN. This folder 
contains results from the raccoon dataset, the base pretrained model with MS-COCO, bad results 
from before fixing the faster RCNN, and good results for both real and synthetic testing.

the labelmaps folder contains labelmaps generated using the RecordCreate.py script provided by 
the HANDS dataset.

The RPN folder contains the pretrained Region Proposal Network (RPN) that was downloaded from 
Tensorflow's model zoo. The checkpoint file was removed due to the file size.

The faster RCNN folder usually contains a tfrecords and trainingcheckpoints folder, but they were left out
because of the large file sizes. The tfrecords folder contains the generated tfrecord files, which is 
a file that contains the image and ground truth data in byte format as a single file. The trainingcheckpoints 
folder contains the checkpoints created during training. 

Evaluation for the mAP (mean average precision) on a specified dataset is done through the model_main_tf2.py 
script, but for testing individual images, the fasterrcnn.py script is used. This script allows for the 
direct choice of which checkpoint to use, and outputs an image with the predicted box and classification 
on it.
