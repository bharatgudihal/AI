using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeLevel : MonoBehaviour {

    public class Node
    {
        public int id;
        public Vector3 position;
        public bool isBlocked = false;
    }

    public class Connection
    {
        public Node from;
        public Node to;
        public float cost = 1;
    }

    public class NodeRecord
    {
        public Node node;
        public Connection connection;
        public float costSoFar;
        public float totalEstimatedCost;
    }    

    private List<Node> nodeList;
    private List<GameObject> tiles;

    public GameObject tilePrefab;
    public Material normalMaterial;
    public Material pathMaterial;

    // Use this for initialization
    void Awake () {        
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        nodeList = new List<Node>();
        tiles = new List<GameObject>();
        for (int i = 0; i < 32; i++)
        {
            for(int j = 0; j < 32; j++)
            {
                Vector3 position = new Vector3();
                position.x = -16.0f + i;
                position.z = -16.0f + j;
                GameObject tile = Instantiate(tilePrefab);
                tile.transform.position = position;
                tile.transform.parent = transform;
                Node node = new Node();
                node.id = i * 32 + j;
                node.position = tile.transform.position;
                nodeList.Add(node);
                tiles.Add(tile);
            }
        }
    }

    private bool CheckConnection(Node node1, Node node2)
    {
        float manhattanDistance = Mathf.Abs(node1.position.x - node2.position.x) + Mathf.Abs(node1.position.z - node2.position.z);
        return manhattanDistance == 1;
    }

    public List<Vector3> GetShortestPath(Vector2 start, Vector2 destination)
    {
        List<Vector3> path = new List<Vector3>();
        Node startNode = GetNode(start);
        Node destinationNode = GetNode(destination);
        //If they are valid nodes
        if (startNode.id != -1 && destinationNode.id != -1)
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
                if (currentRecord.node.id == destinationNode.id)
                {
                    break;
                }

                List<Connection> connections = GetConnections(currentRecord.node);
                for (int i = 0; i < connections.Count; i++)
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

            if (currentRecord.node.id == destinationNode.id)
            {
                while (currentRecord.node.id != startRecord.node.id)
                {
                    path.Add(currentRecord.node.position);
                    currentRecord = GetRecordFromList(closedList, currentRecord.connection.from);
                }

                //Reverse path
                path.Reverse();

                HighlighPath(path);
            }
        }
        return path;
    }

    private void HighlighPath(List<Vector3> path)
    {
        for(int i = 0; i < tiles.Count; i++)
        {
            tiles[i].GetComponent<Renderer>().material = normalMaterial;
        }

        for(int i = 0; i < path.Count; i++)
        {
            float x = path[i].x + 16.0f;
            float y = path[i].z + 16.0f;
            int index = (int)x * 32 + (int)y;
            GameObject tile = tiles[index];
            tile.GetComponent<Renderer>().material = pathMaterial;
        }
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
        for (int i = 0; i < closedList.Count; i++)
        {
            if (closedList[i].node.id == fromNode.id)
            {
                result = true;
                break;
            }
        }
        return result;
    }

    private void AddConnectionIfPresent(ref Vector2 nodePosition, Node node, ref List<Connection> connections)
    {
        if (IsValidNode(nodePosition))
        {
            if (!IsTileBlocked(nodePosition))
            {
                Connection connection = new Connection();
                connection.from = node;
                connection.to = GetNode(nodePosition);
                connections.Add(connection);
            }
        }
    }

    private List<Connection> GetConnections(Node node)
    {
        List<Connection> connections = new List<Connection>();
        
        //Check up connection
        Vector2 upNode = new Vector2(node.position.x, node.position.z + 1);
        AddConnectionIfPresent(ref upNode, node, ref connections);

        //Check down connection
        Vector2 downNode = new Vector2(node.position.x, node.position.z - 1);
        AddConnectionIfPresent(ref downNode, node, ref connections);

        //Check left connection
        Vector2 leftNode = new Vector2(node.position.x - 1, node.position.z);
        AddConnectionIfPresent(ref leftNode, node, ref connections);

        //Check right connection
        Vector2 rightNode = new Vector2(node.position.x + 1, node.position.z);
        AddConnectionIfPresent(ref rightNode, node, ref connections);

        return connections;
    }

    private NodeRecord GetRecordWithLowestCost(List<NodeRecord> openList)
    {
        float lowestCost = Mathf.Infinity;
        NodeRecord lowest = new NodeRecord();
        for (int i = 0; i < openList.Count; i++)
        {
            if (lowestCost > openList[i].totalEstimatedCost)
            {
                lowestCost = openList[i].totalEstimatedCost;
                lowest = openList[i];
            }
        }
        return lowest;
    }

    public void BlockNode(Vector2 position)
    {
        Node node = GetNode(position);
        node.isBlocked = true;
    }

    public void UnBlockNode(Vector2 position)
    {
        Node node = GetNode(position);
        node.isBlocked = false;
    }

    public void UnblockAll()
    {
        for(int i = 0; i < nodeList.Count; i++)
        {
            nodeList[i].isBlocked = false;
        }
    }

    public bool IsTileBlocked(Vector2 position)
    {
        return GetNode(position).isBlocked;
    }

    public bool IsValidNode(Vector2 position)
    {
        return position.x < 16.0f && position.x >= -16.0f && position.y < 16.0f && position.y >= -16.0f;
    }

    private Node GetNode(Vector2 position)
    {
        float x = position.x + 16.0f;
        float y = position.y + 16.0f;
        int index = (int)x * 32 + (int)y;
        return nodeList[index];
    }
}
