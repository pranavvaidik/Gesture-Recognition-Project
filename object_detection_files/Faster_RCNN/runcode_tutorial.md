# **How to run the training and evaluation code**

## Training:
To run training, the model_main_tf2.py file is used, which is in the object_detection folder. There are two flags that must be filled out: pipeline_config_path and model_dir. 
For pipeline_config_path, the path to the .config file being used must be entered. For the model_dir, the folder for saving checkpoint files must be entered. Before running
the training, there are some things to change.

In the model_main_tf2.py, the only thing to change is the value on line 65 to change the number of steps per every checkpoint. All parentheses show file line number. 
In the config file, first check that the number of classes matches. Then move to the train_config at line 84. Adjust batch size (85), learning rate schedule/value (88-97), total
number of steps (113), and data augmentation options (114-121). Once the training config is setup, the input record and label map path must be entered in the train_input_reader 
(126). input_path needs the .record file path (with quotations around the path). If multiple .record files must be input, create a list using brackets, for example:

input_path: ["./tfrecords/subject1.record", "./tfrecords/subject2.record"]

Fill out the label_map_path with the path to the .pbtxt label map, also surrounding the path with quotation marks. The checkpoint used to train from can be changed at line 107,
which originally points to the MS-COCO trained checkpoint. Finally, run the script to start training. An example usage is 
shown below:

python ./object_detection/model_main_tf2.py --pipeline_config_path=./configs/frcnn_resnet50.config --model_dir=./trainingcheckpoints

During training, the code continuously outputs loss metrics for every 100 steps. While training, I sometimes view the metrics using tensorboard. To do this, I open up a second 
terminal and run tensorboard. The only flag is logdir, which should point to the 'train' folder that contains the log file. an example usage is shown below:

tensorboard --logdir=./trainingcheckpoints/train

The metrics are then output to 'localhost:6006' and can be used to view the loss metrics, as well as learning rate, steps per second, and the images used.
Once the training is finished, evaluations can be run on other .record files. After training, I will sometimes leave the checkpoints in the trainingcheckpoints folder to easily
run evaluations, but after I finish running things, I move the 'train' folder along with all the checkpoint files into a folder than I named to describe the training configuration. 


## Evaluation:
To run the evaluation, the same model_main_tf2.py file is used, with an additional flag: checkpoint_dir. The pipeline_config_path is the same, but now the model_dir points to the 
folder for writing evaluation metric result logs. The checkpoint_dir flag must be set to the directory path of the training checkpoints from training. In the config file, 
go to the eval_input_reader (140) and change the input path to the .record you will be evaluating using the checkpoints specified. Make sure the label map matches the original training
label map so there is no mismatch in class number/order. An example usage is shown below:

python ./object_detection/model_main_tf2.py --pipeline_config_path=./configs/frcnn_resnet50.config --model_dir=./eval --checkpoint_dir=./trainingcheckpoints/sub1

After running the evaluation and it finishes every step (every image in the evaluation record), it outputs various metrics for mAP and recall. To view these accuracy metrics again, tensorboard 
can be run with the logdir flag pointing to the evaluation log. TensorFlow saves the folder as eval, but I usually rename the folder to describe the evaluation configuration, and let TensorFlow
write the 'eval' folder again on subsequent evaluation runs. This will then show on 'localhost:6006' the values of the metrics it output before. An example usage is shown below:

tensorboard --logdir=./eval/real1onsyn1

Evaluation can also be run on multiple .record files, and the concept is the same as explained before in the training section. 

To evaluate on single images, I used the fasterrcnn.py file. Before running this file, the pipeline_config and model_dir (55-58) must be set for the model configuration and training checkpoints. 
On line 68, the checkpoint to restore must be changed. Finally, the image path and file to evaluate must be changed on lines 96-103. After running this, the code outputs a view of the image with 
the bounding boxes drawn on the image, and it also saves the figure to 'figure.png' in the same directory as fasterrcnn.py. To adjust the minimum score threshold and other parameters, the function is
run on line 130. I would generally copy the figure.png to a different folder and rename it to describe the evaluation, and when the script is rerun, it rewrites the figure.png. 


## Things to Note:
Before running any evaluations, make sure that the label maps, number of classes, and overall model configurations match. If the checkpoints used have a different configuration than the current run for
evaluation, the run will fail and talk about a mismatch in size of certain variables. 
