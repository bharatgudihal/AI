using System;
using System.Collections;
using System.Collections.Generic;
using CheckersNS;
using UnityEngine;
using UnityEngine.UI;

public class Checkers : MonoBehaviour {

    private int[,] mainBoard;
    private int activePlayer = 0;
    private bool stopGame = false;
    private List<GameObject> player1Tokens;
    private List<GameObject> player2Tokens;
    private CustomLogWriter logger;    
    private int player1Wins = 0;
    private int player2Wins = 0;
    private float minSize = 3.0f;
    private int size = 8;
    private int numberOfTokens = 12;

    [SerializeField]
    private GameObject player1Token;
    [SerializeField]
    private GameObject player2Token;
    [SerializeField]
    private GameObject whiteTile;
    [SerializeField]
    private GameObject brownTile;
    [SerializeField]
    private int currentIteration = 0;
    [SerializeField]
    private int numberOfIterations;    
    [SerializeField]
    private bool doubleAI;
    [SerializeField]
    private Text gameText;

    // Use this for initialization
    void Start () {

        mainBoard = new int[size, size];

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
                    tile = Instantiate(brownTile);
                }
                tile.transform.position = new Vector3(i, j, 0.0f);
                tile.transform.parent = transform;
            }
        }

        player1Tokens = new List<GameObject>();
        player2Tokens = new List<GameObject>();

        for(int i = 0; i < numberOfTokens; i++)
        {
            player1Tokens.Add(Instantiate(player1Token));
            player2Tokens.Add(Instantiate(player2Token));
        }

        float cameraZ = Camera.main.transform.position.z;
        Camera.main.transform.position = new Vector3(size / 2, size / 2, cameraZ);
        logger = gameObject.AddComponent<CustomLogWriter>();
        logger.filePath = "Tic_Tac_Toe_" + (doubleAI ? "AI_v_AI_" : "") + (size + "x" + size + "_") + numberOfIterations;

        ResetBoard();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        return false;
    }

    //Reference:https://www.youtube.com/watch?v=UXW2yZndl7U
    private MCTSNode RunMCTS(int player, float modifier)
    {
        return null;
    }

    public void ResetBoard()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                mainBoard[i, j] = -1;
            }
        }

        int currentToken = 0;
        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                if((i + j) % 2 == 1)
                {
                    player1Tokens[currentToken].transform.position = new Vector3(i, j, -1.0f);
                    currentToken++;
                    mainBoard[i, j] = 0;
                }
            }
        }

        currentToken = 0;
        for(int i = 0; i < size; i++)
        {
            for(int j = 5; j < 8; j++)
            {
                if ((i + j) % 2 == 1)
                {
                    player2Tokens[currentToken].transform.position = new Vector3(i, j, -1.0f);
                    currentToken++;
                    mainBoard[i, j] = 1;
                }
            }
        }

        stopGame = false;
        activePlayer = 0;               
    }

    public void WriteLog()
    {
        logger.Write("Total games: " + currentIteration);
        logger.Write("Player 1 wins: " + player1Wins);
        logger.Write("Player 2 wins: " + player2Wins);
        logger.Write("Total draws: " + (currentIteration - (player1Wins + player2Wins)));
    }
}

namespace CheckersNS
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