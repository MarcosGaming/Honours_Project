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
    }

    public static void AddTask(TaskNode T0, GoalNode g)
    {
        // Remove connections 
        T0.removeConnection(Direction.Right);
        g.removeConnection(Direction.Left);
        // Create new task node
        AlphabetNode T1 = new TaskNode();
        // Reorganize connections
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
        // Modify nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T2);
        T1.setConnection(Direction.Down, T0);
        T2.setConnection(Direction.Up, T0);
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
        // Modify nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        T3.setAsTerminal();
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T3);
        T1.setConnection(Direction.Down, T0);
        T1.setConnection(Direction.Right, T2);
        T2.setConnection(Direction.Left, T1);
        T3.setConnection(Direction.Up, T0);
    }

    public static void ReorganizeFiveTasks(TaskNode T0, TaskNode T1, TaskNode T2, TaskNode T3, TaskNode T4)
    {
        // Remove connections
        T0.removeConnection(Direction.Right);
        T1.removeConnection(Direction.Left);
        T1.removeConnection(Direction.Right);
        T2.removeConnection(Direction.Left);
        T2.removeConnection(Direction.Right);
        T3.removeConnection(Direction.Left);
        T3.removeConnection(Direction.Right);
        T4.removeConnection(Direction.Left);
        // Modify nodes
        T0.setAsTerminal();
        T1.setAsTerminal();
        T2.setAsTerminal();
        T3.setAsTerminal();
        // Reorganize connections
        T0.setConnection(Direction.Up, T1);
        T0.setConnection(Direction.Down, T3);
        T1.setConnection(Direction.Down, T0);
        T1.setConnection(Direction.Right, T2);
        T2.setConnection(Direction.Left, T1);
        T3.setConnection(Direction.Up, T0);
        T3.setConnection(Direction.Right, T4);
        T4.setConnection(Direction.Left, T4);
    }

    public static void ReorganizeSixTasks(TaskNode T0, TaskNode T1, TaskNode T2, TaskNode T3, TaskNode T4, TaskNode T5)
    {
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
        // Two ways of reorganizing six tasks with 50% chance each
        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            // Modify nodes
            T0.setAsTerminal();
            T1.setAsTerminal();
            T2.setAsTerminal();
            T3.setAsTerminal();
            T4.setAsTerminal();
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
        }
        else
        {
            // Modify nodes
            T0.setAsTerminal();
            T1.setAsTerminal();
            T3.setAsTerminal();
            // Add new nodes
            FarTaskNode T2new = new FarTaskNode();
            FarTaskNode T4new = new FarTaskNode();
            // Reorganize connections
            T0.setConnection(Direction.Up, T1);
            T0.setConnection(Direction.Down, T3);
            T1.setConnection(Direction.Down, T0);
            T1.setConnection(Direction.Right, T2new);
            T2new.setConnection(Direction.Left, T1);
            T2new.setConnection(Direction.Down, T4new);
            T3.setConnection(Direction.Up, T0);
            T3.setConnection(Direction.Right, T4new);
            T4new.setConnection(Direction.Up, T2new);
            T4new.setConnection(Direction.Right, T5);
            T5.setConnection(Direction.Left, T4new);
        }
    }
}


