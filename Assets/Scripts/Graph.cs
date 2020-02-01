using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    private AlphabetNode rootNode;      // The root node is going to be the start or entrance node
    private int totalNodes;             // Number of nodes in the graph

    public Graph(StartNode startNode)
    {
        rootNode = startNode;
    }

    public void setRootNode(AlphabetNode node)
    {
        this.rootNode = node;
    }

    public AlphabetNode getRootNode()
    {
        return rootNode;
    }

    public int getNumberNodes()
    {
        return totalNodes;
    }

    // Method that will create the different nodes in the graphs and will organize them using the production rules
    public void GenerateMission(int minTaskNumber, int maxTaskNumber, int minOrganizeTaskTries, int maxOrganizeTaskTries, float probabiltyApplyOrganizationRule)
    {
        // Randomly choose the number task nodes
        int numberTaskNodes = Random.Range(minTaskNumber, maxTaskNumber);
        // Set the number of total nodes in the graph based on the number of task nodes plus the entance and goal nodes
        totalNodes = numberTaskNodes + 2;
        // Randomly choose the number of times the algorithm will try to apply the reorganize tasks production rules
        int numberOrganizeTaskTries = Random.Range(minOrganizeTaskTries, maxOrganizeTaskTries);
        // The graph always start with the start mission production rule
        ProductionRules.StartMission((StartNode)rootNode, this);
        // The first node to the right of the root of the graph is going to be always a task node
        TaskNode currentTaskNode = (TaskNode)rootNode.getConnection(Direction.Right);
        // The node to the right of the first task node is going to be always the goal node
        GoalNode goalNode = (GoalNode)currentTaskNode.getConnection(Direction.Right);
        // The first step to create the mission is to add all the tasks one by one
        for (int i = 0; i < numberTaskNodes; i++)
        {
            ProductionRules.AddTask(currentTaskNode, goalNode);
            currentTaskNode = (TaskNode)goalNode.getConnection(Direction.Left);
        }
        // The next and final step is to reorganize the tasks position starting from the right of the root node, at this stage the root is always going to be an entrance node and no rules can be applied to it
        AlphabetNode currentNode = rootNode.getConnection(Direction.Right);
        while (numberOrganizeTaskTries > 0)
        {
            // Decide whether to try apply rule from current node or go to the next one
            if (currentNode is TaskNode && !currentNode.isTerminal() && Random.Range(0.0f, 1.0f) < probabiltyApplyOrganizationRule)
            {
                // Get number of nodes from the current that are also non terminal task nodes towards the right
                AlphabetNode nextRightNode = currentNode.getConnection(Direction.Right);
                List<AlphabetNode> connections = new List<AlphabetNode>();
                // The biggest number of task nodes taken by a production rule is six
                for (int i = 0; i < 6; i++)
                {
                    if (nextRightNode != null && nextRightNode is TaskNode && !nextRightNode.isTerminal())
                    {
                        connections.Add(nextRightNode);
                        nextRightNode = nextRightNode.getConnection(Direction.Right);
                    }
                }
                // Apply production rules based on the number of right task connections from current node, when multiple rules can be applied, all of them have the same probability of being picked
                switch (connections.Count)
                {
                    case 2:
                        // Only one rule can be applied
                        ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        break;
                    case 3:
                        // Two rules can be applied
                        if (Random.Range(0.0f, 1.0f) > 0.5f)
                        {
                            ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        }
                        else
                        {
                            ProductionRules.ReorganizeFourTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2]);
                        }
                        break;
                    case 4:
                        // Three rules can be applied
                        float random = Random.Range(0.0f, 1.0f);
                        if (random < (1.0f / 3.0f))
                        {
                            ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        }
                        else if (random > (2.0f / 3.0f))
                        {
                            ProductionRules.ReorganizeFourTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2]);
                        }
                        else
                        {
                            ProductionRules.ReorganizeFiveTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2], (TaskNode)connections[3]);
                        }
                        break;
                    case 5:
                        // Four rules can be applied
                        random = Random.Range(0.0f, 1.0f);
                        if (random < 0.25f)
                        {
                            ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        }
                        else if (random > 0.25f && random < 0.5f)
                        {
                            ProductionRules.ReorganizeFourTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2]);
                        }
                        else if (random > 0.75f)
                        {
                            ProductionRules.ReorganizeFiveTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2], (TaskNode)connections[3]);
                        }
                        else
                        {
                            ProductionRules.ReorganizeSixTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1], (TaskNode)connections[2], (TaskNode)connections[3], (TaskNode)connections[4]);
                        }
                        break;
                }
            }
            // Get next node, try right first and down second
            AlphabetNode previousCurrent = currentNode;
            currentNode = currentNode.getConnection(Direction.Right);
            if (currentNode == null)
            {
                currentNode = previousCurrent.getConnection(Direction.Down);
            }
            // If the goal node is reached, try to apply the productions rules from the beginnig
            if (currentNode is GoalNode || currentNode == null)
            {
                currentNode = rootNode;
            }
            // Decrease tries
            numberOrganizeTaskTries--;
        }
    }
}
