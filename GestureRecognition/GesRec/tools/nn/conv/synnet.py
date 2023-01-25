#import necessary packages
import cv2
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import BatchNormalization, Dense, Flatten, Dropout, MaxPool2D, Conv2D, GlobalMaxPool2D, Activation
#from models.mobilenet_model import get_mobilenet_model
from tensorflow.keras import backend as K

class SynNet:
	@staticmethod
	def build(model_input, classes):
		# initializee the model along with input shape to be "channels last"
		depth = 3
		#height = width
		
		#inputShape = (height, width, depth)
		
		chanDim = -1
				
		# if we are using "channels first", update the input shape
		if K.image_data_format() == "channels_first":
			#inputShape = (depth, height, width)
			chanDim = 1
		

		model = Sequential()
		model.add(model_input)

		# First CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(8, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(8, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))
		
		# Second CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(16, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(16, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))

		# Third CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(32, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(32, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))
		
		# Fourth CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(64, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(64, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))

		# Fifth CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(128, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(128, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))
		
		# Sixth CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(256, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(256, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))
		
		# Seventh CONV => RELU => CONV => RELU => POOL layer set
		model.add(Conv2D(512, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(Conv2D(512, kernel_size = (3,3), padding = 'same'))
		model.add(Activation('relu'))
		model.add(BatchNormalization(axis = chanDim))
		
		model.add(MaxPool2D(pool_size = (2,2)))
		model.add(Dropout(0.25))
		
		# FC => RELU Layer
		model.add(Flatten())
		model.add(Dense(128))
		model.add(Activation('relu'))
		model.add(BatchNormalization())
		model.add(Dropout(0.5))
		
		# softmax classifier
		model.add(Dense(classes))
		model.add(Activation("softmax"))
		
		
		
		#model.summary()
		
		return model
