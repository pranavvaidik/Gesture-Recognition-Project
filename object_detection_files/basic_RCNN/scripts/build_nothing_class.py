# import the necessary packages
from pyimagesearch.iou import compute_iou
from pyimagesearch import config
from bs4 import BeautifulSoup
from imutils import paths
import cv2
import os


roiCount = 0

for x in range(1, config.NUM_SUBJECTS + 1):
	print("Processing images from subject {}".format(x))
	#imagePaths = list(paths.list_images(os.path.sep.join([config.ORIG_IMAGES, "Subject{}".format(x)])))
	imageBasePath = os.path.sep.join([config.ORIG_IMAGES, "Subject{}".format(x)])
	allImages = []
	gtBoxes = []
	
	#get annotations from text file
	annotFile = os.path.sep.join([config.ANNOT_BASE_PATH, "Subject{}.txt".format(x)])

	#opens and goes through file
	count = 0
	f = open(annotFile, "r")
	next(f)
	for line in f:
		if (count==325):
			break
		#get the image name
		currentLine = line.split(",")
		currentImage = currentLine[0][currentLine[0].rfind("Color\\")+6:]
		allImages.append(currentImage)
		imageFile = os.path.sep.join([imageBasePath, currentImage])

		#get the ground truth boxes
		num = 0
		for y in currentLine[2:]:
			#if 2 boxes found already, skip the rest
			if num == 2:
				break

			# change the str into a list
			y = y.replace('[', '').replace(']', '').split()
			
            # change numbers from str to float
			for i in range(len(y)):
				y[i] = float(y[i])
				
			#format is [x, y, w, h]
			#if width and height are zero, it's an empty bounding box
			if ((y[2] == 0) & (y[3] == 0)):
				continue

			#convert ground truth boxes to (startX, startY, endX, endY)
			gt = (y[0], y[1], y[0] + y[2], y[1] + y[3])
			gtBoxes.append(gt)
			num += 1
		count += 1
	f.close()
	
	print("allImages length: ", len(allImages))
	print("gtBoxes length: ", len(gtBoxes))
	print("Starting IoU calculations for images from subject {}".format(x))

	#variable to track ground truth boxes (2 boxes per image)
	gtCount = -2

	for imageName in allImages:
		#increment gtCount for next image's ground truth boxes
		gtCount += 2

		if ((gtCount + 1) >= len(gtBoxes)):
			break

		# track number of ROIs per image
		ROIs = 0

		# get and read image
		imagePath = os.path.sep.join([imageBasePath, imageName])
		
		# load the input image from disk
		image = cv2.imread(imagePath)

		# run selective search on the image and initialize our list of
		# proposed boxes
		ss = cv2.ximgproc.segmentation.createSelectiveSearchSegmentation()
		ss.setBaseImage(image)
		ss.switchToSelectiveSearchFast()
		rects = ss.process()
		proposedRects= []
		
		# loop over the rectangles generated by selective search
		for (x, y, w, h) in rects:
			# convert our bounding boxes from (x, y, w, h) to (startX,
			# startY, startX, endY)
			proposedRects.append((x, y, x + w, y + h))
		
		saved = False
		trackNum = 0
		# loop over the maximum number of region proposals
		for proposedRect in proposedRects[:config.MAX_PROPOSALS]:
			if (saved):
				break

			#initialize
			(propStartX, propStartY, propEndX, propEndY) = proposedRect
			iou1 = 0
			iou2 = 0
			# compute the intersection over union between the two
			# boxes and unpack the ground-truth bounding box

			iou1 = compute_iou(gtBoxes[gtCount], proposedRect)
			iou2 = compute_iou(gtBoxes[gtCount + 1], proposedRect)


			# initialize the ROI and output path
			roi = None
			outputPath = None

			# check to see if the IOU is greater than 70% *and* that
			# we have not hit our positive count limit
			if (iou1 < 0.05) and (iou2 < 0.05):
				filename = "{:05d}.png".format(roiCount + 1)
				roi = image[propStartY:propEndY, propStartX:propEndX]
				outputPath = os.path.sep.join([config.BASE_PATH, "nothing", filename])
				roiCount += 1
				trackNum += 1


			# check to see if both the ROI and output path are valid
			if roi is not None and outputPath is not None:
				# resize the ROI to the input dimensions of the CNN
				# that we'll be fine-tuning, then write the ROI to
				# disk
				roi = cv2.resize(roi, config.INPUT_DIMS,
					interpolation=cv2.INTER_CUBIC)
				cv2.imwrite(outputPath, roi)
				saved = True

print("total roi count: ", roiCount)