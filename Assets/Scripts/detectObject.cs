using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Animations;

public class detectObject : MonoBehaviour
{
    public GameObject Cube;
    public OpenCloseGripper gripper_control;
    public Animator gripper_animator;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = GameObject.FindGameObjectWithTag("Gripper");             //finds object with Gripper tag which houses OpenCloseGripper script
        Cube = GameObject.FindGameObjectWithTag("Cube");
        gripper_control = g.GetComponent<OpenCloseGripper>();                                  //gets the script component from that object with tag
        gripper_animator = g.GetComponent<Animator>();                                      //gets the animator to set speed to 0 eventually
    }

    private void OnTriggerEnter(Collider other)
    {
        gripper_animator.SetFloat("speed", 0);                      //when object is detected, set animation speed to 0
        Cube.transform.parent = gripper_control.transform;          //parent cube object to gripper so they actually move together (grip)
        Cube.GetComponent<Rigidbody>().useGravity = false;          //set gravity to 0 so that cube wont slide out of gripper clamps when you move it around
    }

    private void OnTriggerExit(Collider other)
    {
        Cube.transform.parent = null;
        Cube.GetComponent<Rigidbody>().useGravity = true;          //set gravity to 0 so that cube wont slide out of gripper clamps when you move it around
    }
}
