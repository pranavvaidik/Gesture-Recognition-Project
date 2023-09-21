"""
This script takes the csv created using the .txt annotation file and 
reformats it to match the format needed for the generate_tfrecord.py
script that was taken from the raccoon dataset code.
"""

import csv

#function to change the column index into the class name
def index_to_class(index):
    if (index==1 or index==2):
        return "punch"
    elif (index==3 or index==4):
        return "one"
    elif (index==5 or index==6):
        return "two"
    elif (index==7 or index==8):
        return "three"
    elif (index==9 or index==10):
        return "four"
    elif (index==11 or index==12):
        return "five"
    elif (index==13 or index==14):
        return "six"
    elif (index==15 or index==16):
        return "seven"
    elif (index==17 or index==18):
        return "eight"
    elif (index==19 or index==20):
        return "nine"
    elif (index==21 or index==22):
        return "span"
    elif (index>=23 and index<=26):
        return "horiz"
    
imgfilename = ""
width = "960"
height = "540"
imgclass = ""
xmin = ""
ymin = ""
xmax = ""
ymax = ""

#open the csv, skip the first title line, then gather the data
finalrows = [["filename", "width", "height", "class", "xmin", "ymin", "xmax", "ymax"]]
with open('../Subject5/sub5csv.csv', mode='r') as csv_input:
    csv_reader = csv.reader(csv_input, delimiter=',')

    #skip the title line
    skipline = True
    for row in csv_reader:
        if skipline:
            skipline=False
            continue
        
        #iterate over the row elements
        for i in range(len(row)):
            if i==0:
                #remove the path, keep the name of the image file
                imgfilename = row[0][18:]
            else:
                #if the box is all zeroes, continue 
                box = row[i]
                if (box=="[0 0 0 0]"):
                    continue
                else:
                    #remove the brackets and split to obtain the ground truth coords
                    box = box.replace("[", "").replace("]", "").split()
                    xmin = box[0]
                    ymin = box[1]
                    #HANDS format is [xmin,ymin,deltax,deltay]. convert to [xmin,ymin,xmax,ymax]
                    xmax = str(int(float(box[0])) + int(float(box[2])))
                    ymax = str(int(float(box[1])) + int(float(box[3])))
                    imgclass = index_to_class(i)
                    finalrows.append([imgfilename, width, height, imgclass, xmin, ymin, xmax, ymax])

#write all rows to a new csv
with open('../Subject5/sub5.csv', mode='w') as csv_output:
    csv_writer = csv.writer(csv_output, delimiter=',', lineterminator='\n')

    csv_writer.writerows(finalrows)
