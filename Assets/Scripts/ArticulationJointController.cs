using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public enum RotationDirection { None = 0, Positive = 1, Negative = -1};

public class ArticulationJointController : MonoBehaviour
{
    public RotationDirection rotationState = RotationDirection.None;
    public float speed = 0.5f;

    private ArticulationBody articulation;
    bool joint_collision_flag = false;

    // LIFE CYCLE
    void Start()
    {
        articulation = GetComponent<ArticulationBody>();
    }

    void FixedUpdate()
    {
        if (rotationState != RotationDirection.None)
        {
            float rotationChange = (float)rotationState * speed * Time.fixedDeltaTime;
            float rotationGoal = CurrentPrimaryAxisRotation() + rotationChange;
            RotateTo(rotationGoal);
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Collided With Itself");
        joint_collision_flag = true;

    }

    public bool GetCollision()
    {
        return joint_collision_flag;
    }

    public void ResetCollision()
    {
        joint_collision_flag = false;
    }


    // MOVEMENT HELPERS

    float CurrentPrimaryAxisRotation()
    {
        float currentRotationRads = articulation.jointPosition[0];
        float currentRotation = Mathf.Rad2Deg * currentRotationRads;
        return currentRotation;
    }

    void RotateTo(float primaryAxisRotation)
    {
        var drive = articulation.xDrive;
        drive.target = primaryAxisRotation;
        articulation.xDrive = drive;
    }

    public void Reset()
    {
        articulation.jointPosition = new ArticulationReducedSpace(0f, 0f, 0f);
        articulation.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
        articulation.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
        articulation.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);
        RotateTo(0f);
    }

    public float CurrentRot()
    {
        return CurrentPrimaryAxisRotation();
    }

    public void ForceRotate(float primaryAxisRotation)
    {
        var drive = articulation.xDrive;
        drive.target = primaryAxisRotation;
        articulation.xDrive = drive;
    }

}
