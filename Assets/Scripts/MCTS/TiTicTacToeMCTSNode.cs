using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeMCTSNode
{
    public List<TicTacToeMCTSNode> children = new List<TicTacToeMCTSNode>();
    public TicTacToeMCTSNode parent;
    public float successfulPlayouts = 0;
    public float totalPlayouts = 0;
    public int player;
    public int x;
    public int y;

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
        if (parent != null)
        {
            N = parent.totalPlayouts;
        }
        return (successfulPlayouts / totalPlayouts) + Mathf.Sqrt(2.0f * Mathf.Log(N) / totalPlayouts);
    }

    public void Update(bool currentPlayer)
    {
        totalPlayouts++;
        if (currentPlayer)
        {
            successfulPlayouts++;
        }

        if (parent != null)
        {
            parent.Update(!currentPlayer);
        }
    }

    public void PopulateChildren()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (boardState[i, j] == 0)
                {
                    //Change state
                    boardState[i, j] = player + 1;
                    TicTacToeMCTSNode child = new TicTacToeMCTSNode();
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
