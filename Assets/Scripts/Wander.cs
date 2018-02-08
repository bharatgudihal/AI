using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour {

    [SerializeField]
    private float wanderOffset = 5.0f;
    [SerializeField]
    private float wanderRadius = 1.0f;
    [SerializeField]
    private float wanderRate = 1.0f;
    [SerializeField]
    private bool showGoalObject;
    [SerializeField]
    private float wallCheckDistance = 3.0f;
    [SerializeField]
    private List<Transform> safePoints;

    private Goal goal;
    private GameObject goalObject;
    private float orientation;
    Vector3 centerPoint;
    private Kinematic char_RigidBody;
    private DynoArrive arrive;
    private DynoAlign align;
    private DynoSteering ds_force;
    private DynoSteering ds;
    private KinematicSteeringOutput kso;
    private bool isWandering;
    private bool turnLeft = false;
    private bool turnRight = false;

    // Use this for initialization
    private void Awake()
    {
        goal = GetComponent<Goal>();
        orientation = 0.0f;
        goalObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goalObject.transform.position = transform.position;
        goalObject.SetActive(showGoalObject);
        if (goal)
        {
            goal.setGoal(goalObject);
        }
    }

    void Start () {
        char_RigidBody = GetComponent<Kinematic>();
        arrive = GetComponent<DynoArrive>();
        align = GetComponent<DynoAlign>();        
        isWandering = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (isWandering)
        {
            CheckForWallAndTakeDecision();
        }

        if (isWandering)
        {
            GetWanderGoal();
        }
        else if(arrive.HasArrived())
        {
            isWandering = true;
        }

        ds_force = arrive.getSteering();

        ds = align.getSteering();
        ds.force = ds_force.force;

        // Update Kinematic Steering
        kso = char_RigidBody.updateSteering(ds, Time.deltaTime);
        //Debug.Log(kso.position);
        transform.position = new Vector3(kso.position.x, transform.position.y, kso.position.z);
        float rotation = kso.orientation * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    private void CheckForWallAndTakeDecision()
    {
        GameObject wall;
        turnLeft = false;
        turnRight = false;
        if (CheckForWall(out wall))
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                Vector3 wallDirection = (transform.position - wall.transform.position).normalized;
                bool xDirection = Vector3.Dot(transform.right, wall.transform.right) > 0;
                bool zDirection = Vector3.Dot(wallDirection, wall.transform.forward) > 0;
                if (zDirection)
                {
                    if (xDirection)
                    {
                        turnLeft = true;
                    }
                    else
                    {
                        turnRight = true;
                    }
                }
                else
                {
                    if (xDirection)
                    {
                        turnRight = true;
                    }
                    else
                    {
                        turnLeft = true;
                    }
                }
            }
            else
            {
                //Find closest safe point
                float minDistance = Mathf.Infinity;
                Transform closestSafePoint = goalObject.transform;
                foreach (Transform safePoint in safePoints)
                {
                    float distance = Vector3.Distance(safePoint.position, transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestSafePoint = safePoint;
                    }
                }
                goalObject.transform.position = closestSafePoint.position;
                goal.setGoal(goalObject);
                isWandering = false;
            }
        }
    }

    private bool CheckForWall(out GameObject wallPosition)
    {
        wallPosition = null;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")))
        {
            wallPosition = hit.transform.gameObject;
            return true;
        }
        return false;
    }

    private void GetWanderGoal()
    {
        centerPoint = transform.position + transform.right * wanderOffset;
        float offset = UnityEngine.Random.Range(-1.0f, 1.0f) * wanderRate * Time.deltaTime;
        if (turnRight && offset > 0)
        {
            offset = 0.0f;
        }
        else if (turnLeft && offset < 0)
        {
            offset = 0.0f;
        }
        orientation += offset;
        //print(orientation);        
        Vector3 wanderTarget = transform.position;
        //x' = x + r * cos()
        wanderTarget.x = centerPoint.x + wanderRadius * Mathf.Cos(orientation);
        //z' = z + r * sin()
        wanderTarget.z = centerPoint.z + wanderRadius * Mathf.Sin(orientation);
        goalObject.transform.position = wanderTarget;
        goal.setGoal(goalObject);
    }
}
