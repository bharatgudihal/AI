﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynoBehavior : MonoBehaviour {

    private Kinematic char_RigidBody;
    private KinematicSteering ks;
    private DynoSteering ds;

    private KinematicSteeringOutput kso;
    private DynoArrive arrive;
    private DynoAlign align;

    private DynoSteering ds_force;
    private DynoSteering ds_torque;

    [SerializeField]
    private bool recordLogs;
    private CustomLogWriter logWriter;

    // Use this for initialization
    void Start()
    {
        char_RigidBody = GetComponent<Kinematic>();
        arrive = GetComponent<DynoArrive>();
        align = GetComponent<DynoAlign>();
        if (recordLogs)
        {
            logWriter = GetComponent<CustomLogWriter>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Decide on behavior
        ds_force = arrive.getSteering();
        ds_torque = align.getSteering();

        ds = new DynoSteering();
        ds.force = ds_force.force;
        ds.torque = ds_torque.torque;

        // Update Kinematic Steering
        kso = char_RigidBody.updateSteering(ds, Time.deltaTime);
        //Debug.Log(kso.position);
        transform.position = new Vector3(kso.position.x, transform.position.y, kso.position.z);
        transform.rotation = Quaternion.Euler(0f, kso.orientation * Mathf.Rad2Deg, 0f);

        if (recordLogs && logWriter && logWriter.enabled)
        {
            logWriter.Write(char_RigidBody.getVelocity().magnitude.ToString());
        }
    }    
}
