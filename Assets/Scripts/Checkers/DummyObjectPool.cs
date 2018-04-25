using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyObjectPool : MonoBehaviour {

    public static DummyObjectPool Instance;

    public GameObject dummyPrefab;

    private List<GameObject> aliveDummyObjects;
    private List<GameObject> deadDummyObjects;

    // Use this for initialization
    private void Awake()
    {
        Instance = this;
        aliveDummyObjects = new List<GameObject>();
        deadDummyObjects = new List<GameObject>();
    }

    public GameObject GetPoolObject()
    {
        if(aliveDummyObjects.Count > 0)
        {
            GameObject dummy = aliveDummyObjects[0];
            aliveDummyObjects.RemoveAt(0);
            deadDummyObjects.Add(dummy);
            return dummy;
;        }

        GameObject newDummy = new GameObject();
        deadDummyObjects.Add(newDummy);
        return newDummy;
    }

    public void ResetAll()
    {
        aliveDummyObjects.AddRange(deadDummyObjects);
        deadDummyObjects.Clear();
    }
}
