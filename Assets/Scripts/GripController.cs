using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GripController : MonoBehaviour
{
    public GameObject Cube;
    public OpenCloseGripper gripper_control;
    public Animator gripper_animator;
    [HideInInspector] public bool left_detect = false;
    [HideInInspector] public bool right_detect = false;
    [HideInInspector] public bool left_drop = false;
    [HideInInspector] public bool right_drop = false;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = GameObject.FindGameObjectWithTag("Gripper");             //finds object with Gripper tag which houses OpenCloseGripper script
        Cube = GameObject.FindGameObjectWithTag("Cube");
        gripper_control = g.GetComponent<OpenCloseGripper>();                                  //gets the script component from that object with tag
        gripper_animator = g.GetComponent<Animator>();                                      //gets the animator to set speed to 0 eventually
    }

    // Update is called once per frame
    void Update()
    {
        if ((left_detect==true) & (right_detect == true))
        {
            left_detect = false;
            right_detect = false;
            gripper_animator.SetFloat("speed", 0);                      //when object is detected, set animation speed to 0
            Cube.transform.parent = gripper_control.transform;          //parent cube object to gripper so they actually move together (grip)
            Cube.GetComponent<Rigidbody>().useGravity = false;          //set gravity to 0 so that cube wont slide out of gripper clamps when you move it around
        }
        if ((left_drop == true) & (right_drop == true))
        {
            left_drop = false;
            right_drop = false;
            Cube.transform.parent = null;
            Cube.GetComponent<Rigidbody>().useGravity = true;          //set gravity to 0 so that cube wont slide out of gripper clamps when you move it around
        }
    }
}
