using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Snake : MonoBehaviour {

    private enum Direction
    {
        LEFT,RIGHT,UP,DOWN,NONE
    }

    private GameObject goal;
    private List<Vector3> path;
    private int pathIndex;
    private SnakeLevel level;
    private List<GameObject> snakeBodySegments;
    private List<Direction> snakeBodySegmentDirections;
    private bool stopGame;
    private float waitTimeCounter;
    private int score;
    private int cumulativeScore;
    private int iterationCount;
    private Vector3 startPosition = new Vector3(0.0f, 1.0f, 0.0f);

    public GameObject snakeBodyPrefab;
    public float waitTime;
    public bool pathFindEachFrame;
    public Text scoreText;
    public int maxIterations;
    public bool useSpecialPathFinding;


	// Use this for initialization
	void Start () {
        goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);        
        level = FindObjectOfType<SnakeLevel>();
        goal.transform.position = startPosition;
        snakeBodySegments = new List<GameObject>();
        snakeBodySegmentDirections = new List<Direction>();
        stopGame = false;
        GameObject head = Instantiate(snakeBodyPrefab);
        head.transform.position = goal.transform.position;
        level.BlockNode(new Vector2(head.transform.position.x, head.transform.position.z));
        snakeBodySegments.Add(head);
        snakeBodySegmentDirections.Add(Direction.NONE);
        pathIndex = 0;
        waitTimeCounter = 0.0f;
        score = 0;
        iterationCount = 0;
        cumulativeScore = 0;
    }

    private bool PerformSpecialPathFinding(bool specialPathFound)
    {
        Vector2 start = new Vector2(snakeBodySegments[0].transform.position.x, snakeBodySegments[0].transform.position.z);
        Vector2 goalPosition = new Vector2(goal.transform.position.x, goal.transform.position.z);
        path = level.GetShortestPath(start, goalPosition);
        pathIndex = 0;

        if (path.Count == 0)
        {
            //Find any open direction to move in
            Vector3 headPosition = snakeBodySegments[0].transform.position;
            Direction headDirection = snakeBodySegmentDirections[0];

            Vector2 newLeftPosition = new Vector2(headPosition.x - 1, headPosition.z);
            Vector2 newRighPosition = new Vector2(headPosition.x + 1, headPosition.z);
            Vector2 newUpPosition = new Vector2(headPosition.x, headPosition.z + 1);
            Vector2 newDownPosition = new Vector2(headPosition.x, headPosition.z - 1);

            switch (headDirection)
            {
                case Direction.DOWN:                    
                    if (level.IsValidNode(newLeftPosition) && !level.IsTileBlocked(newLeftPosition))
                    {
                        path.Add(new Vector3(newLeftPosition.x, 0.0f, newLeftPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newRighPosition) && !level.IsTileBlocked(newRighPosition))
                    {
                        path.Add(new Vector3(newRighPosition.x, 0.0f, newRighPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newDownPosition) && !level.IsTileBlocked(newDownPosition))
                    {
                        path.Add(new Vector3(newDownPosition.x, 0.0f, newDownPosition.y));
                        specialPathFound = true;
                    }
                    break;
                case Direction.LEFT:
                    if (level.IsValidNode(newLeftPosition) && !level.IsTileBlocked(newLeftPosition))
                    {
                        path.Add(new Vector3(newLeftPosition.x, 0.0f, newLeftPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newUpPosition) && !level.IsTileBlocked(newUpPosition))
                    {
                        path.Add(new Vector3(newUpPosition.x, 0.0f, newUpPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newDownPosition) && !level.IsTileBlocked(newDownPosition))
                    {
                        path.Add(new Vector3(newDownPosition.x, 0.0f, newDownPosition.y));
                        specialPathFound = true;
                    }
                    break;
                case Direction.RIGHT:
                    if (level.IsValidNode(newRighPosition) && !level.IsTileBlocked(newRighPosition))
                    {
                        path.Add(new Vector3(newRighPosition.x, 0.0f, newRighPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newUpPosition) && !level.IsTileBlocked(newUpPosition))
                    {
                        path.Add(new Vector3(newUpPosition.x, 0.0f, newUpPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newDownPosition) && !level.IsTileBlocked(newDownPosition))
                    {
                        path.Add(new Vector3(newDownPosition.x, 0.0f, newDownPosition.y));
                        specialPathFound = true;
                    }
                    break;
                case Direction.UP:
                    if (level.IsValidNode(newUpPosition) && !level.IsTileBlocked(newUpPosition))
                    {
                        path.Add(new Vector3(newUpPosition.x, 0.0f, newUpPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newLeftPosition) && !level.IsTileBlocked(newLeftPosition))
                    {
                        path.Add(new Vector3(newLeftPosition.x, 0.0f, newLeftPosition.y));
                        specialPathFound = true;
                    }
                    else if (level.IsValidNode(newRighPosition) && !level.IsTileBlocked(newRighPosition))
                    {
                        path.Add(new Vector3(newRighPosition.x, 0.0f, newRighPosition.y));
                        specialPathFound = true;
                    }
                    break;
            }
        }
        else
        {
            specialPathFound = true;
        }
        return specialPathFound;
    }

    // Update is called once per frame
    void Update () {        
        if (!stopGame)
        {
            if (waitTimeCounter > waitTime)
            {
                waitTimeCounter = 0.0f;
                if (snakeBodySegments[0].transform.position == goal.transform.position)
                {
                    scoreText.text = "Score: " + score;
                    score++;

                    //Spawn new segment
                    {
                        GameObject segment = Instantiate(snakeBodyPrefab);
                        Vector3 segmentPosition;
                        Vector3 lastSegmentPosition = snakeBodySegments[snakeBodySegments.Count - 1].transform.position;
                        Direction lastSegmentDirection = snakeBodySegmentDirections[snakeBodySegmentDirections.Count - 1];
                        segmentPosition = lastSegmentPosition;
                        switch (lastSegmentDirection)
                        {
                            case Direction.UP:
                                segmentPosition.z -= 1;
                                break;
                            case Direction.DOWN:
                                segmentPosition.z += 1;
                                break;
                            case Direction.LEFT:
                                segmentPosition.x += 1;
                                break;
                            case Direction.RIGHT:
                            default:
                                segmentPosition.x -= 1;
                                break;
                        }
                        segment.transform.position = segmentPosition;
                        level.BlockNode(new Vector2(segmentPosition.x, segmentPosition.z));
                        snakeBodySegments.Add(segment);
                        snakeBodySegmentDirections.Add(Direction.NONE);
                    }

                    //Find random goal
                    {
                        int x = UnityEngine.Random.Range(-16, 16);
                        int y = UnityEngine.Random.Range(-16, 16);
                        Vector2 goalPosition = new Vector2(x, y);

                        while (level.IsTileBlocked(goalPosition))
                        {
                            x = UnityEngine.Random.Range(-16, 16);
                            y = UnityEngine.Random.Range(-16, 16);
                            goalPosition.x = x;
                            goalPosition.y = y;
                        }

                        Vector2 start = new Vector2(snakeBodySegments[0].transform.position.x, snakeBodySegments[0].transform.position.z);
                        goal.transform.position = new Vector3(goalPosition.x, 1.0f, goalPosition.y);

                        //Find path
                        path = level.GetShortestPath(start, goalPosition);
                        pathIndex = 0;

                        if(path.Count == 0 && useSpecialPathFinding)
                        {
                            //Find any open direction to move in
                            Vector3 headPosition = snakeBodySegments[0].transform.position;
                        }
                    }
                }
                else if (path.Count > 0)
                {
                    for (int i = 0; i < snakeBodySegments.Count; i++)
                    {
                        //Get current segment
                        GameObject segment = snakeBodySegments[i];
                        //Unblock the node it is on
                        level.UnBlockNode(new Vector2(segment.transform.position.x, segment.transform.position.z));
                        //Get its current position
                        Vector3 currentPosition = segment.transform.position;
                        //Find out the next position
                        Vector3 nextPosition;
                        if (i == 0)
                        {
                            nextPosition = new Vector3(path[pathIndex].x, 1.0f, path[pathIndex].z);
                        }
                        else
                        {
                            nextPosition = snakeBodySegments[i - 1].transform.position + GetPreviousPositionOffset(snakeBodySegmentDirections[i - 1]);
                        }
                        //Find out the direction
                        Direction movementDirection;
                        Vector3 positionDifference = nextPosition - currentPosition;
                        Debug.Assert(positionDifference.magnitude == 1);
                        if (positionDifference.x == 1)
                        {
                            movementDirection = Direction.RIGHT;
                        }
                        else if (positionDifference.x == -1)
                        {
                            movementDirection = Direction.LEFT;
                        }
                        else if (positionDifference.z == 1)
                        {
                            movementDirection = Direction.UP;
                        }
                        else
                        {
                            movementDirection = Direction.DOWN;
                        }
                        //Update position
                        segment.transform.position = nextPosition;
                        //Update direction
                        snakeBodySegmentDirections[i] = movementDirection;
                        //Block the new node
                        level.BlockNode(new Vector2(nextPosition.x, nextPosition.z));
                    }
                    pathIndex++;

                    if (pathFindEachFrame)
                    {
                        Vector2 start = new Vector2(snakeBodySegments[0].transform.position.x, snakeBodySegments[0].transform.position.z);
                        Vector2 goalPosition = new Vector2(goal.transform.position.x, goal.transform.position.z);
                        path = level.GetShortestPath(start, goalPosition);
                        pathIndex = 0;
                    }
                }
                else
                {
                    bool specialPathFound = false;
                    if (useSpecialPathFinding)
                    {
                        specialPathFound = PerformSpecialPathFinding(specialPathFound);
                    }

                    if (!specialPathFound)
                    {
                        iterationCount++;
                        if (iterationCount >= maxIterations)
                        {
                            //end game
                            print("end");
                            print(cumulativeScore / maxIterations);
                            stopGame = true;
                        }
                        else
                        {
                            //Reset game
                            for (int i = 1; i < snakeBodySegments.Count; i++)
                            {
                                Destroy(snakeBodySegments[i]);
                            }

                            GameObject head = snakeBodySegments[0];
                            head.transform.position = startPosition;
                            snakeBodySegments.Clear();
                            snakeBodySegments.Add(head);
                            goal.transform.position = startPosition;
                            snakeBodySegmentDirections.Clear();
                            snakeBodySegmentDirections.Add(Direction.NONE);

                            //Unblock all tiles
                            level.UnblockAll();
                            level.BlockNode(new Vector2(head.transform.position.x, head.transform.position.z));

                            cumulativeScore += score;
                            score = 0;
                        }
                    }
                }
            }
            else
            {
                waitTimeCounter += Time.deltaTime;
            }
        }
	}

    private Vector3 GetPreviousPositionOffset(Direction direction)
    {
        Debug.Assert(direction != Direction.NONE);
        Vector3 previousPositionOffset = new Vector3();
        switch (direction)
        {
            case Direction.DOWN:
                previousPositionOffset.z = 1;
                break;
            case Direction.LEFT:
                previousPositionOffset.x = 1;
                break;
            case Direction.RIGHT:
                previousPositionOffset.x = -1;
                break;
            case Direction.UP:
                previousPositionOffset.z = -1;
                break;
        }
        return previousPositionOffset;
    }
}
