using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CustomLogWriter))]
public class RandomGraphGenerator : MonoBehaviour {

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

    private List<Node> nodeList;
    private List<List<int>> connectionMatrix;

    [SerializeField]
    private int numberOfNodes;
    [SerializeField]
    private float minRandom;
    [SerializeField]
    private float maxRandom;

    // Use this for initialization
    void Start () {
        
        //Initialize node list
        nodeList = new List<Node>(numberOfNodes);

        //Create nodes at random position
        for (int i = 0; i < numberOfNodes; i++)
        {
            Node node = new Node();
            node.id = i;
            node.position = new Vector2(Random.Range(minRandom, maxRandom), Random.Range(minRandom, maxRandom));
            nodeList.Add(node);
        }

        //Initialize connection matrix to all 0s
        connectionMatrix = new List<List<int>>(numberOfNodes);
        for (int i = 0; i < numberOfNodes; i++)
        {
            connectionMatrix.Add(new List<int>(numberOfNodes));
            for (int j = 0; j < numberOfNodes; j++)
            {
                connectionMatrix[i].Add(0);
            }
        }

        CustomLogWriter logWriter = GetComponent<CustomLogWriter>();
        logWriter.filePath = "Random_Graph_Nodes_" + numberOfNodes;

        int totalConnections = 0;
        //Assign connections
        for (int i=0;i< numberOfNodes; i++)
        {            
            int maxConnections = Random.Range(1, 5);
            int connectionCount = 0;
            
            for(int j = 0; j < numberOfNodes; j++)
            {
                //Avoid self connection
                if (i != j)
                {
                    //If already connected increment connection count
                    if (connectionMatrix[i][j] == 1)
                    {
                        connectionCount++;
                        break;
                    }
                    else
                    {
                        //Randomly assign connections
                        int random = Random.Range(0, numberOfNodes);
                        if (random >= numberOfNodes * 0.99)
                        {
                            connectionMatrix[i][j] = 1;
                        }
                    }

                    //If connection was created
                    if (connectionMatrix[i][j] == 1)
                    {
                        connectionCount++;
                        
                        //Create the opposite connection
                        connectionMatrix[j][i] = 1;
                        
                        //Print out connection to file
                        string logString = i.ToString() + "," + j.ToString() + "," + Vector2.Distance(nodeList[i].position, nodeList[j].position);
                        logWriter.Write(logString);
                    }
                }

                //If we've reached max number of connections we quit
                if(connectionCount >= maxConnections)
                {
                    break;
                }
            }
            totalConnections += connectionCount;
        }
        print("Done! "+ totalConnections);
    }
}
