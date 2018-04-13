using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToe : MonoBehaviour {

    private int[,] mainBoard = new int[3,3];
    private bool turnToggle = false;
    private int turns = 0;

    [SerializeField]
    private GameObject cross;
    [SerializeField]
    private GameObject circle;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                Vector3 position = hit.transform.position;
                int i = (int)position.x + 1;
                int j = (int)position.y + 1;
                if(mainBoard[i,j] == 0)
                {
                    if (!turnToggle)
                    {
                        //Circle's turn
                        GameObject circleDup = Instantiate(circle);
                        circleDup.transform.position = new Vector3(position.x, position.y, -1.0f);
                        mainBoard[i, j] = 1;
                    }
                    else
                    {
                        //Cross' turn
                        GameObject crossDup = Instantiate(cross);
                        crossDup.transform.position = new Vector3(position.x, position.y, -1.0f);
                        mainBoard[i, j] = 2;
                    }

                    turns++;

                    if (CheckForWin(mainBoard))
                    {
                        //End game
                        if (turnToggle) {
                            print("Player 2 wins");
                        }
                        else
                        {
                            print("Player 1 wins");
                        }
                    }
                    else if(turns == 9)
                    {
                        print("Draw");
                    }
                    turnToggle = !turnToggle;
                    RunMCTS(2);
                }
            }
        }
	}

    private bool CheckForWin(int[,] board)
    {
        //Check diagonals        
        {
            if(board[0,0] == board[1, 1] && board[1, 1] == board[2, 2] && board[2, 2] != 0)
            {
                return true;
            }

            if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0] && board[2, 2] != 0)
            {
                return true;
            }
        }

        for (int i = 0; i < 3; i++)
        {            
            //Check rows
            if(board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2] && board[i, 2] != 0)
            {
                return true;
            }

            //Check columns
            if (board[0, i] == board[1, i] && board[1, i] == board[2, i] && board[2, i] != 0)
            {
                return true;
            }
        }
        return false;
    }

    class Node
    {
        public List<Node> children = new List<Node>();
        public Node parent;
        public float successfulPlayouts = 0;
        public float totalPlayouts = 0;
        public int player;

        private int[,] boardState;

        public int[,] BoardState
        {
            set
            {
                //copy board
                boardState = new int[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        boardState[i, j] = value[i, j];
                    }
                }
            }

            get
            {
                int[,] duplicateBoardState = new int[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        duplicateBoardState[i, j] = boardState[i, j];
                    }
                }
                return duplicateBoardState;
            }
        }

        public float UCB1()
        {
            float N = 0.0f;
            if(parent != null)
            {
                N = parent.totalPlayouts;
            }
            return (successfulPlayouts / totalPlayouts) + Mathf.Sqrt(2.0f * Mathf.Log(N) / totalPlayouts);
        }

        public void Update(bool currentPlayer)
        {
            totalPlayouts++;
            if(currentPlayer)
            {
                successfulPlayouts++;
            }

            if(parent != null)
            {
                parent.Update(!currentPlayer);
            }
        }

        public void PopulateChildren()
        {
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if(boardState[i,j] == 0)
                    {
                        //Change state
                        boardState[i, j] = player;
                        Node child = new Node();
                        child.parent = this;
                        child.BoardState = boardState;
                        child.player = player + 1;
                        if(child.player > 2)
                        {
                            child.player = 1;
                        }
                        children.Add(child);
                        //Reset state
                        boardState[i, j] = 0;
                    }
                }
            }
        }

        public bool HasBeenVisited()
        {
            return totalPlayouts > 0;
        }
    }

    //Reference:https://www.youtube.com/watch?v=UXW2yZndl7U
    private void RunMCTS(int player)
    {        
        Node root = new Node();
        root.parent = null;
        root.BoardState = mainBoard;
        root.player = player;

        Node current = root;
        bool reset = false;
        while (true)
        {
            if (reset)
            {
                current = root;
            }

            //Expand
            if (current.children.Count == 0)
            {
                current.PopulateChildren();

                //If end state stop algorithm
                break;
            }

            //Select
            {
                float maxUCB = 0.0f;
                int selectedIndex = 0;
                bool simulationRun = false;
                for (int i = 0; i < current.children.Count; i++)
                {
                    if (!current.children[i].HasBeenVisited())
                    {
                        //Simulate
                        if (Simulate(current.children[i].BoardState, current.children[i].player))
                        {
                            current.children[i].Update(true);                            
                        }
                        else
                        {
                            current.children[i].Update(false);
                        }
                        simulationRun = true;
                        reset = true;
                        break;
                    }
                    else
                    {
                        float UCB = current.children[i].UCB1();
                        if (maxUCB < UCB)
                        {
                            maxUCB = UCB;
                            selectedIndex = i;
                        }
                    }
                }

                if (!simulationRun)
                {
                    current = current.children[selectedIndex];
                }
            }
        }
    }

    private bool Simulate(int[,] boardState, int player)
    {
        return false;
    }
}
