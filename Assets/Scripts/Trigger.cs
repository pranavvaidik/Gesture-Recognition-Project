using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public AutomateMove moveScript;             //uses AutomateMove script
    
    // Start is called before the first frame update
    void Start()
    {
         GameObject g = GameObject.FindGameObjectWithTag("GameController");             //finds object with GameController tag which houses AutomateMove script
         moveScript = g.GetComponent<AutomateMove> ();                                  //gets the script component from that object with tag
    }

    void OnTriggerEnter(Collider other)
    {
        moveScript.trigger = true;                              //when Fence trigger is set off, change boolean value trigger inside of AutomateMove script to true
    }

}