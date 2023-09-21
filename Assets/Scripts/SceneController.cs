using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
//using Unity.VisualScripting;
//using UnityEditor;
//using UnityEditor.Recorder;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    //each of these are declarations that are necessary for future actions in the code. All public variables are able to be manipulated by user.
    private int frameCount = 0;
    public int maxNum;

    Animator animatorBody;
    Animator animatorArm;
    public int ChooseDataset;                       //0 for ASL for Numbers , 1 for ASL for alphabet, 2 for HaGRID
    string nums = "0123456789u";
    string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZsn";     //strings for choice of dataset specific animations
    string hagrid = "0123456789!@#$%^&*";
    string HANDs = "123456789SP";                   //without Horiz for v2
    public Camera HANDsCam;
    public Camera HANDsv2Cam;
    public Camera digitCam;
    public Camera alphabetCam;
    public Camera MNCam;
    public Camera GHCam;                    //Camera declarations for enable/disable and placement later
    public Camera PCam;
    public Camera QCam;
    public Camera XCam;
    public Camera SPACECam;
    public Camera HagridCam;
    public Camera rockCam;
    public Camera stopCam;
    public Camera likeCam;
    public Camera dislikeCam;
    public Camera invCam;
    public Camera callCam;
    public Light LightSource1;                  //lighting declarations for altering intensity later
    public Light LightSource2;
    public string HANDsModel;

    List<string> fileList = new List<string>();     //necessary for grabbing human model .fbx files from file system
    public string[] humanModels;

    private GameObject[] spawned;
    private int[] digitCounter = new int[11];       //these are counters to indicate when maxNum images have
    private int[] alphaCounter = new int[28];       //been recorded for each gesture 
    private int[] hagridCounter = new int[18];
    private int[] HANDsCounter = new int[11];       //11 without Horiz for v2

    // Start is called before the first frame update
    void Start()
    {
        //edge cases of user inputs. ChooseDataset must be 0, 1, or 2. If not, write in console error message and quit Unity run
        if ((ChooseDataset > 3) | (ChooseDataset < 0))
        {
            Debug.Log("Invalid Dataset Choice. Select 0 for ASL Numbers replication or 1 for ASL Letters.");
            //EditorApplication.isPlaying = false;        //stops Unity player
            Application.Quit();

        }
        //maxNum must be positive
        if (maxNum < 0)
        {
            Debug.Log("Invalid maxNum Choice. Choose a positive integer.");
            //EditorApplication.isPlaying = false;        //stops Unity player
            Application.Quit();
        }
        GetModels();                        //on start, call GetModels function
        spawned = new GameObject[1];        //indicate that spawned array contains a model now
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCount % 50 == 0)      //every 50 frames, activate (my home computer uses 50 frames but powerful lab computer uses 30 frames for complete set generation)
        {
            GenerateRandom();          //calls GenerateRandom function
        }
        frameCount++;                  //increase framecount
    }

    //GetModels inputs all created MakeHuman models into an array for GenerateRandom to use
    void GetModels()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/Models/");   //selects directory to grab models from
        FileInfo[] files = dir.GetFiles("mass*.fbx");   //places all files with name starting with human and ending in .fbx from chosen directory into a files folder
        if (ChooseDataset == 3)
        {
            dir = new DirectoryInfo("Assets/Resources/Models/HANDSv2_Models/" + HANDsModel + "/");   //selects directory to grab models from (HANDSv2 CHOOSE MODEL NUMBER)
            files = dir.GetFiles("mass*.prefab");
        }

        foreach (FileInfo file in files)
        {
            string name = file.Name.Split('.')[0];      //for each file, disconnect the .fbx from the name
            fileList.Add(name);                         //and add the model name into the list of file names
        }

        humanModels = fileList.ToArray();           //translates file of models into GameObject Array for use
    }

    //GenerateRandom is used to spawn a randomly selected human model into scene
    void GenerateRandom()
    {
        if (spawned[0] != null)         //checks if there is already a spawned model in the scene
        {
            Destroy(spawned[0]);        //if there is, delete it before placing a new one
        }
        int HumanIndex = Random.Range(0, humanModels.Length);       //selects random index of human model array
        string filename = $"Models/{humanModels[HumanIndex]}";      //gets human model filename depending on randomly chosen index
        if (ChooseDataset == 3)
        {
            filename = $"Models/HANDSv2_Models/Model1/{humanModels[HumanIndex]}";      //MUST CHANGE FOR HANDSv2 MODEL USEAGE
        }
        GameObject spawnedModel = Instantiate(Resources.Load<GameObject>(filename));    //places chosen model in scene
        spawned[0] = spawnedModel;      //indicates a new model is spawned
        PlayAnimation(spawnedModel);    //Calls PlayAnimation function
    }

    //PlayAnimation places animator component on the spawned model's lowerarm then places the correct animation controller
    //inside of the animator component and selects a random animation to play
    void PlayAnimation(GameObject model)
    {
        //Checks if there is a pre-existing animator component on the model GameObject and disables it if there is
        animatorBody = model.GetComponent<Animator>();
        if (animatorBody != null)
        {
            animatorBody.enabled = false;
        }

        //Indexes through the models bone hierarchy until the lowerarm02 bone is reached
        GameObject rig = model.transform.Find("MakeHuman default skeleton").gameObject;
        GameObject root = rig.transform.GetChild(0).gameObject;
        GameObject spine5 = root.transform.GetChild(2).gameObject;
        GameObject spine4 = spine5.transform.GetChild(0).gameObject;
        GameObject spine3 = spine4.transform.GetChild(0).gameObject;
        GameObject spine2 = spine3.transform.GetChild(0).gameObject;
        GameObject spine1 = spine2.transform.GetChild(2).gameObject;
        GameObject clavicle = spine1.transform.GetChild(0).gameObject;
        GameObject shoulder = clavicle.transform.GetChild(0).gameObject;
        GameObject uparm1 = shoulder.transform.GetChild(0).gameObject;
        GameObject uparm2 = uparm1.transform.GetChild(0).gameObject;
        GameObject lowarm1 = uparm2.transform.GetChild(0).gameObject;
        GameObject lowarm2 = lowarm1.transform.GetChild(0).gameObject;

        //Places animator component onto the lowerarm02 bone
        Animator animatorArm = lowarm2.AddComponent(typeof(Animator)) as Animator;
        //Gets animator component for manipulation
        animatorArm = lowarm2.GetComponent<Animator>();
        animatorArm.enabled = true;             //enables the animator and applies root motion
        animatorArm.applyRootMotion = true;

        //randomization of light source conditions
        LightSource1.intensity = Random.Range(.2f, .7f);
        LightSource2.intensity = Random.Range(.2f, .7f);

        //This block is for playing ASL for Numbers animations since ChooseDataset is marked as 0
        if (ChooseDataset == 0)
        {
            //Places animation controller fit with ASL number animations into arm animator component
            //animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/DigitsData.controller", typeof(RuntimeAnimatorController));
            animatorArm.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controllers/DigitsData");

            //chooses random index within string of animation choices for determination of gesture to play
            int indexNum = Random.Range(0, nums.Length);
            char randChar = nums[indexNum];                         //character at the string index corresponds to the gesture chosen
            int randNum;
            if (randChar != 'u')                            //if the character chosen isn't 'u', they are just gestures 0-9
            {
                randNum = randChar - '0';                           //changing character into integer
                animatorArm.Play("Sign" + randNum.ToString());      //plays animation clip labeled Sign + randomly chosen character
            }
            else                            //if the chosen character is 'u'
            {
                randNum = 10;            //number is valued as 10 since the index location of unknown in the number counter is 10
                Destroy(spawned[0]);     //if unknown is chosen, we want to show only background images so we delete the model before taking image
            }
            PlaceCamera(randChar, 0);               //Calls PlaceCamera function to enable correct camera for image capture
            digitCounter[randNum] += 1;             //adds 1 to value at index of animation that just played

            StartCoroutine(TakeImage(.02f, randChar, digitCounter[randNum], 0));  //Takes image       

            // if the animation reaches the maximum number of plays chosen by the user, remove it from string of choices
            if (digitCounter[randNum] >= maxNum)
            {
                for (int i = 0; i < nums.Length; i++)
                {
                    if (nums[i] == randChar)          //sift through nums and once randChar is found, remove it from the string
                    {                               //so it cannot be selected again in the next updates
                        nums = nums.Remove(i, 1);
                    }
                }
            }
        }
        //This block is for playing ASL alphabet animations since ChooseDataset is marked as 1
        else if (ChooseDataset == 1)
        {
            //using st to pick random character A-Z as well as s for Space and n for Nothing)
            int indexNum = Random.Range(0, st.Length);      //chooses random index of string
            char randChar = st[indexNum];               //assigns character at that chosen index to randChar variable

            IncreaseCount(randChar, ChooseDataset);                    //Calls IncreaseCount, which keeps track of how many times each animation has played

            //Places animation controller containing alphabet animations onto lowerarm animator component
            //animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/AlphabetData.controller", typeof(RuntimeAnimatorController));
            animatorArm.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controllers/AlphabetData");

            PlaceCamera(randChar, 1);           //place the camera in correct location based on character

            //These sections determine camera placement and animation to play based on chosen character
            if (randChar != 's' & randChar != 'n')
            {
                animatorArm.Play("Sign" + randChar);    //if not space or nothing, play ..., SignB, SignC, etc...
            }
            else if (randChar == 's')
            {
                animatorArm.Play("SignSPACE");      //if space is chosen, play signSpace
            }
            else
            {
                Destroy(spawned[0]);                //if nothing is chosen, delete the model from the scene
            }
            int letterCount = DisplayCount(randChar, ChooseDataset);   //Calls DisplayCount to read the value of the counter for a given gesture
            //Debug.Log("Performed " + randChar + ": iteration " + letterCount);

            StartCoroutine(TakeImage(.02f, randChar, letterCount, ChooseDataset));         //takes image

            //OPTIMIZATION: if maxNum images have been taken for a given gesture, we dont want to play that gesture anymore
            if (letterCount >= maxNum)
            {
                for (int i = 0; i < st.Length; i++)
                {
                    if (st[i] == randChar)          //sift through st and once randChar is found, remove it from the string
                    {                               //so it cannot be selected again in the next updates
                        st = st.Remove(i, 1);
                        //Debug.Log(randChar + " has been removed: reached " + maxNum.ToString() + " number of performances. New string of choices: " + st);
                    }
                }
            }
        }

        //Hagrid dataset chosen for generation
        else if (ChooseDataset == 2)
        {
            //chooses random index that corresponds to a character representing a gesture in the HAGRID dataset
            int index = Random.Range(0, hagrid.Length);
            char gesture = hagrid[index];

            //increases index value by 1 of animation clip that is selected
            IncreaseCount(gesture, ChooseDataset);

            //place correct hagrid controller on model arm
            //animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/HaGRIDContrl.controller", typeof(RuntimeAnimatorController));
            animatorArm.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controllers/HaGRIDContrl");

            PlaceCamera(gesture, 2);    //place camera in correct location depending on gesture chosen and dataset

            if (gesture == '0') { animatorArm.Play("ok"); }
            else if (gesture == '1') { animatorArm.Play("one"); }
            else if (gesture == '2') { animatorArm.Play("twoup"); }
            else if (gesture == '3') { animatorArm.Play("three"); }
            else if (gesture == '4') { animatorArm.Play("four"); }
            else if (gesture == '5') { animatorArm.Play("palm"); }
            else if ((gesture == '6') | (gesture == '7')) { animatorArm.Play("like"); }         //definitions of the gesture that each character within string of
            else if (gesture == '8') { animatorArm.Play("stop"); }                              //possible choices represent
            else if (gesture == '9') { animatorArm.Play("stop_inv"); }
            else if (gesture == '!') { animatorArm.Play("peace"); }                             //depending on the chosen character, run distinct animation
            else if (gesture == '@') { animatorArm.Play("peace_inv"); }
            else if (gesture == '#') { animatorArm.Play("twoup_inv"); }
            else if (gesture == '$') { animatorArm.Play("three2"); }
            else if (gesture == '%') { animatorArm.Play("call"); }
            else if (gesture == '^') { animatorArm.Play("fist"); }
            else if (gesture == '&') { animatorArm.Play("SignD"); }
            else if (gesture == '*') { animatorArm.Play("rock"); }

            int gestureCount = DisplayCount(gesture, ChooseDataset);        //read back the amount of times a gesture has been played

            StartCoroutine(TakeImage(.02f, gesture, gestureCount, ChooseDataset));   //take image

            //OPTIMIZATION: if maxNum images have been taken for a given gesture, we dont want to play that gesture anymore
            if (gestureCount >= maxNum)
            {
                for (int i = 0; i < hagrid.Length; i++)
                {
                    if (hagrid[i] == gesture)               //sift through hagrid string of choices and once randChar is found, remove it from the string
                    {                                       //so it cannot be selected again in the next updates
                        hagrid = hagrid.Remove(i, 1);
                    }
                }
            }
        }

        else if (ChooseDataset == 3)
        {
            int index = Random.Range(0, HANDs.Length);
            char gesture = HANDs[index];
            IncreaseCount(gesture, ChooseDataset);

            //place correct hagrid controller on model arm
            //animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/HANDsController.controller", typeof(RuntimeAnimatorController));
            animatorArm.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Controllers/HANDsController");
            PlaceCamera(gesture, 3);
            if (gesture == '1') { animatorArm.Play("1"); }
            else if (gesture == '2') { animatorArm.Play("2"); }
            else if (gesture == '3') { animatorArm.Play("3"); }
            else if (gesture == '4') { animatorArm.Play("4"); }
            else if (gesture == '5') { animatorArm.Play("5"); }         //definitions of the gesture that each character within string of
            else if (gesture == '6') { animatorArm.Play("6"); }
            else if (gesture == '7') { animatorArm.Play("7"); }                              //possible choices represent
            else if (gesture == '8') { animatorArm.Play("8"); }
            else if (gesture == '9') { animatorArm.Play("9"); }                             //depending on the chosen character, run distinct animation
            else if (gesture == 'S') { animatorArm.Play("Span"); }
            else if (gesture == 'P') { animatorArm.Play("punch"); }

            int gestureCount = DisplayCount(gesture, ChooseDataset);        //read back the amount of times a gesture has been played

            StartCoroutine(TakeImage(.02f, gesture, gestureCount, ChooseDataset));   //take image

            //OPTIMIZATION: if maxNum images have been taken for a given gesture, we dont want to play that gesture anymore
            if (gestureCount >= maxNum)
            {
                for (int i = 0; i < HANDs.Length; i++)
                {
                    if (HANDs[i] == gesture)               //sift through HANDs string of choices and once gesture characterr is found, remove it from the string
                    {                                       //so it cannot be selected again in the next updates
                        HANDs = HANDs.Remove(i, 1);
                    }
                }
            }
        }
    }

    //TakeAlphabetImage screen captures what is shown in the display as determined by PlaceCamera, similar to previous
    IEnumerator TakeImage(float delayTime, char character, int gestureNum, int dataset)
    {
        int gesturesDone = 0;
        string folderPath;
        string filename;                //declarations for future assignment depending on dataset being replicated
        int numGestures;
        int[] counter;
        yield return new WaitForSeconds(delayTime);    //again delays so animation can play out completely before screen capture

        //determination of variables depending on the dataset chosen
        if (dataset == 0)
        {
            counter = digitCounter;     //use digitCounter for later loop to check whether all gestures have been recorded maxnum times
            numGestures = 11;                                                   //selects max number of gestures to complete as 11 since Numbers dataset has 11 total gestures
            string number = convertNumToWord(character);                                //calls convertNumToWord to name the file as the dataset we are replicating did
            filename = $"{number}_{gestureNum.ToString().PadLeft(5, '0')}.jpg";         //names the output file
            if (character != 'u')
            {
                folderPath = Directory.GetCurrentDirectory() + $"/SignLanguageForNumbers/Train/{character.ToString()}";   //if not 'u', folder is just labeled 0-9
            }
            else
            {
                folderPath = Directory.GetCurrentDirectory() + $"/SignLanguageForNumbers/Train/unknown";  //if is 'u', folder called unknown
            }
        }
        else if (dataset == 1)
        {
            counter = alphaCounter;    //selects alphaCounter as counter to use later
            numGestures = 28;               //alphabet dataset has 28 total gestures
            filename = $"{gestureNum.ToString().PadLeft(5, '0')}.jpg";                        //naming output file as dataset we are replicating has
            folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train";

            if (character != 's' & character != 'n')
            {
                folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/{character.ToString()}";  //if not Space or Nothing, just A-Z folder labels
            }
            else if (character == 's')
            {
                folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/Space";   //place in Space folder
            }
            else
            {
                folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/Nothing";   //output path as Nothing folder
            }
        }
        else if (dataset == 2)
        {
            folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train";
            filename = $"{gestureNum.ToString().PadLeft(5, '0')}.jpg";   //naming output file as dataset we are replicating has
            numGestures = 18;  //hagrid set has 18 total gestures
            counter = hagridCounter;        //use hagridCounter in future

            if (character == '0') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/ok"; }
            else if (character == '1') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/one"; }
            else if (character == '2') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/two_up"; }
            else if (character == '3') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/three"; }
            else if (character == '4') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/four"; }        //depending on chosen character, place in correct folder
            else if (character == '5') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/palm"; }        //following definitions provided earlier
            else if (character == '6') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/like"; }
            else if (character == '7') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/dislike"; }
            else if (character == '8') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/stop"; }
            else if (character == '9') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/stop_inverted"; }
            else if (character == '!') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/peace"; }
            else if (character == '@') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/peace_inverted"; }
            else if (character == '#') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/two_up_inverted"; }
            else if (character == '$') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/three2"; }
            else if (character == '%') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/call"; }
            else if (character == '^') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/fist"; }
            else if (character == '&') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/mute"; }
            else if (character == '*') { folderPath = Directory.GetCurrentDirectory() + $"/Hagrid/Train/rock"; }
        }
        else
        {
            folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/";
            filename = $"{gestureNum.ToString().PadLeft(5, '0')}.jpg";   //naming output file as dataset we are replicating has
            numGestures = 11;  //hands set has 12 total gestures - 11 without Horiz for v2
            counter = HANDsCounter;        //use HANDsCounter in future

            if (character == '1') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/One_VFR"; }
            else if (character == '2') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Two_VFR"; }
            else if (character == '3') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Three_VFR"; }
            else if (character == '4') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Four_VFR"; }
            else if (character == '5') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Five_VFR"; }
            else if (character == '6') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Six_VFR"; }
            else if (character == '7') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Seven_VFR"; }
            else if (character == '8') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Eight_VFR"; }        //depending on chosen character, place in correct folder
            else if (character == '9') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Nine_VFR"; }        //following definitions provided earlier
            else if (character == 'S') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Span_VFR"; }
            else if (character == 'P') { folderPath = Directory.GetCurrentDirectory() + $"/HANDSv2/" + HANDsModel + "/Punch_VFR"; }
        }
        ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, filename));      //take a screenshot with output path and filename determined above

        foreach (int i in counter)
        {
            if (i >= maxNum)
            {
                gesturesDone += 1;          //uses the counter set above to check how many gestures have reached maximum number of plays
            }
        }
        if (gesturesDone == numGestures)                    //if the number of completed gestures matches total number of gestures in the dataset,
        {
            //EditorApplication.isPlaying = false;        //stops Unity player
            Application.Quit();
        }
    }

    //convertNumToWord is used for naming the output file within the TakeDigitImage coroutine
    string convertNumToWord(char character)
    {
        //adds possible number strings to a list and chooses which to use based on the index chosen previously
        string GestureWord;
        int value;
        List<string> possibleWords = new List<string>();
        possibleWords.Add("zero");
        possibleWords.Add("one");
        possibleWords.Add("two");
        possibleWords.Add("three");
        possibleWords.Add("four");
        possibleWords.Add("five");
        possibleWords.Add("six");
        possibleWords.Add("seven");
        possibleWords.Add("eight");
        possibleWords.Add("nine");
        possibleWords.Add("unknown");
        if (character != 'u')
        {
            value = character - '0';        //if 'u' isnt selected, character '0'->'9' is changed to integer 0->9
        }
        else
        {
            value = 10;                     //if 'u' is selected, use value 10 as possibleWords[10] is unknown
        }
        GestureWord = possibleWords[value];
        return GestureWord;                     //output word as a string
    }

    //IncreaseCount is necessary in PlayAnimation function to count the amount of times each gesture has been played
    void IncreaseCount(char c, int ChooseDataset)
    {
        if (ChooseDataset == 1)
        {
            //each letter/space/nothing corresponds to an index in the alphaCounter, which is incremented if the randChar chosen is
            //that letter/space/nothing
            if (c == 'A') { alphaCounter[0] += 1; }
            else if (c == 'B') { alphaCounter[1] += 1; }
            else if (c == 'C') { alphaCounter[2] += 1; }
            else if (c == 'D') { alphaCounter[3] += 1; }
            else if (c == 'E') { alphaCounter[4] += 1; }
            else if (c == 'F') { alphaCounter[5] += 1; }
            else if (c == 'G') { alphaCounter[6] += 1; }
            else if (c == 'H') { alphaCounter[7] += 1; }
            else if (c == 'I') { alphaCounter[8] += 1; }
            else if (c == 'J') { alphaCounter[9] += 1; }
            else if (c == 'K') { alphaCounter[10] += 1; }
            else if (c == 'L') { alphaCounter[11] += 1; }
            else if (c == 'M') { alphaCounter[12] += 1; }
            else if (c == 'N') { alphaCounter[13] += 1; }
            else if (c == 'O') { alphaCounter[14] += 1; }
            else if (c == 'P') { alphaCounter[15] += 1; }
            else if (c == 'Q') { alphaCounter[16] += 1; }
            else if (c == 'R') { alphaCounter[17] += 1; }
            else if (c == 'S') { alphaCounter[18] += 1; }
            else if (c == 'T') { alphaCounter[19] += 1; }
            else if (c == 'U') { alphaCounter[20] += 1; }
            else if (c == 'V') { alphaCounter[21] += 1; }
            else if (c == 'W') { alphaCounter[22] += 1; }
            else if (c == 'X') { alphaCounter[23] += 1; }
            else if (c == 'Y') { alphaCounter[24] += 1; }
            else if (c == 'Z') { alphaCounter[25] += 1; }
            else if (c == 's') { alphaCounter[26] += 1; }
            else if (c == 'n') { alphaCounter[27] += 1; }
        }
        //each index of the hagridCounter corresponds to an individual gesture, which is incremented everytime that gesture is played
        else if (ChooseDataset == 2)
        {
            if (c == '0') { hagridCounter[0] += 1; }
            else if (c == '1') { hagridCounter[1] += 1; }
            else if (c == '2') { hagridCounter[2] += 1; }
            else if (c == '3') { hagridCounter[3] += 1; }
            else if (c == '4') { hagridCounter[4] += 1; }
            else if (c == '5') { hagridCounter[5] += 1; }
            else if (c == '6') { hagridCounter[6] += 1; }
            else if (c == '7') { hagridCounter[7] += 1; }
            else if (c == '8') { hagridCounter[8] += 1; }
            else if (c == '9') { hagridCounter[9] += 1; }
            else if (c == '!') { hagridCounter[10] += 1; }
            else if (c == '@') { hagridCounter[11] += 1; }
            else if (c == '#') { hagridCounter[12] += 1; }
            else if (c == '$') { hagridCounter[13] += 1; }
            else if (c == '%') { hagridCounter[14] += 1; }
            else if (c == '^') { hagridCounter[15] += 1; }
            else if (c == '&') { hagridCounter[16] += 1; }
            else if (c == '*') { hagridCounter[17] += 1; }
        }
        else if (ChooseDataset == 3)
        {
            if (c == '1') { HANDsCounter[0] += 1; }
            else if (c == '2') { HANDsCounter[1] += 1; }
            else if (c == '3') { HANDsCounter[2] += 1; }
            else if (c == '4') { HANDsCounter[3] += 1; }
            else if (c == '5') { HANDsCounter[4] += 1; }
            else if (c == '6') { HANDsCounter[5] += 1; }
            else if (c == '7') { HANDsCounter[6] += 1; }
            else if (c == '8') { HANDsCounter[7] += 1; }
            else if (c == '9') { HANDsCounter[8] += 1; }
            else if (c == 'S') { HANDsCounter[9] += 1; }
            else if (c == 'P') { HANDsCounter[10] += 1; }
        }
    }

    //DisplayCount is necessary in PlayAnimation to check the amount of times an animation is played
    //this is used in PlayAnimation to determine if a gesture has already reached maxNum images
    int DisplayCount(char c, int ChooseDataset)
    {
        int value = 0;
        if (ChooseDataset == 1)
        {
            //depending on the randChar chosen in PlayAnimation, the assigned index of alphaCounter is returned to the function call
            if (c == 'A') { value = alphaCounter[0]; }
            else if (c == 'B') { value = alphaCounter[1]; }
            else if (c == 'C') { value = alphaCounter[2]; }
            else if (c == 'D') { value = alphaCounter[3]; }
            else if (c == 'E') { value = alphaCounter[4]; }
            else if (c == 'F') { value = alphaCounter[5]; }
            else if (c == 'G') { value = alphaCounter[6]; }
            else if (c == 'H') { value = alphaCounter[7]; }
            else if (c == 'I') { value = alphaCounter[8]; }
            else if (c == 'J') { value = alphaCounter[9]; }
            else if (c == 'K') { value = alphaCounter[10]; }
            else if (c == 'L') { value = alphaCounter[11]; }
            else if (c == 'M') { value = alphaCounter[12]; }
            else if (c == 'N') { value = alphaCounter[13]; }
            else if (c == 'O') { value = alphaCounter[14]; }
            else if (c == 'P') { value = alphaCounter[15]; }
            else if (c == 'Q') { value = alphaCounter[16]; }
            else if (c == 'R') { value = alphaCounter[17]; }
            else if (c == 'S') { value = alphaCounter[18]; }
            else if (c == 'T') { value = alphaCounter[19]; }
            else if (c == 'U') { value = alphaCounter[20]; }
            else if (c == 'V') { value = alphaCounter[21]; }
            else if (c == 'W') { value = alphaCounter[22]; }
            else if (c == 'X') { value = alphaCounter[23]; }
            else if (c == 'Y') { value = alphaCounter[24]; }
            else if (c == 'Z') { value = alphaCounter[25]; }
            else if (c == 's') { value = alphaCounter[26]; }
            else if (c == 'n') { value = alphaCounter[27]; }
        }
        // display amount of times a gesture from the hagrid dataset has played
        else if (ChooseDataset == 2)
        {
            if (c == '0') { value = hagridCounter[0]; }
            else if (c == '1') { value = hagridCounter[1]; }
            else if (c == '2') { value = hagridCounter[2]; }
            else if (c == '3') { value = hagridCounter[3]; }
            else if (c == '4') { value = hagridCounter[4]; }
            else if (c == '5') { value = hagridCounter[5]; }
            else if (c == '6') { value = hagridCounter[6]; }
            else if (c == '7') { value = hagridCounter[7]; }
            else if (c == '8') { value = hagridCounter[8]; }
            else if (c == '9') { value = hagridCounter[9]; }
            else if (c == '!') { value = hagridCounter[10]; }
            else if (c == '@') { value = hagridCounter[11]; }
            else if (c == '#') { value = hagridCounter[12]; }
            else if (c == '$') { value = hagridCounter[13]; }
            else if (c == '%') { value = hagridCounter[14]; }
            else if (c == '^') { value = hagridCounter[15]; }
            else if (c == '&') { value = hagridCounter[16]; }
            else if (c == '*') { value = hagridCounter[17]; }
        }
        else if (ChooseDataset == 3)
        {
            if (c == '1') { value = HANDsCounter[0]; }
            else if (c == '2') { value = HANDsCounter[1]; }
            else if (c == '3') { value = HANDsCounter[2]; }
            else if (c == '4') { value = HANDsCounter[3]; }
            else if (c == '5') { value = HANDsCounter[4]; }
            else if (c == '6') { value = HANDsCounter[5]; }
            else if (c == '7') { value = HANDsCounter[6]; }
            else if (c == '8') { value = HANDsCounter[7]; }
            else if (c == '9') { value = HANDsCounter[8]; }
            else if (c == 'S') { value = HANDsCounter[9]; }
            else if (c == 'P') { value = HANDsCounter[10]; }
        }
        return value;
    }

    //PlaceCamera is required to enable/disable and randomly adjust camera angles for each gesture
    void PlaceCamera(char randChar, int dataset)
    {
        //begin by disabling all cameras 
        alphabetCam.enabled = false;
        HagridCam.enabled = false;
        rockCam.enabled = false;
        stopCam.enabled = false;
        likeCam.enabled = false;
        dislikeCam.enabled = false;
        invCam.enabled = false;
        callCam.enabled = false;
        MNCam.enabled = false;
        GHCam.enabled = false;
        PCam.enabled = false;
        QCam.enabled = false;
        SPACECam.enabled = false;
        digitCam.enabled = false;
        XCam.enabled = false;
        HANDsCam.enabled = false;
        HANDsv2Cam.enabled = false;

        //if ASL for Numbers is being run,
        if (dataset == 0)
        {
            digitCam.enabled = true;                                        //enable digitCam
            Vector3 startingPos = new Vector3(-13.58f, 13.79f, 110.88f);    //assign its starting position to a variable
            float x_pos_change = Random.Range(-.35f, .35f);
            float y_pos_change = Random.Range(-.35f, .35f);             //select random adjustments in the x,y,z directions
            float z_pos_change = Random.Range(-.35f, .35f);
            //place camera at its starting position so it is moved from here every iteration
            digitCam.transform.localPosition = startingPos;
            //place camera at starting position + alterations in x,y,z directions
            digitCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
        }
        // if ASL for alphabet is being run, need to select camera based on gesture (G, H, M, N, P, Q, and Space have
        // unique camera placement)
        else if (dataset == 1)
        {
            // if space is chosen, perform exact same method as previously described in digitCam alterations
            if (randChar == 's')
            {
                float x_pos_change = Random.Range(-.4f, .3f);
                float y_pos_change = Random.Range(-.5f, .4f);               //alterations for SpaceCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-14.07f, 14.97f, 110.11f);    //SpaceCam starting position
                SPACECam.enabled = true;
                SPACECam.transform.localPosition = startingPos;         //SpaceCam location assignment/alteration
                SPACECam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            // if M or N is chosen, use MNCam
            else if ((randChar == 'M') | (randChar == 'N'))
            {
                float x_pos_change = Random.Range(-.2f, .2f);
                float y_pos_change = Random.Range(-.2f, .2f);          //alterations for MNCam
                float z_pos_change = Random.Range(-.2f, .2f);
                Vector3 startingPos = new Vector3(-14.61f, 15.14f, 111.19f);        //MNCam starting position for M
                if (randChar == 'N')
                {
                    startingPos = new Vector3(-14.27f, 15.21f, 111.36f);        //MNCam starting position for N

                }
                MNCam.enabled = true;
                MNCam.transform.localPosition = startingPos;            //MNCam location assignment/alteration
                MNCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            // if G or H is chosen, use GHCam
            else if ((randChar == 'G') | (randChar == 'H'))
            {
                float x_pos_change = Random.Range(-.5f, .2f);
                float y_pos_change = Random.Range(-.35f, .3f);         //alterations for GHCam
                float z_pos_change = Random.Range(-.3f, .35f);
                Vector3 startingPos = new Vector3(-13.32f, 13.94f, 110.27f);         //GHCam starting position
                GHCam.enabled = true;
                GHCam.transform.localPosition = startingPos;            //GHCam location assignment/alteration
                GHCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if P is chosen, use PCam
            else if (randChar == 'P')
            {
                float x_pos_change = Random.Range(-.25f, .25f);
                float y_pos_change = Random.Range(-.35f, .35f);           //alterations for PCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-12.74f, 17.15f, 111.46f);        //PCam starting position
                PCam.enabled = true;
                PCam.transform.localPosition = startingPos;             //PCam location assignment/alteration
                PCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if Q is chosen, use QCam
            else if (randChar == 'Q')
            {
                float x_pos_change = Random.Range(-.2f, .2f);
                float y_pos_change = Random.Range(-.2f, .2f);         //alterations for QCam
                float z_pos_change = Random.Range(-.2f, .2f);
                Vector3 startingPos = new Vector3(-12.79f, 18.36f, 109.47f);       //QCam starting position
                QCam.enabled = true;
                QCam.transform.localPosition = startingPos;             //QCam location assignment/alteration
                QCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            else if (randChar == 'X')
            {
                float x_pos_change = Random.Range(-.25f, .25f);
                float y_pos_change = Random.Range(-.25f, .25f);         //alterations for XCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-12.85f, 14.98f, 111.46f);        //XCam starting position
                XCam.enabled = true;
                XCam.transform.localPosition = startingPos;      //XCam location assignment/alteration
                XCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //slight alterations for Y since fingers sometimes are cut off screen
            //still use alphabet cam because although position changes slightly, rotation is the same
            else if (randChar == 'Y')
            {
                float x_pos_change = Random.Range(-.35f, .35f);
                float y_pos_change = Random.Range(-.35f, .35f);         //alterations for alphabetCam
                float z_pos_change = Random.Range(-.3f, .3f);
                Vector3 startingPos = new Vector3(-13.43f, 14.41f, 110.91f);        //alphabetCam starting position
                alphabetCam.enabled = true;
                alphabetCam.transform.localPosition = startingPos;      //alphabetCam location assignment/alteration
                alphabetCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //more slight alterations for J and Z once again due to fingers being cut off screen view
            //also use alphabet cam due to rotation being the same in camera angle required
            else if ((randChar == 'J') | (randChar == 'Z'))
            {
                float x_pos_change = Random.Range(-.35f, .35f);
                float y_pos_change = Random.Range(-.35f, .35f);         //alterations for alphabetCam
                float z_pos_change = Random.Range(-.3f, .3f);
                Vector3 startingPos = new Vector3(-13.44f, 14.26f, 110.58f);        //alphabetCam starting position
                alphabetCam.enabled = true;
                alphabetCam.transform.localPosition = startingPos;      //alphabetCam location assignment/alteration
                alphabetCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            // if any other character is chosen, use alphabetCam
            else
            {
                float x_pos_change = Random.Range(-.25f, .25f);
                float y_pos_change = Random.Range(-.25f, .4f);         //alterations for alphabetCam
                float z_pos_change = Random.Range(-.25f, .4f);
                Vector3 startingPos = new Vector3(-13.36f, 14.33f, 111f);        //alphabetCam starting position
                alphabetCam.enabled = true;
                alphabetCam.transform.localPosition = startingPos;      //alphabetCam location assignment/alteration
                alphabetCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
        }
        //camera placement and randomization for HAGRID dataset, still a WORK IN PROGRESS
        else if (dataset == 2)
        {
            float x_pos_change = Random.Range(-.25f, .25f);
            float y_pos_change = Random.Range(-.25f, .25f);         //general alterations
            float z_pos_change = Random.Range(-.25f, .25f);

            //if chosen characters represent rock or three
            if ((randChar == '3') | (randChar == '*'))
            {
                Vector3 startingPos = new Vector3(-13.04f, 14.35f, 110.98f);        //rockCam position placement/alteration
                rockCam.enabled = true;
                rockCam.transform.localPosition = startingPos;
                rockCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if chosen characters represent one or stop or mute
            else if ((randChar == '1') | (randChar == '8') | (randChar == '&'))
            {
                Vector3 startingPos = new Vector3(-13.26f, 13.93f, 110.23f);        //stopCam position placement/alteration
                stopCam.enabled = true;
                stopCam.transform.localPosition = startingPos;
                stopCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if chosen characters represents like
            else if (randChar == '6')
            {
                Vector3 startingPos = new Vector3(-15.363f, 17.318f, 107.813f);    //likeCam position placement/alteration      
                likeCam.enabled = true;
                likeCam.transform.localPosition = startingPos;
                likeCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if chosen characters represents dislike
            else if (randChar == '7')
            {
                Vector3 startingPos = new Vector3(-12.13f, 14.59f, 109.9f);     //dislikeCam position placement/alteration
                dislikeCam.enabled = true;
                dislikeCam.transform.localPosition = startingPos;
                dislikeCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if chosen characters represents call
            else if (randChar == '%')
            {
                Vector3 startingPos = new Vector3(-14.661f, 17.706f, 107.774f);    //callCam position placement/alteration
                callCam.enabled = true;
                callCam.transform.localPosition = startingPos;
                callCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if chosen characters represent stop_inverted, peace_inverted, or two_up_inverted
            else if ((randChar == '9') | (randChar == '@') | (randChar == '#'))
            {
                float x_pos_change_inv = Random.Range(-.5f, .25f);
                float y_pos_change_inv = Random.Range(-.7f, .65f);         //specific alterations
                float z_pos_change_inv = Random.Range(-.8f, .55f);
                Vector3 startingPos = new Vector3(-14.99f, 18.2f, 107.33f);    //invCam position placement/alteration
                invCam.enabled = true;
                invCam.transform.localPosition = startingPos;
                invCam.transform.localPosition = startingPos + new Vector3(x_pos_change_inv, y_pos_change_inv, z_pos_change_inv);
            }
            //if chosen characters represent any other option
            else
            {
                Vector3 startingPos = new Vector3(-13.26f, 13.93f, 110.23f);
                if (randChar == '^')
                {
                    startingPos = new Vector3(-13.05f, 14.06f, 110.25f);        //change position slightly for fist
                }
                HagridCam.enabled = true;
                HagridCam.transform.localPosition = startingPos;            //HagridCam position placement/alteration
                HagridCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
        }
        else if (dataset == 3)
        {
            //float x_pos_change = Random.Range(-.45f, .65f);
            //float y_pos_change = Random.Range(-.85f, .85f);         //general alterations
            //float z_pos_change = Random.Range(-.05f, .15f);
            //Vector3 startingPos = new Vector3(-12.4f, 13.7f, 111.9f);        //HANDsCam position placement/alteration
            HANDsv2Cam.enabled = true;
            //HANDsv2Cam.transform.localPosition = startingPos;
            //HANDsCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
        }
    }
}
