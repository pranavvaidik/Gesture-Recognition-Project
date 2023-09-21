"""
This script was written to randomize the order of the images within the csv. However, the
names of the images can't be alphabetical or the tfrecord still doesn't shuffle, so this script
needs to be followed up with rename_shuffle.py to rename the image files and rename_filename_csv.py
to correct the filename within the csv.
"""

import csv
import random


#runs through all 5 models
for i in range(1,6):
	filename = "model{}.csv".format(i)
	newfilename = "shuffledmodel{}.csv".format(i)
	shuffledrows = []

	#reads all rows except the title row in the csv
	with open(filename, mode='r') as csv_file:
		csv_reader = csv.reader(csv_file)
		shuffledrows = list(csv_reader)[1:]

	#shuffles the rows
	random.shuffle(shuffledrows)

	#writes them back in to a new csv
	with open(newfilename, mode='w') as csv_file:
		csv_writer = csv.writer(csv_file, delimiter=',', lineterminator='\n')
		csv_writer.writerow(["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"])
		csv_writer.writerows(shuffledrows)
