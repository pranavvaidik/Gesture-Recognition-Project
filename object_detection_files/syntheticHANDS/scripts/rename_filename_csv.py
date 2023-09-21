"""
This script was written to correct the names of the files in the csv after the images were renamed
because I forgot to combine it into one script. It reads the shuffledmodel{}.csv and renames the 
images to color_{count}.jpg to prevent the tfrecord from pulling the images in alphabetical order.
This is done because even though the csv is shuffled, the tfrecord still organizes the images 
by alphabetical order, so the training still didn't shuffle the data.
"""

import csv

allrows = []
modelnum = "5"

#open the shuffledmodel.csv and pull the rows
with open(f'shuffledmodel{modelnum}.csv', mode='r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')
    
    allrows = list(csv_reader)[1:]

#rename the filename in the rows from 0 to len(allrows)
for count in range(len(allrows)):
    allrows[count][0] = "color_" + f'{(count+1):05}.jpg'

#write the rows back into a new csv
with open(f'fixedshuffledmodel{modelnum}.csv', mode='w') as csv_file:
    csv_writer = csv.writer(csv_file, delimiter=',', lineterminator='\n')

    csv_writer.writerow(["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"])
    csv_writer.writerows(allrows)