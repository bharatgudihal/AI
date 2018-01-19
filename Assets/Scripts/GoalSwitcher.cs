using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalSwitcher : MonoBehaviour {

    public float goalSwitchTime = 2f;
    private Goal goal_script;
    private int goal_index = 0;
    public List<GameObject> ordered_goals;
    private float time_since_goal_switch = 0f;
	// Use this for initialization

    void Start()
    {
        goal_script = GetComponent<Goal>();
        goal_script.setGoal(ordered_goals[goal_index]);
    }
	
	// Update is called once per frame
	void Update () {
        time_since_goal_switch += Time.deltaTime;

        if (time_since_goal_switch > goalSwitchTime)
        {
            goal_index = Random.Range(0, ordered_goals.Count);
            goal_script.setGoal(ordered_goals[goal_index]);
            time_since_goal_switch = 0f;
        }
	}
}
