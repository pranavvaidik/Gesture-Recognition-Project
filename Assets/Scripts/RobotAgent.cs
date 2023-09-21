using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };

public class RobotAgent : Agent
{

    

    [System.Serializable]
    public struct Joint
    {
        public float speed;
        public RotationDirection rDir;
        public ArticulationBody robotPartArticulation;
    }

    public GameObject targetObjectParent;
    // Choose game object you want to keep spawning
    public GameObject spawnObject;
    public float maxObjects;
    public Joint[] joints;
    public GameObject gripper;
    public GameObject table;
    bool collision_detector;
    float prevDist;

    //public ArticulationBody[] robotPartArticulations;
    //ArticulationBody[] aBodies;




    // Start is called before the first frame update
    void Start()
    {
        Transform[] allChildren = targetObjectParent.GetComponentsInChildren<Transform>();
        


        // Get list of all articulation bodies for each robot part
        //foreach (GameObject rPart in robotParts)
        //{

        //}


    }

    public override void OnEpisodeBegin()
    {
        //// reset the table
        // Delete the cubes that reamined from last episode // this doesn't work for now
        

        Transform[] allChildren = targetObjectParent.GetComponentsInChildren<Transform>();

        if (allChildren.Length > 0)
        {
            foreach (Transform child in targetObjectParent.transform)
            {
                //if (child.position.y < 0)
                //{

                GameObject.Destroy(child.gameObject);
                //}
            }
        }


        table.GetComponent<CollisionDetector>().collision_flag = false;

        foreach (Joint joint in joints)
        {
            var drive = joint.robotPartArticulation.xDrive;//.target = 0;
            drive.target = Random.Range(-0.3f, 0.3f);//0f;
            drive.targetVelocity = 0f;

            joint.robotPartArticulation.xDrive = drive;
        }


        // randomly spawn cubes
        float numObjects = Random.Range(1, maxObjects);
        for (int i = 0; i < maxObjects; i++)
        {
            SpawnRandom();
        }

        // Keep the position of the robot unchanged from the previous episode end.


        


        // If robot reaches the cube, or hits itself, or hit's the table 
        // robot starts from default position
        // Cube is initiated randomly in space










    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Debug.Log("Test");
        float rotationChange;
        // read all action output values
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].speed = actionBuffers.ContinuousActions[i]; // add an upper limit on this later
            
            if (actionBuffers.DiscreteActions[i] == 0)
            {
                joints[i].rDir = RotationDirection.None;
            }
            else if (actionBuffers.DiscreteActions[i] == 1)
            {
                joints[i].rDir = RotationDirection.Positive;
            }
            else if (actionBuffers.DiscreteActions[i] == 2)
            {
                joints[i].rDir = RotationDirection.Negative;
            }


            // apply all motion
            rotationChange = (float)joints[i].rDir * joints[i].speed * Time.fixedDeltaTime;
            float rotationGoal = joints[i].robotPartArticulation.jointPosition[0]*Mathf.Rad2Deg + rotationChange;
            
            var drive = joints[i].robotPartArticulation.xDrive;
            drive.target = rotationGoal;
            joints[i].robotPartArticulation.xDrive = drive;


            //RotateTo(rotationGoal);
        }

        // apply all motion


        // stop all motion


        // apply reward for each case
        // case 4: hasn't done anything; reward = -0.25
        AddReward(-0.025f);

        // case 1: reached a cube; reward = 5
        Transform[] allTargets = targetObjectParent.GetComponentsInChildren<Transform>();
        Transform nextTarget = allTargets[0];
        
        float distanceToNextTarget = Vector3.Distance(gripper.transform.position, nextTarget.position);
        AddReward(prevDist-distanceToNextTarget);
        prevDist = distanceToNextTarget;

        //AddReward(-distanceToNextTarget*0.1f);

        if (distanceToNextTarget < 0.3)
        {
            AddReward(1f);

            int len = allTargets.Length;
            Destroy(nextTarget.gameObject);

            if (len - 1 == 0)
            {
                AddReward(1f);
                EndEpisode();
            }

        }

        // end episode if all 

        // case 2: hit the table; reward = -100*collision_speed
        collision_detector = table.GetComponent<CollisionDetector>().collision_flag;
        if (collision_detector)
        {
            AddReward(-1f);
            EndEpisode();
        }

        // case 3: hit itself; reward = -100



    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision Detected");
        GameObject collidedObject = collision.gameObject;


        if (collision.gameObject.name == "Table")
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    // Helpers

    /*
    public void StopAllJointRotations()
    {
        for (int i = 0; i < joints.Length; i++)
        {
            GameObject robotPart = joints[i].robotPart;
            UpdateRotationState(RotationDirection.None, robotPart);
        }
    }
    */

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
        foreach (Joint j in joints)
        {
            sensor.AddObservation(j.robotPartArticulation.xDrive.target);
            sensor.AddObservation(j.robotPartArticulation.xDrive.targetVelocity);
        }
        */

        sensor.AddObservation(gripper.transform.position);


        // Delete the cubes that fell off the table // this doesn't work for now
        Transform[] allChildren = targetObjectParent.GetComponentsInChildren<Transform>();

        sensor.AddObservation(allChildren[0].position);

    }

    // object spawn helpers
    void SpawnRandom()
    {
        // choose prefab
        GameObject prefab = spawnObject;

        // generate random position
        float newX, newY, newZ;
        newX = Random.Range(-2.0f, 2.0f);
        newY = Random.Range(0f, 4.0f);
        newZ = Random.Range(-2.0f, 2.0f);
        //newZ = Random.Range(0.0f, 15.0f);

        Vector3 newPos = new Vector3(newX, newY, newZ);
        newPos = targetObjectParent.transform.TransformPoint(newPos);
        // rotation
        float rotX, rotY, rotZ;
        rotX = Random.Range(-1.0f, 1.0f);
        rotY = Random.Range(-1.0f, 1.0f);
        rotZ = Random.Range(-1.0f, 1.0f);

        Quaternion newRot = new Quaternion();//Random.rotation;
        newRot.Set(rotX, rotY, rotZ, 1);

        // instantiate
        GameObject newObj = Instantiate(prefab, newPos, newRot, targetObjectParent.transform);
        Rigidbody rb = newObj.AddComponent(typeof(Rigidbody)) as Rigidbody;


        // color

        float newR, newG, newB;
        newR = Random.Range(0.0f, 1.0f);
        newG = Random.Range(0.0f, 1.0f);
        newB = Random.Range(0.0f, 1.0f);

        Color newColor = new Color(newR, newG, newB);

        //MeshRenderer childRenderer = childTransform.GetComponent<MeshRenderer>();
        MeshRenderer renderer = newObj.GetComponent<MeshRenderer>();



        Material newMaterial = new Material(Shader.Find("Standard"));

        newMaterial.color = newColor;
        renderer.material = newMaterial;


        //}



    }

    

}
