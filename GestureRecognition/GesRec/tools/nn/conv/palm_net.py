#import necessary packages
import cv2
import tensorflow as tf
from tensorflow import keras
from tensorflow.keras.models import load_model
from tensorflow.keras.layers import Dense, Flatten, Dropout, MaxPool2D, Conv2D, GlobalMaxPool2D
from models.mobilenet_model import get_mobilenet_model
from tensorflow.keras import backend as K

class PalmNet:
	@staticmethod
	def build(width, classes):
		# initializee the model along with input shape to be "channels last"
		depth = 3
		height = width
		inputShape = (height, width, depth)
		
		# Load openpose keras model
		op_model = get_mobilenet_model(alpha = 1.0, rows = width)
		
		# if we are using "channels first", update the input shape
		if K.image_data_format() == "channels_first":
			inputShape = (depth, height, width)
			
		weights_path = "best_pose_mobilenet_model/weights.best.mobilenet.h5"
		op_model.load_weights(weights_path)
		
		# freeze older layers
		for layer in op_model.layers:
			layer.trainable = False

		# start adding new layers
		op_out = op_model.output
		out = tf.concat(axis=3, values=[op_out[2], op_out[3]], name='concat_stage2_outs')
		out = Conv2D(64,kernel_size=(3,3), strides = (2,2),padding='same', activation='relu')(out)
		out = Conv2D(128,kernel_size=(3,3),padding='same', strides = (2,2), activation='relu')(out)
		out = Conv2D(128,kernel_size=(3,3),padding='same', strides = (2,2), activation='relu')(out)
		out = GlobalMaxPool2D()(out)
		
		# left hand branch
		out_left = Dropout(0.5)(out)
		out_left = Dense(512, activation = 'relu')(out_left)
		out_left = Dropout(0.5)(out_left)
		out_left = Dense(256, activation = 'relu')(out_left)
		out_left = Dropout(0.5)(out_left)
		out_left = Dense(classes, activation = 'softmax', name = 'left_out')(out_left)
		
		# right hand branch
		out_right = Dropout(0.5)(out)
		out_right = Dense(512, activation = 'relu')(out_right)
		out_right = Dropout(0.5)(out_right)
		out_right = Dense(256, activation = 'relu')(out_right)
		out_right = Dropout(0.5)(out_right)
		out_right = Dense(classes, activation = 'softmax', name='right_out')(out_right)
		
		
		# concatenate left and right blocks as outputs for final output
		model = keras.Model(inputs = op_model.input, outputs = [out_left,out_right])
		model.summary()
		
		return model
