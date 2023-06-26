# Gesture-Recognition-Project

Gestures and Unity

Code and animation clips as of 4/26/2023

Includes complete functionality for digits, alphabet, and HANDSv2 dataset.

# INSTRUCTIONS FOR USE OF DATASET GENERATION TOOL:
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
  - 2 is for HaGrid, which we dropped and did not end up finalizing
  - 3 is for HANDSv2 which includes full body images of HANDS gestures

Note: When using HANDSv2 dataset, make sure to alter SceneController line 113 to choose the correct Model you wish to generate with. In addition, if you want more models than the 5 we have generated, you will have to manually alter the positions of the arm bones on a MakeHuman model and save the altered version as a new prefab model.

Alter TexChangeTime within Random Texture (script) placed on Walls object to change the speed at which new images are placed on each wall
  - reducing the number speeds up wall randomization
  - increasing the number slows down wall randomization

For the next alterations, you will need a text editor such as visual studio.

  - Within the GetModels function in the SceneController script, you may change the name of the .fbx files to import.
    - For testing purposes, I created models beginning with "human" in their names. When we run our script using MakeHuman mass produced models, the models are named beginning with "mass" followed by numbers, which we alter in the code.
  
  - Within the Update function of the SceneController script, you may alter line 45 by increasing/decreasing the speed at which the animations play/images are taken

# Instructions for Use of Robotic Arm Training Environment
SETTING UP ENVIRONMENT/VERIFYING ALL NECESSARY COMPONENTS
1. Pull all project folder files from GitHub Desktop
2. Verify all of the following entities are present:
  - Within the Hierarchy panel, extend "Objects" GameObject. Within "Objects", you should see the following GameObjects
    - Wall1, Wall2, Wall3, Wall4, Floor, NOZONE, and Fence_Cylinder all enabled
  - Select the HumanController GameObject from the Hierarchy. Verify that an "AutomateMove" script is attached as a component.
    - Verify that the MoveHuman Animation Controller is selected in the Controller variable within the AutoMate move inspector panel.
  - Verify that the robotic arm and table are visible in the scene and the GameObjects are locatable in the Hierarchy.
  - Verify that there is a ManualInput GameObject in the Heriarchy. Select it and verify that the Robot Manual Input script is added as a component with Body10 selected in the Robot variable.
3. If any colors are abnormal looking, head to the Assets->NewAssets->Materials folder location and drag the material color over any GameObject within the Hierarchy.
4. Verify that the MoveHuman Animation Controller exists within Assets->NewAssets->Resources->Controllers and contains all necessary animation clips.
  - If any animation clips are missing from their labeled states in the Animation Controller, navigate to Assets->NewAssets->Recordings->MixamoTests folder location and drag and drop the animation clips from the behavior specific folder into the corresponding missing state.
5. Verify that all MakeHuman models used in this training environment are present at Assets->NewAssets->Resources->Models->PrefabModels
  - All models are human model prefabs which contain all of the following components upon spawn: Animator, RigidBody, and Capsule Collider

HOW TO USE AUTOMATE MOVE SCRIPT
- Open the scene named "AutomateCharacter" from the Assets/Scenes folder
- Navigate to the HumanController Object in the Hierarchy and observe the Automate Move script attached to it in the Inspector panel. We will cover now what each parameter is responsible for.
1. Human Models parameter stores all PrefabModels to be randomly spawned into the scene upon run. This is automatically done in the AutomateMove script. You do not need to change this variable manually at all.
2. Behavior Chains can be created automatically or manually depending on whether or not TestMode and ManualSpawnTest are enabled.
  - If the TestMode is off, you do not need to worry about constructing these chains.
  - If TestMode and ManualSpawnTest is on, you must build your own behavior chains. A behavior chain is contructed using characters that represent individual behaviors. Use 'a' to perform an Action animation, 'F' to move the character forward, 'B' to move the character backward, 'L' to make the character turn left, or 'R' to make the character turn right. You must end every behavior chain in a period ('.'). If you do this wrong, the system will halt the run and print an error message in the log.
3. Controller is the selection of the Animation Controller used to determine how behavior animations can interface with one another. MoveHuman should be selected.
4. Behavior Chain Num is the user selection of the number of behavior chains they wish to be automatically, randomly generated when TestMode is off. This must be a positive number.
5. Max Chain Length determines how many behaviors can potentially make up a behavior chain. This number must be positive. For instance, if Max Chain Length is 2, the behavior chain "aFB." cannot be generated because that chain is length 3. However, the behavior chain "a." can be generated because it is under the Max Chain Length.
6. Num F Reaction Anims must be given by the user to identify how many potential Reaction animations can be chosen. You can enter the MoveHuman animation controller and enter the ForwardReactions state to see how many forward reaction animations there are currently (5). If the number is inaccurate, either some animations will not be chosen or there will be an error in the selection of an animation. This must be a positive number.
7. Num B Reaction Anims is the same description as above, only for backward animation clips.
8. Num Action Anims is the same description as the previous two, only for action animation clips.
9. TestMode tells the system not to perform its typical run. It is useful for debugging or validation of functionality. When TestMode is enabled, you must also select a test type: Manual Spawn Test, Multi Angle Reaction Test, or Reaction Distance Test.
10. Manual Spawn Test allows a user to select the spawn location and rotation parameters of a character. With this enabled, you must build your own behavior chains.
11. The next 6 parameters describe the manually spawned characters position/rotation in space. You may not spawn a model inside of the robotic arm region nor can you spawn a model outside of the testing region walls.
12. Multi Angle Reaction Test is a test type that is useful for validating that a reaction animation can play no matter the angle that a user approaches the robot from. With this test type enabled, you must select one of the two choices below: Forward or backward reaction test. 
13. Forward Reaction Test spawns a character always facing the center of the testing region and makes characters walk forward, interacting with the arm collider and therefore performing only forward reaction animations.
14. Backward Reaction Test spawns a character facing away from the central region and has the character walk backwards and therefore perform only backward reaction animations.
15. SpawnDistance tells the system how far away each character must be from the center of the map when they are spawned. They will remain this distance at every iteration of spawning in a circle around the robotic arm. This number must be greater than 25 to ensure that the user doesn't spawn immediately on the collider.
16. NumPoints tells the system how many points around the model a character should spawn to test their reaction performances. If this number is 2 for instance, a character will spawn only twice on both sides of the robot. This number must be positive.
17. Reaction Distance Test is a test type that is useful for validating that a reaction animation will play if a character interacts with the robotic arm very soon after they begin walking or very long after they begin walking, as long as at one point or another they do come into contact with the collider surrounding the robot. You must select one of the options beneath Reaction Distance Test to designate whether you wish the test to perform forward or backward animations.
18. Forward Distance Test tells the system to spawn the characters facing towards the central region and move forward into contact.
19. Backward Distance Test tells the system to spawn the characters facing away from the central region and move backward into contact.
20. Distance Test Points is a user selection to determine how many different increments of distance away from the robot arm they wish to test. This must be a positive number.

INSTRUCTIONS TO MAKE CHANGES TO THE SYSTEM
1. If you wish to change potential animation clips to use in the system, you must alter them through the MoveHuman Animation Controller.
  - To use different/new animation clips, simply drag and drop the animation clip you wish to use into the correct state in the animation controller and create a transition from the entry state to the new animation clip state. Follow the same pattern that you see in the automatically included animation clip states. Within the transition, depending on where you have placed the new animation, you must create a transition condition. This is usally in the form of an Index parameter equalling a certain value. Once again, observe the pattern that is present in other transition conditions and make your new condition accordingly. After you add a clip, make sure to change the number of animation clips for reaction or action behaviors from within the Automate Move script located on the Human Controller.
  - In addition, new animation clips will require different timings that must be altered in the Automate Move script. To determine the amount of time needed in the WaitForSeconds function, I typically observe the animation clip length of time then round up to the nearest whole number.
  - Within the PlayAnimation function of the SceneController script, we call a Coroutine which delays by a given number of seconds before taking an image
    - we have the delay set to .05, which has worked well for us, but you may change this value to a higher or lower value depending on how fast you want to run the dataset generation.
