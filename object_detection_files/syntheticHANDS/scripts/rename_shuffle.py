"""
takes the shuffled csv and renames the images in the model folder to match
the shuffled csv image order.

this is being done because the csv was shuffled so the tfrecord would
also shuffle, but apparently the TFRecord still pulls images in
alphabetical order, so now the images need to be in alphanumeric order
to match the corresponding shuffled csv.
"""

import csv
import os

allrows = []
model_number = "5"

#read rows from existing csv
csv_name = "shuffledmodel{}.csv".format(model_number)
with open(csv_name, mode='r') as csv_file:
    csv_reader = csv.reader(csv_file, delimiter=',')

    allrows = list(csv_reader)[1:]

#rename images in folder numerically in order of shuffled csv
for count in range(len(allrows)):
    src = f"./Model{model_number}/" + allrows[count][0]
    dst = f"./Model{model_number}/color_" + f'{(count+1):05}.jpg'

    os.rename(src, dst)