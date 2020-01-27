using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneratorGraphGrammar : MonoBehaviour
{
    [SerializeField] Vector3 dungeonTopLeftCellPosition;    // Top left position of dungeon cell [0,0]

    [SerializeField] int minTaskNumber;                     // The minimum number of tasks in the graph
    [SerializeField] int maxTaskNumber;                     // The maximum number of tasks in the graph

    [SerializeField] int minOrganizeTasksTries;             // The minimum number of tries to apply the reorganize production rules
    [SerializeField] int maxOrganizeTasksTries;             // The maximum number of tries to apply the reorganize production rules

    [Range(0.0f, 1.0f)]
    [SerializeField] float probabiltyApplyOrganizationRule; // The probablity of a reorganization rule to be applied

    [SerializeField] int roomMinTilesWidth;                 // Minimum number of column cells a room needs to have
    [SerializeField] int roomMaxTilesWidth;                 // Maximum number of column cells a room can have
    [SerializeField] int roomMinTilesHeight;                // Minimum number of row cells a room needs to have
    [SerializeField] int roomMaxTilesHeight;                // Maximum number of row cells a room can have

    [SerializeField] Material floorMaterial;                // Material that will be used for the floor
    [SerializeField] Vector3 floorTileDimensions;           // Dimensions of each floor tile
    [SerializeField] Material wallMaterial;                 // Material of the walls
    [SerializeField] float wallHeight;                      // Height of each wall

    private Graph graph;                                    // Graph containing all the nodes
    private StartNode S;                                    // Node form which the grammar will generate the mission

    private Dungeon dungeon;                                // Dungeon class which basically consits in a 2D array of cells

    private List<Corridor> corridors;                       // List with all the corridors in the dungeon


    // Start is called before the first frame update
    void Start()
    {
        // Make sure that the min room width is at least half the max room width

        // Make sure that the min room height is at least half the max room height

        // Build the dungeon
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildDungeon()
    {
        // Generate the graph with the mission
        graph.GenerateMission(minTaskNumber, maxTaskNumber, minOrganizeTasksTries, maxOrganizeTasksTries, probabiltyApplyOrganizationRule);
        // Transform the graph into the dungeon space

    }
}

public class Graph
{ 
    private AlphabetNode rootNode;      // The root node is going to be the start or entrance node

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

    // Method that will create the different nodes in the graphs and will organize them using the production rules
    public void GenerateMission(int minTaskNumber, int maxTaskNumber, int minOrganizeTaskTries, int maxOrganizeTaskTries, float probabiltyApplyOrganizationRule)
    {
        // Randomly choose the number task nodes
        int numberTaskNodes = Random.Range(minTaskNumber, maxTaskNumber);
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
        while(numberOrganizeTaskTries > 0)
        {
            // Decide whether to try apply rule from current node or go to the next one
            if(currentNode is TaskNode && !currentNode.isTerminal() && Random.Range(0.0f, 1.0f) > probabiltyApplyOrganizationRule)
            {
                // Get number of nodes from the current that are also non terminal task nodes towards the right
                AlphabetNode nextRightNode = currentNode.getConnection(Direction.Right);
                List<AlphabetNode> connections = new List<AlphabetNode>();
                // The biggest number of task nodes taken by a production rule is six
                for (int i = 0; i < 6; i++)
                {
                    if(nextRightNode != null && nextRightNode is TaskNode && !nextRightNode.isTerminal())
                    {
                        connections.Add(nextRightNode);
                        nextRightNode = nextRightNode.getConnection(Direction.Right);
                    }
                }
                // Apply production rules based on the number of right task connections from current node, when multiple rules can be applied, all of them have the same probability of being picked
                switch(connections.Count)
                {
                    case 2:
                        // Only one rule can be applied
                        ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        break;
                    case 3:
                        // Two rules can be applied
                        if(Random.Range(0.0f, 1.0f) > 0.5f)
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
                        if (random < (1.0f/3.0f))
                        {
                            ProductionRules.ReorganizeThreeTasks((TaskNode)currentNode, (TaskNode)connections[0], (TaskNode)connections[1]);
                        }
                        else if(random > (2.0f/3.0f))
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
                        else if( random > 0.75f)
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
            if(currentNode == null)
            {
                currentNode = previousCurrent.getConnection(Direction.Down);
            }
            if(currentNode is GoalNode || currentNode == null)
            {
                break;
            }
            // Decrease tries
            numberOrganizeTaskTries--;
        }
    }
}
