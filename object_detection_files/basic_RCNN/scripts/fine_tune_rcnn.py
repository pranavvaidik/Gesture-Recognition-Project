# import the necessary packages
from pyimagesearch import config
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.layers import AveragePooling2D, GlobalAveragePooling2D, Rescaling, MaxPool2D
from tensorflow.keras.layers import Dropout
from tensorflow.keras.layers import Flatten
from tensorflow.keras.layers import Dense, Conv2D
from tensorflow.keras.layers import Input
from tensorflow.keras.models import Model
from tensorflow.keras.optimizers import Adam, SGD
from tensorflow.keras.preprocessing.image import img_to_array
from tensorflow.keras.preprocessing.image import load_img
from tensorflow.keras.utils import to_categorical
from sklearn.preprocessing import LabelBinarizer
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from imutils import paths
import matplotlib.pyplot as plt
import numpy as np
import argparse
import pickle
import os
import cv2

import keras
from tensorflow.keras.models import load_model
from tensorflow.keras.models import Sequential
from tensorflow.keras import layers
from tensorflow.keras.utils import image_dataset_from_directory
from classification_models.tfkeras import Classifiers
import tensorflow.keras.applications as apps
from tensorflow.keras.applications.vgg16 import preprocess_input
import tensorflow as tf

# construct the argument parser and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-p", "--plot", type=str, default="plot.png",
	help="path to output loss/accuracy plot")
args = vars(ap.parse_args())

# initialize the initial learning rate, number of epochs to train for,
# and batch size
INIT_LR = 1e-5
EPOCHS = 25
BS = 16

# access model and preprocessing function
#modelBase, preprocess_input = Classifiers.get('vgg16')

print("[INFO] loading images...")
# load the images (pipeline due to size)
traindata = image_dataset_from_directory(config.BASE_PATH, image_size=(224, 224), shuffle=True,
										 label_mode='categorical', batch_size=BS)


# setup data augmentation and iterator
data_augmentation = Sequential([
#	layers.RandomFlip("horizontal"),
#	layers.RandomRotation(0.05, fill_mode='nearest'),
	layers.RandomZoom(0.05, fill_mode='nearest'),
	layers.RandomTranslation(0.08, 0.08, fill_mode='nearest')])


traindata = traindata.map(lambda x, y: (((x)/255.0), y))
train_scaled_iterator = traindata.as_numpy_iterator()

batch = train_scaled_iterator.next()
for i in range(10):
	cv2.imshow("input image", batch[0][i])
	cv2.waitKey(0)

# set train/validation/test partition (50/25/25)
train_size = int(len(traindata)*0.6)
val_size = int(len(traindata)*0.2)
test_size = val_size

#if it doesn't add to the total number of batches, add the missing batches to validation
if (train_size+val_size+test_size != len(traindata)):
	for i in range(len(traindata)-(train_size+val_size+test_size)):
		val_size += 1

train = traindata.take(train_size)
val = traindata.skip(train_size).take(val_size)
test = traindata.skip(train_size).skip(val_size).take(test_size)

if(config.RETRAIN):
	model = load_model(config.MODEL_PATH)
else:
	# build base feature model
	baseModel = apps.mobilenet_v2.MobileNetV2(input_shape=(224,224,3), weights='imagenet', include_top=False)
	#baseModel = modelBase(input_shape=(224,224,3), weights='imagenet', include_top=False)

	# construct head model (top of base model)
	headModel = baseModel.output
	headModel = Flatten()(headModel)
	headModel = Dense(256, activation="relu")(headModel)
	#headModel = Dropout(0.2)(headModel)
	#headModel = Dense(512, activation="relu")(headModel)
	headModel = Dropout(0.3)(headModel)
	headModel = Dense(len(config.LABELS), activation="softmax")(headModel)

	# put headModel onto baseModel
	model = Model(inputs=[baseModel.input], outputs=[headModel])

	# freeze base model to prevent any training (for transfer learning, just train the 
	# head model and leave the base model parameters alone)
	n=0	#last block starts at 16
	for layer in baseModel.layers:
		#n+=1
		#if(n==18):
		#	break
		layer.trainable = False

	# compile the model
	print("[INFO] compiling the model...")
	opt = SGD(learning_rate=INIT_LR)
	model.compile(loss="categorical_crossentropy", optimizer=opt,
		metrics=["accuracy"])

model.summary()

callback = tf.keras.callbacks.EarlyStopping(monitor='val_loss', patience=3, verbose=1)
# train the head of the model
print("[INFO] training head of model...")
H = model.fit(
	train,
	validation_data=(val),
	epochs=EPOCHS,
	callbacks=[callback])

# make predictions with testing set
print("[INFO] evaluating network...")
model.evaluate(test, verbose=1)

#predIdxs = model.predict(test, batch_size=BS, verbose=1)

# for each image in the testing set we need to find the index of the
# label with corresponding largest predicted probability
#predIdxs = np.argmax(predIdxs, axis=1)

# show a nicely formatted classification report
#print(classification_report(testLabels, predIdxs,
#	labels=config.LABELS))

# serialize the model to disk
print("[INFO] saving mask detector model...")
model.save(config.MODEL_PATH, save_format='h5')

# plot the training loss and accuracy
N = EPOCHS
plt.style.use("ggplot")
plt.figure()
plt.plot(np.arange(0, N), H.history["loss"], label="train_loss")
plt.plot(np.arange(0, N), H.history["val_loss"], label="val_loss")
plt.plot(np.arange(0, N), H.history["accuracy"], label="train_acc")
plt.plot(np.arange(0, N), H.history["val_accuracy"], label="val_acc")
plt.title("Training Loss and Accuracy")
plt.xlabel("Epoch #")
plt.ylabel("Loss/Accuracy")
plt.legend(loc="lower left")
plt.savefig(args["plot"])

print("[INFO] Fine tuning complete")




"""
N = EPOCHS
plt.style.use("ggplot")
plt.figure()
plt.xlabel("Epoch #")
ax1 = plt.subplot()
ax1.plot(np.arange(0, N), H.history["loss"], label="train_loss")
ax1.plot(np.arange(0, N), H.history["val_loss"], label="val_loss")
ax1.ylabel("Loss")
ax1.legend(loc="lower left")

ax2 = ax1.twinx()
ax2.plot(np.arange(0, N), H.history["accuracy"], label="train_acc")
ax2.plot(np.arange(0, N), H.history["val_accuracy"], label="val_acc")
ax2.ylabel("Accuracy")
ax2.legend(loc="upper left")

plt.savefig(args["plot"])
"""






"""
# import the necessary packages
from pyimagesearch import config
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.layers import AveragePooling2D, GlobalAveragePooling2D, Rescaling
from tensorflow.keras.layers import Dropout
from tensorflow.keras.layers import Flatten
from tensorflow.keras.layers import Dense
from tensorflow.keras.layers import Input
from tensorflow.keras.models import Model
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.preprocessing.image import img_to_array
from tensorflow.keras.preprocessing.image import load_img
from tensorflow.keras.utils import to_categorical
from sklearn.preprocessing import LabelBinarizer
from sklearn.model_selection import train_test_split
from sklearn.metrics import classification_report
from imutils import paths
import matplotlib.pyplot as plt
import numpy as np
import argparse
import pickle
import os
import cv2

import keras
from tensorflow.keras.models import Sequential
from tensorflow.keras import layers
from tensorflow.keras.utils import image_dataset_from_directory
from classification_models.tfkeras import Classifiers
import tensorflow.keras.applications as apps
from tensorflow.keras.applications.vgg16 import preprocess_input

# construct the argument parser and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-p", "--plot", type=str, default="plot.png",
	help="path to output loss/accuracy plot")
args = vars(ap.parse_args())

# initialize the initial learning rate, number of epochs to train for,
# and batch size
INIT_LR = 1e-4
EPOCHS = 12
BS = 32

# access model and preprocessing function
#modelBase, preprocess_input = Classifiers.get('vgg16')

print("[INFO] loading images...")
# load the images (pipeline due to size)
traindata = image_dataset_from_directory(config.BASE_PATH, image_size=(224, 224), shuffle=True)

"""
# setup data augmentation and iterator
data_augmentation = Sequential([
	layers.RandomFlip("horizontal"),
	layers.RandomRotation(0.1, fill_mode='nearest'),
	layers.RandomZoom(0.1, fill_mode='nearest'),
	layers.RandomTranslation(0.15, 0.15, fill_mode='nearest'),
	layers.Rescaling(scale=1./255)])
"""

traindata = traindata.map(lambda x, y: (data_augmentation(preprocess_input(x)), y))
train_scaled_iterator = traindata.as_numpy_iterator()

batch = train_scaled_iterator.next()
for i in range(5):
	cv2.imshow("input image", batch[0][i])
	cv2.waitKey(0)

# set train/validation/test partition (50/25/25)
train_size = int(len(traindata)*0.5)
val_size = int(len(traindata)*0.25)
test_size = val_size

#if it doesn't add to the total number of batches, add the missing batches to validation
if (train_size+val_size+test_size != len(traindata)):
	for i in range(len(traindata)-(train_size+val_size+test_size)):
		val_size += 1

train = traindata.take(train_size)
val = traindata.skip(train_size).take(val_size)
test = traindata.skip(train_size).skip(val_size).take(test_size)

# build base feature model
baseModel = apps.vgg16.VGG16(input_shape=(224,224,3), weights='imagenet', include_top=False)
#baseModel = modelBase(input_shape=(224,224,3), weights='imagenet', include_top=False)

# construct head model (top of base model)
headModel = baseModel.output
headModel = Flatten()(headModel)
#headModel = Dense(256, activation="relu")(headModel)
#headModel = Dropout(0.2)(headModel)
headModel = Dense(128, activation="relu")(headModel)
headModel = Dropout(0.2)(headModel)
headModel = Dense(len(config.LABELS), activation="softmax")(headModel)

# put headModel onto baseModel
model = Model(inputs=[baseModel.input], outputs=[headModel])

# freeze base model to prevent any training (for transfer learning, just train the 
# head model and leave the base model parameters alone)
for layer in baseModel.layers:
	layer.trainable = False

# compile the model
print("[INFO] compiling the model...")
opt = Adam(learning_rate=INIT_LR)
model.compile(loss="sparse_categorical_crossentropy", optimizer=opt,
	metrics=["accuracy"])

model.summary()

# train the head of the model
print("[INFO] training head of model...")
H = model.fit(
	train,
	validation_data=(val),
	epochs=EPOCHS)

# make predictions with testing set
print("[INFO] evaluating network...")
model.evaluate(test, verbose=1)

#predIdxs = model.predict(test, batch_size=BS, verbose=1)

# for each image in the testing set we need to find the index of the
# label with corresponding largest predicted probability
#predIdxs = np.argmax(predIdxs, axis=1)

# show a nicely formatted classification report
#print(classification_report(testLabels, predIdxs,
#	labels=config.LABELS))

# serialize the model to disk
print("[INFO] saving mask detector model...")
model.save(config.MODEL_PATH, save_format="h5")

# plot the training loss and accuracy
N = EPOCHS
plt.style.use("ggplot")
plt.figure()
plt.plot(np.arange(0, N), H.history["loss"], label="train_loss")
plt.plot(np.arange(0, N), H.history["val_loss"], label="val_loss")
plt.plot(np.arange(0, N), H.history["accuracy"], label="train_acc")
plt.plot(np.arange(0, N), H.history["val_accuracy"], label="val_acc")
plt.title("Training Loss and Accuracy")
plt.xlabel("Epoch #")
plt.ylabel("Loss/Accuracy")
plt.legend(loc="lower left")
plt.savefig(args["plot"])

print("[INFO] Fine tuning complete")
"""