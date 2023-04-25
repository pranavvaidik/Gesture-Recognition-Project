using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
//using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class applyAnimation : MonoBehaviour
{
    Animator animatorBody;
    Animator animatorArm;
//  private int frameCount = 0;
    public int ChooseDataset;
    string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZs";
    public Camera digitCam;
    public Camera alphabetCam;
    public Camera MNCam;
    public Camera GHCam;
    public Camera PCam;
    public Camera QCam;
    public Camera SPACECam;

    // Start is called before the first frame update
    void Start()
    {
        animatorBody = gameObject.GetComponent<Animator>();
        animatorBody.enabled = false;

        digitCam.enabled = false;
        alphabetCam.enabled = false;
        MNCam.enabled = false;
        GHCam.enabled = false;
        PCam.enabled = false;
        QCam.enabled = false;
        SPACECam.enabled = false;

        GameObject rig = gameObject.transform.Find("MakeHuman default skeleton").gameObject;
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

        Animator animatorArm = lowarm2.AddComponent(typeof(Animator)) as Animator;
        animatorArm = lowarm2.GetComponent<Animator>();
        animatorArm.enabled = true;
        animatorArm.applyRootMotion = true;
        if (ChooseDataset == 0)
        {
            digitCam.enabled = true;
            animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/DigitsData.controller", typeof(RuntimeAnimatorController));
            int randNum = Random.Range(0, 9);
            animatorArm.Play("Sign" + randNum.ToString());
        }
        else if (ChooseDataset == 1)
        {
            alphabetCam.enabled = true;
            animatorArm.runtimeAnimatorController = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Resources/Controllers/AlphabetData.controller", typeof(RuntimeAnimatorController));
            char randChar = st[Random.Range(0, st.Length)];
            MNCam.enabled = false;
            GHCam.enabled = false;
            PCam.enabled = false;
            QCam.enabled = false;
            SPACECam.enabled = false;
            if (randChar != 's')
            {
                if ((randChar == 'M') | (randChar == 'N'))
                {
                    alphabetCam.enabled = false;
                    MNCam.enabled = true;
                }
                else if ((randChar == 'G') | (randChar == 'H'))
                {
                    alphabetCam.enabled = false;
                    GHCam.enabled = true;
                }
                else if (randChar == 'P')
                {
                    alphabetCam.enabled = false;
                    PCam.enabled = true;
                }
                else if (randChar == 'Q')
                {
                    alphabetCam.enabled = false;
                    QCam.enabled = true;
                }
                animatorArm.Play("Sign" + randChar);
            }
            else
            {
                alphabetCam.enabled = false;
                SPACECam.enabled = true;
                animatorArm.Play("SignSPACE");
            }      
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
        

        //animator.avatar.humanDescription.skeleton
        //add component character controller
        
}
