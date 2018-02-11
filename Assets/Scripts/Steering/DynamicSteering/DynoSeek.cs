using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynoSeek : MonoBehaviour {

    private SteeringParams sp;
    private Goal goalObject;
    private Transform goal;
    private DynoSteering steering;

    [SerializeField]
    private float goalRadius;

    // Use this for initialization
    void Start () {
        sp = GetComponent<SteeringParams>();
        goalObject = GetComponent<Goal>();
    }
	
	// Update is called once per frame
	public DynoSteering getSteering() {
        steering = new DynoSteering();

        goal = goalObject.getGoal();
        steering.force = goal.position - transform.position;
        steering.force.Normalize();
        steering.force = steering.force * sp.MAXACCEL;
        steering.torque = 0f;

        return steering;
	}

    public bool HasArrived()
    {
        Vector3 targetPosition = goalObject.getGoal().position;
        targetPosition.y = 0;
        Vector3 startPosition = transform.position;
        startPosition.y = 0;
        return (targetPosition - startPosition).magnitude < goalRadius;
    }
}
