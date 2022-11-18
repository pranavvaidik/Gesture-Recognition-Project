# Gesture-Recognition-Project

Gestures and Unity

Code and animation clips as of 11/15/2022

Includes complete functionality for digits and alphabet dataset.

# INSTRUCTIONS FOR USE:
First, you will need to create a 3D Unity Project. We have tested this project using Unity versions 2021.3.9f1 and 2021.3.8f1
  - I named the project ModelGesture, but it does not matter what you call it

After creating the project, navigate to the project base directory from your file explorer.
  - You should see folders Assets, Library, Logs, etc to know you are in the right place
  - Create 2 folders, one called SignLanguageForNumbers and another called AmericanSignLanguage
  - Inside of each, create 2 folders called Test and Train
  - Inside of each of these new folders within SignLanguageForNumbers, create 11 new folders labeled 0, 1, 2, ..., 9 and unknown
  - Inside of each (Test and Train) within the AmericanSignLanguage folder, create 28 new folders labeled A, B, C, ..., Z, Space, and Nothing

Now, navigate inside the Assets folder of your new Unity 3D project still in the file explorer. 
  - Inside of Assets, create 4 new folders called Scenes, Resources, Recordings, and Scripts for organization
  - Now, download the FinalScene.unity file from this Git repo and save it into the Scenes folder you just created within your new Unity project
  - Next, download the ASL_AnimClips and SignLanguageNumbers_AnimClips from this Git repo and save both folders with all contents into the Recordings folder
  - Next, download the RandomTexture.cs and SceneController.cs scripts and save them into the newly created Scripts folder

The Resources folder you just created will include 3 folders which we will discuss here
  - Download the Controllers folder from this Git repo and save it with all contents into the Resources folder
  - Next, create a Models folder (which is where you will import your MakeHuman models as .fbx files for spawning)
  - Finally, search "Describable Textures Dataset" online and follow the instructions for the download of this dataset. Save the "dtd" dataset into Resources as well

You are almost ready to run,
  - Open your Unity project and allow each newly created folder/file to import.
  - At the bottom of your screen, you will see a "Project" tab where you can select files from your Assets folder
  - Navigate to your Scenes folder and open the FinalSceneDraft unity scene.

Unity is sometimes a bit weird in allowing users to import scenes in this way, which causes some components to not upload correctly,
so I will describe how to ensure the scene is properly set up.
  - Once opened, in the "Hierarchy" panel on the left side of your screen, you should see a few objects labeled Walls, Cameras, and SceneControl
  - Select the drop down triangle next to Walls to see each wall labeled Roof, Floor, etc. Same goes with Cameras to see each individual camera.
  - If you click on the Walls object itself, the "inspector" window will open on the right side of your screen. You need to ensure that the RandomTexture script is added. If it is not, simply click Add Component and search for it by name.
  - Within the RandomTexture script in your inspector panel, you have a few alterations to make. 
    - You will see an item labeled Walls with a box to its right. Within the box, write the number 5. An array with empty elements will appear. To fill these elements with each wall object, simply drag and drop each Floor, Roof, etc from the Hierarchy panel into the empty element slot.
    - Next, you have 2 public variables. I have Tex Change Time set to .3 and Change Delay set to 0. You can adjust the value of Tex Change Time to make the walls change images faster or slower

  - Now, click on the SceneConrol object within the Hierarchy panel. If you do not see a SceneController script active on this object in the inspector window, add one using the Add Component feature.
  - You will see a number of empty boxes in the SceneController script within the SceneControl inspector panel. 
    - Select the dataset you wish to replicate by placing a 0 for Numbers and a 1 for Alphabet
    - Select the number of images you wish to record per gesture by altering maxNum
    - Now, for each instance of a Cam in this inspector window, you will need to drag and drop the corresponding camera from the hierarchy underneath "Cameras".
    - DigitsCam must include NumCam, AlphabetCam includes AlphabetCam, MNCam includes MNCam and so on.

YOU ARE DONE! THE SCENE IS FULLY FUNCTIONAL. NOW, PRESS THE PLAY BUTTON AT THE TOP OF THE SCREEN AND WATCH AS RANDOM GESTURES ARE APPLIED TO RANDOM MODELS WITH VARYING BACKGROUNDS.
  - If you wish to stop the playing, simply press the play button again.
  - If you wish to create a full dataset with maxNum pictures of each gesture, just let the playing carry out. Once the dataset is complete, it will automatically stop
  - To check output, navigate to the SignLanguageForNumbers or AmericanSignLanguage folder you created in the beginning of this process (depending on which dataset you selected to play). Enter the Train folder and enter each gesture folder to ensure that the correct number of images were taken for each.

# Alterations you can make to the scene/code
Alter maxNum within the Scene Controller (script) placed on the SceneControl hierarchy object to change the amount of images you want taken per gesture.

Alter ChooseDataset as mentioned previously
  - 0 means SignLanguageForNumbers dataset
  - 1 means AmericanSignLanguage dataset

Alter TexChangeTime within Random Texture (script) placed on Walls object to change the speed at which new images are placed on each wall
  - reducing the number speeds up wall randomization
  - increasing the number slows down wall randomization

For the next alterations, you will need a text editor such as visual studio.

  - Within the GetModels function in the SceneController script, you may change the name of the .fbx files to import.
    - For testing purposes, I created models beginning with "human" in their names. When we run our script using MakeHuman mass produced models, the models are named beginning with "mass" followed by numbers, which we alter in the code.
  
  - Within the Update function of the SceneController script, you may alter line 45 by increasing/decreasing the speed at which the animations play/images are taken
  - Within the PlayAnimation function of the SceneController script, we call a Coroutine which delays by a given number of seconds before taking an image
    - we have the delay set to .05, which has worked well for us, but you may change this value to a higher or lower value depending on how fast you want to run the dataset generation.
