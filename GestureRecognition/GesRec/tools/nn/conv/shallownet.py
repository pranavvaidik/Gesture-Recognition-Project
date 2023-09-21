#import necessary packages
import cv2
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import BatchNormalization, Dense, Flatten, Dropout, MaxPool2D, Conv2D, GlobalMaxPool2D, Activation
#from models.mobilenet_model import get_mobilenet_model
from tensorflow.keras import backend as K

class ShallowNet:
	@staticmethod
	def build(width, height, classes):
		# initializee the model along with input shape to be "channels last"
		depth = 3
		#height = width
		inputShape = (height, width, depth)
		chanDim = -1
				
		# if we are using "channels first", update the input shape
		if K.image_data_format() == "channels_first":
			inputShape = (depth, height, width)
			chanDim = 1
		
		model = Sequential()
		

		model.add(Conv2D(32, kernel_size = (3,3), padding = 'same', input_shape = inputShape))
		model.add(Activation('relu'))
		#model.add(BatchNormalization(axis = chanDim))
		
		# FC => RELU Layer
		model.add(Flatten())
		
		# softmax classifier
		model.add(Dense(classes))
		model.add(Activation("softmax"))
		
		
		
		#model.summary()
		
		return model
