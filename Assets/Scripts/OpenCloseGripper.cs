using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Animations;
using UnityEngine;

public class OpenCloseGripper : MonoBehaviour
{
    Animator OpenCloseAnimator;
    public RuntimeAnimatorController controller;
    //public string inputAxis;


    // Start is called before the first frame update
    void Start()
    {
        OpenCloseAnimator = gameObject.GetComponent<Animator>();
        OpenCloseAnimator.runtimeAnimatorController = controller;
        //Debug.Log(OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).IsName("close"));
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime);
        if(OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && OpenCloseAnimator.GetFloat("speed") > 0)
        {
            OpenCloseAnimator.SetFloat("speed", 0);
        }
        else if(OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0 && OpenCloseAnimator.GetFloat("speed") < 0)
        {
            OpenCloseAnimator.SetFloat("speed", 0);
        }
        /*
        if (Input.GetKeyDown("p"))
        {
            OpenCloseAnimator.SetFloat("speed", 1);
        } 
        if (Input.GetKeyDown("o"))
        {
            OpenCloseAnimator.SetFloat("speed", -1);
        }*/

    }

    public void updateMovement(float inputVal)
    {
        if (OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && inputVal > 0)
        {
            OpenCloseAnimator.SetFloat("speed", inputVal);
        }
        else if (OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0 && inputVal < 0)
        {
            OpenCloseAnimator.SetFloat("speed", inputVal);
        }
    }

    public float getAnimationProgress()
    {
        return OpenCloseAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public float getAnimationSpeed()
    {
        return OpenCloseAnimator.GetFloat("speed");
    }

    public void resetAnimation()
    {
        OpenCloseAnimator.Play("close", 0, 0.0f);
        OpenCloseAnimator.SetFloat("speed", 0);
    }
}
