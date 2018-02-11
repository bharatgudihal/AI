using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {

    [SerializeField]
    private bool recordLogs;

    [SerializeField]
    private Material selected;

    [SerializeField]
    private Material defaultMaterial;

    private Kinematic char_RigidBody;
    private KinematicSteering ks;
    private DynoSteering ds;

    private KinematicSteeringOutput kso;
    private DynoArrive arrive;
    private DynoSeek seek;
    private DynoAlign align;

    private DynoSteering ds_force;
    private DynoSteering ds_torque;
    
    private CustomLogWriter logWriter;
    private List<Vector3> path;
    private int currentGoal;
    private bool isLastGoal;
    private GameObject goalObject;
    private Goal goal;
    private LevelManager levelManager;
    private GameObject finalGoal;

    // Use this for initialization
    void Awake()
    {
        char_RigidBody = GetComponent<Kinematic>();
        arrive = GetComponent<DynoArrive>();
        align = GetComponent<DynoAlign>();
        seek = GetComponent<DynoSeek>();
        goal = GetComponent<Goal>();
        levelManager = GetComponent<LevelManager>();

        if (recordLogs)
        {
            logWriter = GetComponent<CustomLogWriter>();
        }

        goalObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goalObject.transform.position = transform.position;
        goal.setGoal(goalObject);

        currentGoal = 0;
        path = new List<Vector3>();
        isLastGoal = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Check for click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
            {
                //Get Path
                Vector3 destination = hit.collider.transform.position;
                Vector3 start = transform.position;
                path = levelManager.GetShortestPath(start, destination);
                currentGoal = 0;
                isLastGoal = false;

                if (finalGoal)
                {
                    finalGoal.GetComponent<Renderer>().material = defaultMaterial;
                }

                finalGoal = hit.collider.gameObject;
                finalGoal.GetComponent<Renderer>().material = selected;
            }
        }

        //Update goal positions
        if (currentGoal < path.Count)
        {
            if (seek.HasArrived())
            {
                goalObject.transform.position = path[currentGoal];
                goal.setGoal(goalObject);
                currentGoal++;
                if (currentGoal == path.Count)
                {
                    isLastGoal = true;
                }
            }
        }

        // Decide on behavior
        if (isLastGoal) {
            ds_force = arrive.getSteering();
        }
        else
        {
            ds_force = seek.getSteering();
        }
        ds_torque = align.getSteering();

        ds = new DynoSteering();
        ds.force = ds_force.force;
        ds.torque = ds_torque.torque;

        // Update Kinematic Steering
        kso = char_RigidBody.updateSteering(ds, Time.deltaTime);
        transform.position = new Vector3(kso.position.x, transform.position.y, kso.position.z);
        transform.rotation = Quaternion.Euler(0f, kso.orientation * Mathf.Rad2Deg, 0f);

        if (recordLogs && logWriter && logWriter.enabled)
        {
            logWriter.Write(char_RigidBody.getVelocity().magnitude.ToString());
        }
    }    
}
