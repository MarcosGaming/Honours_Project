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

    private List<Room> rooms;                               // List with all the rooms in the dungeon

    private List<Corridor> corridors;                       // List with all the corridors in the dungeon
    private int minCorridorLengthWhenVertical;              // Minimum corridor length when it is placed vertically
    private int maxCorridorLengthWhenVertical;              // Maximum corridor length when it is placed vertically
    private int minCorridorLengthWhenHorizontal;            // Minimum corridor length when it is placed horizontally
    private int maxCorridorLengthWhenHorizontal;            // Maximum corridor length when it is placed horizontally

    // Start is called before the first frame update
    void Start()
    {
        // Initialise rooms and corridors lists
        rooms = new List<Room>();
        corridors = new List<Corridor>();
        // Make sure that the wall height is at least one
        wallHeight = Mathf.Max(1.0f, wallHeight);
        // Make sure that the tile dimensions are at least 1,0.5,1
        floorTileDimensions.x = Mathf.Max(1.0f, floorTileDimensions.x);
        floorTileDimensions.y = Mathf.Max(0.5f, floorTileDimensions.y);
        floorTileDimensions.z = Mathf.Max(1.0f, floorTileDimensions.z);
        // Make sure that the minimum width and height of a room is at least four tiles
        roomMinTilesWidth = Mathf.Max(roomMinTilesWidth, 4);
        roomMinTilesHeight = Mathf.Max(roomMinTilesHeight, 4);
        // Make sure that the maximum width and height of a room is greater or equal to the minimum height and width of a room
        roomMaxTilesWidth = Mathf.Max(roomMinTilesWidth, roomMaxTilesWidth);
        roomMaxTilesHeight = Mathf.Max(roomMinTilesHeight, roomMaxTilesHeight);
        // Make sure that the min room width and height is at least half the max room width and height
        roomMinTilesWidth = Mathf.Max(roomMinTilesWidth, roomMaxTilesWidth / 2);
        roomMinTilesHeight = Mathf.Max(roomMinTilesHeight, roomMaxTilesHeight / 2);
        // The min corridor length is going to be half the max room size
        minCorridorLengthWhenVertical = roomMaxTilesHeight / 2;
        minCorridorLengthWhenHorizontal = roomMinTilesWidth / 2;
        // The max corridor length is going to be equal to the max room size
        maxCorridorLengthWhenVertical = roomMaxTilesHeight;
        maxCorridorLengthWhenHorizontal = roomMaxTilesWidth;
        // Build the dungeon
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildDungeon()
    {
        // Initialize starting node and graph
        S = new StartNode();
        graph = new Graph(S);
        // Generate the graph with the mission
        graph.GenerateMission(minTaskNumber, maxTaskNumber, minOrganizeTasksTries, maxOrganizeTasksTries, probabiltyApplyOrganizationRule);
        // Transform the graph into the dungeon space
        TransformGraphIntoDungeon();
    }

    private void TransformGraphIntoDungeon()
    {
        // Create dungeon grid
        int dungeonHeight = graph.getNumberNodes() * roomMaxTilesHeight * maxCorridorLengthWhenVertical;
        int dungeonWidth = graph.getNumberNodes() * roomMaxTilesWidth * maxCorridorLengthWhenHorizontal;
        // Create dungeon grid
        dungeon = new Dungeon(dungeonHeight, dungeonWidth);
        ref DungeonCell[,] dungeonGrid = ref dungeon.getDungeonGrid();
        for (int i = 0; i < dungeonHeight; i++)
        {
            for (int j = 0; j < dungeonWidth; j++)
            {
                Vector3 cellPosition = dungeonTopLeftCellPosition + new Vector3(floorTileDimensions.x * j, floorTileDimensions.y, -floorTileDimensions.z * i);
                dungeonGrid[i, j] = new DungeonCell(cellPosition, i, j);
            }
        }
        // Place rooms in dungeon starting from the entrance (root node of the graph), given the productions rules, there can only be one level of rooms above the entrance
        int entranceTopLeftTileRow = roomMaxTilesHeight + maxCorridorLengthWhenVertical;
        int entranceTopLeftTileColumn = 0;
        int entranceRandomRoomHeight = Random.Range(roomMinTilesHeight, roomMaxTilesHeight);
        int entrnaceRandomRoomWidth = Random.Range(roomMinTilesWidth, roomMaxTilesWidth);
        rooms.Add(new Room(ref dungeon, entranceTopLeftTileRow, entranceTopLeftTileColumn, entranceRandomRoomHeight, entrnaceRandomRoomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        // Create a list with the nodes which room has already been placed
        Dictionary<AlphabetNode, Room> nodesWithRoom = new Dictionary<AlphabetNode, Room>();
        nodesWithRoom.Add(graph.getRootNode(), rooms[0]);
        // Create a list with the far task nodes connected
        List<AlphabetNode> farTaskNodesConnected = new List<AlphabetNode>();
        // Create a list with the nodes that need to be visited
        List<AlphabetNode> nodesToVisit = new List<AlphabetNode>();
        nodesToVisit.Add(graph.getRootNode());
        // Create a room for each node and connect the rooms
        while (nodesToVisit.Count > 0)
        {
            AlphabetNode currentNode = nodesToVisit[0];
            bool checkUp = true;
            bool checkDown = true;
            // When the node is a far task node, a room is not created only the connection
            if(currentNode is FarTaskNode && !farTaskNodesConnected.Contains(currentNode))
            {
                // Find in which direction is the other far task node connected to this one, given the production rules, far task nodes are only connected down or up
                if(currentNode.getConnection(Direction.Up) is FarTaskNode && nodesWithRoom.ContainsKey(currentNode.getConnection(Direction.Up)))
                {
                    ConnectFarTaskNodesRooms(nodesWithRoom[currentNode.getConnection(Direction.Up)], nodesWithRoom[currentNode]);
                    farTaskNodesConnected.Add(currentNode);
                    farTaskNodesConnected.Add(currentNode.getConnection(Direction.Up));
                    checkUp = false;
                }
                else if (currentNode.getConnection(Direction.Down) is FarTaskNode && nodesWithRoom.ContainsKey(currentNode.getConnection(Direction.Down)))
                {
                    ConnectFarTaskNodesRooms(nodesWithRoom[currentNode], nodesWithRoom[currentNode.getConnection(Direction.Down)]);
                    farTaskNodesConnected.Add(currentNode);
                    farTaskNodesConnected.Add(currentNode.getConnection(Direction.Down));
                    checkDown = false;
                } 
            }
            // Check right connection
            if(currentNode.getConnection(Direction.Right) != null && !nodesWithRoom.ContainsKey(currentNode.getConnection(Direction.Right)))
            {
                CreateRoomAndConnectNodes(nodesWithRoom, nodesToVisit, currentNode, Direction.Right);
            }
            // Check left connection
            if (nodesToVisit[0].getConnection(Direction.Left) != null && !nodesWithRoom.ContainsKey(nodesToVisit[0].getConnection(Direction.Left)))
            {
                CreateRoomAndConnectNodes(nodesWithRoom, nodesToVisit, currentNode, Direction.Left);
            }
            // Check up connection
            if (checkUp && nodesToVisit[0].getConnection(Direction.Up) != null && !nodesWithRoom.ContainsKey(nodesToVisit[0].getConnection(Direction.Up)))
            {
                CreateRoomAndConnectNodes(nodesWithRoom, nodesToVisit, currentNode, Direction.Up);
            }
            // Check down connection
            if (checkDown && nodesToVisit[0].getConnection(Direction.Down) != null && !nodesWithRoom.ContainsKey(nodesToVisit[0].getConnection(Direction.Down)))
            {
                CreateRoomAndConnectNodes(nodesWithRoom, nodesToVisit, currentNode, Direction.Down);
            }
            // Remove first node in the list
            nodesToVisit.RemoveAt(0);
        }
    }

    private void CreateRoomAndConnectNodes(Dictionary<AlphabetNode, Room> nodesWithRoom, List<AlphabetNode> nodesToVisit, AlphabetNode currentNode, Direction dir)
    {
        // Choose random corridor length to separate the rooms by that distance
        int randomCorridorLength = 1;
        if(dir == Direction.Right || dir == Direction.Left)
        {
            randomCorridorLength = Random.Range(minCorridorLengthWhenHorizontal, maxCorridorLengthWhenHorizontal);
        } 
        else
        {
            randomCorridorLength = Random.Range(minCorridorLengthWhenVertical, maxCorridorLengthWhenVertical);
        }
        // Choose random room width
        int randomRoomWidth = Random.Range(roomMinTilesWidth, roomMaxTilesWidth);
        // Choose random room height
        int randomRoomHeight = Random.Range(roomMinTilesHeight, roomMaxTilesHeight);
        // Calculate top left corner column and row based on the direction and the middle cell of corresponding external side to align the rooms as much as possible
        int topLeftCornerColumn = 0;
        int topLeftCornerRow = 0;
        switch(dir)
        {
            case Direction.Right:
                int rigthColumnTilesNumber = nodesWithRoom[nodesToVisit[0]].getTilesRightColumn().Count;
                DungeonCell rightColumnMiddleCell = nodesWithRoom[nodesToVisit[0]].getTilesRightColumn()[rigthColumnTilesNumber / 2].getCorrespondingDungeonCell();
                topLeftCornerColumn = rightColumnMiddleCell.getCellColumnPositionInGrid() + randomCorridorLength + 1;
                topLeftCornerRow = rightColumnMiddleCell.getCellRowPositionInGrid() - (randomRoomHeight / 2);
                break;
            case Direction.Left:
                int leftColumnTilesNumber = nodesWithRoom[nodesToVisit[0]].getTilesLeftColumn().Count;
                DungeonCell leftColumnMiddleCell = nodesWithRoom[nodesToVisit[0]].getTilesLeftColumn()[leftColumnTilesNumber / 2].getCorrespondingDungeonCell();
                topLeftCornerColumn = leftColumnMiddleCell.getCellColumnPositionInGrid() - randomCorridorLength - randomRoomWidth;
                topLeftCornerRow = leftColumnMiddleCell.getCellRowPositionInGrid() - (randomRoomHeight / 2);
                break;
            case Direction.Up:
                int upRowTilesNumber = nodesWithRoom[nodesToVisit[0]].getTilesUpRow().Count;
                DungeonCell upRowMiddleCell = nodesWithRoom[nodesToVisit[0]].getTilesUpRow()[upRowTilesNumber / 2].getCorrespondingDungeonCell();
                topLeftCornerColumn = upRowMiddleCell.getCellColumnPositionInGrid() - (randomRoomWidth / 2);
                topLeftCornerRow = upRowMiddleCell.getCellRowPositionInGrid() - randomCorridorLength - randomRoomHeight;
                break;
            case Direction.Down:
                int downRowTilesNumber = nodesWithRoom[nodesToVisit[0]].getTilesDownRow().Count;
                DungeonCell downRowMiddleCell = nodesWithRoom[nodesToVisit[0]].getTilesDownRow()[downRowTilesNumber / 2].getCorrespondingDungeonCell();
                topLeftCornerColumn = downRowMiddleCell.getCellColumnPositionInGrid() - (randomRoomWidth / 2);
                topLeftCornerRow = downRowMiddleCell.getCellRowPositionInGrid() + randomCorridorLength + 1;
                break;
        }
        // Create room
        rooms.Add(new Room(ref dungeon, topLeftCornerRow, topLeftCornerColumn, randomRoomHeight, randomRoomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        // Add node to lists
        nodesWithRoom.Add(currentNode.getConnection(dir), rooms[rooms.Count - 1]);
        nodesToVisit.Add(currentNode.getConnection(dir));
        // Connect rooms
        corridors.Add(new Corridor(ref dungeon, nodesWithRoom[currentNode], nodesWithRoom[currentNode.getConnection(dir)], floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
    }

    private void ConnectFarTaskNodesRooms(Room upperRoom, Room lowerRoom)
    {
        // Choose random cell in the lower part of the upper room
        int random = Random.Range(0, upperRoom.getTilesDownRow().Count - 1);
        DungeonCell firstCell = upperRoom.getTilesDownRow()[random].getCorrespondingDungeonCell();
        // Choose random cell in the upper part of the lower room
        random = Random.Range(0, lowerRoom.getTilesUpRow().Count - 1);
        DungeonCell lastCell = lowerRoom.getTilesUpRow()[random].getCorrespondingDungeonCell();
        // Check if both cells are aligned
        if (firstCell.getCellColumnPositionInGrid() == lastCell.getCellColumnPositionInGrid())
        {
            corridors.Add(new Corridor(ref dungeon, ref firstCell, ref lastCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, false));
        }
        else
        {
            // Get first  middle point between both rooms
            int middle = (firstCell.getCellRowPositionInGrid() + lastCell.getCellRowPositionInGrid()) / 2;
            DungeonCell firstMiddleCell = dungeon.getDungeonGrid()[middle, firstCell.getCellColumnPositionInGrid()];
            // Get second middle point
            DungeonCell secondMiddleCell = dungeon.getDungeonGrid()[middle, lastCell.getCellColumnPositionInGrid()];
            // Calculate direction of middle corridor
            Direction middleDirection = Direction.Right;
            if (firstMiddleCell.getCellColumnPositionInGrid() > secondMiddleCell.getCellColumnPositionInGrid())
            {
                middleDirection = Direction.Left;
            }
            // Create corridors
            corridors.Add(new Corridor(ref dungeon, ref firstCell, ref firstMiddleCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, true));
            corridors.Add(new Corridor(ref dungeon, ref firstMiddleCell, ref secondMiddleCell, middleDirection, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, true));
            corridors.Add(new Corridor(ref dungeon, ref secondMiddleCell, ref lastCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, false));
        }
    }
}

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
            if(currentNode is TaskNode && !currentNode.isTerminal() && Random.Range(0.0f, 1.0f) < probabiltyApplyOrganizationRule)
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
                currentNode = rootNode;
            }
            // Decrease tries
            numberOrganizeTaskTries--;
        }
    }
}
