using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour {

    private enum Direction
    {
        LEFT,RIGHT,UP,DOWN,NONE
    }

    private GameObject goal;
    private List<Vector3> path;
    private SnakeLevel level;
    private List<GameObject> snakeBodySegments;
    private List<Direction> snakeBodySegmentDirections;
    private bool stopGame;
    public GameObject snakeBodyPrefab;


	// Use this for initialization
	void Start () {
        goal = GameObject.CreatePrimitive(PrimitiveType.Sphere);        
        level = FindObjectOfType<SnakeLevel>();
        goal.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
        snakeBodySegments = new List<GameObject>();
        snakeBodySegmentDirections = new List<Direction>();
        stopGame = false;
        GameObject head = Instantiate(snakeBodyPrefab);
        head.transform.position = goal.transform.position;
        level.BlockNode(new Vector2(head.transform.position.x, head.transform.position.z));
        snakeBodySegments.Add(head);
        snakeBodySegmentDirections.Add(Direction.NONE);
    }
	
	// Update is called once per frame
	void Update () {
        if (!stopGame)
        {
            if (snakeBodySegments[0].transform.position == goal.transform.position)
            {
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
                }
            }
            else if (path.Count > 0)
            {
                for(int i = 0; i < snakeBodySegments.Count; i++)
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
                        nextPosition = new Vector3(path[0].x, 1.0f, path[0].z);
                    }
                    else
                    {
                        nextPosition = snakeBodySegments[i - 1].transform.position + GetPreviousPositionOffset(snakeBodySegmentDirections[i - 1]);
                    }
                    //Find out the direction
                    Direction movementDirection;
                    Vector3 positionDifference = nextPosition - currentPosition;
                    Debug.Assert(positionDifference.magnitude == 1);
                    if(positionDifference.x == 1)
                    {
                        movementDirection = Direction.RIGHT;
                    }else if(positionDifference.x == -1)
                    {
                        movementDirection = Direction.LEFT;
                    }else if(positionDifference.z == 1)
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
                
                Vector2 start = new Vector2(snakeBodySegments[0].transform.position.x, snakeBodySegments[0].transform.position.z);                

                //Find path
                path = level.GetShortestPath(start, new Vector2(goal.transform.position.x, goal.transform.position.z));
            }
            else
            {
                //end game
                print("end");
                stopGame = true;
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
