import numpy as np
import os
import cv2

class SimpleDatasetLoader:
    def __init__(self, preprocessors = None):
        # takes a list of preprocessors as inputs and stores them
        self.preprocessors = preprocessors
        
        # if preprocessors are None, initialize as empty list
        if self.preprocessors is None:
            self.preprocessors = []
            
    def load(self, imagePaths, verbose = -1):
        # initialize the data and get the list of labels
        data = []
        labels = self.load_labels(imagePaths)
        
        # loop over all the image paths
        for i, imagePath in enumerate(imagePaths):
            # Load the image and it's corresponding label from the correct JSON file
            image = cv2.imread(imagePath)
            
            # check if preprocessors are given
            if self.preprocessors is not None:
                # apply each of the preprocessing to the image loaded in the order
                # represented in the list
                for p in self.preprocessors:
                    image = p.preprocess(image)
            
            # append image and labels to the list of features and labels        
            data.append(image)
            
            # show an update for every `verbose` images
            if verbose > 0 and i > 0 and (i+1) % verbose == 0:
                print("[INFO] processed {}/{}".format(i+1,len(imagePaths)))
                
        # return a tuple of data and labels as numpy arrays
        return (np.array(data), np.array(labels))
    
    def load_labels(self, imagePaths, verbose = -1, folder_label=False):
        # initialize labels
        labels = []
        
        # loop over all the image paths
        for i, imagePath in enumerate(imagePaths):
            # get the label from the image names
            
            if folder_label:
                label = imagePath.split(os.path.sep)[-2]
            else:
                image_name = imagePath.split(os.path.sep)[-1]
                label = image_name.split('_')[-3]
                
            # append image and labels to the list of labels        
            labels.append(label)
            
            # show an update for every `verbose` images
            if verbose > 0 and i > 0 and (i+1) % verbose == 0:
                print("[INFO] processed {}/{}".format(i+1,len(imagePaths)))
                
        # return a tuple of data and labels as numpy arrays
        return np.array(labels)
        
        
