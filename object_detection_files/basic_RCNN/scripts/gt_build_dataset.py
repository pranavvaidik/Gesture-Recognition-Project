# import the necessary packages
from pyimagesearch.iou import compute_iou
from pyimagesearch import config
from bs4 import BeautifulSoup
from imutils import paths
import cv2
import os

# This function pulls the ground truth regions from the provided real images
# and places them in a dataset folder for training the hand gesture 
# recognition CNN within the RCNN


#number of total ROIs for each gesture (for naming)
one = 0
two = 0
three = 0
four = 0
five = 0
six = 0
seven = 0
eight = 0
nine = 0
punch = 0
span = 0
horiz = 0

for x in range(1, config.NUM_SUBJECTS + 1):
	print("[INFO] Processing images from subject {}".format(x))
	imageBasePath = os.path.sep.join([config.ORIG_IMAGES, "Subject{}".format(x)])
	allImages = []
	allLabels = []
	gtBoxes = []
	
	#get annotations from text file
	annotFile = os.path.sep.join([config.ANNOT_BASE_PATH, "Subject{}.txt".format(x)])

	#opens and goes through file
	f = open(annotFile, "r")
	next(f)
	for line in f:
		#get the image name
		currentLine = line.split(",")
		currentImage = currentLine[0][currentLine[0].rfind("Color\\")+6:]
		allImages.append(currentImage)
		imageFile = os.path.sep.join([imageBasePath, currentImage])

		#get the ground truth boxes
		index = 0
		count = 0
		label = ""
		for y in currentLine[2:]:
			#if 2 boxes found already, skip the rest
			if count == 2:
				break

			# change the str into a list
			y = y.replace('[', '').replace(']', '').split()
			
            # change numbers from str to float
			for i in range(len(y)):
				y[i] = int(float(y[i]))
				
			#format is [x, y, w, h]
			#if width and height are zero, it's an empty bounding box
			if ((y[2] == 0) & (y[3] == 0)):
				index += 1
				continue

			# exclude two handed gestures
			if index > 25:
				allImages.pop()
				break

			#getting label of img
			if 0 <= index <= 1:
				label = "punch"
			elif 2 <= index <= 3:
				label = "one"
			elif 4 <= index <= 5:
				label = "two"
			elif 6 <= index <= 7:
				label = "three"
			elif 8 <= index <= 9:
				label = "four"
			elif 10 <= index <= 11:
				label = "five"
			elif 12 <= index <= 13:
				label = "six"
			elif 14 <= index <= 15:
				label = "seven"
			elif 16 <= index <= 17:
				label = "eight"
			elif 18 <= index <= 19:
				label = "nine"
			elif 20 <= index <= 21:
				label = "span"
			elif 22 <= index <= 25:
				label = "horiz"


			allLabels.append(label)

			#convert ground truth boxes to (startX, startY, endX, endY)
			gt = (y[0], y[1], y[0] + y[2], y[1] + y[3])
			gtBoxes.append(gt)
			index += 1
			count += 1
	f.close()
	
	print("[INFO] Starting IoU calculations for images from subject {}".format(x))
	#IoU for images of current subject

	#variable to track ground truth boxes (2 boxes per image)
	gtCount = -2

	for imageName in allImages:	
		#increment gtCount for next image's ground truth boxes
		gtCount += 2

		# get and read image
		imagePath = os.path.sep.join([imageBasePath, imageName])
		
		# load the input image from disk
		image = cv2.imread(imagePath)

		for gtBox in gtBoxes[gtCount:gtCount + 2]:
			# compute the intersection over union between the two
			# boxes and unpack the ground-truth bounding box


			(gtStartX, gtStartY, gtEndX, gtEndY) = gtBox

			# initialize the ROI and output path
			roi = None
			outputPath = None


			# extract the ROI and then derive the output path to
			# the positive instance
			roi = image[gtStartY:gtEndY, gtStartX:gtEndX]

			# keeps track of image count for each gesture (for naming)
			if (allLabels[gtCount]) == "one":	
				filename = "{:05d}.png".format((one + 1))
				one += 1
			elif (allLabels[gtCount]) == "two":
				filename = "{:05d}.png".format((two + 1))
				two += 1
			elif (allLabels[gtCount]) == "three":
				filename = "{:05d}.png".format((three + 1))
				three += 1
			elif (allLabels[gtCount]) == "four":
				filename = "{:05d}.png".format((four + 1))
				four += 1
			elif (allLabels[gtCount]) == "five":
				filename = "{:05d}.png".format((five + 1))
				five += 1
			elif (allLabels[gtCount]) == "six":
				filename = "{:05d}.png".format((six + 1))
				six += 1
			elif (allLabels[gtCount]) == "seven":
				filename = "{:05d}.png".format((seven + 1))
				seven += 1
			elif (allLabels[gtCount]) == "eight":
				filename = "{:05d}.png".format((eight + 1))
				eight += 1
			elif (allLabels[gtCount]) == "nine":
				filename = "{:05d}.png".format((nine + 1))
				nine += 1
			elif (allLabels[gtCount]) == "punch":
				filename = "{:05d}.png".format((punch + 1))
				punch += 1
			elif (allLabels[gtCount]) == "span":
				filename = "{:05d}.png".format((span + 1))
				span += 1
			elif (allLabels[gtCount]) == "horiz":
				filename = "{:05d}.png".format((horiz + 1))
				horiz += 1

			outputPath = os.path.sep.join([config.BASE_PATH, allLabels[gtCount], filename])

			# check to see if both the ROI and output path are valid
			if roi is not None and outputPath is not None:
				# resize the ROI to the input dimensions of the CNN
				# that we'll be fine-tuning, then write the ROI to
				# disk
				roi = cv2.resize(roi, config.INPUT_DIMS,
					interpolation=cv2.INTER_CUBIC)
				cv2.imwrite(outputPath, roi)
			

print("[INFO] Total number of images (1-9, punch, span, horiz):")
print(one, two, three, four, five, six, seven, eight, nine, punch, span, horiz)
