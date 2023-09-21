# import packages

# set the matplotlib backend so figures can be saved in background
import matplotlib
matplotlib.use("Agg")

# other imports
from config import synnet_config as config
from tools.preprocessing import ImageToArrayPreprocessor, SimplePreprocessor,MeanPreprocessor
from tools.callbacks import TrainingMonitor
from tools.io import HDF5DatasetGenerator, SynDatasetGenerator
from tools.nn.conv import SynNet, ShallowNet, FCHeadNet, EnsembleSoftmax
from tensorflow.keras.callbacks import ModelCheckpoint
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.models import Model
from tensorflow.keras.applications import VGG16, VGG19, ResNet50, InceptionV3, Xception
from tensorflow.keras.optimizers import Adam, RMSprop, SGD
from tensorflow.keras.metrics import TruePositives, FalsePositives, TrueNegatives, FalseNegatives, CategoricalAccuracy, Precision, Recall
from tensorflow.distribute import MirroredStrategy
from tensorflow.keras.layers import Input
from tools.datasets import VideoPredictor
from tools.datasets import SimpleDatasetLoader
from imutils import paths
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
import json
import os
import pickle
import argparse
import progressbar
import cv2
import numpy as np

#Will need to remove this part soon
######################################################################################
# construct argument parse and parse the arguments
ap = argparse.ArgumentParser()
#ap.add_argument("-model", "--model", type = str, default = "vgg16", help ="name of pre-trained network to use")
#args = vars(ap.parse_args())

# define a dictionary taht maps models to their classes
#MODELS = {"vgg16" : VGG16, "vgg19": VGG19, "resnet" : ResNet50, "inception" : InceptionV3, "xception" : Xception}
MODELS = {"vgg16" : VGG16, "inception" : InceptionV3, "xception" : Xception, "synnet" : SynNet }
#MODELS = {"xception" : Xception, "synnet" : SynNet }
model_names = [*MODELS]

#if args["model"] not in MODELS.keys():
#	raise AssertionError("The --model command line argument should be a key in the `MODELS` dictionary")
#######################################################################################

# initialize input shape based on model chosen
inputShape = (224,224)


# if using InceptionV3 or Xception, use (229x229) instead
#if args["model"] in ("inception","xception"):
#	inputShape = (229,229)

# construct training image generator for data augmentation
aug = ImageDataGenerator(rotation_range = 20, zoom_range = [0.85, 1.15], width_shift_range = 0.1,
			height_shift_range = 0.1, shear_range = 0.15, horizontal_flip = False, 
			fill_mode = "nearest")

# grab the paths to the images
imagePaths = list(paths.list_images('../data/images/'))[:500]

# get the labels
sdl = SimpleDatasetLoader()
labels = sdl.load_labels(imagePaths)

# encode and transform the classes
le = LabelEncoder()
labels = le.fit_transform(labels)

# perform sampling from the training set to build validation and test sets
(trainPaths, valPaths, trainLabels, valLabels) = train_test_split(imagePaths, labels, test_size = 0.4, random_state=42)
(testPaths, valPaths, testLabels, valLabels) = train_test_split(valPaths, valLabels, test_size = 0.5, random_state=42)

# calculate RGB means for training set
print("[INFO] Initializing RGB means for train set...")
# initialize image preprocessor and list of RGB channel averages
sp = SimplePreprocessor(inputShape[0],inputShape[1])
(R,G,B) = ([],[],[])

#initialize progress bar
widgets = ["Calculating Means:", progressbar.Percentage()," ", progressbar.Bar()," ",progressbar.ETA()]
pbar = progressbar.ProgressBar(maxval=len(trainPaths), widgets=widgets).start()

for (i, path) in enumerate(trainPaths):
	# read and process the image
	image = cv2.imread(path)/255.0
	image = sp.preprocess(image)
	
	# then compute the mean of each channel in the image and update the lists
	(b,g,r) = cv2.mean(image)[:3]
	R.append(r)
	G.append(g)
	B.append(b)
	
	pbar.update(i)
	
pbar.finish()

means = {"R" : np.mean(R), "G" : np.mean(G), "B":np.mean(B)}


# load the RGB means for training set
#means =  json.loads(open(config.DATASET_MEAN).read())

# initialize the other image preprocessors
mp = MeanPreprocessor(means["R"], means["G"], means["B"])
iap = ImageToArrayPreprocessor()


# initialize training and validation dataset generators
trainGen = SynDatasetGenerator(trainPaths, trainLabels, config.BATCH_SIZE, aug=aug, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
valGen = SynDatasetGenerator(valPaths, valLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
#testGen = HDF5DatasetGenerator(config.VAL_HDF5, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)

# initialize the optimizer
print("[INFO] compiling model...")
learning_rate_dict = {"vgg16" : 1e-4, "inception" : 1e-4, "xception" : 5e-4, "synnet" : 1e-3 } 
#opt = SGD(lr = 1e-3)#, decay = 0.001/200, momentum = 0.5, nesterov = True)
#opt = Adam(lr=learning_rate)
opt_dict = {m : Adam(lr=v) for (m,v) in learning_rate_dict.items() }

# load class weights
f = open(config.OUTPUT_PATH+"/class_weights.pkl","rb")
weights = pickle.loads(f.read())
f.close()


# instantiate Mirrored Strategy
strategy = MirroredStrategy()
#print('Number of devices: {}'.format(strategy.num_replicas_in_sync))

model_list = []
model_input =  Input(shape=inputShape+(3,))

with strategy.scope():
	
	for i in range(len(model_names)):


		# metrics for analysis
		metrics = [TruePositives(name = 'tp'), FalsePositives(name = 'fp'), TrueNegatives(name = 'tn'), FalseNegatives(name = 'fn'), CategoricalAccuracy(name="categorical_accuracy"), Precision(name='precision'), Recall(name = 'recall')]



		# Load the model
		#Network = MODELS[args["model"]]
		if model_names[i] != "synnet":
			Network = MODELS[model_names[i]] # change MODELS to a list instead		

			baseModel = Network(weights = None, include_top = False, input_tensor = model_input)

			# freeze all weights in baseModel
			for layer in baseModel.layers:#[:-25]: # 4 for vgg16, 15 for resnet,  
				layer.trainable = True

			# initialize the head model
			headModel = FCHeadNet.build(baseModel, classes = 12, D=256)

			# place fully connected model on top of base model
			model = Model(inputs = baseModel.input, outputs = headModel)
		else:
			model = MODELS[model_names[i]].build(model_input, classes=12)
		# Change layer names to unique names
		for layer in model.layers:
			layer._name = layer.name + '_' + str(i)
		
		model_list.append(model)

		# compile the model
		model_list[i].compile(optimizer=opt_dict[model_names[i]], loss="categorical_crossentropy",metrics=metrics)

	model_list[i].summary()

#input("Press Enter to start training")



epoch_values = {"vgg16" : 75, "inception" : 75, "xception" : 20, "synnet" : 75 }

# train the networks
for i,model in enumerate(model_list):
	print ("training model: ",model_names[i])

	# construct the set of callbacks
	FigPath = os.path.sep.join([ config.OUTPUT_PATH,model_names[i]+"_"+str(learning_rate_dict[model_names[i]])+"_performance.png"])
	checkpoint = ModelCheckpoint(config.OUTPUT_PATH + "/"+model_names[i]+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
	#callbacks = [checkpoint]
	callbacks = [TrainingMonitor(FigPath), checkpoint]
	
	H = model.fit(x = trainGen.generator(), 
				steps_per_epoch = trainGen.numImages//config.BATCH_SIZE,
				validation_data = valGen.generator(), 
				validation_steps=valGen.numImages//config.BATCH_SIZE,
				epochs = epoch_values[model_names[i]],
				#max_queue_size = 4,
				class_weight = weights,
				callbacks=callbacks,
				verbose=1)					
					

	print(H.history.keys())

	# evaluate the model
	#test_results = model.evaluate_generator(testGen.generator(),steps = testGen.numImages//config.BATCH_SIZE ,verbose = 1)

	#print(test_results)

	## save model to file			
	#print("[INFO] saving the model...")       
	#model.save(config.MODEL_PATH, overwrite = True)

	import pickle
	"""with open(config.OUTPUT_PATH + "/"+args["model"]+"_history.pkl","wb") as fp:
		#pickle.dump({'history': H.history, 'results': test_results}, fp)
		pickle.dump({'history': H.history}, fp)"""

# instantiate Ensemble Model
es = EnsembleSoftmax()

# build Ensemble Model
ensemble_model = es.build(model_list = model_list, model_input = model_input)

ensemble_model.summary()



