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


ap = argparse.ArgumentParser()
ap.add_argument("-dataset", "--dataset", type = str, default = "", help ="Choose the dataset to train")
#ap.add_argument("-output_path", "--oPath", type = str, default = "output/", help ="path to the output directory to store the models and other plot files")
args = vars(ap.parse_args())

# define a dictionary that maps models to their classes
MODELS = {"resnet50" : ResNet50, "vgg16" : VGG16, "vgg19": VGG19, "inception" : InceptionV3, "xception" : Xception}
DATASETS = ["ASL_real","ASL_syn","Numbers_real","Numbers_syn"]

dataset = args["dataset"]

if args["dataset"] not in DATASETS:
	raise AssertionError("The --dataset command line argument should in the `DATASETS` list")


# initialize input shape
inputShape = (224,224)

# construct training image generator for data augmentation
aug = ImageDataGenerator(rotation_range = 20, zoom_range = [0.85, 1.15], width_shift_range = 0.1,
		    height_shift_range = 0.1, shear_range = 0.15, horizontal_flip = False, 
		    fill_mode = "nearest")


# initiate preprocessors
sp = SimplePreprocessor(inputShape[0],inputShape[1])

# load the RGB means for training set
print("loading dataset mean..")
means =  json.loads(open(config.DATASET_MEAN[dataset]).read())

# initialize the other image preprocessors
mp = MeanPreprocessor(means["R"], means["G"], means["B"])
iap = ImageToArrayPreprocessor()


# initialize training and validation dataset generators
trainGen = HDF5DatasetGenerator(config.TRAIN_HDF5[dataset], config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES[dataset])
valGen = HDF5DatasetGenerator(config.VAL_HDF5[dataset], config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES[dataset])
testGen = HDF5DatasetGenerator(config.VAL_HDF5[dataset], config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES[dataset])


# initialize the optimizer
print("[INFO] compiling model...")
learning_rate = 1e-5
#opt = SGD(lr = 1e-3)#, decay = 0.001/200, momentum = 0.5, nesterov = True)
opt = Adam(lr=learning_rate)


# load class weights
f = open(config.OUTPUT_PATH[dataset]+"/class_weights.pkl","rb")
weights = pickle.loads(f.read())
f.close()



# instantiate Mirrored Strategy
strategy = MirroredStrategy()
#print('Number of devices: {}'.format(strategy.num_replicas_in_sync))


Aug_dict = {True:None, False:aug}
Aug_paths = {True:"aug", False:"no_aug"}

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
    #trainGen = SynDatasetGenerator(trainPaths, trainLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    #valGen = SynDatasetGenerator(valPaths, valLabels, config.BATCH_SIZE, aug=None, preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
    
    for augmentation_flag in [True, False]: 
    
        trainGen = HDF5DatasetGenerator(config.TRAIN_HDF5[dataset], config.BATCH_SIZE, aug=Aug_dict[augmentation_flag], preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
        valGen = HDF5DatasetGenerator(config.VAL_HDF5[dataset], config.BATCH_SIZE, aug=Aug_dict[augmentation_flag], preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
        testGen = HDF5DatasetGenerator(config.TEST_HDF5[dataset], config.BATCH_SIZE, aug=Aug_dict[augmentation_flag], preprocessors=[sp, mp, iap], classes=config.NUM_CLASSES)
        

        for transfer_learning_flag in [True, False]:   
            with strategy.scope():


                # metrics for analysis
                metrics = [TruePositives(name = 'tp'), FalsePositives(name = 'fp'), TrueNegatives(name = 'tn'), FalseNegatives(name = 'fn'), CategoricalAccuracy(name="categorical_accuracy"), Precision(name='precision'), Recall(name = 'recall')]



                # Load the model
                #Network = MODELS[args["model"]]
                Network = MODELS[model_arg]

                if transfer_learning_flag:
                
                    if os.path.exists(config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag] + "/"+model_arg+"_TransferLearning_"+str(learning_rate)+".h5"):
                        continue
                    
                    baseModel = Network(weights = "imagenet", include_top = False, input_tensor = Input(shape=inputShape+(3,)))
                    
                    print("[INFO] Transfer learning is ON")
                    # freeze all weights in baseModel, except for a few final layers
                    for layer in baseModel.layers:#[:-25]: # 4 for vgg16, 15 for resnet,  
                        layer.trainable = False
                        
                else:
                
                    if os.path.exists(config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag]+ "/"+model_arg+"_fromScratch_"+str(learning_rate)+".h5"):
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
                history_file_path = config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag] + "/"+model_arg+"_TransferLearning_"+str(learning_rate)+"_history.pkl"
                FigPath = os.path.sep.join([ config.OUTPUT_PATH[dataset]+"/" + Aug_paths[augmentation_flag], model_arg+"_TransferLearning_"+str(learning_rate)+"_performance.png"])
                checkpoint = ModelCheckpoint(config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag]+ "/"+model_arg+"_TransferLearning_"+str(learning_rate)+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
            else:
                history_file_path = config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag] + "/"+model_arg+"_fromScratch_"+str(learning_rate)+"_history.pkl"
                FigPath = os.path.sep.join([ config.OUTPUT_PATH[dataset]+"/" + Aug_paths[augmentation_flag],model_arg+"_fromScratch_"+str(learning_rate)+"_performance.png"])
                checkpoint = ModelCheckpoint(config.OUTPUT_PATH[dataset] +"/" + Aug_paths[augmentation_flag] + "/"+model_arg+"_fromScratch_"+str(learning_rate)+".h5", monitor = "val_categorical_accuracy", save_best_only=True, mode = "max", verbose = 1)
            
            
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
print("[INFO] Training ended successfully!!.. Please check the relevant plots!!.. Closing the datasets...")
trainGen.close()
valGen.close()
testGen.close()


