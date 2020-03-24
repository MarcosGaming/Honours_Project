using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for all the nodes that will be used in the production rules of the graph grammar
public abstract class AlphabetNode
{
    // To make the dungeon generation using grammars approach feasable using the system developed for the other two algorithms, a node in the graph is limited to have four connections
    private AlphabetNode leftConnection;
    private AlphabetNode rightConnection;
    private AlphabetNode upConnection;
    private AlphabetNode downConnection;
    // Whether the node is terminal or non-terminal
    protected bool terminal;

    public void setConnection(Direction dir, AlphabetNode node)
    {
        switch(dir)
        {
            case Direction.Right:
                rightConnection = node;
                break;
            case Direction.Left:
                leftConnection = node;
                break;
            case Direction.Up:
                upConnection = node;
                break;
            case Direction.Down:
                downConnection = node;
                break;
        }
    }

    public AlphabetNode getConnection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Right:
                return rightConnection;
            case Direction.Left:
                return leftConnection;
            case Direction.Up:
                return upConnection;
            case Direction.Down:
                return downConnection;
        }
        return null;
    }

    public void removeConnection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Right:
                rightConnection = null;
                break;
            case Direction.Left:
                leftConnection = null;
                break;
            case Direction.Up:
                upConnection = null;
                break;
            case Direction.Down:
                downConnection = null;
                break;
        }
    }

    public void setAsTerminal()
    {
        terminal = true;
    }

    public void setAsNonTerminal()
    {
        terminal = false;
    }

    public bool isTerminal()
    {
        return terminal;
    }
}

// Start symbol from wich the grammar will generate the mision, always non-terminal
public class StartNode : AlphabetNode
{
    public StartNode()
    {
        this.terminal = false;
    }
}

// Starting room for the player, always a terminal node
public class EntranceNode : AlphabetNode
{
    public EntranceNode()
    {
        this.terminal = true;
    }
}

// Node indicating a task (for this project, all the tasks are simply going to be rooms of different sizes), this node can be terminal or non-terminal
public class TaskNode : AlphabetNode
{
    public TaskNode()
    {
        // Default terminality of a task node is non-terminal
        this.terminal = false;
    }
}

// Node indicating a task that is connected to another task but that they are far away from each other
public class FarTaskNode : AlphabetNode
{
    public FarTaskNode()
    {
        // Default terminality of a far task node is terminal
        this.terminal = true;
    }
}

// Room that the player needs to reach to progress to the next level, always a terminal node
public class GoalNode : AlphabetNode
{
    public GoalNode()
    {
        this.terminal = true;
    }
}