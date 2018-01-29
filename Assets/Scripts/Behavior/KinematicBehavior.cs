﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KinematicBehavior : MonoBehaviour {

    private Kinematic char_kinematic;
    private KinematicSteering ks;
    private DynoSteering ds;
    private KinematicSteeringOutput kso;
    private KinematicSeek seek;
    private KinematicArrive arrive;

    private KinematicSteering seeking_output;
    private Vector3 new_velocity;
    private DynoAlign dynoAlign;

    [SerializeField]
    private bool recordLogs;
    private CustomLogWriter logWriter;

    // Use this for initialization
    void Start () {
        char_kinematic = GetComponent<Kinematic>();
        seek = GetComponent<KinematicSeek>();
        arrive = GetComponent<KinematicArrive>();
        dynoAlign = GetComponent<DynoAlign>();
        if (recordLogs)
        {
            logWriter = GetComponent<CustomLogWriter>();
        }
    }

    // Update is called once per frame
    void Update () {
        ks = new KinematicSteering();
        ds = new DynoSteering();

        // Decide on behavior
        //seeking_output = seek.updateSteering();
        seeking_output = arrive.getSteering();
        //seeking_output = seek.getSteering();
        char_kinematic.setVelocity(seeking_output.velc);

        // Manually set orientation for now
        if (dynoAlign && dynoAlign.enabled)
        {
            ds = dynoAlign.getSteering();
        }
        else
        {
            float new_orient = char_kinematic.getNewOrientation(seeking_output.velc);
            char_kinematic.setOrientation(new_orient);
            char_kinematic.setRotation(0f);
        }

        // Update Kinematic Steering
        kso = char_kinematic.updateSteering(ds, Time.deltaTime);
        
        transform.position = new Vector3(kso.position.x, transform.position.y, kso.position.z);
        transform.rotation = Quaternion.Euler(0f, kso.orientation * Mathf.Rad2Deg, 0f);

        if (recordLogs && logWriter && logWriter.enabled)
        {
            logWriter.Write(char_kinematic.getVelocity().magnitude.ToString());
        }
    }

    private void kinematicSeekBehavior()
    {
        ds = new DynoSteering();

        // Decide on behavior
        seeking_output = seek.getSteering();
        char_kinematic.setVelocity(seeking_output.velc);
        // Manually set orientation for now
        float new_orient = char_kinematic.getNewOrientation(seeking_output.velc);
        char_kinematic.setOrientation(new_orient);
        char_kinematic.setRotation(0f);

        // Update Kinematic Steering
        kso = char_kinematic.updateSteering(ds, Time.deltaTime);

        transform.position = new Vector3(kso.position.x, transform.position.y, kso.position.z);
        transform.rotation = Quaternion.Euler(0f, kso.orientation * Mathf.Rad2Deg, 0f);
    }
    
}
