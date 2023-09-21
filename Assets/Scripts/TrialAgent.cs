using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using Random = UnityEngine.Random;


public class TrialAgent : Agent
{
    public GameObject robot;
    public GameObject targetObjectParent;
    public GameObject spawnObject;
    public float maxObjects;
    public GameObject gripper;
    public GameObject gripperArea;
    public GameObject table;
    public float rotationSpeed;
    bool collision_detector;
    float prevDist;
    Vector3 initialpos;
    Vector3 newPos;
    Quaternion initialrot;
    float episodetime;
    int epNum;
    //Vector3 LastGripperPos;
    [HideInInspector] public bool left_detect = false;
    [HideInInspector] public bool right_detect = false;
    [HideInInspector] public bool left_drop = false;
    [HideInInspector] public bool right_drop = false;
    Transform OriginalCubeParent;
    float pointRecord;
    float potentialPointRecord;
    float tableCollisionPenalty = -100000f;




    void Start()
    {
        initialpos = targetObjectParent.GetComponent<Transform>().position;
        initialrot = targetObjectParent.GetComponent<Transform>().rotation;
        epNum = 0;
        OriginalCubeParent = targetObjectParent.transform.parent;
        pointRecord = tableCollisionPenalty;
        potentialPointRecord = pointRecord;
    }

    public override void OnEpisodeBegin()
    {
        if (potentialPointRecord > pointRecord)
        {
            pointRecord = potentialPointRecord;
        }
        epNum++;
        if(epNum % 100 == 0)
        {
            Debug.Log("Episode Number " + epNum + "------------------------------------------------- High Score: " + pointRecord);
        }


        SetReward(0f);
        episodetime = Time.time;

        

        float newX, newY, newZ;
        newX = Random.Range(-3.0f, 3.0f);
        newY = Random.Range(0f, 0f);
        newZ = Random.Range(-3.0f, 3.0f);
        newPos = new Vector3(initialpos.x + newX, initialpos.y + newY, initialpos.z + newZ);
        //Vector3 newPos = initialpos;

        RobotController robotController = robot.GetComponent<RobotController>();
        OpenCloseGripper openCloseGripper = gripper.GetComponent<OpenCloseGripper>();

        table.GetComponent<CollisionDetector>().collision_flag = false;

        robotController.ResetJoints();
        openCloseGripper.resetAnimation();
        left_detect = false;
        right_detect = false;
        left_drop = false;
        right_drop = false;
        targetObjectParent.transform.parent = OriginalCubeParent;
        targetObjectParent.GetComponent<Rigidbody>().useGravity = true;
        targetObjectParent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        targetObjectParent.GetComponent<Transform>().position = newPos;
        targetObjectParent.GetComponent<Transform>().rotation = initialrot;
        targetObjectParent.GetComponent<Rigidbody>().velocity = Vector3.zero;


        
        for (int i = 0; i < robotController.joints.Length; i++)
        {
            float inputVal = Random.Range(-60f, 60f);
            if (Mathf.Abs(inputVal) > 0)
            {
                robotController.ForceRotateJoint(i, inputVal);
            }
        }
        robotController.StopAllJointRotations();

        //LastGripperPos = gripper.transform.position;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        potentialPointRecord = GetCumulativeReward();
        RobotController robotController = robot.GetComponent<RobotController>();
        OpenCloseGripper openCloseGripper = gripper.GetComponent<OpenCloseGripper>();
        var continuousActions = actionBuffers.ContinuousActions;

        //resets cube as it sometimes flies off at random
        //targetObjectParent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //targetObjectParent.GetComponent<Transform>().position = newPos;
        //targetObjectParent.GetComponent<Transform>().rotation = initialrot;
        //targetObjectParent.GetComponent<Rigidbody>().velocity = Vector3.zero;


        // read all action output values
        for (int i = 0; i < robotController.joints.Length; i++)
        {
            //Debug.Log(i + ":" + continuousActions[i]);
            float inputVal = rotationSpeed * Mathf.Clamp(continuousActions[i], -1f, 1f);
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

        float inputGripVal = Mathf.Clamp(continuousActions[robotController.joints.Length], -1f, 1f);
        openCloseGripper.updateMovement(inputGripVal);




        // apply reward for each case
        // case 4: hasn't done anything; reward = -0.25
        //AddReward(-0.025f);

        // case 1: reached a cube; reward = 5
        Transform nextTarget = targetObjectParent.GetComponent<Transform>();

        float distanceToNextTarget = Vector3.Distance(gripperArea.transform.position, nextTarget.position);
        AddReward(prevDist - distanceToNextTarget);

        if(prevDist > distanceToNextTarget)
        {
            //AddReward(1f);
        }
        else
        {
            AddReward(-(distanceToNextTarget * 3.0f));
        }

        prevDist = distanceToNextTarget;
        //Debug.Log("Current Distance: " + distanceToNextTarget);

       

        if (distanceToNextTarget < 0.2)
        {
            AddReward(200f);
            Debug.Log("Cube In Range");
            Debug.Log("Points: " + GetCumulativeReward());
        }

        if (targetObjectParent.transform.parent == gripper.transform)
        {
            AddReward(10000f);
            Debug.Log("!----------! Cube Acquired !----------!");
            Debug.Log("Points: " + GetCumulativeReward());
            EndEpisode();
        }

        // case 2: hit the table; reward = -100*collision_speed
        collision_detector = table.GetComponent<CollisionDetector>().collision_flag;
        if (collision_detector)
        {
            //Vector3 collisionVelocity = (gripper.transform.position - LastGripperPos) / (episodetime - Time.time);
            //float collisionSpeed = Mathf.Sqrt(Mathf.Pow(collisionVelocity.x, 2) + Mathf.Pow(collisionVelocity.y, 2) + Mathf.Pow(collisionVelocity.z, 2));
            //AddReward(-(100f) * collisionSpeed);
            AddReward(tableCollisionPenalty);
            //Debug.Log("Collided. Points: " + GetCumulativeReward() + ". Collision Speed: " + collisionSpeed);
            table.GetComponent<CollisionDetector>().CollisionReset();
            EndEpisode();
        }

        //episodetime = Time.time;
        //LastGripperPos = gripper.transform.position;

        // case 3: hit itself; reward = -200
        if(robotController.CheckCollisions())
        {
            //Debug.Log("Collision Detected");
            AddReward(-(200f));
            robotController.ResetCollisions();
        }


        //if((GetCumulativeReward() < (pointRecord - 300)))
        //{
            //Debug.Log("Negative points exceeds limit. Restarting.");
            //EndEpisode();
        //}
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


    public override void CollectObservations(VectorSensor sensor)
    {
        RobotController robotController = robot.GetComponent<RobotController>();
        for (int i = 0; i < robotController.joints.Length; i++)
        {

            if (robotController.GetJointRotation(i) % 360 > 180)
            {
                sensor.AddObservation(-(360 - Mathf.Abs(robotController.GetJointRotation(i) % 360)));
            }
            else if (robotController.GetJointRotation(i) % 360 < -180)
            {
                sensor.AddObservation(360 - Mathf.Abs(robotController.GetJointRotation(i) % 360));
            }
            else
            {
                sensor.AddObservation(robotController.GetJointRotation(i) % 360);
            }
        }
        

        sensor.AddObservation(gripperArea.transform.position);
        

        Transform nextTarget = targetObjectParent.GetComponent<Transform>();

        sensor.AddObservation(nextTarget.position);


        //sensor.AddObservation(Vector3.Distance(gripper.transform.position, nextTarget.position));

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
        newRot.Set(0f, rotY, 0f, 1);

        // instantiate
        GameObject newObj = Instantiate(prefab, newPos, newRot, targetObjectParent.transform);
        //Rigidbody rb = newObj.AddComponent(typeof(Rigidbody)) as Rigidbody;


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

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Shoulder");
        continuousActionsOut[1] = Input.GetAxis("Upper_Arm");
        continuousActionsOut[2] = Input.GetAxis("Lower_Arm");
        continuousActionsOut[3] = Input.GetAxis("Upper_Wrist");
        continuousActionsOut[4] = Input.GetAxis("Lower_Wrist");
        continuousActionsOut[5] = Input.GetAxis("Tool_Flange");
        continuousActionsOut[6] = Input.GetAxis("Gripper");
    }
}
