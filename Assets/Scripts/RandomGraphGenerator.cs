using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    class NodeRecord
    {
        public Node node;
        public Connection connection;
        public float costSoFar;
        public float totalEstimatedCost;
    }

    private List<Node> nodeList;
    private List<List<byte>> connectionMatrix;
    private CustomLogWriter nodeLogWriter;
    private CustomLogWriter aStarLogWriter;
    private CustomLogWriter dijkstraLogWriter;

    [SerializeField]
    private int numberOfNodes;
    [SerializeField]
    private float minRandom;
    [SerializeField]
    private float maxRandom;
    [SerializeField]
    private float maxIterations;

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
        connectionMatrix = new List<List<byte>>(numberOfNodes);
        for (int i = 0; i < numberOfNodes; i++)
        {
            connectionMatrix.Add(new List<byte>(numberOfNodes));
            for (int j = 0; j < numberOfNodes; j++)
            {
                connectionMatrix[i].Add(0);
            }
        }
        
        nodeLogWriter = gameObject.AddComponent<CustomLogWriter>();
        nodeLogWriter.filePath = "Random_Graph_Nodes_" + numberOfNodes;

        aStarLogWriter = gameObject.AddComponent<CustomLogWriter>();
        aStarLogWriter.filePath = "A_Star_Nodes_" + numberOfNodes;

        dijkstraLogWriter = gameObject.AddComponent<CustomLogWriter>();
        dijkstraLogWriter.filePath = "Dijkstra_Nodes_" + numberOfNodes;

        int totalConnections = 0;
        //Assign connections
        nodeLogWriter.Write("start node index, end node index, connection length");
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
                        nodeLogWriter.Write(logString);
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

        //Run A* maxIterations times
        long totalOpenListCount = 0;
        long totalClosedListCount = 0;
        long totalTime = 0;
        long totalPathSize = 0;
        float averageOpenListCount = 0.0f;
        float averageClosedListCount = 0.0f;
        float averageTime = 0.0f;
        float averagePathSize = 0.0f;

        for (int i = 0; i < maxIterations; i++)
        {
            //Get random start node
            Node start = nodeList[Random.Range(0, nodeList.Count)];
            //Get random end node
            Node end = nodeList[Random.Range(0, nodeList.Count)];
            while(start.id == end.id)
            {
                end = nodeList[Random.Range(0, nodeList.Count)];
            }
            //Get start time
            System.DateTime startTime = System.DateTime.Now;
            //Run algorithm
            int openListCount, closedListCount;
            List<Vector2> path = GetShortestPath(start, end, false, out openListCount, out closedListCount);
            //Get end time
            System.DateTime endTime = System.DateTime.Now;
            //Get length of list
            int pathLength = path.Count;

            //Update accumulators
            totalOpenListCount += openListCount;
            totalClosedListCount += closedListCount;
            totalTime += (endTime - startTime).Milliseconds;
            totalPathSize += pathLength;
        }
        print("Done with A*");
        //Log all the values
        averageOpenListCount = totalOpenListCount / maxIterations;
        averageClosedListCount = totalClosedListCount / maxIterations;
        averageTime = totalTime / maxIterations;
        averagePathSize = totalPathSize / maxIterations;
        aStarLogWriter.Write("Total iterations: " + maxIterations);
        aStarLogWriter.Write("Average open list count: " + averageOpenListCount);
        aStarLogWriter.Write("Average closed list count: " + averageClosedListCount);
        aStarLogWriter.Write("Average time in milliseconds: " + averageTime);
        aStarLogWriter.Write("Average path size: " + averagePathSize);

        //Run dijkstra maxIterations times
        totalOpenListCount = 0;
        totalClosedListCount = 0;
        totalTime = 0;
        totalPathSize = 0;
        averageOpenListCount = 0.0f;
        averageClosedListCount = 0.0f;
        averageTime = 0.0f;
        averagePathSize = 0.0f;

        for (int i = 0; i < maxIterations; i++)
        {
            //Get random start node
            Node start = nodeList[Random.Range(0, nodeList.Count)];
            //Get random end node
            Node end = nodeList[Random.Range(0, nodeList.Count)];
            while (start.id == end.id)
            {
                end = nodeList[Random.Range(0, nodeList.Count)];
            }
            //Get start time
            System.DateTime startTime = System.DateTime.Now;
            //Run algorithm
            int openListCount, closedListCount;
            List<Vector2> path = GetShortestPath(start, end, true, out openListCount, out closedListCount);
            //Get end time
            System.DateTime endTime = System.DateTime.Now;
            //Get length of list
            int pathLength = path.Count;

            //Update accumulators
            totalOpenListCount += openListCount;
            totalClosedListCount += closedListCount;
            totalTime += (endTime - startTime).Milliseconds;
            totalPathSize += pathLength;

        }
        print("Done with Dijsktra");
        //Log all the values
        averageOpenListCount = totalOpenListCount / maxIterations;
        averageClosedListCount = totalClosedListCount / maxIterations;
        averageTime = totalTime / maxIterations;
        averagePathSize = totalPathSize / maxIterations;
        dijkstraLogWriter.Write("Total iterations: " + maxIterations);
        dijkstraLogWriter.Write("Average open list count: " + averageOpenListCount);
        dijkstraLogWriter.Write("Average closed list count: " + averageClosedListCount);
        dijkstraLogWriter.Write("Average time in milliseconds: " + averageTime);
        dijkstraLogWriter.Write("Average path size: " + averagePathSize);
        
        print("Done! " + totalConnections);
    }

    private List<Vector2> GetShortestPath(Node start, Node destination, bool useDijkstra, out int openListCount, out int closedListCount)
    {
        List<Vector2> path = new List<Vector2>();
        Node startNode = start;
        Node destinationNode = destination;
        openListCount = 0;
        closedListCount = 0;
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
            openListCount++;
            NodeRecord currentRecord = new NodeRecord();

            while (openList.Count > 0)
            {
                currentRecord = GetRecordWithLowestCost(openList);
                openList.Remove(currentRecord);
                closedList.Add(currentRecord);
                closedListCount++;
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
                        toNodeHeuristic = GetHeuristic(toNodeRecord.node, destinationNode, useDijkstra);
                    }
                    //Update record values
                    toNodeRecord.costSoFar = toNodeCost;
                    toNodeRecord.connection = connection;
                    toNodeRecord.totalEstimatedCost = toNodeCost + toNodeHeuristic;

                    //Add to the open list if it's not there
                    if (!ListContains(openList, toNode))
                    {
                        openList.Add(toNodeRecord);
                        openListCount++;
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
            }
        }
        return path;
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

    private List<Connection> GetConnections(Node node)
    {
        List<Connection> connections = new List<Connection>();
        List<byte> connectionsArray = connectionMatrix[node.id];
        for (int i = 0; i < connectionsArray.Count; i++)
        {
            if (i != node.id && connectionsArray[i] == 1)
            {
                Connection connection = new Connection();
                connection.from = node;
                connection.to = nodeList[i];
                connection.cost = Vector2.Distance(connection.from.position, connection.to.position);
                connections.Add(connection);
            }
        }
        return connections;
    }

    private float GetHeuristic(Node node, Node destinationNode, bool isDijkstra)
    {
        return isDijkstra ? 0.0f : Mathf.Abs(node.position.x - destinationNode.position.x) + Mathf.Abs(node.position.y - destinationNode.position.y);
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
}
