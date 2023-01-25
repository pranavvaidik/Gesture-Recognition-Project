# import
from config import synnet_config as config
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
from tools.preprocessing import ImageToArrayPreprocessor, SimplePreprocessor
from tools.io import HDF5DatasetWriter
from tools.datasets import SimpleDatasetLoader
from imutils import paths
import numpy as np
import progressbar
import json
import cv2
import os
import pickle


print("Building...")

# grab the paths to the images
print("Collecting Image paths.....")
#imagePaths = list(paths.list_images('../Datasets/SignLanguageForNumbers/Train/'))#[:500]
imagePaths = list(paths.list_images('../Datasets/AmericanSignLanguage_Real/Train'))#[:500]

if not len(imagePaths):
    print("Please check the image path provided..")
    exit()

# get the labels
print("Loading labels...")
sdl = SimpleDatasetLoader()
trainLabels = sdl.load_labels(imagePaths, folder_label=True)

# define input shape
inputShape = (224,224)

# encode and transform the classes
print("Encoding labels...")
le = LabelEncoder()
trainLabels = le.fit_transform(trainLabels)

# save the encoded labels to file for future use
f = open("output/label_encoders.pkl","wb")
f.write(pickle.dumps(le.classes_))
f.close()

# perform sampling from the training set to build validation and test sets
(trainPaths, valPaths, trainLabels, valLabels) = train_test_split(imagePaths, trainLabels, test_size = 0.4, random_state=42)
(testPaths, valPaths, testLabels, valLabels) = train_test_split(valPaths, valLabels, test_size = 0.5, random_state=42)

# calculate class weights
class_weights = {x: (len(le.classes_) * len(trainLabels))/list(trainLabels).count(x) for x in set(trainLabels)}

# save class weights
f = open(config.OUTPUT_PATH+"/class_weights.pkl","wb")
f.write(pickle.dumps(class_weights))
f.close()

# construct a list pairing the training, validation and  testing image paths
# along with their corresponding labels and output HDF5 files
datasets = [("train", trainPaths, trainLabels, config.TRAIN_HDF5),
			("val", valPaths,valLabels, config.VAL_HDF5)]
#,			("test", testPaths, testLabels, config.TEST_HDF5)]
			
# initialize image preprocessor and list of RGB channel averages
sp = SimplePreprocessor(inputShape[0],inputShape[1])
(R,G,B) = ([],[],[])


"""
for (dType, paths, labels, outputPath) in datasets:
	# create HDF5 writer
	print("[INFO] building {}...".format(outputPath))
	writer = HDF5DatasetWriter( (len(paths),) + inputShape + (3,) , outputPath)
	
	#initialize progress bar
	widgets = ["Building Dataset:", progressbar.Percentage()," ", progressbar.Bar()," ",progressbar.ETA()]
	pbar = progressbar.ProgressBar(maxval=len(paths), widgets=widgets).start()
	
	for (i, (path, label)) in enumerate(zip(paths, labels)):
		# read and process the image
		image = cv2.imread(path)/255.0
		image = sp.preprocess(image)
		
		# if we are building a training dataset, then compute the mean of 
		# each channel in the image and update the lists
		if dType == "train":
			(b,g,r) = cv2.mean(image)[:3]
			R.append(r)
			G.append(g)
			B.append(b)
			
		# add image and labels to HDF5 writer
		writer.add([image],[label])
		pbar.update(i)
		
	# close HDF5 writer
	pbar.finish()
	writer.close()
"""

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




# construct a dictionary of averages then serialize the means to a JSON file
print("[INFO] serializing means...")
means = {"R" : np.mean(R), "G" : np.mean(G), "B":np.mean(B)}
f = open(config.DATASET_MEAN,"w")
f.write(json.dumps(means))
f.close()


# construct a dictionary of train, test and validation paths

data_paths = {"trainPaths" : trainPaths, "testPaths" : testPaths, "valPaths":valPaths, "trainLabels": trainLabels, "testLabels": testLabels, "valLabels": valLabels}
f = open(config.DATA_PATHS,"wb")
f.write(pickle.dumps(data_paths))
f.close()


