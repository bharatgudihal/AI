using System;
using System.Collections;
using System.Collections.Generic;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopGame)
        {
            if (activePlayer == 1 && !doubleAI)
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
                        if (mainBoard[i, j] == 0)
                        {
                            GameObject crossDup = Instantiate(cross);
                            crossDup.transform.position = new Vector3(position.x, position.y, -1.0f);
                            crossDup.transform.parent = transform;
                            tokens.Add(crossDup);
                            mainBoard[i, j] = activePlayer + 1;
                            turns++;

                            if (CheckForWin(mainBoard))
                            {
                                gameText.text = "Cross Wins";
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
                TicTacToeMCTSNode node = RunMCTS(activePlayer, difficulty);
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
                mainBoard[node.x, node.y] = activePlayer + 1;

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
            if(board[i,i] == 0)
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
            if (board[i, size - 1 - i] == 0)
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
                if (board[i, j] == 0)
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
                if (board[j, i] == 0)
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
    private TicTacToeMCTSNode RunMCTS(int player, float modifier)
    {
        TicTacToeMCTSNode root = new TicTacToeMCTSNode(size);
        root.parent = null;
        root.BoardState = mainBoard;
        root.player = player;        


        TicTacToeMCTSNode current = root;
        bool reset = false;
        int totalIterations = 0;
        DateTime start = DateTime.Now;

        while((DateTime.Now - start).Milliseconds < Time.fixedDeltaTime * 1000 * modifier * (size / minSize))
        {            
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
                        float score = Simulate(current.children[i].BoardState, current.children[i].player);
                        current.children[i].Update(true, score);
                        
                        simulationRun = true;
                        reset = true;
                        totalIterations++;
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
                        reset = true;
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

    private float Simulate(int[,] boardState, int player)
    {
        List<int> availableXPosition = new List<int>();
        List<int> availableYPosition = new List<int>();
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
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
        System.Random random = new System.Random(1024);

        while (CheckForWin(boardState))
        {
            //Check if its a draw
            if(availableXPosition.Count == 0)
            {
                isDraw = true;
                break;
            }

            //Make a random decision
            int index = random.Next(0, availableXPosition.Count);
            int x = availableXPosition[index];
            int y = availableXPosition[index];
            
            //Update board state
            boardState[x, y] = currentPlayer + 1;
            currentPlayer = (currentPlayer + 1) % 2;

            //Update possibility space
            availableXPosition.RemoveAt(index);
            availableYPosition.RemoveAt(index);
        }

        float result = 0.0f;
        if (isDraw)
        {
            //Draw is considered a 50%
            result = 0.5f;
        }
        else
        {
            //Because we update the current player at the end of each turn,
            //if current player is not equal to the player that originally started the simulation, the original player has won
            result = currentPlayer != player ? 1.0f : 0.0f;            
        }

        return result;
    }

    public class TicTacToeMCTSNode
    {
        public List<TicTacToeMCTSNode> children = new List<TicTacToeMCTSNode>();
        public TicTacToeMCTSNode parent;
        public float score = 0;
        public float totalPlayouts = 0;
        public int player;
        public int x;
        public int y;

        private int size;
        private int[,] boardState;

        public TicTacToeMCTSNode(int size)
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

        public float UCB1()
        {
            float N = 0.0f;
            if (parent != null)
            {
                N = parent.totalPlayouts;
            }
            return (score / totalPlayouts) + Mathf.Sqrt(2.0f * Mathf.Log(N) / totalPlayouts);
        }

        public void Update(bool currentPlayer, float value)
        {
            totalPlayouts++;
            if (currentPlayer)
            {
                score += value;
            }

            if (parent != null)
            {
                parent.Update(!currentPlayer, 1.0f - value);
            }
        }

        public void PopulateChildren()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (boardState[i, j] == 0)
                    {
                        //Change state
                        boardState[i, j] = player + 1;
                        TicTacToeMCTSNode child = new TicTacToeMCTSNode(size);
                        child.parent = this;
                        child.BoardState = boardState;
                        child.x = i;
                        child.y = j;
                        child.player = (player + 1) % 2;
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

    public void ResetBoard()
    {
        for(int i=0; i < tokens.Count; i++)
        {
            Destroy(tokens[i]);
        }
        tokens.Clear();
        turns = 0;
        stopGame = false;
        activePlayer = 0;

        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                mainBoard[i, j] = 0;
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
