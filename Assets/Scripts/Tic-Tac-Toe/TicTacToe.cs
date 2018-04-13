using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToe : MonoBehaviour {

    private int[,] mainBoard = new int[3,3];
    private int activePlayer = 0;
    private int turns = 0;
    private bool stopGame = false;

    [SerializeField]
    private GameObject cross;
    [SerializeField]
    private GameObject circle;

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (!stopGame)
        {
            if (activePlayer == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        Vector3 position = hit.transform.position;
                        int i = (int)position.x + 1;
                        int j = (int)position.y + 1;
                        if (mainBoard[i, j] == 0)
                        {
                            //Circle's turn
                            GameObject circleDup = Instantiate(circle);
                            circleDup.transform.position = new Vector3(position.x, position.y, -1.0f);
                            mainBoard[i, j] = 1;

                            turns++;

                            if (CheckForWin(mainBoard))
                            {
                                print("Player 1 wins");
                                stopGame = true;
                            }
                            else if (turns == 9)
                            {
                                print("Draw");
                                stopGame = true;
                            }
                            activePlayer++;
                            activePlayer %= 2;
                        }
                    }
                }
            }
            else
            {
                TicTacToeMCTSNode node = RunMCTS(activePlayer);
                float x = node.x - 1;
                if (x > 0)
                {
                    x += 0.05f;
                }
                else
                {
                    x -= 0.05f;
                }

                float y = node.y - 1;
                if (y > 0)
                {
                    y += 0.05f;
                }
                else
                {
                    y -= 0.05f;
                }

                //Cross's turn
                GameObject crossDup = Instantiate(cross);
                crossDup.transform.position = new Vector3(x, y, -1.0f);
                mainBoard[node.x, node.y] = 2;

                turns++;

                if (CheckForWin(mainBoard))
                {
                    print("Player 2 wins");
                    stopGame = true;
                }
                else if (turns == 9)
                {
                    print("Draw");
                    stopGame = true;
                }
                activePlayer++;
                activePlayer %= 2;
            }
        }
	}

    private bool CheckForWin(int[,] board)
    {
        //Check diagonals        
        {
            if(board[2,0] == board[1, 1] && board[1, 1] == board[0, 2] && board[0, 2] != 0)
            {
                return true;
            }

            if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2] && board[2, 2] != 0)
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

    //Reference:https://www.youtube.com/watch?v=UXW2yZndl7U
    private TicTacToeMCTSNode RunMCTS(int player)
    {
        TicTacToeMCTSNode root = new TicTacToeMCTSNode();
        root.parent = null;
        root.BoardState = mainBoard;
        root.player = player;

        TicTacToeMCTSNode current = root;
        bool reset = false;
        int totalIterations = 0;
        while (totalIterations < 10000)
        {
            totalIterations++;
            if (reset)
            {
                current = root;
                reset = false;
            }

            //Expand
            if (current.children.Count == 0)
            {
                current.PopulateChildren();
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
                    if(selectedIndex < current.children.Count)
                    {
                        current = current.children[selectedIndex];
                    }
                    else
                    {
                        //All nodes visited
                        break;
                    }                    
                }
            }
        }

        //Make final selection
        float finalMaxUCB = 0.0f;
        int finalSelectedIndex = 0;
        for (int i = 0; i < root.children.Count; i++)
        {
            float UCB = root.children[i].UCB1();
            if (finalMaxUCB < UCB)
            {
                finalMaxUCB = UCB;
                finalSelectedIndex = i;
            }
        }
        TicTacToeMCTSNode selectedNode = root.children[finalSelectedIndex];
        return selectedNode;
    }

    private bool Simulate(int[,] boardState, int player)
    {
        List<int> availableXPosition = new List<int>();
        List<int> availableYPosition = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == 0)
                {
                    availableXPosition.Add(i);
                    availableYPosition.Add(j);
                }
            }
        }

        int currentPlayer = player;

        bool isDraw = false;
        while (CheckForWin(boardState))
        {
            //Check if its a draw
            if(availableXPosition.Count == 0)
            {
                isDraw = true;
                break;
            }

            //Make a random decision
            int xIndex = UnityEngine.Random.Range(0, availableXPosition.Count);
            int yIndex = UnityEngine.Random.Range(0, availableYPosition.Count);
            int x = availableXPosition[xIndex];
            int y = availableXPosition[yIndex];
            
            //Update board state
            boardState[x, y] = currentPlayer + 1;
            currentPlayer = (currentPlayer + 1) % 2;

            //Update possibility space
            availableXPosition.RemoveAt(xIndex);
            availableYPosition.RemoveAt(yIndex);
        }

        bool result = false;
        if (isDraw)
        {
            //Draw is considered a win
            result = true;
        }
        else
        {
            //Because we update the current player at the end of each turn,
            //if current player is not equal to the player that originally started the simulation, the original player has won
            result = currentPlayer != player;            
        }

        return result;
    }
}
