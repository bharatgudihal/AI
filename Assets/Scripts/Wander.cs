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
            GetWanderGoal();
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

    private bool CheckForWall(out Vector3 wallPosition)
    {
        wallPosition = Vector3.zero;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, LayerMask.GetMask("Wall")))
        {
            wallPosition = hit.transform.position;
            //float incidenceAngle = char_RigidBody.getOrientation() * Mathf.Rad2Deg;
            //incidenceAngle = 180 - incidenceAngle;
            //char_RigidBody.setOrientation(incidenceAngle * Mathf.Deg2Rad);
            //transform.rotation = Quaternion.Euler(0, incidenceAngle, 0);
            //isWandering = false;
            return true;
        }
        return false;
    }

    private void GetWanderGoal()
    {
        Vector3 wallPosition;
        float leftWeight = -1.0f;
        float rightWeight = 1.0f;
        if (CheckForWall(out wallPosition))
        {
            Vector3 wallDirection = (wallPosition - transform.position).normalized;
            float weightOffset = Vector3.Dot(wallDirection, transform.right);
            //Do something
        }
        centerPoint = transform.position + transform.right * wanderOffset;       
        orientation += UnityEngine.Random.Range(leftWeight, rightWeight) * wanderRate * Time.deltaTime;
        Vector3 wanderTarget = transform.position;
        //x' = x + r * cos()
        wanderTarget.x = centerPoint.x + wanderRadius * Mathf.Cos(orientation);
        //z' = z + r * sin()
        wanderTarget.z = centerPoint.z + wanderRadius * Mathf.Sin(orientation);
        goalObject.transform.position = wanderTarget;
        if (goal)
        {
            goal.setGoal(goalObject);
        }
    }
}
