"""
this script takes the existing csv for tfrecord generation and removes
the rows that contain images of the 'horiz' gesture. This was done 
because we didn't have time to add the functionality for the full-body
synthetic 'horiz' gesture, and we needed to change the real datasets to
match our synthetic datasets for proper testing.
"""

import csv

allrows = []
popindex = []

#change this number to choose which subject to work on
modelnum = "5"

#open the 12class csv
with open(f'../Subject{modelnum}/sub{modelnum}.csv', mode='r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')

    allrows = list(csv_reader)[1:]

#save the index of all the 'horiz' gesture rows
for i in range(len(allrows)):
    if allrows[i][3] == 'horiz':
        popindex.append(i)

#list comprehension, saving all elements if their index isn't in the popindex list
newrows = [ele for idx, ele in enumerate(allrows) if idx not in popindex]

#writing remaining rows to a new csv without the 'horiz' gesture
with open(f'../Subject{modelnum}/sub{modelnum}nohoriz.csv', mode='w') as csv_file:
    csv_writer = csv.writer(csv_file, delimiter=',', lineterminator='\n')

    csv_writer.writerow(["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"])
    csv_writer.writerows(newrows)