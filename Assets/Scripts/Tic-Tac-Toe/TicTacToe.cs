using System;
using System.Collections;
using System.Collections.Generic;
using TTT;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToe : MonoBehaviour {

    private int[,] mainBoard;
    private int activePlayer = 0;
    private int turns = 0;
    private bool stopGame = false;    
    private int maxTurns;
    private List<GameObject> tokens;
    private CustomLogWriter logger;    
    private int circleWins = 0;
    private int crossWins = 0;
    private float minSize = 3.0f;

    [SerializeField]
    private GameObject cross;
    [SerializeField]
    private GameObject circle;
    [SerializeField]
    private GameObject whiteTile;
    [SerializeField]
    private GameObject blackTile;    
    [SerializeField]
    private int size;
    [SerializeField]
    private int currentIteration = 0;
    [SerializeField]
    private int numberOfIterations;    
    [SerializeField]
    private bool doubleAI;
    [SerializeField]
    private Text gameText;
    [SerializeField]
    private float circleTime;
    [SerializeField]
    private float crossTime;

    // Use this for initialization
    void Start () {

        mainBoard = new int[size, size];
        maxTurns = size * size;

        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                GameObject tile;
                if ((i+j) % 2 == 0)
                {
                    tile = Instantiate(whiteTile);
                }
                else
                {
                    tile = Instantiate(blackTile);
                }
                tile.transform.position = new Vector3(i, j, 0.0f);
                tile.transform.parent = transform;
            }
        }

        tokens = new List<GameObject>();
        float cameraZ = Camera.main.transform.position.z;
        Camera.main.transform.position = new Vector3(size / 2, size / 2, cameraZ);
        logger = gameObject.AddComponent<CustomLogWriter>();
        logger.filePath = "Tic_Tac_Toe_" + (doubleAI ? "AI_v_AI_" : "") + (size + "x" + size + "_") + (circleTime + "_" + crossTime) + ("_" + numberOfIterations);

        ResetBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopGame)
        {
            if (activePlayer == 0 && !doubleAI)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {                        
                        Vector3 position = hit.transform.position;
                        int i = (int)position.x;
                        int j = (int)position.y;
                        if (mainBoard[i, j] == -1)
                        {
                            GameObject circleDup = Instantiate(circle);
                            circleDup.transform.position = new Vector3(position.x, position.y, -1.0f);
                            circleDup.transform.parent = transform;
                            tokens.Add(circleDup);
                            mainBoard[i, j] = activePlayer;
                            turns++;

                            if (CheckForWin(mainBoard))
                            {
                                gameText.text = "Circle Wins";
                                crossWins++;
                                currentIteration++;                                
                                stopGame = true;
                            }
                            else if (turns == maxTurns)
                            {
                                gameText.text = "Draw";
                                currentIteration++;
                                stopGame = true;
                            }
                            else
                            {
                                gameText.text = "Cross's Turn";
                            }
                            activePlayer++;
                            activePlayer %= 2;
                        }
                    }
                }
            }
            else
            {
                float difficulty = activePlayer == 0 ? circleTime : crossTime;
                MCTSNode node = RunMCTS(activePlayer, difficulty);
                if (activePlayer == 0)
                {                    
                    //Circle's turn
                    GameObject circleDup = Instantiate(circle);
                    circleDup.transform.position = new Vector3(node.x, node.y, -1.0f);
                    circleDup.transform.parent = transform;
                    tokens.Add(circleDup);
                }
                else
                {
                    //Cross's turn
                    GameObject crossDup = Instantiate(cross);
                    crossDup.transform.position = new Vector3(node.x, node.y, -1.0f);
                    crossDup.transform.parent = transform;
                    tokens.Add(crossDup);
                }
                mainBoard[node.x, node.y] = activePlayer;

                turns++;

                if (CheckForWin(mainBoard))
                {
                    if (activePlayer == 0)
                    {
                        gameText.text = "Circle wins";
                        circleWins++;
                        currentIteration++;
                    }
                    else
                    {
                        gameText.text = "Cross wins";
                        crossWins++;
                        currentIteration++;
                    }
                    stopGame = true;
                    CheckToRestart();
                }
                else if (turns == maxTurns)
                {
                    gameText.text = "Draw";
                    currentIteration++;
                    stopGame = true;
                    CheckToRestart();
                }
                else
                {
                    if(activePlayer == 0)
                    {
                        gameText.text = "Cross's Turn";
                    }
                    else
                    {
                        gameText.text = "Circle's Turn";
                    }
                }
                activePlayer++;
                activePlayer %= 2;
            }
        }
	}

    private void CheckToRestart()
    {
        if (doubleAI && currentIteration < numberOfIterations)
        {
            ResetBoard();
        }else if(currentIteration >= numberOfIterations)
        {
            WriteLog();
        }
    }

    private bool CheckForWin(int[,] board)
    {
        //Check diagonals
        bool result = false;
        for(int i = 0; i < size - 1; i++)
        {
            if(board[i,i] == -1)
            {
                result = false;
                break;
            }

            result = board[i, i] == board[i + 1, i + 1];
            if (!result)
            {
                break;
            }
        }
        if (result)
        {
            return true;
        }

        result = false;
        for (int i = 0; i < size - 1; i++)
        {
            if (board[i, size - 1 - i] == -1)
            {
                result = false;
                break;
            }

            result = board[i, size - 1 - i] == board[i + 1, size - 1 - (i + 1)];
            if (!result)
            {
                break;
            }
        }
        if (result)
        {
            return true;
        }

        //Check rows
        result = false;
        for (int i = 0; i < size; i++)
        {            
            for(int j = 0; j < size - 1; j++)
            {
                if (board[i, j] == -1)
                {
                    result = false;
                    break;
                }

                result = board[i, j] == board[i, j + 1];
                if (!result)
                {
                    break;
                }
            }
            if (result)
            {
                break;
            }
        }
        if (result)
        {
            return true;
        }

        //Check columns
        result = false;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size - 1; j++)
            {
                if (board[j, i] == -1)
                {
                    result = false;
                    break;
                }

                result = board[j, i] == board[j + 1, i];
                if (!result)
                {
                    break;
                }
            }
            if (result)
            {
                break;
            }
        }
        if (result)
        {
            return true;
        }

        return false;
    }

    //Reference:https://www.youtube.com/watch?v=UXW2yZndl7U
    private MCTSNode RunMCTS(int player, float modifier)
    {
        MCTSNode root = new MCTSNode(size);
        root.BoardState = mainBoard;
        root.parent = null;
        root.player = (activePlayer + 1) % 2;
        root.score = 0.0f;
        root.totalPlayouts = 0.0f;
        int totalIterations = 0;

        DateTime start = DateTime.Now;


        //while((DateTime.Now - start).Milliseconds < Time.maximumDeltaTime * 1000 * modifier)
        while (totalIterations < 100000)
        {
            MCTSNode current = root;

            //Selection
            while(current.children.Count != 0)
            {
                current = current.GetChildWithBestUCB1();
            }

            //Simulation
            if(current.totalPlayouts == 0)
            {
                float score = Simulate(current.BoardState, activePlayer);

                //Back Propogate
                current.BackPropogate(score);
            }
            else
            {
                //Expansion
                current.Expand();
            }

            totalIterations++;
        }

        //Find node with best score
        float bestScore = root.children[0].score / root.children[0].totalPlayouts;
        int selectedIndex = 0;
        for(int i = 1; i < root.children.Count; i++)
        {
            float score = root.children[i].score / root.children[i].totalPlayouts;
            if (bestScore < score)
            {
                bestScore = score;
                selectedIndex = i;
            }
        }

        return root.children[selectedIndex];
    }

    private float Simulate(int[,] boardState, int testingPlayer)
    {
        List<int> availableXPosition = new List<int>();
        List<int> availableYPosition = new List<int>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (boardState[i, j] == -1)
                {
                    availableXPosition.Add(i);
                    availableYPosition.Add(j);
                }
            }
        }

        int currentPlayer = testingPlayer;
        bool isDraw = false;

        while (!CheckForWin(boardState))
        {
            //Check if its a draw
            if(availableXPosition.Count == 0)
            {
                isDraw = true;
                break;
            }

            //Make a random decision
            int index = UnityEngine.Random.Range(0, availableXPosition.Count);
            int x = availableXPosition[index];
            int y = availableYPosition[index];

            //Update board state
            boardState[x, y] = currentPlayer;
            currentPlayer = (currentPlayer + 1) % 2;

            //Update possibility space
            availableXPosition.RemoveAt(index);
            availableYPosition.RemoveAt(index);
        }

        float result = 0.0f;
        if (isDraw)
        {
            //Draw is considered a 50%
            result = 1.0f;
        }
        else
        {
            //Because we update the current player at the end of each turn,
            //if current player is not equal to the player that originally started the simulation, the original player has won
            result = currentPlayer == testingPlayer ? 1.0f : 0.0f;
        }

        return result;
    }

    public void ResetBoard()
    {
        for (int i = 0; i < tokens.Count; i++)
        {
            Destroy(tokens[i]);
        }
        tokens.Clear();
        turns = 0;
        stopGame = false;
        activePlayer = 0;
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                mainBoard[i, j] = -1;
            }
        }        
    }

    public void WriteLog()
    {
        logger.Write("Total games: " + currentIteration);
        logger.Write("Total circle wins: " + circleWins);
        logger.Write("Total cross wins: " + crossWins);
        logger.Write("Total draws: " + (currentIteration - (crossWins + circleWins)));
    }
}

namespace TTT
{
    public class MCTSNode
    {
        public List<MCTSNode> children = new List<MCTSNode>();
        public MCTSNode parent;
        public float score = 0;
        public float totalPlayouts = 0;
        public int player;
        public int x;
        public int y;

        private int size;
        private int[,] boardState;

        public MCTSNode(int size)
        {
            this.size = size;
        }

        public int[,] BoardState
        {
            set
            {
                //copy board
                boardState = new int[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        boardState[i, j] = value[i, j];
                    }
                }
            }

            get
            {
                int[,] duplicateBoardState = new int[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        duplicateBoardState[i, j] = boardState[i, j];
                    }
                }
                return duplicateBoardState;
            }
        }

        public float UCB1
        {
            get{
                float N = 0.0f;
                if (parent != null)
                {
                    N = parent.totalPlayouts;
                }

                if (totalPlayouts == 0.0f)
                {
                    return Mathf.Infinity;
                }
                else
                {                    
                    return (score / totalPlayouts) + 2.0f * Mathf.Sqrt(Mathf.Log(N) / totalPlayouts);
                }
            }
        }

        public void BackPropogate(float value)
        {
            totalPlayouts++;           
            score += value;

            if (parent != null)
            {
                parent.BackPropogate(value);
            }
        }

        public void Expand()
        {
            if (children.Count == 0)
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (boardState[i, j] == -1)
                        {
                            //Change state
                            boardState[i, j] = player;
                            MCTSNode child = new MCTSNode(size);
                            child.parent = this;
                            child.BoardState = boardState;
                            child.x = i;
                            child.y = j;
                            child.player = (player + 1) % 2;
                            children.Add(child);
                            //Reset state
                            boardState[i, j] = -1;
                        }
                    }
                }
            }
        }

        internal MCTSNode GetChildWithBestUCB1()
        {
            float maxUCB1 = children[0].UCB1;
            int selectedIndex = 0;
            for(int i = 1; i < children.Count; i++)
            {
                if(maxUCB1 < children[i].UCB1)
                {
                    maxUCB1 = children[i].UCB1;
                    selectedIndex = i;
                }
            }
            return children[selectedIndex];
        }
    }
}