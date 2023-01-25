# import packages

# set the matplotlib backend so figures can be saved in background
import matplotlib
matplotlib.use("Agg")

# other imports
from config import synnet_config as config
from tools.preprocessing import ImageToArrayPreprocessor, SimplePreprocessor,MeanPreprocessor
from tools.callbacks import TrainingMonitor
from tools.io import HDF5DatasetGenerator, SynDatasetGenerator
from tools.nn.conv import SynNet, ShallowNet, FCHeadNet
from tensorflow.keras.callbacks import ModelCheckpoint, EarlyStopping
from tensorflow.keras.preprocessing.image import ImageDataGenerator
from tensorflow.keras.models import Model
from tensorflow.keras.applications import VGG16, VGG19, ResNet50, InceptionV3, Xception
from tensorflow.keras.optimizers import Adam, RMSprop, SGD
from tensorflow.keras.metrics import TruePositives, FalsePositives, TrueNegatives, FalseNegatives, CategoricalAccuracy, Precision, Recall
from tensorflow.distribute import MirroredStrategy
from tensorflow.keras.layers import Input
from tools.datasets import VideoPredictor
from tools.datasets import SimpleDatasetLoader
from imutils import paths
from sklearn.preprocessing import LabelEncoder
from sklearn.model_selection import train_test_split
import json
import os
import pickle
import argparse
import progressbar
import cv2
import numpy as np
#import build_syn_net

# construct argument parse and parse the arguments
ap = argparse.ArgumentParser()
ap.add_argument("-model", "--model", type = str, default = "vgg16", help ="name of pre-trained network to use")
args = vars(ap.parse_args())

# define a dictionary that maps models to their classes
MODELS = {"resnet50" : ResNet50, "vgg16" : VGG16, "vgg19": VGG19, "inception" : InceptionV3, "xception" : Xception}

#transfer_learning_trainable_layers = {"resnet50" : ResNet50, "vgg16" : VGG16, "vgg19": VGG19, "inception" : InceptionV3, "xception" : Xception}

if args["model"] not in MODELS.keys():
	raise AssertionError("The --model command line argument should be a key in the `MODELS` dictionary")

# initialize input shape based on model chosen
inputShape = (224,224)

# if using InceptionV3 or Xception, use (229x229) instead
#if args["model"] in ("inception","xception"):
#	inputShape = (229,229)

# construct training image generator for data augmentation
aug = ImageDataGenerator(rotation_range = 20, zoom_range = [0.85, 1.15], width_shift_range = 0.1,
			height_shift_range = 0.1, shear_range = 0.15, horizontal_flip = False, 
			fill_mode = "nearest")


# initiate preprocessors
sp = SimplePreprocessor(inputShape[0],inputShape[1])



if os.path.exists(config.DATA_PATHS):
    f = open(config.DATA_PATHS,"rb")
    data = pickle.load(f)
    f.close()

    trainPaths = data["trainPaths"]
    testPaths = data["testPaths"]
    valPaths = data["valPaths"]

    trainLabels = data["trainLabels"]

    testLabels = data["testLabels"]
    valLabels = data["valLabels"]

    fresh_data_flag = False

else:

    # grab the paths to the images
    print("Collecting Image paths.....")
    #imagePaths = list(paths.list_images('../Datasets/SignLanguageForNumbers/Train'))#[:500]
    imagePaths = list(paths.list_images('../Datasets/AmericanSignLanguage/Train'))#[:500]

    if not len(imagePaths):
        print("Please check the image path provided..")
        exit()

    # get the labels
    print("Loading labels...")
    sdl = SimpleDatasetLoader()
    labels = sdl.load_labels(imagePaths, folder_label=True)

    # encode and transform the classes
    print("Encoding labels...")
    le = LabelEncoder()
    labels = le.fit_transform(labels)

    # perform sampling from the training set to build validation and test sets
    (trainPaths, valPaths, trainLabels, valLabels) = train_test_split(imagePaths, labels, test_size = 0.4, random_state=42)
    (testPaths, valPaths, testLabels, valLabels) = train_test_split(valPaths, valLabels, test_size = 0.5, random_state=42)

    
    
    
    
    # set a flag when data is freshly shuffled
    fresh_data_flag = True



if os.path.exists(config.DATASET_MEAN) and not fresh_data_flag:
    # load the RGB means for training set
    print("loading dataset mean..")
    means =  json.loads(open(config.DATASET_MEAN).read())
else:
    # calculate RGB means for training set
    print("[INFO] Initializing RGB means for train set...")
    # initiate list of RGB channel averages
    (R,G,B) = ([],[],[])

    
    #initialize progress bar
    widgets = ["Calculating Means:", progressbar.Percentage()," ", progressbar.Bar()," ",progressbar.ETA()]
    pbar = progressbar.ProgressBar(maxval=len(trainPaths), widgets=widgets).start()

    for (i, path) in enumerate(trainPaths):
	    # read and process the image
	    image = cv2.imread(path)/255.0
	    image = sp.preprocess(image)
	    
	    # then compute the mean of each channel in the image and update the lists
	    (b,g,r) = cv2.mean(image)[:3]
	    R.append(r)
	    G.append(g)
	    B.append(b)
	    
	    pbar.update(i)
	    
    pbar.finish()

    print("[INFO] serializing means...")
    means = {"R" : np.mean(R), "G" : np.mean(G), "B":np.mean(B)}
    f = open(config.DATASET_MEAN,"w")
    f.write(json.dumps(means))
    f.close()


# initialize the other image preprocessors
mp = MeanPreprocessor(means["R"], means["G"], means["B"])
iap = ImageToArrayPreprocessor()

"""
# initialize training and validation dataset generators
trainGen = SynDatasetGenerator(trainPaths, trainLabels, config.BATCH_SIZE, aug=aug, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
valGen = SynDatasetGenerator(valPaths, valLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
#testGen = HDF5DatasetGenerator(config.VAL_HDF5, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
"""

# initialize the optimizer
print("[INFO] compiling model...")
learning_rate = 1e-5
#opt = SGD(lr = 1e-3)#, decay = 0.001/200, momentum = 0.5, nesterov = True)
opt = Adam(lr=learning_rate)






# load class weights
f = open(config.OUTPUT_PATH+"/class_weights.pkl","rb")
weights = pickle.loads(f.read())
f.close()



# instantiate Mirrored Strategy
strategy = MirroredStrategy()
#print('Number of devices: {}'.format(strategy.num_replicas_in_sync))


        
for model_arg in MODELS.keys():


    # if using InceptionV3 or Xception, use (229x229) instead
    if model_arg in ("inception","xception"):
    	inputShape = (229,229)
    else:
        inputShape = (224,224)



    # initiate preprocessors
    sp = SimplePreprocessor(inputShape[0],inputShape[1])

    

    # initialize training and validation dataset generators
    #trainGen = SynDatasetGenerator(trainPaths, trainLabels, config.BATCH_SIZE, aug=aug, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    trainGen = SynDatasetGenerator(trainPaths, trainLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    valGen = SynDatasetGenerator(valPaths, valLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    #testGen = HDF5DatasetGenerator(config.VAL_HDF5, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    

    for transfer_learning_flag in [True, False]:   
        with strategy.scope():


            # metrics for analysis
            metrics = [TruePositives(name = 'tp'), FalsePositives(name = 'fp'), TrueNegatives(name = 'tn'), FalseNegatives(name = 'fn'), CategoricalAccuracy(name="categorical_accuracy"), Precision(name='precision'), Recall(name = 'recall')]



            # Load the model
            #Network = MODELS[args["model"]]
            Network = MODELS[model_arg]

            if transfer_learning_flag:
            
                if os.path.exists(config.OUTPUT_PATH + "/"+model_arg+"_TransferLearning_"+str(learning_rate)+".h5"):
                    continue
                
                baseModel = Network(weights = "imagenet", include_top = False, input_tensor = Input(shape=inputShape+(3,)))
                
                print("[INFO] Transfer learning is ON")
                # freeze all weights in baseModel, except for a few final layers
                for layer in baseModel.layers:#[:-25]: # 4 for vgg16, 15 for resnet,  
                    layer.trainable = False
                    
            else:
            
                if os.path.exists(config.OUTPUT_PATH + "/"+model_arg+"_fromScratch_"+str(learning_rate)+".h5"):
                    continue
                
                baseModel = Network(weights = None, include_top = False, input_tensor = Input(shape=inputShape+(3,)))
                
                print("[INFO] Transfer learning is OFF")
                for layer in baseModel.layers:  
                    layer.trainable = True

            # initialize the head model
            headModel = FCHeadNet.build(baseModel, classes = 28, D=256) # change to 12 for numbers dataset

            # place fully connected model on top of base model
            model = Model(inputs = baseModel.input, outputs = headModel)
            #model = SynNet.build(width = 224, height = 224, classes=12)


        # compile the model
        model.compile(optimizer=opt, loss="categorical_crossentropy",metrics=metrics)
        
        
        #print(model_arg)
        print("Getting model summary for {}".format(model_arg))
        model.summary()

        #input("Press Enter to start training")


        # construct the set of callbacks
        
        
        if transfer_learning_flag:
            history_file_path = config.OUTPUT_PATH + "/"+model_arg+"_TransferLearning_"+str(learning_rate)+"_history.pkl"
            FigPath = os.path.sep.join([ config.OUTPUT_PATH, model_arg+"_TransferLearning_"+str(learning_rate)+"_performance.png"])
            checkpoint = ModelCheckpoint(config.OUTPUT_PATH + "/"+model_arg+"_TransferLearning_"+str(learning_rate)+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
        else:
            history_file_path = config.OUTPUT_PATH + "/"+model_arg+"_fromScratch_"+str(learning_rate)+"_history.pkl"
            FigPath = os.path.sep.join([ config.OUTPUT_PATH,model_arg+"_fromScratch_"+str(learning_rate)+"_performance.png"])
            checkpoint = ModelCheckpoint(config.OUTPUT_PATH + "/"+model_arg+"_fromScratch_"+str(learning_rate)+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
        
        
        #FigPath = os.path.sep.join([ config.OUTPUT_PATH,args["model"]+"_"+str(learning_rate)+"_performance.png"])
        #checkpoint = ModelCheckpoint(config.OUTPUT_PATH + "/"+args["model"]+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
        #callbacks = [checkpoint]
        
        earlyStopping = EarlyStopping(monitor = "val_categorical_accuracy", min_delta = 0.01, patience = 3, verbose=1, mode = "max")
        
        callbacks = [TrainingMonitor(FigPath), checkpoint, earlyStopping]

        # train the network



        H = model.fit(x = trainGen.generator(), 
		                #steps_per_epoch = valGen.numImages//config.BATCH_SIZE,
		                steps_per_epoch = trainGen.numImages//config.BATCH_SIZE,
		                validation_data = valGen.generator(), 
		                validation_steps=valGen.numImages//config.BATCH_SIZE,
		                epochs = 100,
		                #max_queue_size = 4,
		                class_weight = weights,
		                callbacks=callbacks,
		                verbose=1)					
			                

        print(H.history.keys())

        # evaluate the model
        #test_results = model.evaluate_generator(testGen.generator(),steps = testGen.numImages//config.BATCH_SIZE ,verbose = 1)

        #print(test_results)

        ## save model to file			
        #print("[INFO] saving the model...")       
        #model.save(config.MODEL_PATH, overwrite = True)


        

        with open(history_file_path,"wb") as fp:
            #pickle.dump({'history': H.history, 'results': test_results}, fp)
            pickle.dump({'history': H.history}, fp)



# close the HDF5 datasets
#trainGen.close()
#valGen.close()
#testGen.close()

# evaluate videos
#d = "../data/old data"
#video_file_paths = [d+'/'+ filename for filename in os.listdir(d)]

# initialize data processor
#vp = VideoPredictor(model = model, preprocessors=[sp,mp,iap])
#vp.load(video_file_paths)

