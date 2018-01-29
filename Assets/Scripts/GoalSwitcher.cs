using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalSwitcher : MonoBehaviour {
    
    private Goal goal_script;
    private int goal_index = 0;
    public List<GameObject> ordered_goals;
    private KinematicArrive kinematicArrive;
    private DynoArrive dynoArrive;

    void Start()
    {
        goal_script = GetComponent<Goal>();
        goal_script.setGoal(ordered_goals[goal_index]);
        kinematicArrive = GetComponent<KinematicArrive>();
        dynoArrive = GetComponent<DynoArrive>();
    }
	
	// Update is called once per frame
	void Update () {
        if (dynoArrive)
        {
            if (dynoArrive.HasArrived())
            {
                goal_index++;
                goal_index %= ordered_goals.Count;
                goal_script.setGoal(ordered_goals[goal_index]);
            }
        }
        else
        {
            if (kinematicArrive.HasArrived())
            {
                goal_index++;
                goal_index %= ordered_goals.Count;
                goal_script.setGoal(ordered_goals[goal_index]);
            }
        }
	}
}
