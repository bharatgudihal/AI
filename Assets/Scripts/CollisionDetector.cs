using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetector : MonoBehaviour {

    private Kinematic kinematic;
    private Goal goal;

    // Use this for initialization
    void Start () {
        kinematic = GetComponent<Kinematic>();
        goal = GetComponent<Goal>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        float orientation = kinematic.getNewOrientation(goal.getGoal().position - transform.position);
        orientation = 180 - orientation;
        kinematic.setOrientation(orientation);
    }
}
