using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class OpenCloseGripper : MonoBehaviour
{
    Animator OpenCloseAnimator;
    public AnimatorController controller;

    // Start is called before the first frame update
    void Start()
    {
        OpenCloseAnimator = gameObject.GetComponent<Animator>();
        OpenCloseAnimator.runtimeAnimatorController = controller;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            OpenCloseAnimator.SetFloat("speed",1);
        }
        if (Input.GetKeyDown("o"))
        {
            OpenCloseAnimator.SetFloat("speed", -1);
        }
    }
}
