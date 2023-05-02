using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rightDetect : MonoBehaviour
{
    public GripController gripController;

    // Start is called before the first frame update
    void Start()
    {
        GameObject g = GameObject.FindGameObjectWithTag("GripController");             //finds object with Gripper tag which houses OpenCloseGripper script
        gripController = g.GetComponent<GripController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        gripController.right_detect = true;
    }
    private void OnTriggerExit(Collider other)
    {
        gripController.right_drop = true;
    }
}
