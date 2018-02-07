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

    private Goal goal;
    private GameObject goalObject;
    private float orientation;
    Vector3 centerPoint;

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
        
    }
	
	// Update is called once per frame
	void Update () {
        orientation += Random.Range(-1.0f, 1.0f) * wanderRate * Time.deltaTime;
        centerPoint = transform.position + transform.right * wanderOffset;
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
