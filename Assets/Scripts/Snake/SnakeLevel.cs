using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeLevel : MonoBehaviour {

    struct Node
    {
        public int id;
        public Vector2 position;
    }

    class Connection
    {
        public Node from;
        public Node to;
        public float cost;
    }

    class NodeRecord
    {
        public Node node;
        public Connection connection;
        public float costSoFar;
        public float totalEstimatedCost;
    }

    private List<Node> nodeList;
    private List<List<byte>> connectionMatrix;

    // Use this for initialization
    void Start () {
        GenerateLevel();
	}

    private void GenerateLevel()
    {
        for(int i = 0; i < 32; i++)
        {
            for(int j = 0; j < 32; j++)
            {
                Vector3 position = new Vector3();
                position.x = -16.0f + i;
                position.z = -16.0f + j;
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.position = position;
            }
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
