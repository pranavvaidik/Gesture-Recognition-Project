//using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    private int frameCount = 0;
    //public string camChoice;
    Vector3 startingPos = new Vector3(-13.58f, 13.79f, 110.88f);

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LateUpdate()
    {
        
        //x dir .4, y dir .4, z dir .5
        if (frameCount % 60 == 0)
        {
            transform.localPosition = startingPos;
            float x_pos_change = Random.Range(-.35f, .35f);
            float y_pos_change = Random.Range(-.35f, .35f);
            float z_pos_change = Random.Range(-.35f, .35f);

            transform.localPosition = startingPos + new Vector3(x_pos_change, y_pos_change, z_pos_change);
        }
        frameCount++;
        
    }
}
