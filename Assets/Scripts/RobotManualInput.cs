using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotManualInput : MonoBehaviour
{
    public GameObject robot;
    //public GameObject gripper;

    void Update()
    {
        RobotController robotController = robot.GetComponent<RobotController>();
        //RobotController gripperController = gripper.GetComponent<RobotController>();

        for (int i = 0; i < robotController.joints.Length; i++)
        {
            float inputVal = Input.GetAxis(robotController.joints[i].inputAxis);
            if (Mathf.Abs(inputVal) > 0)
            {
                RotationDirection direction = GetRotationDirection(inputVal);
                robotController.RotateJoint(i, direction);
                //return;
            }
            else
            {
                robotController.StopJointRotation(i);
            }
        }
        //robotController.StopAllJointRotations();

        /*
        for (int i = 0; i < gripperController.joints.Length; i++)
        {
            float inputVal = Input.GetAxis(gripperController.joints[i].inputAxis);
            if (Mathf.Abs(inputVal) > 0)
            {
                RotationDirection direction = GetRotationDirection(inputVal);
                gripperController.RotateJoint(i, direction);
                return;
            }
        }
        gripperController.StopAllJointRotations();
        */
    }

    // HELPERS

    static RotationDirection GetRotationDirection(float inputVal)
    {
        if (inputVal > 0)
        {
            return RotationDirection.Positive;
        }
        else if (inputVal < 0)
        {
            return RotationDirection.Negative;
        }
        else
        {
            return RotationDirection.None;
        }
    }
}
