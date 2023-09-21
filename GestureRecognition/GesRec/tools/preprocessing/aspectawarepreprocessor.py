import cv2
import imutils

class AspectAwarePreprocessor:
	def __init__(self, width, height, inter = cv2.INTER_AREA):
		# store target image dimensions and interpolation methods 
		#for resizing
		self.width = width
		self.height = height
		self.inter = inter
		
	def preprocess(self, image):
		# grab the dimensions of the image and then initialize the 
		#details to use when cropping
		(h,w) = image.shape[:2]
		dW = 0
		dH = 0

		# if width is smaller than height, then resize along the width
		# (i.e., along the smaller dimension) and then update the deltas
		# to crop the height to the desired dimension
		if w < h:
			image = imutils.resize(image,width = self.width, inter = self.inter)
			dH = int((image.shape[0] - self.height)/2.0)
		# otherwise resize along height
		else:
			image = imutils.resize(image, height=self.height, inter=self.inter)
			dW = int((image.shape[1] - self.width)/2.0)

		# now regrab the dimensions and perform the crop
		(h,w) = image.shape[:2]
		image = image[dH:h-dH, dW:w-dW]

		# now resize the image to required spatial dimensions
		return cv2.resize(image, (self.width, self.height), interpolation=self.inter)


