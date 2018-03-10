using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    static private int tileSize = 2;

    protected struct Node
    {
        public int id;
        public Vector3 position;
    }

    protected class Connection
    {
        public Node from;
        public Node to;
        public float cost = tileSize;
    }

    protected class NodeRecord
    {        
        public Node node;
        public Connection connection;
        public float costSoFar;
        public float totalEstimatedCost;
    }

    protected List<Node> nodeList;
    protected List<List<byte>> connectionMatrix;    

    // Use this for initialization
    void Start() {
        CreateLevelGraph();
    }

    protected void CreateLevelGraph()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Floor");
        nodeList = new List<Node>(tiles.Length);
        connectionMatrix = new List<List<byte>>(tiles.Length);
        for (int i = 0; i < tiles.Length; i++)
        {
            Node node = new Node();
            node.id = i;
            node.position = tiles[i].transform.position;
            nodeList.Add(node);
        }

        for (int i = 0; i < nodeList.Count; i++)
        {
            connectionMatrix.Add(new List<byte>(nodeList.Count));
            for (int j = 0; j < nodeList.Count; j++)
            {
                //Self connection
                if (i == j)
                {
                    connectionMatrix[i].Add(1);
                }
                else
                {
                    connectionMatrix[i].Add(CheckConnection(nodeList[i], nodeList[j]) ? (byte)1 : (byte)0);
                }
            }
        }
    }

    private bool CheckConnection(Node node1, Node node2)
    {
        float manhattanDistance = Mathf.Abs(node1.position.x - node2.position.x) + Mathf.Abs(node1.position.z - node2.position.z);
        return manhattanDistance <= tileSize;
    }

    private Node GetNode(Vector3 position)
    {
        Node node = new Node();
        node.id = -1;
        for(int i = 0; i < nodeList.Count; i++)
        {
            if(nodeList[i].position == position)
            {
                node = nodeList[i];
                break;
            }
        }
        return node;
    }

    private Node GetClosestNode(Vector3 source)
    {
        //Round to nearest tile position
        float xPos = Mathf.RoundToInt(source.x / tileSize) * tileSize;
        float zPos = Mathf.RoundToInt(source.z / tileSize) * tileSize;

        Vector3 localizedPosition = new Vector3(xPos, 0, zPos);
        return GetNode(localizedPosition);
    }

    public List<Vector3> GetShortestPath(Vector3 start, Vector3 destination)
    {
        List<Vector3> path = new List<Vector3>();
        Node startNode = GetClosestNode(start);
        Node destinationNode = GetClosestNode(destination);
        //If they are valid nodes
        if(startNode.id != -1 && destinationNode.id != -1)
        {
            //Start path finding
            List<NodeRecord> openList = new List<NodeRecord>();
            List<NodeRecord> closedList = new List<NodeRecord>();

            //Initialize starting node
            NodeRecord startRecord = new NodeRecord();
            startRecord.node = startNode;
            startRecord.connection = null;
            startRecord.costSoFar = 0;
            startRecord.totalEstimatedCost = 0;
            openList.Add(startRecord);
            NodeRecord currentRecord = new NodeRecord();

            while (openList.Count > 0)
            {
                currentRecord = GetRecordWithLowestCost(openList);
                openList.Remove(currentRecord);
                closedList.Add(currentRecord);                
                if(currentRecord.node.id == destinationNode.id)
                {
                    break;
                }

                List<Connection> connections = GetConnections(currentRecord.node);
                for(int i = 0; i < connections.Count; i++)
                {
                    Connection connection = connections[i];
                    Node toNode = connection.to;
                    float toNodeCost = currentRecord.costSoFar + connection.cost;
                    float toNodeHeuristic = 0;
                    NodeRecord toNodeRecord = new NodeRecord();
                    //Check if the tile is already in the closed list, skip it
                    if (ListContains(closedList, toNode))
                    {
                        continue;
                    }
                    else
                    {
                        //Create new record
                        toNodeRecord.node = toNode;
                        toNodeHeuristic = GetHeuristic(toNodeRecord.node, destinationNode);
                    }
                    //Update record values
                    toNodeRecord.costSoFar = toNodeCost;
                    toNodeRecord.connection = connection;
                    toNodeRecord.totalEstimatedCost = toNodeCost + toNodeHeuristic;

                    //Add to the open list if it's not there
                    if (!ListContains(openList, toNode))
                    {
                        openList.Add(toNodeRecord);
                    }
                }
            }

            if(currentRecord.node.id == destinationNode.id)
            {
                while (currentRecord.node.id != startRecord.node.id)
                {
                    path.Add(currentRecord.node.position);
                    currentRecord = GetRecordFromList(closedList, currentRecord.connection.from);
                }

                //Reverse path
                path.Reverse();
            }
        }
        return path;
    }

    private NodeRecord GetRecordFromList(List<NodeRecord> closedList, Node node)
    {
        NodeRecord record = new NodeRecord();
        for (int i = 0; i < closedList.Count; i++)
        {
            if (closedList[i].node.id == node.id)
            {
                record = closedList[i];
                break;
            }
        }
        return record;
    }

    private float GetHeuristic(Node node, Node destinationNode)
    {
        return Mathf.Abs(node.position.x - destinationNode.position.x) + Mathf.Abs(node.position.z - destinationNode.position.z);
    }

    private bool ListContains(List<NodeRecord> closedList, Node fromNode)
    {
        bool result = false;
        for(int i = 0; i < closedList.Count; i++)
        {
            if(closedList[i].node.id == fromNode.id)
            {
                result = true;
                break;
            }
        }
        return result;
    }

    private List<Connection> GetConnections(Node node)
    {
        List<Connection> connections = new List<Connection>();
        List<byte> connectionsArray = connectionMatrix[node.id];
        for(int i = 0; i < connectionsArray.Count; i++)
        {
            if(i != node.id && connectionsArray[i] == 1)
            {
                Connection connection = new Connection();
                connection.from = node;
                connection.to = nodeList[i];
                connections.Add(connection);
            }
        }
        return connections;
    }

    private NodeRecord GetRecordWithLowestCost(List<NodeRecord> openList)
    {
        float lowestCost = Mathf.Infinity;
        NodeRecord lowest = new NodeRecord();
        for(int i = 0; i < openList.Count; i++)
        {
            if(lowestCost > openList[i].totalEstimatedCost)
            {
                lowestCost = openList[i].totalEstimatedCost;
                lowest = openList[i];
            }
        }
        return lowest;
    }
}
