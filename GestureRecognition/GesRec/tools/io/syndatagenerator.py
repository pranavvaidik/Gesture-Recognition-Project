from tensorflow.keras.utils import to_categorical
import numpy as np
import cv2

class SynDatasetGenerator:
	def __init__(self, imagePaths, labels, batchSize, preprocessors=None, aug=None, binarize=True, classes =12):
		self.batchSize = batchSize
		self.preprocessors = preprocessors
		self.aug = aug
		self.binarize = binarize
		self.classes = classes
		self.imagePaths = imagePaths
		self.labels = np.array(labels)
		self.numImages = len(imagePaths)
		
	def generator(self, passes=np.inf):
		#initialize the epoch count
		epochs = 0
		
		# keep looking infinitely -- model will stop once it reaches a desired number of 
		# epochs
		while epochs < passes:
			# loop over hdf5 dataset
			for i in np.arange(0, self.numImages, self.batchSize):
				
				# load images and labels from paths
				images = self.load_batch(self.imagePaths[i:i+self.batchSize])
				labels = self.labels[i:i+self.batchSize]
				
				# check if labels should be binarized
				if self.binarize:
					labels = to_categorical(labels, self.classes)
										

				# if data augmentor exists, apply it
				if self.aug is not None:
					(images, labels)	= next(self.aug.flow(images, labels, batch_size=self.batchSize))
					
				yield (images, labels)
				
			# increment epochs
			epochs += 1
		
	def load_batch(self,imagePaths):
		images = []
		
		for path in imagePaths:
			image = cv2.imread(path)/255.0
			
			# check if preprocessors need to be applied
			if self.preprocessors is not None:
				# initialize list of processed images
				procImages = []
				
				for p in self.preprocessors:
					image = p.preprocess(image)
					
			images.append(image)	
		images = np.array(images)
		
		return images
