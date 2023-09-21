The HANDS dataset originally has more gestures, but we excluded the two-hand gestures
and only used the single hand gestures. After moving to the faster RCNN, we also excluded
the 'horiz' gesture since we lacked time to implement the functionality for it. 

Each subject has a .txt file of the image path and ground truths for that image. To change this,
we imported the .txt into an Excel file, deleted the rows and columns of unused images and gestures,
and then used the reformat_csv.py script and other scripts to reformat the information to match
the input needed for the generate_tfrecord.py script taken from the raccoon dataset code.