using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEditor;
//using UnityEditor.Animations;
using UnityEngine;

public class ControlHuman : MonoBehaviour
{
    Animator animatorBody;
   // private int frameCount = 0;
    public RuntimeAnimatorController controller;
    //public AvatarMask upperBody;
   // public AvatarMask lowerBody;

    List<string> fileList = new List<string>();     //necessary for grabbing human model .fbx files from file system
    public AnimationClip ActionClip;
    public string[] humanModels;
    private GameObject[] spawned;
    //public char BehaviorChoice;
    //private char PastBehavior;
    /*
    private string MoveFChoices = "0123";
    private string MoveBChoices = "0";
    private string ActChoices = "012345";
    private string ReactChoices = "0";
    */
    // Start is called before the first frame update
    void Start()
    {
        spawned = new GameObject[1];        //indicate that spawned array contains a model now 
        DirectoryInfo dir = new DirectoryInfo("Assets/Resources/Models");   //selects directory to grab models from
        FileInfo[] files = dir.GetFiles("human*.fbx");   //places all files with name starting with human and ending in .fbx
                                                         //from chosen directory into a files folder
        foreach (FileInfo file in files)
        {
            string name = file.Name.Split('.')[0];      //for each file, disconnect the .fbx from the name
            fileList.Add(name);                         //and add the model name into the list of file names
        }

        humanModels = fileList.ToArray();           //translates file of models into GameObject Array for use

        int HumanIndex = Random.Range(0, humanModels.Length);       //selects random index of human model array
        string filename = $"Models/{humanModels[HumanIndex]}";      //gets human model filename depending on randomly chosen index
        GameObject spawnedModel = Instantiate(Resources.Load<GameObject>(filename));    //places chosen model in scene
        spawned[0] = spawnedModel;      //indicates a new model is spawned
        animatorBody = spawnedModel.GetComponent<Animator>();
        animatorBody.runtimeAnimatorController = controller;

        /*
        while (true)
        {
            
            yield return new WaitForSeconds(3);
            animatorBody.SetInteger("ActionIndex", Random.Range(0, 6));
            animatorBody.SetTrigger("Action");
            
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.UpArrow))
        {
            animatorBody.Play("Walk_Forward3");
        }

        /*
        if (Input.GetKey(KeyCode.UpArrow) & Input.GetKey(KeyCode.R))
        {
            controller.layers[0].avatarMask = upperBody;
            controller.layers[1].avatarMask = lowerBody;
            animatorBody.Play("Base Layer.Restrain", 0);
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
            animatorBody.Play("Walk_Forward3");
            /*
            if (indexNum == 0) { animatorBody.Play("Walk_Forward1"); }
            else if (indexNum == 1) { animatorBody.Play("Walk_Forward2"); }
            else if (indexNum == 2) { animatorBody.Play("Walk_Forward3"); }
            else { animatorBody.Play("Run_Forward1"); }
            
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //int indexNum = Random.Range(0, MoveBChoices.Length);
            //if (indexNum == 0) {}
            animatorBody.Play("Walk_Back1");
        }
        else if (Input.GetKey(KeyCode.A))        //BehaviorChoice == 'A')
        {
            //int indexNum = Random.Range(0, ActChoices.Length);
            //if (indexNum == 0) {
            animatorBody.Play("Air_Squat");
            
            else if (indexNum == 1) { animatorBody.Play("Dance"); }
            else if (indexNum == 2) { animatorBody.Play("Joyful_Jump"); }
            else if (indexNum == 3) { animatorBody.Play("Picking_Up"); }
            else if (indexNum == 4) { animatorBody.Play("Quick_Informal_Bow"); }
            else { animatorBody.Play("Waving"); }
            
        }
        else if (Input.GetKey(KeyCode.R))        //BehaviorChoice == 'R')
        {
            //int indexNum = Random.Range(0, ReactChoices.Length);
            //if (indexNum == 0) {
            animatorBody.Play("Restrain");
        }
        else if (Input.GetKey(KeyCode.LeftArrow))    // BehaviorChoice == ',')
        {
            animatorBody.Play("LeftTurn");
        }
        else if (Input.GetKey(KeyCode.RightArrow)) // BehaviorChoice == '.')
        {
            animatorBody.Play("RightTurn");
        }
        else
        {
            animatorBody.Play("Idle");
        }
        */
        }


    }
