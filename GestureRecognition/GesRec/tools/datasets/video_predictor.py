import cv2
import numpy as np
import json

class VideoPredictor:
	def __init__(self, model, classes,preprocessors=None):
		self.model = model
		self.preprocessors = preprocessors
		self.classes = classes

		if self.preprocessors is None:
			self.preprocessors = []
			
	def load(self, videoPaths):
		(classes_left,classes_right) = self.classes
		# loop over each video
		for video_path in videoPaths:
			# get file path for JSON laebls for the video
			json_file_path = video_path[:-4]+'_predicted.json'

			# initialize the labels in the required json format
			json_labels = {"left":{},"right":{}}
			for label in classes_left:
				json_labels["left"][label] = []
			for label in classes_right:
				json_labels["right"][label] = []
			

			# read video file
			video = cv2.VideoCapture(video_path)

			# get video metadata information
			frame_count = int(video.get(cv2.CAP_PROP_FRAME_COUNT))
			fps = video.get(cv2.CAP_PROP_FPS)
			duration = frame_count/fps
			width  = video.get(cv2.CAP_PROP_FRAME_WIDTH)  # float
			height = video.get(cv2.CAP_PROP_FRAME_HEIGHT) # float

			print("[INFO] processing file: ",video_path)

			# skip to next video if there is a problem opening the file
			if (video.isOpened()== False):  
				print("Error opening video file:",video_path)
				continue

			frame_number=0


			# Read until video is completed 
			while(video.isOpened()): 
				# videoture frame-by-frame 
				ret, frame = video.read()
				frame_time = frame_number/fps 
				frame_number += 1

				if ret == True:
					if self.preprocessors is not None:
						# apply each of the preprocessing to the image 
						# loaded in the order represented in the list
						for p in self.preprocessors:
							frame = p.preprocess(frame)
					
					frame = np.expand_dims(frame, axis=0)
					# change data format later
					left_pred, right_pred = self.model.predict(frame)
					for i,label in enumerate(classes_left):
						#print(left_pred)
						json_labels["left"][label].append([frame_time, float(left_pred[0][i])])
					for i,label in enumerate(classes_right):
						json_labels["right"][label].append([frame_time, float(right_pred[0][i]) ] )
							
					#json_labels.append({'frame_number':frame_number, 'time':frame_time,'predictions': [t.tolist() for t in ] })
					if frame_number%50 == 0:
						print(frame_number," frames have been processed")

				else:
					with open(json_file_path, 'w') as fp:
						json.dump(json_labels, fp)
						
					break
			
			
			
			
