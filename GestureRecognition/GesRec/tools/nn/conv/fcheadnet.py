# import necessary packages
from tensorflow.keras.layers import Dropout, Flatten, Dense, GlobalMaxPool2D

class FCHeadNet:
	@staticmethod
	def build(baseModel, classes, D = 128):
		# initialize the head model that will be placed on top of the base, then add an FC layer
		headModel = baseModel.output
		#headModel = Flatten(name="flatten")(headModel)
		headModel = GlobalMaxPool2D(name="global_max_pool")(headModel)
		#headModel = Dropout(0.1)(headModel)
		headModel = Dense(D, activation = "relu")(headModel)
		#headModel = Dropout(0.1)(headModel)
		
		# add softmax layer
		headModel = Dense(classes, activation = "softmax")(headModel)
		
		# return model
		return headModel
