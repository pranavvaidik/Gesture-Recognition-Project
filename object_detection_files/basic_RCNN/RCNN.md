This includes the scripts and code for the basic RCNN before it was scrapped for 
the faster RCNN.

The dataset folder shows an example of the images used to train the CNN within
the RCNN. Using the ground truth boxes for the HANDS dataset, cropped images of
the hand gestures were saved for training. 

the pyimagesearch folder contains most of the functions used for the RCNN, including
the intersection over union and non maxima suppression functions.

the imutils folder also contains many of the functions used for the RCNN.



Most of the code originates from this post by Adrian Rosebrock on pyimagesearch:

https://pyimagesearch.com/2020/07/13/r-cnn-object-detection-with-keras-tensorflow-and-deep-learning/