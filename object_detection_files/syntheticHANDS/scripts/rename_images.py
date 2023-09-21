"""
renames the images in each gesture of a model folder to
the naming convention of gesture_number.jpg

for tfrecord generation purposes, the gesture folders were scrapped
and all images were put into one folder. The images also had to be
renamed (explained in rename_shuffle.py), so this script is no
longer useful.
"""

import os

# choose gesture manually
gesture = "span"
folder = "./Model5/{}".format(gesture)
name = "{}_".format(gesture)


for count, filename in enumerate(os.listdir(folder)):
    count = count + 1
    dst = name + f'{count:05}' + f'.jpg'
    src =f"{folder}/{filename}"  # foldername/filename, if .py file is outside folder
    dst =f"{folder}/{dst}"
        
    # rename() function will
    # rename all the files
    os.rename(src, dst)