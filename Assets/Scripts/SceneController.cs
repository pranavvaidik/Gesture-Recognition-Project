using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
//using Unity.VisualScripting;
using UnityEditor;
//using UnityEditor.Recorder;
using UnityEngine;

public class SceneController : MonoBehaviour
{
    //each of these are declarations that are necessary for future actions in the code
    private int frameCount = 0;
    public int maxNum;

    Animator animatorBody;
    Animator animatorArm;
    public int ChooseDataset;                       //0 for ASL for Numbers , 1 for ASL for alphabet
    string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZsn";     //useful in randomly choosing alphabet animations
    public Camera digitCam;
    public Camera alphabetCam;
    public Camera MNCam;
    public Camera GHCam;                    //Camera declarations for enable/disable and placement later
    public Camera PCam;
    public Camera QCam;
    public Camera SPACECam;

    List<string> fileList = new List<string>();     //necessary for grabbing human model .fbx files from file system
    public string[] humanModels;

    private GameObject[] spawned;
    private int[] digitCounter = new int[11];       //these are counters to indicate when maxNum images have
    private int[] alphaCounter = new int[28];       //been recorded for each gesture 

    // Start is called before the first frame update
    void Start()
    {
        GetModels();                        //on start, call GetModels function
        spawned = new GameObject[1];        //indicate that spawned array contains a model now
    }

    // Update is called once per frame
    void Update()
    {
         if (frameCount % 60 == 0)      //every 60 frames activate (can be changed depending on system)
         {
             GenerateRandom();          //calls GenerateRandom function
         }
         frameCount++;                  //increase framecount
    }

    //GetModels inputs all created MakeHuman models into an array for GenerateRandom to use
    void GetModels()
    {
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/Models");   //selects directory to grab models from
        FileInfo[] files = dir.GetFiles("human*.fbx");   //places all files with name starting with human and ending in .fbx
                                                         //from chosen directory into a files folder

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
        GameObject spawnedModel = Instantiate(Resources.Load<GameObject>(filename));    //places chosen model in scene
        spawned[0] = spawnedModel;      //indicates a new model is spawned
        PlayAnimation(spawnedModel);    //Calls PlayAnimation function
        //synth.OnSceneChange();
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

        //This block is for playing ASL for Numbers animations since ChooseDataset is marked as 0
        if (ChooseDataset == 0)
        {
            //Places animation controller fit with ASL number animations into arm animator component
            animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/DigitsData.controller", typeof(RuntimeAnimatorController));
            
            //chooses random value 0-10 for random determination of animation to play
            int randNum = Random.Range(0, 11);
            if (randNum < 10)                                       //if the number is less than 10,
            {
                animatorArm.Play("Sign" + randNum.ToString());      //the index means exactly the animation to play
            }
            else
            {
                Destroy(spawned[0]);     //if 10 is chosen, we want to show "nothing" so we delete the model before taking image
            }
            char randChar = (char)randNum;          //for Camera placement useage
            PlaceCamera(randChar, 0);               //Calls PlaceCamera function to enable correct camera for image capture
            digitCounter[randNum] += 1;             //adds 1 to value at index of animation that just played

            StartCoroutine(TakeDigitImage(.05f, randNum, digitCounter[randNum]));  //Takes image       
        
        }
        //This block is for playing ASL alphabet animations since ChooseDataset is marked as 1
        else if (ChooseDataset == 1)
        {
            //using st to pick random character A-Z as well as s for Space and n for Nothing
            int indexNum = Random.Range(0, st.Length);      //chooses random index of string
            char randChar = st[indexNum];               //assigns character at that chosen index to randChar variable
            IncreaseCount(randChar);                    //Calls IncreaseCount, which keeps track of how many times each animation has played

            //Places animation controller containing alphabet animations onto lowerarm animator component
            animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/AlphabetData.controller", typeof(RuntimeAnimatorController));

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
            int letterCount = DisplayCount(randChar);   //Calls DisplayCount to read the value of the counter for a given gesture

            StartCoroutine(TakeAlphabetImage(.05f, randChar, letterCount));         //takes image

            //OPTIMIZATION: if maxNum images have been taken for a given gesture, we dont want to play that gesture anymore
            if (letterCount >= maxNum)              
            {
                for (int i=0; i<st.Length; i++)         
                {
                    if (st[i] == randChar)          //sift through st and once randChar is found, remove it from the string
                    {                               //so it cannot be selected again in the next updates
                        st = st.Remove(i,1);
                    }
                }
            }
        }
    }

    //TakeDigitImage screen captures what is shown in the display as determined by PlaceCamera
    IEnumerator TakeDigitImage(float delayTime, int value, int gestureNum)
    {
        int gesturesDone = 0;
        string folderPath;
        string number = convertNumToWord(value);    //calls convertNumToWord to name the file as the dataset we are replicating did
        string filename = $"{number}_{gestureNum.ToString().PadLeft(5, '0')}.jpg"; //names the output file
        yield return new WaitForSeconds(delayTime);     //needs to delay so animation can fully occur before image is taken

        if (value < 10)         //value is the randomly generated number 0-10. if 0-9, 0-9 animation is played
        {                                                   //so animation must be placed in the correct folder 0-9
            folderPath = Directory.GetCurrentDirectory() + $"/SignLanguageForNumbers/Train/{value.ToString()}";
        }
        else                    //if 10 is chosen, "nothing" is happening so we choose the folder path to be in unknown
        {
            folderPath = Directory.GetCurrentDirectory() + $"/SignLanguageForNumbers/Train/unknown";
        }

        //checks if number of gestures has exceeded maxNum wanted
        if (gestureNum <= maxNum)
        {
            //if it hasn't, take a screenshot named filename and place in folderPath
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, filename));     
        }

        //sifts through digitCounter to determine if all gestures are complete
        foreach (int i in digitCounter)
        {
            if (i >= maxNum)
            {
                gesturesDone += 1;
            }
        }
        //if all gestures are complete with maxNum pictures for each,
        if (gesturesDone == 11)                 
        {
            EditorApplication.isPlaying = false;        //stops Unity player
        }
    }

    //TakeAlphabetImage screen captures what is shown in the display as determined by PlaceCamera, similar to previous
    IEnumerator TakeAlphabetImage(float delayTime, char letter, int gestureNum)
    {
        int gesturesDone = 0;
        string folderPath;
        string filename = $"{gestureNum.ToString().PadLeft(5, '0')}.jpg";   //naming output file as dataset we are replicating has
        yield return new WaitForSeconds(delayTime);  //again delays so animation can play out before screen capture

        //these next if-else statements decide where to store the output images depending on the character being animated
        if (letter != 's' & letter != 'n')
        {
            folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/{letter.ToString()}";
        }
        else if (letter == 's')
        {
            folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/Space";
        }
        else
        {
            folderPath = Directory.GetCurrentDirectory() + $"/AmericanSignLanguage/Train/Nothing";
        }

        //checks if gesture already has maxNum images taken of it
        if (gestureNum <= maxNum)
        {
            //if not, take an image called filename and store in folderPath
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folderPath, filename));
        }

        //sifts through alphaCounter to check if all gestures have maxNum images
        foreach (int i in alphaCounter)
        {
            if (i >= maxNum)
            {
                gesturesDone += 1;
            }
        }

        //if all gestures have maxNum images each,
        if (gesturesDone == 28)                 //28 for 26 letters, 1 space, 1 nothing
        {
            EditorApplication.isPlaying = false;        //stops Unity player
        }
        
    }

    //convertNumToWord is used for naming the output file within the TakeDigitImage coroutine
    string convertNumToWord(int value)
    {
        //adds possible number strings to a list and chooses which to use based on the index chosen previously
        string GestureWord;
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
        GestureWord = possibleWords[value];
        return GestureWord;
    }

    //IncreaseCount is necessary in PlayAnimation function to count the amount of times each gesture has been played
    void IncreaseCount(char c)
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

    //DisplayCount is necessary in PlayAnimation to check the amount of times an animation is played
    //this is used in PlayAnimation to determine if a gesture has already reached maxNum images
    int DisplayCount(char c)
    {
        //depending on the randChar chosen in PlayAnimation, the assigned index of alphaCounter is returned to the function call
        int value = 0;
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
        return value;
    }

    //PlaceCamera is required to enable/disable and randomly adjust camera angles for each gesture
    void PlaceCamera(char randChar, int dataset)
    {
        //begin by disabling all cameras 
        alphabetCam.enabled = false;

        MNCam.enabled = false;
        GHCam.enabled = false;
        PCam.enabled = false;
        QCam.enabled = false;
        SPACECam.enabled = false;
        digitCam.enabled = false;

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
                float x_pos_change = Random.Range(-.3f, .3f);
                float y_pos_change = Random.Range(-.5f, .35f);           //alterations for PCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-12.74f, 17.15f, 111.46f);        //PCam starting position
                PCam.enabled = true;
                PCam.transform.localPosition = startingPos;             //PCam location assignment/alteration
                PCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
            //if Q is chosen, use QCam
            else if (randChar == 'Q')
            {
                float x_pos_change = Random.Range(-.25f, .25f);
                float y_pos_change = Random.Range(-.25f, .25f);         //alterations for QCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-12.79f, 18.36f, 109.47f);       //QCam starting position
                QCam.enabled = true;
                QCam.transform.localPosition = startingPos;             //QCam location assignment/alteration
                QCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
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
                float y_pos_change = Random.Range(-.25f, .25f);         //alterations for alphabetCam
                float z_pos_change = Random.Range(-.25f, .25f);
                Vector3 startingPos = new Vector3(-13.36f, 14.33f, 111f);        //alphabetCam starting position
                alphabetCam.enabled = true;
                alphabetCam.transform.localPosition = startingPos;      //alphabetCam location assignment/alteration
                alphabetCam.transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
            }
        }
    }
}
