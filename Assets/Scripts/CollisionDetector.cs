using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public GameObject robot;
    public bool collision_flag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Cube(Clone)" && collision.gameObject.name != "Cube")
        {
            //Debug.Log("Collision happened!");
            //Debug.Log(collision.gameObject.name);

            collision_flag = true;
        }
        
    }

    public void CollisionReset()
    {
        collision_flag = false;
    }

}
