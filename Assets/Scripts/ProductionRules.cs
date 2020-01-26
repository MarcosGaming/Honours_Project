using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProductionRules
{
    public static void StartMission(StartNode S,Graph graph)
    {
        // Create RHS nodes
        EntranceNode e = new EntranceNode();
        TaskNode T = new TaskNode();
        GoalNode g = new GoalNode();
        // Connect the new nodes
        e.setConnection(Direction.Right, T);
        T.setConnection(Direction.Left, e);
        T.setConnection(Direction.Right, g);
        g.setConnection(Direction.Left, T);
        // Set the entrance node to be the root of the graph
        graph.setRootNode(e);
        S = null;
    }

    public static void AddTask(TaskNode T0, GoalNode g)
    {
        // Create new task node
        AlphabetNode T1 = new TaskNode();
        // Reorganize the connections
        T0.setConnection(Direction.Right, T1);
        T1.setConnection(Direction.Left, T0);
        T1.setConnection(Direction.Right, g);
        g.setConnection(Direction.Left, T1);
    }

    public static void ReorganizeThreeTasks(TaskNode T0, TaskNode T1, TaskNode T2)
    {
        // Remove connections
        T0.removeConnection(Direction.Right);
        T1.removeConnection(Direction.Left);
        T1.removeConnection(Direction.Right);
        T2.removeConnection(Direction.Left);
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T2);
        T1.setConnection(Direction.Down, T0);
        T2.setConnection(Direction.Up, T0);
        // Set terminal nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
    }

    public static void ReorganizeFourTasks(TaskNode T0, TaskNode T1, TaskNode T2, TaskNode T3)
    {
        // Remove connections
        T0.removeConnection(Direction.Right);
        T1.removeConnection(Direction.Left);
        T1.removeConnection(Direction.Right);
        T2.removeConnection(Direction.Left);
        T2.removeConnection(Direction.Right);
        T3.removeConnection(Direction.Left);
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T3);
        T1.setConnection(Direction.Down, T0);
        T1.setConnection(Direction.Right, T2);
        T2.setConnection(Direction.Left, T1);
        T3.setConnection(Direction.Up, T0);
        // Set terminal nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        T3.setAsTerminal();
    }

    public static void ReorganizeFiveTasks(TaskNode T0, TaskNode T1, TaskNode T2, TaskNode T3, TaskNode T4)
    {
        // Two ways of reorganizing five tasks with 50% chance each
        bool additionalConnection = false;
        if(Random.Range(0.0f, 1.0f) > 0.5f)
        {
            additionalConnection = true;
        }
        // Remove connections
        T0.removeConnection(Direction.Right);
        T1.removeConnection(Direction.Left);
        T1.removeConnection(Direction.Right);
        T2.removeConnection(Direction.Left);
        T2.removeConnection(Direction.Right);
        T3.removeConnection(Direction.Left);
        T3.removeConnection(Direction.Right);
        T4.removeConnection(Direction.Left);
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T3);
        T1.setConnection(Direction.Down, T0);
        T1.setConnection(Direction.Right, T2);
        T2.setConnection(Direction.Left, T1);
        T3.setConnection(Direction.Up, T0);
        T3.setConnection(Direction.Right, T4);
        T4.setConnection(Direction.Left, T4);
        if (additionalConnection)
        {
            T4.setConnection(Direction.Up, T2);
            T2.setConnection(Direction.Down, T4);
        }
        // Set terminal nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        T3.setAsTerminal();
        if(additionalConnection)
        {
            T4.setAsTerminal();
        }
    }

    public static void ReorganizeSixTasks(TaskNode T0, TaskNode T1, TaskNode T2, TaskNode T3, TaskNode T4, TaskNode T5)
    {
        // Two ways of reorganizing six tasks with 50% chance each
        bool additionalConnection = false;
        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            additionalConnection = true;
        }
        // Remove connections
        T0.removeConnection(Direction.Right);
        T1.removeConnection(Direction.Left);
        T1.removeConnection(Direction.Right);
        T2.removeConnection(Direction.Left);
        T2.removeConnection(Direction.Right);
        T3.removeConnection(Direction.Left);
        T3.removeConnection(Direction.Right);
        T4.removeConnection(Direction.Left);
        T4.removeConnection(Direction.Right);
        T5.removeConnection(Direction.Left);
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T4);
        T1.setConnection(Direction.Down, T0);
        T1.setConnection(Direction.Right, T2);
        T2.setConnection(Direction.Left, T1);
        T2.setConnection(Direction.Right, T3);
        T3.setConnection(Direction.Left, T2);
        T4.setConnection(Direction.Up, T0);
        T4.setConnection(Direction.Right, T5);
        T5.setConnection(Direction.Left, T4);
        if (additionalConnection)
        {
            T5.setConnection(Direction.Up, T2);
            T2.setConnection(Direction.Down, T5);
        }
        // Set terminal nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        T3.setAsTerminal();
        T4.setAsTerminal();
        if (additionalConnection)
        {
            T5.setAsTerminal();
        }
    }
}


