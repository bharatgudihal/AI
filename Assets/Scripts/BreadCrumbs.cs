using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreadCrumbs : MonoBehaviour {

    [SerializeField]
    private Material crumbMaterial;
    [SerializeField]
    private float interval;
    [SerializeField]
    private int maxCrumbs;

    private float localTime;
    private int currentCrumbIndex;
    private List<GameObject> breadCrumbList;

	// Use this for initialization
	void Start () {
        currentCrumbIndex = 0;
        breadCrumbList = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
        localTime += Time.deltaTime;
        if(localTime > interval)
        {
            PlaceBreadCrumb();
            localTime = 0.0f;
        }
	}

    private void PlaceBreadCrumb()
    {
        GameObject breadCrumb = GetNextCrumb();
        breadCrumb.transform.position = transform.position;
    }
    
    private GameObject GetNextCrumb()
    {
        GameObject newCrumb = null;
        if (currentCrumbIndex < breadCrumbList.Count)
        {
            newCrumb = breadCrumbList[currentCrumbIndex];
        }
        else
        {
            newCrumb = CreateCrumb();
            breadCrumbList.Add(newCrumb);
        }
        currentCrumbIndex++;
        currentCrumbIndex %= maxCrumbs;
        return newCrumb;
    }

    private GameObject CreateCrumb()
    {
        GameObject newCrumb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        newCrumb.GetComponent<Renderer>().material = crumbMaterial;
        newCrumb.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        return newCrumb;
    }
}
