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
    private List<Token> player1Tokens;
    private List<Token> player2Tokens;
    private CustomLogWriter logger;    
    private int player1Wins = 0;
    private int player2Wins = 0;
    private int size = 8;
    private int numberOfTokens = 12;
    private int moveCount;

    private enum PlayState
    {
        SELECTION,
        PLACEMENT
    }

    private PlayState currentPlayState;
    private Token selectedToken;

    [SerializeField]
    private GameObject player1Token;
    [SerializeField]
    private GameObject player2Token;
    [SerializeField]
    private GameObject whiteTile;
    [SerializeField]
    private GameObject blackTile;
    [SerializeField]
    private Text gameText;
    [SerializeField]
    private int currentIteration = 0;
    [SerializeField]
    private int numberOfIterations;    
    [SerializeField]
    private bool doubleAI;
    [SerializeField]
    private int maxMinimaxDepth;
    [SerializeField]
    private int maxMCTSTime;
    [SerializeField]
    private int maxNumberOfMoves;

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
                    tile = Instantiate(blackTile);
                }
                tile.transform.position = new Vector3(i, j, 0.0f);
                tile.transform.parent = transform;
            }
        }

        player1Tokens = new List<Token>();
        player2Tokens = new List<Token>();

        for(int i = 0; i < numberOfTokens; i++)
        {
            player1Tokens.Add(new Token(Instantiate(player1Token), 0));
            player2Tokens.Add(new Token(Instantiate(player2Token), 1));
        }

        float cameraZ = Camera.main.transform.position.z;
        Camera.main.transform.position = new Vector3(size / 2, size / 2, cameraZ);
        //logger = gameObject.AddComponent<CustomLogWriter>();
        //logger.filePath = "Checkers_" + (doubleAI ? "AI_v_AI_" : "") + (size + "x" + size + "_") + numberOfIterations;

        ResetBoard();
    }

    // Update is called once per frame
    void Update()
    {
        if (!stopGame)
        {
            if (activePlayer == 0)
            {
                if (Input.GetMouseButtonDown(0) && !doubleAI)
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (currentPlayState == PlayState.SELECTION)
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Token")))
                        {
                            selectedToken = GetSelectedToken(player1Tokens, hit.transform.position);
                            if (selectedToken != null && selectedToken.IsAlive)
                            {
                                currentPlayState = PlayState.PLACEMENT;
                            }
                            else
                            {
                                selectedToken = null;
                            }
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Board")))
                        {
                            Vector3 hitPosition = hit.transform.position;
                            if (IsValidMovePosition(hitPosition, selectedToken, mainBoard))
                            {
                                UpdateSelectedTokenPosition(hitPosition, selectedToken, mainBoard);
                                stopGame = CheckForWin(mainBoard) != -1;
                                activePlayer = (activePlayer + 1) % 2;
                                moveCount++;
                            }
                            else if (IsValidJumpPosition(hitPosition, selectedToken, mainBoard))
                            {
                                RemoveJumpedToken(hitPosition, selectedToken, player2Tokens, mainBoard);
                                UpdateSelectedTokenPosition(hitPosition, selectedToken, mainBoard);
                                stopGame = CheckForWin(mainBoard) != -1;
                                activePlayer = (activePlayer + 1) % 2;
                                moveCount++;
                            }
                        }
                    }
                }

                if(doubleAI)
                {
                    //AI code
                    CheckersNode node = RunMCTS(activePlayer);
                    Vector3 tokenPosition = new Vector3(node.xStart, node.yStart, -1.0f);
                    Vector3 movePosition = new Vector3(node.xEnd, node.yEnd);
                    Token tokenToMove = GetSelectedToken(player2Tokens, tokenPosition);

                    Debug.Assert(tokenToMove.IsAlive);

                    if (node.isJump)
                    {
                        RemoveJumpedToken(movePosition, tokenToMove, player1Tokens, mainBoard);
                    }

                    UpdateSelectedTokenPosition(movePosition, tokenToMove, mainBoard);
                    stopGame = CheckForWin(mainBoard) != -1;
                    activePlayer = (activePlayer + 1) % 2;
                    moveCount++;
                }
            }
            else
            {
                //AI code
                //CheckersNode node = RunMinimax(activePlayer);
                CheckersNode node = RunMCTS(activePlayer);
                Vector3 tokenPosition = new Vector3(node.xStart, node.yStart, -1.0f);
                Vector3 movePosition = new Vector3(node.xEnd, node.yEnd);
                Token tokenToMove = GetSelectedToken(player2Tokens, tokenPosition);

                Debug.Assert(tokenToMove.IsAlive);

                if (node.isJump)
                {
                    RemoveJumpedToken(movePosition, tokenToMove, player1Tokens, mainBoard);                    
                }

                UpdateSelectedTokenPosition(movePosition, tokenToMove, mainBoard);
                stopGame = CheckForWin(mainBoard) != -1;
                activePlayer = (activePlayer + 1) % 2;
                moveCount++;
            }

            gameText.text = "Player " + (activePlayer + 1);

            if(moveCount > maxNumberOfMoves)
            {
                stopGame = true;
            }
        }
	}

    private void LateUpdate()
    {
        GameObject[] dummyObjects = GameObject.FindGameObjectsWithTag("Dummy");
        if(dummyObjects != null)
        {
            for(int i = 0; i < dummyObjects.Length; i++)
            {
                //DestroyImmediate(dummyObjects[i], true);
            }
        }
    }

    private void RemoveJumpedToken(Vector3 jumpPosition, Token jumpingToken, List<Token> opponentTokens, int[,] board)
    {
        int x = (int)jumpPosition.x;
        int y = (int)jumpPosition.y;
        int tokenX = jumpingToken.xPosition;
        int tokenY = jumpingToken.yPosition;
        int midX = (tokenX + x) / 2;
        int midY = (tokenY + y) / 2;

        Vector3 position = new Vector3(midX, midY, -1.0f);
        Token token = null;
        token = GetSelectedToken(opponentTokens, position);

        token.IsAlive = false;
        board[midX, midY] = -1;
    }

    private void RevertJumpedToken(Vector3 jumpPosition, Token jumpingToken, List<Token> opponentTokens, int[,] board)
    {
        int x = (int)jumpPosition.x;
        int y = (int)jumpPosition.y;
        int tokenX = jumpingToken.xPosition;
        int tokenY = jumpingToken.yPosition;
        int midX = (tokenX + x) / 2;
        int midY = (tokenY + y) / 2;

        Vector3 position = new Vector3(midX, midY, -1.0f);
        Token token = null;
        token = GetDeadToken(opponentTokens, position);

        Debug.Assert(token != null);

        token.IsAlive = true;
        board[midX, midY] = token.player;
    }

    private bool IsValidJumpPosition(Vector3 position, Token token, int[,] board)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int tokenX = token.xPosition;
        int tokenY = token.yPosition;
        int midX = (tokenX + x) / 2;
        int midY = (tokenY + y) / 2;
        bool result = false;
        int opponent = (token.player + 1) % 2;

        if (IsOnBoard(x, y) && board[x,y] == -1 && board[midX, midY] == opponent)
        {            
            if (token.IsKing)
            {
                int manhattanDistance = Math.Abs(tokenX - x) + Math.Abs(tokenY - y);
                

                if (manhattanDistance == 4)
                {
                    result = true;
                }
            }
            else
            {
                if (token.player == 0)
                {
                    if (tokenY < y)
                    {
                        int manhattanDistance = Math.Abs(tokenX - x) + y - tokenY;
                        if (manhattanDistance == 4)
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    if (tokenY > y)
                    {
                        int manhattanDistance = Math.Abs(tokenX - x) + tokenY - y;
                        if (manhattanDistance == 4)
                        {
                            result = true;
                        }
                    }
                }
            }
        }

        return result;
    }

    private void UpdateSelectedTokenPosition(Vector3 position, Token token, int[,] board)
    {
        int oldX = token.xPosition;
        int oldY = token.yPosition;
        int newX = (int)position.x;
        int newY = (int)position.y;
        board[oldX, oldY] = -1;
        board[newX, newY] = token.player;
        token.gameObject.transform.position = new Vector3(position.x, position.y, -1);
        currentPlayState = PlayState.SELECTION;
    }

    private bool IsValidMovePosition(Vector3 position, Token token, int[,] board)
    {
        int x = (int)position.x;
        int y = (int)position.y;
        int tokenX = token.xPosition;
        int tokenY = token.yPosition;
        bool result = false;

        //Is position open?
        if (IsOnBoard(x, y) && board[x, y] == -1)
        {
            //Is the token king?
            if (token.IsKing)
            {
                int manhattanDistance = Math.Abs(tokenX - x) + Math.Abs(tokenY - y);
                //Is it 1 diagonal space in any direction?
                if (manhattanDistance == 2)
                {
                    result = true;
                }
            }
            else
            {
                if(token.player == 0)
                {
                    if(tokenY < y)
                    {
                        int manhattanDistance = Math.Abs(tokenX - x) + y - tokenY;
                        if (manhattanDistance == 2)
                        {
                            result = true;
                        }
                    }
                }
                else
                {
                    if (tokenY > y)
                    {
                        int manhattanDistance = Math.Abs(tokenX - x) + tokenY - y;
                        if (manhattanDistance == 2)
                        {
                            result = true;
                        }
                    }
                }
            }
        }

        return result;
    }

    private bool IsOnBoard(int x, int y)
    {
        return x > -1 && x < size && y > -1 && y < size;
    }

    private Token GetSelectedToken(List<Token> playerTokens, Vector3 position)
    {
        Token result = null;
        for(int i = 0; i < playerTokens.Count; i++)
        {
            Token token = playerTokens[i];            
            if(token.IsAlive && token.xPosition == position.x && token.yPosition == position.y)
            {
                result = token;
                break;
            }
        }
        return result;
    }

    private Token GetDeadToken(List<Token> playerTokens, Vector3 position)
    {
        Token result = null;
        for (int i = 0; i < playerTokens.Count; i++)
        {
            Token token = playerTokens[i];
            if (!token.IsAlive && token.xPosition == position.x && token.yPosition == position.y)
            {
                result = token;
                break;
            }
        }
        return result;
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

    private int CheckForWin(int[,] board)
    {
        int winner = -1;
        int player1TokenCount = 0;
        int player2TokenCount = 0;
        for (int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                if(board[i,j] == 0)
                {
                    player1TokenCount++;
                }else if (board[i, j] == 1)
                {
                    player2TokenCount++;
                }
            }
        }

        if(player1TokenCount == 0)
        {
            winner = 1;
        }else if(player2TokenCount == 0)
        {
            winner = 0;
        }

        return winner;
    }

    //Reference:https://www.youtube.com/watch?v=UXW2yZndl7U
    private CheckersNode RunMCTS(int player)
    {
        CheckersNode root = null;
        if (player == 0)
        {
            root = new CheckersNode(size, player, null, player1Tokens, player2Tokens);
        }
        else
        {
            root = new CheckersNode(size, player, null, player2Tokens, player1Tokens);
        }

        root.BoardState = mainBoard;
        DateTime start = DateTime.Now;

        while((DateTime.Now - start).TotalMilliseconds < maxMCTSTime)
        {
            CheckersNode current = root;

            //Selection
            while (current.children.Count != 0)
            {
                current = current.GetChildWithBestUCB1();
            }

            //Simulation
            if (current.totalPlayouts == 0)
            {
                int score = Simulate(current, player);

                //Back Propagate
                current.BackPropagate(score);
            }
            else
            {
                //Expansion
                GenerateChildNodes(current);
            }
        }

        //Find node with best score
        float bestScore = root.children[0].GetScore() / root.children[0].totalPlayouts;
        int selectedIndex = 0;
        for (int i = 1; i < root.children.Count; i++)
        {
            float score = root.children[i].GetScore() / root.children[i].totalPlayouts;
            if (bestScore < score)
            {
                bestScore = score;
                selectedIndex = i;
            }
        }

        return root.children[selectedIndex];
    }

    private int Simulate(CheckersNode node, int winPlayer)
    {
        int[,] boardState = node.BoardState;
        List<Token> playerTokens = new List<Token>();
        List<Token> opponentTokens = new List<Token>();
        int currentPlayer = node.player;

        for (int i=0;i< node.playerTokens.Count; i++)
        {
            Token playerToken = new Token(node.playerTokens[i]);
            Token opponentToken = new Token(node.opponentTokens[i]);
            playerTokens.Add(playerToken);
            opponentTokens.Add(opponentToken);
        }

        int score = 0;
        int currentMoveCount = moveCount;
        int winner = -1;
        bool instaLose = false;
        while(winner == -1 && currentMoveCount < maxNumberOfMoves)
        {
            int index;
            Vector3 destination;
            bool isJump;

            //Get random move
            GetRandomMove(boardState, playerTokens, out index, out destination, out isJump);
            if(index == -1)
            {
                //insta lose fam
                instaLose = true;
                break;
            }

            //Move
            Token token = playerTokens[index];
            if (isJump)
            {
                RemoveJumpedToken(destination, token, opponentTokens, boardState);
            }
                        
            int originalX = token.xPosition;
            int originalY = token.yPosition;
            boardState[originalX, originalY] = -1;
            boardState[(int)destination.x, (int)destination.y] = token.player;
            token.gameObject.transform.position = destination;

            //Swap players
            currentPlayer = (currentPlayer + 1) % 2;
            List<Token> temp = new List<Token>(playerTokens);
            playerTokens.Clear();
            playerTokens.AddRange(opponentTokens);
            opponentTokens.Clear();
            opponentTokens.AddRange(temp);
            temp.Clear();

            winner = CheckForWin(boardState);
            currentMoveCount++;
        }

        if (instaLose)
        {
            if (currentPlayer == winPlayer)
            {
                score = 0;
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (boardState[i, j] == winPlayer)
                        {
                            score++;
                        }
                    }
                }
            }
        }
        else if (winPlayer == winner)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (boardState[i, j] == winPlayer)
                    {
                        score++;
                    }
                }
            }
        }

        return score;
    }

    struct Move
    {
        public int tokenIndex;
        public Vector3 destination;
        public bool isJump;
    };

    private void GetRandomMove(int[,] boardState, List<Token> playerTokens, out int index, out Vector3 destination, out bool isJump)
    {
        index = -1;
        destination = new Vector3();
        isJump = false;
        List<Move> moves = new List<Move>();
        
        //Find all possible moves
        for(int i = 0; i < playerTokens.Count; i++)
        {
            //Check moves
            Token token = playerTokens[i];
            if (token.IsAlive)
            {
                bool hasJumped = false;
                //Check jumps
                int x = token.xPosition - 2;
                int y = token.yPosition + (token.player == 0 ? 2 : -2);
                Vector3 position = new Vector3(x, y);
                if (IsValidJumpPosition(position, token, boardState))
                {
                    Move move = new Move();
                    move.destination = position;
                    move.isJump = true;
                    move.tokenIndex = i;
                    moves.Add(move);
                    hasJumped = true;
                }

                x = token.xPosition + 2;
                position = new Vector3(x, y);
                if (IsValidJumpPosition(position, token, boardState))
                {
                    Move move = new Move();
                    move.destination = position;
                    move.isJump = true;
                    move.tokenIndex = i;
                    moves.Add(move);
                    hasJumped = true;
                }

                //We always want to prioritise jumping
                if (!hasJumped)
                {
                    x = token.xPosition - 1;
                    y = token.yPosition + (token.player == 0 ? 1 : -1);
                    position = new Vector3(x, y);
                    if (IsValidMovePosition(position, token, boardState))
                    {
                        Move move = new Move();
                        move.destination = position;
                        move.isJump = false;
                        move.tokenIndex = i;
                        moves.Add(move);
                    }

                    x = token.xPosition + 1;
                    position = new Vector3(x, y);
                    if (IsValidMovePosition(position, token, boardState))
                    {
                        Move move = new Move();
                        move.destination = position;
                        move.isJump = false;
                        move.tokenIndex = i;
                        moves.Add(move);
                    }
                }

                hasJumped = false;

                if (token.IsKing)
                {
                    //Check king jumps
                    x = token.xPosition - 2;
                    y = token.yPosition + (token.player == 0 ? -2 : 2);
                    position = new Vector3(x, y);
                    if (IsValidJumpPosition(position, token, boardState))
                    {
                        Move move = new Move();
                        move.destination = position;
                        move.isJump = true;
                        move.tokenIndex = i;
                        moves.Add(move);
                        hasJumped = true;
                    }

                    x = token.xPosition + 2;
                    position = new Vector3(x, y);
                    if (IsValidJumpPosition(position, token, boardState))
                    {
                        Move move = new Move();
                        move.destination = position;
                        move.isJump = true;
                        move.tokenIndex = i;
                        moves.Add(move);
                        hasJumped = true;
                    }

                    //We always want to prioritise jumping
                    if (!hasJumped)
                    {
                        //Check king moves
                        x = token.xPosition - 1;
                        y = token.yPosition + (token.player == 0 ? -1 : 1);
                        position = new Vector3(x, y);
                        if (IsValidMovePosition(position, token, boardState))
                        {
                            Move move = new Move();
                            move.destination = position;
                            move.isJump = false;
                            move.tokenIndex = i;
                            moves.Add(move);
                        }

                        x = token.xPosition + 1;
                        position = new Vector3(x, y);
                        if (IsValidMovePosition(position, token, boardState))
                        {
                            Move move = new Move();
                            move.destination = position;
                            move.isJump = false;
                            move.tokenIndex = i;
                            moves.Add(move);
                        }
                    }
                }
            }
        }

        if (moves.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, moves.Count);
            index = moves[randomIndex].tokenIndex;
            destination = moves[randomIndex].destination;
            isJump = moves[randomIndex].isJump;
        }
    }

    private void PrintBoard(int[,] board)
    {
        string output = "";
        for(int i = 0; i < size; i++)
        {
            string row = "";
            for(int j = 0; j < size; j++)
            {
                row = row + board[i, j] + "\t";
            }
            output = row + "\n" + output;
        }
        print(output);
    }


    //Reference:https://www.youtube.com/watch?v=zp3VMe0Jpf8
    private CheckersNode RunMinimax(int player)
    {
        CheckersNode root = null;
        if (player == 0)
        {
            root = new CheckersNode(size, player, null, player1Tokens, player2Tokens);
        }
        else
        {
            root = new CheckersNode(size, player, null, player2Tokens, player1Tokens);
        }
        
        root.BoardState = mainBoard;
        int depth = maxMinimaxDepth;
        Maximize(root, depth);
        return root.GetMaxChildNode();
    }

    private int Maximize(CheckersNode node, int depth)
    {
        if(depth == 0)
        {
            return node.GetScore();
        }

        GenerateChildNodes(node);
        for (int i = 0; i < node.children.Count; i++)
        {
            node.UpdateScore(true, Minimize(node.children[i], depth - 1));
        }

        return node.GetScore();
    }

    private int Minimize(CheckersNode node, int depth)
    {
        if (depth == 0)
        {
            return node.GetScore();
        }

        GenerateChildNodes(node);
        for (int i = 0; i < node.children.Count; i++)
        {
            node.UpdateScore(false, Maximize(node.children[i], depth - 1));
        }

        return node.GetScore();
    }

    private void AddMoveChildNode(Token token, int[,] board, int x, int y, Vector3 position, CheckersNode node, List<Token> opponentTokens, List<Token> playerTokens, bool isJump)
    {
        int originalX = token.xPosition;
        int originalY = token.yPosition;
        board[originalX, originalY] = -1;
        board[x, y] = token.player;
        token.gameObject.transform.position = position;

        CheckersNode childNode = new CheckersNode(size, (token.player + 1) % 2, node, opponentTokens, playerTokens);
        childNode.BoardState = board;
        childNode.xStart = originalX;
        childNode.yStart = originalY;
        childNode.xEnd = x;
        childNode.yEnd = y;
        childNode.isJump = isJump;

        board[originalX, originalY] = token.player;
        board[x, y] = -1;
        token.gameObject.transform.position = new Vector3(originalX, originalY, -1.0f);

        node.children.Add(childNode);
    }

    private void GenerateChildNodes(CheckersNode node)
    {
        int[,] board = node.BoardState;
        List<Token> playerTokens = new List<Token>();
        for (int i = 0; i < node.playerTokens.Count; i++)
        {
            playerTokens.Add(node.playerTokens[i]);
        }

        List<Token> opponentTokens = new List<Token>();
        for (int i = 0; i < node.opponentTokens.Count; i++)
        {
            opponentTokens.Add(node.opponentTokens[i]);
        }
        //Find all possible moveable token positions        
        for(int i = 0; i < playerTokens.Count; i++)
        {            
            Token token = playerTokens[i];
            if (token.IsAlive)
            {
                bool hasJumped = false;

                //Check jumps
                int x = token.xPosition - 2;
                int y = token.yPosition + (token.player == 0 ? 2 : -2);
                Vector3 position = new Vector3(x, y);
                if (IsValidJumpPosition(position, token, board))
                {
                    RemoveJumpedToken(position, token, opponentTokens, board);
                    AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, true);
                    RevertJumpedToken(position, token, opponentTokens, board);
                    hasJumped = true;
                }

                x = token.xPosition + 2;
                position = new Vector3(x, y);
                if (IsValidJumpPosition(position, token, board))
                {
                    RemoveJumpedToken(position, token, opponentTokens, board);
                    AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, true);
                    RevertJumpedToken(position, token, opponentTokens, board);
                    hasJumped = true;
                }

                //We always want to prioritise jumping
                if (!hasJumped)
                {
                    //Check moves
                    x = token.xPosition - 1;
                    y = token.yPosition + (token.player == 0 ? 1 : -1);
                    position = new Vector3(x, y);
                    if (IsValidMovePosition(position, token, board))
                    {
                        AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, false);
                    }

                    x = token.xPosition + 1;
                    position = new Vector3(x, y);
                    if (IsValidMovePosition(position, token, board))
                    {
                        AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, false);
                    }
                }

                hasJumped = false;

                if (token.IsKing)
                {

                    //Check king jumps
                    x = token.xPosition - 2;
                    y = token.yPosition + (token.player == 0 ? -2 : 2);
                    position = new Vector3(x, y);
                    if (IsValidJumpPosition(position, token, board))
                    {
                        RemoveJumpedToken(position, token, opponentTokens, board);
                        AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, true);
                        RevertJumpedToken(position, token, opponentTokens, board);
                        hasJumped = true;
                    }

                    x = token.xPosition + 2;
                    position = new Vector3(x, y);
                    if (IsValidJumpPosition(position, token, board))
                    {
                        RemoveJumpedToken(position, token, opponentTokens, board);
                        AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, true);
                        RevertJumpedToken(position, token, opponentTokens, board);
                        hasJumped = true;
                    }

                    if (!hasJumped)
                    {
                        //Check king moves
                        x = token.xPosition - 1;
                        y = token.yPosition + (token.player == 0 ? -1 : 1);
                        position = new Vector3(x, y);
                        if (IsValidMovePosition(position, token, board))
                        {
                            AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, false);
                        }

                        x = token.xPosition + 1;
                        position = new Vector3(x, y);
                        if (IsValidMovePosition(position, token, board))
                        {
                            AddMoveChildNode(token, board, x, y, position, node, opponentTokens, playerTokens, false);
                        }
                    }                    
                }
            }
        }        
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
                    player1Tokens[currentToken].gameObject.transform.position = new Vector3(i, j, -1.0f);
                    player1Tokens[currentToken].IsKing = false;
                    player1Tokens[currentToken].IsAlive = true;
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
                    player2Tokens[currentToken].gameObject.transform.position = new Vector3(i, j, -1.0f);
                    player2Tokens[currentToken].IsKing = false;
                    player2Tokens[currentToken].IsAlive = true;
                    currentToken++;
                    mainBoard[i, j] = 1;
                }
            }
        }

        stopGame = false;
        activePlayer = 0;
        currentPlayState = PlayState.SELECTION;

        gameText.text = "Player 1";
        moveCount = 0;
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
    public class Token
    {
        public GameObject gameObject;
        public int player;
        private bool isKing;        

        public Token(GameObject gameObject, int player)
        {
            this.gameObject = gameObject;
            this.player = player;
        }
        
        public Token(Token other)
        {
            gameObject = new GameObject();
            gameObject.name = "Dummy";
            gameObject.tag = "Dummy";
            gameObject.transform.position = other.gameObject.transform.position;            
            player = other.player;
            IsAlive = other.IsAlive;
            IsKing = other.IsKing;
        }

        public bool IsAlive
        {
            set
            {
                gameObject.SetActive(value);
            }

            get
            {
                return gameObject.activeSelf;
            }
        }

        public int xPosition
        {
            get
            {
                return (int)gameObject.transform.position.x;
            }
        }

        public int yPosition
        {
            get
            {
                return (int)gameObject.transform.position.y;
            }
        }

        public bool IsKing
        {
            get
            {
                if (!isKing)
                {
                    if(player == 0)
                    {
                        isKing = yPosition == 7;
                        return isKing;
                    }
                    else
                    {
                        isKing = yPosition == 0;
                        return isKing;
                    }
                }
                else
                {
                    return isKing;
                }
            }

            set
            {
                isKing = value;
            }
        }
        
    }

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

    public class CheckersNode
    {
        public int xStart;
        public int yStart;
        public int xEnd;
        public int yEnd;
        public List<CheckersNode> children = new List<CheckersNode>();
        public List<Token> playerTokens;
        public List<Token> opponentTokens;
        public int player;
        public bool isJump = false;
        public int totalPlayouts;

        private int size;        
        private int score;
        private int[,] boardState;
        private CheckersNode parent;
        private bool hasScoreBeenCalculated;

        public CheckersNode(int size, int player, CheckersNode parent, List<Token> playerTokens, List<Token> opponentTokens)
        {
            this.size = size;
            this.player = player;
            this.parent = parent;
            hasScoreBeenCalculated = false;
            score = 0;
            this.playerTokens = new List<Token>(playerTokens.Count);
            for(int i = 0; i < playerTokens.Count; i++)
            {
                Token token = new Token(playerTokens[i]);
                this.playerTokens.Add(token);
            }

            this.opponentTokens = new List<Token>(opponentTokens.Count);
            for (int i = 0; i < opponentTokens.Count; i++)
            {
                Token token = new Token(opponentTokens[i]);
                this.opponentTokens.Add(token);
            }
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

        public int GetScore()
        {
            if (!hasScoreBeenCalculated && boardState != null)
            {
                score = CalculateScore();
                hasScoreBeenCalculated = true;
            }
            return score;
        }

        internal float UCB1
        {
            get
            {
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

        internal CheckersNode GetChildWithBestUCB1()
        {
            float maxUCB1 = children[0].UCB1;
            int selectedIndex = 0;
            for (int i = 1; i < children.Count; i++)
            {
                if (maxUCB1 < children[i].UCB1)
                {
                    maxUCB1 = children[i].UCB1;
                    selectedIndex = i;
                }
            }
            return children[selectedIndex];
        }

        internal CheckersNode GetMaxChildNode()
        {
            int maxScore = children[0].GetScore();
            int selectedIndex = 0;

            for (int i = 1; i < children.Count; i++)
            {
                if(maxScore < children[i].GetScore())
                {
                    maxScore = children[i].GetScore();
                    selectedIndex = i;
                }
            }

            return children[selectedIndex];
        }

        internal CheckersNode GetMinChildNode()
        {
            int minScore = children[0].GetScore();
            int selectedIndex = 0;

            for (int i = 1; i < children.Count; i++)
            {
                if (minScore > children[i].GetScore())
                {
                    minScore = children[i].GetScore();
                    selectedIndex = i;
                }
            }

            return children[selectedIndex];
        }

        internal void UpdateScore(bool maximize, int newScore)
        {
            if (!hasScoreBeenCalculated)
            {
                score = newScore;
                hasScoreBeenCalculated = true;
            }
            else
            {
                if (maximize && score < newScore)
                {
                    score = newScore;
                }
                else if (score > newScore)
                {
                    score = newScore;
                }
            }
        }

        private int CalculateScore()
        {
            int playerScore = 0;
            int opponentScore = 0;
            int totalTokens = 0;

            for(int i = 0; i < size; i++)
            {
                for(int j = 0; j < size; j++)
                {
                    if(boardState[i,j] == player)
                    {
                        playerScore++;
                        totalTokens++;
                    }else if(boardState[i, j] != -1)
                    {
                        totalTokens++;
                    }
                }
            }

            opponentScore = totalTokens - playerScore;

            return playerScore - opponentScore;
        }

        internal void BackPropagate(int value)
        {
            totalPlayouts++;
            score += value;
            hasScoreBeenCalculated = true;

            if (parent != null)
            {
                parent.BackPropagate(value);
            }
        }
    }
}

