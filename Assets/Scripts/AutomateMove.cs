using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;
using System.Diagnostics;
//using Unity.VisualScripting;

public class AutomateMove : MonoBehaviour
{
    private string behaviorChain;                   //individual string of behaviors inside BehaviorChains array
    private Animator animatorBody;                  //animator on MakeHuman model
    private int behaviorIndex;                      //index for behvaior inside of behavior chain
    private int behaviorChainIndex;                 //index for behavior chain inside of behavior chains array
    private char behavior;                          //character for behavior inside of behaviorChain
    List<string> fileList = new List<string>();     //necessary for grabbing human model .fbx files from file system
    public string[] humanModels;
    public string[] BehaviorChains;                 //array of all behavior chains to be played
    private GameObject[] spawned;
    public AnimatorController controller;           //animator controller in Unity that stores all clips and transitions
    bool ReactAnimDone = false;                     //set to true once Reaction is played if conditions are met (interacts with collider)
    public int BehaviorChainNum;                    //user selects number of behavior chains they want to play out in simulation
    public int MaxChainLength;
    public int numFReactionAnims;                   //more user selected quantities
    public int numBReactionAnims;
    public int numActionAnims;
    [HideInInspector] public bool trigger;          //trigger variable is altered by collider on Fence object
    public bool TestMode;                           //when TestMode is on, user spawns character and builds behavior chains manually
    public bool ManualSpawnTest;
    public float spawnLocationX, spawnLocationY, spawnLocationZ, spawnRotationX, spawnRotationY, spawnRotationZ;        //user-inputted spawn location/rotation
    float spawnX, spawnY, spawnZ, rotX, rotY, rotZ;
    public bool MultiAngleReactionTest;
    public bool ForwardReactionTest;
    public bool BackwardReactionTest;
    public float spawnDistance;
    public int numPoints;
    float angleShift;
    int iteration;
    float addAngle;
    public bool ReactionDistanceTest;
    public bool ForwardDistanceTest;
    public bool BackwardDistanceTest;
    public int distanceTestPoints;
    public int startDistance;
    

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if ((numFReactionAnims <= 0) || (numActionAnims <= 0) || (numBReactionAnims <= 0) || (BehaviorChainNum <= 0) || (MaxChainLength <= 0))           //if inputted number of animations is 0 or below, print error message and exit application
        {
            EditorApplication.isPlaying = false;
            Debug.Log("Invalid Input. All user inputs beneath Controller and above Test Mode selection must be greater than 0.");
        }
        trigger = false;                        //trigger is false upon starting unity run
        if (TestMode == false)
        {
            BuildBehaviorChains();              //if TestMode is not on, enable automatic creation of behavior chains and carry on
        }
        else
        {
            //The following are edge-case checks to ensure proper user entries
            //if test mode is on, a test type needs to be selected. Check for this before continuing.
            if (MultiAngleReactionTest == false && ManualSpawnTest == false && ReactionDistanceTest == false)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("Please select a test type. Manual Spawn Test, Multi Angle Reaction Test, or Reaction Distance Test.");
            }
            //if a multi angle reaction test is selected, a user must designate forward or backward type
            if (MultiAngleReactionTest == true && ForwardReactionTest == false && BackwardReactionTest == false)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("While running a Multi Angle Reaction Test, you must select between the forward or backward test options.");
            }
            //if a reaction distance test is selected, a user must designate forward or backward type
            if (ReactionDistanceTest == true && ForwardDistanceTest == false && BackwardDistanceTest == false)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("While running a Reaction Distance Test, you must select between the forward or backward test options beneath.");
            }
            //2 test types cannot be selected at the same time. Check for this before continuing
            if ((ManualSpawnTest == true && MultiAngleReactionTest == true) || (ManualSpawnTest == true && ReactionDistanceTest == true) || (MultiAngleReactionTest == true && ReactionDistanceTest == true))
            {
                EditorApplication.isPlaying = false;
                Debug.Log("Please only select one test type.");
            }
            //the base reaction distance test type must be selected in addition to the forward and backward selections. 
            if ((ForwardDistanceTest == true || BackwardDistanceTest == true) && ReactionDistanceTest == false)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("Please check the ReactionDistanceTest selection if you wish to perform a forward or backward distance test.");
            }
            //the base reaction angle test type must be selected in addition to the forward and backward selections. 
            if ((ForwardReactionTest == true || BackwardReactionTest == true) && MultiAngleReactionTest == false)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("Please check the MultiAngleReactionTest selection if you wish to perform a forward or backward reaction angle test.");
            }

            //if multi angle reaction test is selected..
            if (MultiAngleReactionTest == true)
            {
                if (spawnDistance <= 25)                                                //check that the spawn distance isnt within the collider region
                {
                    EditorApplication.isPlaying = false;
                    Debug.Log("Invalid Spawn Distance selection. Please provide a number above 25.");
                }
                if (numPoints <= 0)                                                     //check that the number of points is above 0
                {
                    EditorApplication.isPlaying = false;
                    Debug.Log("Invalid MultiAngleReactionTest Number of Points selection. Please provide a number above 0.");
                }
                spawnX = spawnDistance;                                                 //start with spawn in X direction as user-selected distance
                spawnY = 0;
                spawnZ = 0;
                rotX = 0;
                rotY = 0;
                rotZ = 0;
                angleShift = 2 * Mathf.PI / numPoints;                                  //angle shift used determine the rotation that must be added after each iteration 
                iteration = 0;
                BehaviorChains = new string[numPoints];
                for (int i = 0; i < numPoints; i++)                                     //loop until numPoints amount of behaviorChains have been created
                {
                    string chain = "";                                                  //create empty string for a chain
                    int chainLength = 3;                                                //chooses test length of chain to be 3
                    for (int j = 1; j <= chainLength + 1; j++)                          //loop until chain contains the chainLength number of behaviors
                    {
                        if (j < chainLength + 1)                                        //if we are not at the last behavior on the chain...
                        {
                            if (ForwardReactionTest == true)                            //if forward reaction test is selected, add behavior F
                            {
                                chain += 'F';                                           
                            }
                            else if (BackwardReactionTest == true)                      //if backward reaction test is selected, add behavior B
                            {
                                chain += 'B';
                            }
                        }
                        else
                        {
                            chain += ".";                                           //if the chain is on its last character, place a '.'
                        }
                    }
                    BehaviorChains[i] = chain;                                      //add the created chain to the BehaviorChains array
                }
            }
            else if (ManualSpawnTest == true)
            {
                spawnX = spawnLocationX;                                            //if ManualSpawnTest is on, use user-inputted spawn locations
                spawnY = spawnLocationY;
                spawnZ = spawnLocationZ;
                rotX = spawnRotationX;
                rotY = spawnRotationY;
                rotZ = spawnRotationZ;

                //if spawn location is inside of arm region or outside of planar bounds, error message and exit
                if (((spawnX >= -25 && spawnX <= 25) && (spawnZ >= -20 && spawnZ <= 20)) || (spawnX > 96) || (spawnZ > 96))  
                {
                    Debug.Log("Invalid player spawn. Please provide location outside of off-limits region.");
                    EditorApplication.isPlaying = false;
                }
                if (BehaviorChains.Length == 0)                                     //if no behavior chains are manually provided, error message and exit
                {
                    Debug.Log("No Behavior Chains were provided. You must fill BehaviorChains array yourself in TestMode.");
                    EditorApplication.isPlaying = false;
                }
                
            }
            //if ReactionDistanceTest is selected...
            else if (ReactionDistanceTest == true)
            {
                //check to make sure the number of test points is above 0
                if (distanceTestPoints <= 0)
                {
                    EditorApplication.isPlaying = false;
                    Debug.Log("Invalid Number of DistanceTest Points selection. Please provide a number above 0.");
                }
                if (startDistance <= 15)
                {
                    EditorApplication.isPlaying = false;
                    Debug.Log("Please provide a start distance further away from the model.");
                }
                iteration = 0;
                spawnX = 0;
                spawnY = 0;
                spawnZ = startDistance;                                                        //use start point right next to arm region
                rotX = 0;
                rotY = 180;                                                         //rotate 180 degrees so character faces the arm
                rotZ = 0;
                BehaviorChains = new string[distanceTestPoints];
                for (int i = 0; i < distanceTestPoints; i++)                        //loop until distanceTestPoints amount of behaviorChains have been created
                {
                    string chain = "";                                              //create empty string for a chain
                    int chainLength = 2;                                            //chooses length of 2 for each chain for this test
                    for (int j = 1; j <= chainLength + 1; j++)                      //loop until chain contains the chainLength number of behaviors
                    {
                        if (j < chainLength + 1)                                    //if we are not at the last behavior on the chain,
                        {
                            if (ForwardDistanceTest == true)                        //if forward distance test is selected, add behavior F    
                            {
                                chain += 'F';                        
                            }
                            else if (BackwardDistanceTest == true)                  //if backward distance test is selected, add behavior B
                            {
                                chain += 'B';
                            }
                        }
                        else
                        {
                            chain += ".";                                           //if the chain is on its last character, place a '.'
                        }
                    }
                    BehaviorChains[i] = chain;                                      //add the created chain to the BehaviorChains array
                }

            }

            //if behavior chains are manually inputted (only in ManualSpawnTest case), check that all behavior chains end in a '.'
            if (MultiAngleReactionTest == false && ReactionDistanceTest == false)
            {
                for (int i = 0; i < BehaviorChains.Length; i++)
                {
                    behaviorChain = BehaviorChains[i];
                    if (behaviorChain[behaviorChain.Length - 1] != '.')              //checks to see if last character in behaviorChain is '.'
                    {
                        EditorApplication.isPlaying = false;
                        Debug.Log("Behavior Chain Element #" + i + " must end with a period.");             //if not, end run and type error message
                    }
                }
            }
        }
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/Models/PrefabModels");       //selects directory to grab models from
        FileInfo[] files = dir.GetFiles("mass*.prefab");                            //places all files with name starting with mass and ending in .prefab
                                                                                    //from chosen directory into a files folder
        foreach (FileInfo file in files)
        {
            string name = file.Name.Split('.')[0];                                  //for each file, disconnect the .prefab from the name
            fileList.Add(name);                                                     //and add the model name into the list of file names
        }
        humanModels = fileList.ToArray();                                           //transfers file of models into GameObject Array for use

        yield return new WaitForSeconds(2f);
        spawned = new GameObject[1];                                                //creates length 1 array of GameObjects to keep track of spawned player model
        if (ReactionDistanceTest == false)
        {
            SpawnModel(0);                                                          //if Reaction Distance Test isnt selected, we want any model to be chosen                                                
        }
        else
        {
            SpawnModel(1);                                                          //if the Distance Test is selected, we want only one character to be used in testing
        }
        behaviorChainIndex = 0;
        behaviorChain = BehaviorChains[behaviorChainIndex];                     //initializes values for reading BehaviorChains array
        behaviorIndex = 0;
        behavior = behaviorChain[behaviorIndex];
        int array_length = BehaviorChains.Length;

        for (int i=0; i<array_length; i++)                                      //loop to read all BehaviorChains indexes
        {
            animatorBody.SetBool("NOZONE", false);                          //reset boolean parameters in Animator to false
            animatorBody.SetBool("WalkF", false);
            animatorBody.SetBool("WalkB", false);
            while (behavior != '.')                                         //loop reading each index of behaviorChain inside of BehaviorChains array
            {              
                if (behavior == 'a')                                        //if 'a' is read, action is performed
                {
                    float playTime = PlayAction();
                    yield return new WaitForSeconds(playTime);              //must delay while loop run until action is fully performed to avoid overlap of animations
                }
                else if (behavior == 'B')                                   //if 'B' is read, Walking Backwards animation is played
                {
                    WalkBOn();                                              //calls for walking backwards boolean to be true
                    yield return new WaitForSecondsRealtime(5f);            //waits 5 time units
                    WalkBOff();                                             //turns walking backwards boolean to false (animation stops)
                    yield return new WaitForSecondsRealtime(2.5f);          //delays to ensure animation is fully stopped before starting next one
                }
                else if (behavior == 'F')                                   //if 'F', walking forwards animation played
                {
                    WalkFOn();                                              //same process as walking backwards
                    yield return new WaitForSecondsRealtime(5f);
                    WalkFOff();
                    yield return new WaitForSecondsRealtime(2.5f);
                }
                else if (behavior == 'L')                                   //if 'L' is read, turn left animation is played
                {
                    TurnLeft();
                    yield return new WaitForSecondsRealtime(3.2f);          //wait to ensure animation is played out before starting new
                }
                else if (behavior == 'R')                                   //if 'R' is read, turn right animation is played
                {
                    TurnRight();
                    yield return new WaitForSecondsRealtime(3.2f);          //wait to ensure animation is played out before starting new
                }
                else                                                        //if a different character is read, print error message and stop running
                {
                    Debug.Log("Invalid Behavior Choice. Only acceptable character inputs are F,B,R,L,a. Aborting Unity run.");
                    EditorApplication.isPlaying = false;
                }
                if (ReactAnimDone == true)                              //if Reaction animation is played, we dont want any more behaviors to play after it
                {
                    Destroy(spawned[0]);                                //destroy model
                    ReactAnimDone = false;                              //turn boolean false
                    break;                                              //break from while loop without continuing indexing of behaviorChain
                }
                behaviorIndex++;                                        //increment index to read next behavior inside of behaviorChain
                behavior = behaviorChain[behaviorIndex];                //determine what the next behavior is
                if (behavior == '.')                                    
                {
                    Destroy(spawned[0]);                                //if the next behavior is '.', chain is over so delete the model
                }
            }
            if (i != array_length - 1)                                  //as long as there are more behaviorChains to still be played,
            {
                yield return new WaitForSeconds(.5f);                   //pause for less than a second to ensure no overlap
                
                //if a MultiAngleReactionTest is being run, print console message and move character spawn to match desired next angle
                if (TestMode == true && MultiAngleReactionTest == true)
                {
                    Debug.Log("Iteration #"+(iteration+1)+" complete. SpawnAngle = " + (iteration * angleShift*Mathf.Rad2Deg));
                    iteration += 1;
                    addAngle = iteration * angleShift;
                    spawnX = spawnDistance * Mathf.Cos(addAngle);
                    spawnZ = spawnDistance * Mathf.Sin(addAngle);
                    yield return new WaitForSeconds(.5f);
                }
                //if a distance test is being run, increase the distance by 2 units and print to console.
                else if (TestMode == true && ReactionDistanceTest == true)
                {
                    Debug.Log("Iteration #" + (iteration + 1) + " complete. SpawnDistance = " + (startDistance + iteration * 2));
                    iteration += 1;
                    spawnZ += 2;
                    yield return new WaitForSeconds(.5f);
                }
                if (ReactionDistanceTest == false)
                {
                    SpawnModel(0);                                        //again spawn new model depending on which test type is being run                                                
                }
                else
                {
                    SpawnModel(1);                                               
                }
                behaviorChainIndex++;                                     //increment behaviorChainIndex to read next behaviorChain inside of BehaviorChains array
                behaviorChain = BehaviorChains[behaviorChainIndex]; 
                behaviorIndex = 0;                                        //restart behaviorIndex to 0
                behavior = behaviorChain[behaviorIndex];                  //evaluate behavior at index 0 of behaviorChain
            }
            else
            {
                //if the final iteration of each test type is carried out, print final console message and end run.
                if (TestMode == true && MultiAngleReactionTest == true)
                {
                    Debug.Log("Final Iteration #" + (iteration+1) + " complete. SpawnAngle = " + (0 + iteration * angleShift*Mathf.Rad2Deg));
                }
                else if (TestMode == true && ReactionDistanceTest == true)
                {
                    Debug.Log("Final Iteration #" + (iteration + 1) + " complete. SpawnDistance = " + (startDistance + iteration * 2));
                }
                EditorApplication.isPlaying = false;                      //if the last behaviorChain has been played, exit unity run
            }
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        //we need to constantly check if player has interacted with the Fence collider which has its own Trigger script connected to this script
        if (trigger == true)                                              //every frame checks trigger (which is changed to true when interaction occrs)
        {
            trigger = false;                                              //immediately change value of trigger back to false so that update is not repeatedly called
            animatorBody.SetBool("NOZONE", true);                         //set boolean parameter in animation controller to indicate that player is in Fenced region
            StartCoroutine(PlayReaction());                               //play a reaction animation
            ReactAnimDone = true;                                         //indicate that a reaction has been played
        }
    }

    //this method is used to automatically create behavior chains
    void BuildBehaviorChains()
    {
        string BehaviorChoices = "FBRLa";                                 //string of potential behavior choices (character selected is behavior choice)
        int numChains = BehaviorChainNum;                                 //use user-inputted number of behavior chains to build correct number
        BehaviorChains = new string[numChains];                           //create BehaviorChains array of size numChains (desired by user)
        for (int i=0; i<numChains; i++)                                   //loop until numChains amount of behaviorChains have been created
        {
            string chain = "";                                              //create empty string for a chain
            int chainLength = Random.Range(1, MaxChainLength+1);            //choose random length of chain from 1 to user selected number
            for (int j=1; j<=chainLength+1; j++)                            //loop until chain contains the chainLength number of behaviors
            {
                if (j < chainLength+1)                                      //if we are not at the last behavior on the chain,
                {
                    int index = Random.Range(0, BehaviorChoices.Length);    //choose random index to pick a behavior
                    chain += BehaviorChoices[index];                        //add behavior choice to initially empty chain string
                }
                else
                {
                    chain += ".";                                           //if the chain is on its last character, place a '.'
                }
            }
            BehaviorChains[i] = chain;                                      //add the created chain to the BehaviorChains array
        }
    }

    void SpawnModel(int distanceTest)
    {
        //this portion of SpawnModel obtains correct position and rotation values of character depending on test type or normal run
        float theta = 0f;
        Vector3 spawnPos = new Vector3(0,0,0);                      //initialization of spawn location and rotation that will be altered in next if-else statements
        Quaternion spawnRot = Quaternion.Euler(0, 0, 0);

        if (TestMode == true && ManualSpawnTest == true)
        {
            spawnPos = new Vector3(spawnX, spawnY, spawnZ);         //if ManualSpawnTest is on, use the manually inputted location and rotations
            spawnRot = Quaternion.Euler(rotX, rotY, rotZ);
        }
        else if (TestMode == true && MultiAngleReactionTest == true)                    //if MultiAngleReactionTest is on, spawn character in a circle around the arm region
        {
            if (spawnZ == 0 && spawnX > 0)                                      //depending on the spawnX and Z locations, calculate angle for character to spawn 
            {                                                                   //so that they will be facing the center of the robotic arm region every time
                if (ForwardReactionTest == true) { theta = -90; }
                else if (BackwardReactionTest == true) { theta = 90; }
            }
            else if (spawnZ == 0 && spawnX < 0)
            {
                if (ForwardReactionTest == true) { theta = 90; }
                else if (BackwardReactionTest == true) { theta = -90; }
            }
            else if (spawnZ > 0 && spawnX == 0)
            {
                if (ForwardReactionTest == true) { theta = 180; }
                else if (BackwardReactionTest == true) { theta = 0; }
            }
            else if (spawnZ < 0 && spawnX == 0)
            {
                if (ForwardReactionTest == true) { theta = 0; }
                else if (BackwardReactionTest == true) { theta = 180; }
            }
            else if (((spawnZ < 0) && (spawnX > 0)) || ((spawnZ < 0) && (spawnX < 0)))           
            {
                if (ForwardReactionTest == true) { theta = Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg; }
                else if (BackwardReactionTest == true) { theta = 180 + Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg; }
            }
            else if ((spawnZ > 0) && (spawnX > 0))
            {
                if (ForwardReactionTest == true) { theta = -(180 - (Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg)); }
                else if (BackwardReactionTest == true) { theta = (Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg); }
            }
            else if ((spawnZ > 0) && (spawnX < 0))
            {
                if (ForwardReactionTest == true) { theta = 180 + (Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg); }
                else if (BackwardReactionTest == true) { theta = (Mathf.Atan(spawnX / spawnZ) * Mathf.Rad2Deg); }
            }
            spawnPos = new Vector3(spawnX, spawnY, spawnZ);                             //use X,Y,Z locations and calculated rotation theta to spawn character properly
            spawnRot = Quaternion.Euler(0f, theta, 0f);
        }
        else if (TestMode == true && ReactionDistanceTest == true)
        {
            spawnPos = new Vector3(0, 0, spawnZ);                                       //if ReactionDistanceTest is on, spawn character incrementally along z axis
            if (ForwardDistanceTest == true) { spawnRot = Quaternion.Euler(0f, 180f, 0f); }         //adjust rotation depending on choice of forward or backward test
            else if (BackwardDistanceTest == true) { spawnRot = Quaternion.Euler(0f, 0f, 0f); }
        }
        else                                                                            //if TestMode isnt on,
        {
            float randX = Random.Range(-80f, 80f);                                      //choose random location in XZ plane to spawn character
            float randZ = Random.Range(-80f, 80f);                                      //Y value doesnt matter, just value close to 0 so player will be walking on ground
            while ((randX <= 25 && randX >= -25) && (randZ <= 20 && randZ >= -20))
            {
                randX = Random.Range(-80f, 80f);                                        //if model is spawned inside of arm region, repeat spawn location randomization
                randZ = Random.Range(-80f, 80f);
            }
            if (randZ == 0 && randX > 0)                                                //then depending on spawn location, calculate rotation necessary for character to face
            {                                                                           //the center of the map
                theta = -90;
            }
            else if (randZ == 0 && randX < 0)
            {
                theta = 90;
            }
            else if (randZ > 0 && randX == 0)
            {
                theta = 180;
            }
            else if (randZ < 0 && randX == 0)
            {
                theta = 0;
            }
            else if (((randZ < 0) && (randX > 0)) || ((randZ < 0) && (randX < 0)))           
            {
                theta = Mathf.Atan(randX / randZ) * Mathf.Rad2Deg;
            }
            else if ((randZ > 0) && (randX > 0))
            {
                theta = -(180 - (Mathf.Atan(randX / randZ) * Mathf.Rad2Deg));
            }
            else if ((randZ > 0) && (randX < 0))
            {
                theta = 180 + (Mathf.Atan(randX / randZ) * Mathf.Rad2Deg);
            }
            spawnPos = new Vector3(randX, 0.15f, randZ);                                //finally, change spawn position and rotation to selected values
            spawnRot = Quaternion.Euler(0f, theta + Random.Range(-15f, 15f), 0f);       //slight randomization in rotation so character runs into central area at random angles
        }

        //now for the spawning of the model
        if (spawned[0] != null)                                                         //checks if there is already a spawned model in the scene
        {
            Destroy(spawned[0]);                                                        //if there is, delete it before placing a new one
        }
        int HumanIndex = 0;
        if (distanceTest == 0)
        {
            HumanIndex = Random.Range(0, humanModels.Length);                           //selects random index of human model array
        }
        else
        {
            HumanIndex = 5;                                                             //if distanceTest is on, we want to use only one model for consistency in test results
        }
        string filename = $"Models/PrefabModels/{humanModels[HumanIndex]}";             //gets human model filename depending on randomly chosen index
        GameObject spawnedModel = Instantiate(Resources.Load<GameObject>(filename), spawnPos, spawnRot);    //places chosen model in scene at random location and rotation
        spawned[0] = spawnedModel;                                                      //indicates a new model is spawned
        animatorBody = spawnedModel.GetComponent<Animator>();                           //gets Animator component on model
        animatorBody.runtimeAnimatorController = controller;                            //places user-selected Animation Controller on Player Animator component
    }
    void WalkFOn() { animatorBody.SetBool("WalkF", true); }             //turns boolean for walking forwards animation to occur on in Animation Controller
    void WalkFOff() { animatorBody.SetBool("WalkF", false); }           //turns boolean for walking forwards animation to occur off in Animation Controller

    void WalkBOn() { animatorBody.SetBool("WalkB", true); }             //turns boolean for walking backwards animation to occur on in Animation Controller
    void WalkBOff() { animatorBody.SetBool("WalkB", false); }           //turns boolean for walking backwards animation to occur off in Animation Controller

    float PlayAction()
    {
        float neededTime = 0;                                           //neededTime is decided based on duration of animation clip selected to play out
        int index = Random.Range(0, numActionAnims);                    //chooses random index from 0 -> amount of action animations-1
        animatorBody.SetInteger("ActionIndex", index);                  //changes integer in Animation Controller to properly make a transition between animations
        animatorBody.SetTrigger("Action");                              //sets trigger for Action so transition can occur
        if (index == 0 | index == 2) { neededTime = 2f; }               //these are manually placed times based on duration of animation
        else if (index == 1 | index == 3) { neededTime = 5f; }
        else if (index == 4) { neededTime = 4f; }
        return neededTime;                                              //returns neededTime useful in delaying Start method
    }
    IEnumerator PlayReaction()
    {
        int index = 0;
        if (behavior == 'F')                                            //if player entered walking forwards, 
        {
            index = Random.Range(0, numFReactionAnims);                 //use number of forward reaction animations
        }
        else if (behavior == 'B')
        {
            index = Random.Range(0, numBReactionAnims);                 //same logic for if player entered backwards
        }
        animatorBody.SetInteger("ReactionIndex", index);                //Random Reaction animation is chosen
        yield return new WaitForSecondsRealtime(1.5f);                  //pause running so reaction animation can be fully played out
    }
    void TurnLeft() { animatorBody.SetTrigger("TurnLeft"); }            //triggers for transition to be made for character to turn left
    void TurnRight() { animatorBody.SetTrigger("TurnRight"); }          //triggers for transition to be made for character to turn right

}
