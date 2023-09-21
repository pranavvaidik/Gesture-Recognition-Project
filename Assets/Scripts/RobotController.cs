using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [System.Serializable]
    public struct Joint
    {
        public string inputAxis;
        public GameObject robotPart;
    }

    public Joint[] joints;
    

    // CONTROL

    public void StopAllJointRotations()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            UpdateRotationState(RotationDirection.None, robotPart);
        }
    }

    public void StopJointRotation(int jointIndex)
    {
        GameObject robotPart = joints[jointIndex].robotPart;
        UpdateRotationState(RotationDirection.None, robotPart);
    }

    public void ResetJoints()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            UpdateRotationState(RotationDirection.None, robotPart);
            ArticulationJointController jointController = robotPart.GetComponent<ArticulationJointController>();
            jointController.Reset();
        }
    }

    public void RotateJoint(int jointIndex, RotationDirection direction)
    {
        //StopAllJointRotations();
        Joint joint = joints[jointIndex];
        UpdateRotationState(direction, joint.robotPart);

    }

    public float GetJointRotation(int jointIndex)
    {
        //StopAllJointRotations();
        Joint joint = joints[jointIndex];
        ArticulationJointController jointController = joint.robotPart.GetComponent<ArticulationJointController>();
        return jointController.CurrentRot();

    }

    public void ForceRotateJoint(int jointIndex, float rotation)
    {
        //StopAllJointRotations();
        Joint joint = joints[jointIndex];
        ArticulationJointController jointController = joint.robotPart.GetComponent<ArticulationJointController>();
        jointController.ForceRotate(rotation);
    }

    // HELPERS

    static void UpdateRotationState(RotationDirection direction, GameObject robotPart)
    {
        ArticulationJointController jointController = robotPart.GetComponent<ArticulationJointController>();
        jointController.rotationState = direction;
    }

    public bool CheckCollisions()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            ArticulationJointController jointController = robotPart.GetComponent<ArticulationJointController>();
            if(jointController.GetCollision())
            {
                //Debug.Log("Collision Detected");
                return true;
            }
        }
        return false;
    }

    public void ResetCollisions()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            ArticulationJointController jointController = robotPart.GetComponent<ArticulationJointController>();
            jointController.ResetCollision();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
