# import the necessary packages
from pyimagesearch.nms import non_max_suppression_slow
from pyimagesearch import config
from tensorflow.keras.applications.vgg16 import preprocess_input
from tensorflow.keras.preprocessing.image import img_to_array
from tensorflow.keras.models import load_model
import numpy as np
import argparse
import imutils
import pickle
import cv2


import keras
from tensorflow.keras.models import Sequential
from tensorflow.keras import layers
from tensorflow.keras.utils import image_dataset_from_directory
from classification_models.tfkeras import Classifiers

# access model and preprocessing function
#modelBase, preprocess_input = Classifiers.get('vgg16')

# construct the argument parser and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-i", "--image", required=True,
	help="path to input image")
args = vars(ap.parse_args())

# load the our fine-tuned model and label binarizer from disk
print("[INFO] loading model...")
model = load_model(config.MODEL_PATH)

# load the input image from disk
image = cv2.imread(args["image"])
image = imutils.resize(image, height=640, width=640)


# run selective search on the image to generate bounding box proposal
# regions
print("[INFO] running selective search...")
ss = cv2.ximgproc.segmentation.createSelectiveSearchSegmentation()
ss.setBaseImage(image)
ss.switchToSelectiveSearchQuality()
#ss.switchToSelectiveSearchFast()
rects = ss.process()


# initialize the list of region proposals that we'll be classifying
# along with their associated bounding boxes
proposals = []
boxes = []

# loop over the region proposal bounding box coordinates generated by
# running selective search
for (x, y, w, h) in rects[:config.MAX_PROPOSALS_INFER]:
	# extract the region from the input image, convert it from BGR to
	# RGB channel ordering, and then resize it to the required input
	# dimensions of our trained CNN
	roi = image[y:y + h, x:x + w]
	roi = cv2.cvtColor(roi, cv2.COLOR_BGR2RGB)
	roi = cv2.resize(roi, config.INPUT_DIMS,
		interpolation=cv2.INTER_CUBIC)
	
	# further preprocess the ROI
	roi = img_to_array(roi)/255.0
	#roi = preprocess_input(roi)
	
	# update our proposals and bounding boxes lists
	proposals.append(roi)
	boxes.append((x, y, x + w, y + h))

# convert the proposals and bounding boxes into NumPy arrays
proposals = np.array(proposals, dtype="float32")
boxes = np.array(boxes, dtype="int32")
print("[INFO] proposal shape: {}".format(proposals.shape))

# classify each of the proposal ROIs using fine-tuned model
print("[INFO] classifying proposals...")
proba = model.predict(proposals, batch_size=8)


for i in range(300):
	keep = False
	for x in proba[i]:
		if x > 0.7:
			keep = True
		
	if(keep):
		cv2.imshow("proposed region", proposals[i])
		print(proba[i])
		cv2.waitKey(0)


# further filter indexes by enforcing a minimum prediction
# probability be met
classIndex = np.argmax(proba, axis=1)
idxs = []
for i in range(len(proba)):
	if proba[i][classIndex[i]] >= config.MIN_PROBA:
		idxs.append(i)
boxes = boxes[idxs]
proba = proba[idxs]
classIndex = classIndex[idxs]

# clone the original image so that we can draw on it
clone = image.copy()

# loop over the bounding boxes and associated probabilities
for (box, prob, index) in zip(boxes, proba, classIndex):
	predProb = prob[index]

	if(index!=5):
		continue

	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = box
	cv2.rectangle(clone, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "{}".format(config.LABELS[int(index)]) + ": {:.2f}%".format(float(predProb) * 100)
	cv2.putText(clone, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 1)
	
# show the output after *before* running NMS
cv2.imshow("Before NMS", clone)

# run non-maxima suppression on the bounding boxes
boxIdxs, pick = non_max_suppression_slow(boxes, 0.4)
classIndex = classIndex[pick]
proba = proba[pick]

# loop over the bounding box indexes
for (i, prob, index) in zip(boxIdxs, proba, classIndex):
	predProb = prob[index]
	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = i
	cv2.rectangle(image, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "{}".format(config.LABELS[int(index)]) + ": {:.2f}%".format(float(predProb) * 100)
	cv2.putText(image, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 1)
	
# show the output image *after* running NMS
cv2.imshow("After NMS", image)
cv2.waitKey(0)

#code before allowing one class for the image (original code before testing)
"""
# clone the original image so that we can draw on it
clone = image.copy()

# loop over the bounding boxes and associated probabilities
for (box, prob, index) in zip(boxes, proba, classIndex):
	predProb = prob[index]

	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = box
	cv2.rectangle(clone, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "{}".format(config.LABELS[int(index)]) + ": {:.2f}%".format(float(predProb) * 100)
	cv2.putText(clone, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 1)
	
# show the output after *before* running NMS
cv2.imshow("Before NMS", clone)

# run non-maxima suppression on the bounding boxes

boxIdxs, pick = non_max_suppression_slow(boxes, 0.4)
proba = proba[pick]
classIndex = classIndex[pick]

# loop over the bounding box indexes
for (i, prob, index) in zip(boxIdxs, proba, classIndex):
	predProb = prob[index]
	
	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = i
	cv2.rectangle(image, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "{}".format(config.LABELS[int(index)]) + ": {:.2f}%".format(float(predProb) * 100)
	cv2.putText(image, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 1)
	
# show the output image *after* running NMS
cv2.imshow("After NMS", image)
cv2.waitKey(0)

"""









"""
# construct the argument parser and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-i", "--image", required=True,
	help="path to input image")
args = vars(ap.parse_args())

# load the our fine-tuned model and label binarizer from disk
print("[INFO] loading model and label binarizer...")
model = load_model(config.MODEL_PATH)
lb = pickle.loads(open(config.ENCODER_PATH, "rb").read())

# load the input image from disk
image = cv2.imread(args["image"])
image = imutils.resize(image, width=500)

# run selective search on the image to generate bounding box proposal
# regions
print("[INFO] running selective search...")
ss = cv2.ximgproc.segmentation.createSelectiveSearchSegmentation()
ss.setBaseImage(image)
ss.switchToSelectiveSearchFast()
rects = ss.process()

# initialize the list of region proposals that we'll be classifying
# along with their associated bounding boxes
proposals = []
boxes = []

# loop over the region proposal bounding box coordinates generated by
# running selective search
for (x, y, w, h) in rects[:config.MAX_PROPOSALS_INFER]:
	# extract the region from the input image, convert it from BGR to
	# RGB channel ordering, and then resize it to the required input
	# dimensions of our trained CNN
	roi = image[y:y + h, x:x + w]
	roi = cv2.cvtColor(roi, cv2.COLOR_BGR2RGB)
	roi = cv2.resize(roi, config.INPUT_DIMS,
		interpolation=cv2.INTER_CUBIC)
	
	# further preprocess the ROI
	roi = img_to_array(roi)
	roi = preprocess_input(roi)
	
	# update our proposals and bounding boxes lists
	proposals.append(roi)
	boxes.append((x, y, x + w, y + h))
	
# convert the proposals and bounding boxes into NumPy arrays
proposals = np.array(proposals, dtype="float32")
boxes = np.array(boxes, dtype="int32")
print("[INFO] proposal shape: {}".format(proposals.shape))

# classify each of the proposal ROIs using fine-tuned model
print("[INFO] classifying proposals...")
proba = model.predict(proposals)

# find the index of all predictions that are positive for the
# "raccoon" class
print("[INFO] applying NMS...")
labels = lb.classes_[np.argmax(proba, axis=1)]
idxs = np.where(labels == "raccoon")[0]

# use the indexes to extract all bounding boxes and associated class
# label probabilities associated with the "raccoon" class
boxes = boxes[idxs]
proba = proba[idxs][:, 1]

# further filter indexes by enforcing a minimum prediction
# probability be met
idxs = np.where(proba >= config.MIN_PROBA)
boxes = boxes[idxs]
proba = proba[idxs]

# clone the original image so that we can draw on it
clone = image.copy()

# loop over the bounding boxes and associated probabilities
for (box, prob) in zip(boxes, proba):
	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = box
	cv2.rectangle(clone, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "Raccoon: {:.2f}%".format(prob * 100)
	cv2.putText(clone, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 2)
	
# show the output after *before* running NMS
cv2.imshow("Before NMS", clone)

# run non-maxima suppression on the bounding boxes
boxIdxs = non_max_suppression(boxes, proba)

# loop over the bounding box indexes
for i in boxIdxs:
	# draw the bounding box, label, and probability on the image
	(startX, startY, endX, endY) = boxes[i]
	cv2.rectangle(image, (startX, startY), (endX, endY),
		(0, 255, 0), 2)
	y = startY - 10 if startY - 10 > 10 else startY + 10
	text= "Raccoon: {:.2f}%".format(proba[i] * 100)
	cv2.putText(image, text, (startX, y),
		cv2.FONT_HERSHEY_SIMPLEX, 0.45, (0, 255, 0), 2)
	
# show the output image *after* running NMS
cv2.imshow("After NMS", image)
cv2.waitKey(0)
"""
