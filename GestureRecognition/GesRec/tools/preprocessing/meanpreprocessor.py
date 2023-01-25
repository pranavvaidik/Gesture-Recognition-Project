import cv2

class MeanPreprocessor:
	def __init__(self, rMean, gMean, bMean):
		# store the r,g,b channel averages across the train set
		self.rMean = rMean
		self.gMean = gMean
		self.bMean = bMean
		
	def preprocess(self, image):
		# split image to channels
		(B,G,R) = cv2.split(image.astype("float32"))
		
		# subtract means from each channel
		R -= self.rMean
		B -= self.bMean
		G -= self.gMean
		
		# merge channels back together
		return cv2.merge([B,G,R])
