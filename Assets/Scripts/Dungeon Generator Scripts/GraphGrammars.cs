using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGrammars : DungeonGenerator
{
    [Header("Each task is going to be a room")]
    [SerializeField] int minTaskNumber;                     // The minimum number of tasks in the graph
    [SerializeField] int maxTaskNumber;                     // The maximum number of tasks in the graph
    [Header("Number of tries to reoder the rooms")]
    [SerializeField] int minOrganizeTasksTries;             // The minimum number of tries to apply the reorganize production rules
    [SerializeField] int maxOrganizeTasksTries;             // The maximum number of tries to apply the reorganize production rules
    [Header("Probabilty of reording the rooms")]
    [Range(0.0f, 1.0f)]
    [SerializeField] float probabiltyApplyOrganizationRule; // The probablity of a reorganization rule to be applied

    private Graph graph;                                    // Graph containing all the nodes
    private StartNode S;                                    // Node form which the grammar will generate the mission (maybe remove and place below)

    private int minCorridorLengthWhenVertical;              // Minimum corridor length when it is placed vertically
    private int maxCorridorLengthWhenVertical;              // Maximum corridor length when it is placed vertically
    private int minCorridorLengthWhenHorizontal;            // Minimum corridor length when it is placed horizontally
    private int maxCorridorLengthWhenHorizontal;            // Maximum corridor length when it is placed horizontally

    private int exitRoomIndex;                              // Index of the last room in the dungeon rooms list

    public override void BuildDungeon()
    {
        AssertProperties();
        // Initialize starting node and graph
        S = new StartNode();
        graph = new Graph(S);
        // Generate the graph with the mission
        graph.GenerateMission(minTaskNumber, maxTaskNumber, minOrganizeTasksTries, maxOrganizeTasksTries, probabiltyApplyOrganizationRule);
        // Transform the graph into the dungeon space
        TransformGraphIntoDungeon();
        // Set entrance and exit rooms
        dungeon.chooseEntranceRoomAndExitRoom(0, exitRoomIndex);
        // Set the corridors and rooms to be children of the dungeon game object
        dungeon.setRoomsAndCorridorsAsDungeonChildren();
        // Set that the dungeon has finished building
        dungeonBuildingFinished = true;
    }

    protected override void AssertProperties()
    {
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
        minCorridorLengthWhenHorizontal = roomMaxTilesWidth / 2;
        // The max corridor length is going to be equal to the max room size
        maxCorridorLengthWhenVertical = roomMaxTilesHeight;
        maxCorridorLengthWhenHorizontal = roomMaxTilesWidth;
    }

    private void TransformGraphIntoDungeon()
    {
        // Calculate dungeon dimensions
        int dungeonHeight = graph.getNumberNodes() * roomMaxTilesHeight * maxCorridorLengthWhenVertical;
        int dungeonWidth = graph.getNumberNodes() * roomMaxTilesWidth * maxCorridorLengthWhenHorizontal;
        // Create dungeon grid
        dungeon = new Dungeon(dungeonHeight, dungeonWidth);
        dungeon.createDungeonGrid(dungeonTopLeftCellPosition, floorTileDimensions);
        // Place rooms in dungeon starting from the entrance (root node of the graph), given the productions rules, there can only be one level of rooms above the entrance and no rooms can be at its left
        int entranceTopLeftTileRow = roomMaxTilesHeight + maxCorridorLengthWhenVertical;
        int entranceTopLeftTileColumn = 0;
        int entranceRandomRoomHeight = Random.Range(roomMinTilesHeight, roomMaxTilesHeight);
        int entrnaceRandomRoomWidth = Random.Range(roomMinTilesWidth, roomMaxTilesWidth);
        dungeon.getDungeonRooms().Add(new Room(dungeon, entranceTopLeftTileRow, entranceTopLeftTileColumn, entranceRandomRoomHeight, entrnaceRandomRoomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        // Create a list with the nodes which room has already been placed
        Dictionary<AlphabetNode, Room> nodesWithRoom = new Dictionary<AlphabetNode, Room>();
        nodesWithRoom.Add(graph.getRootNode(), dungeon.getDungeonRooms()[0]);
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
            // When the node is a far task node, a room is not created, only the connection
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
            // When the current node is the goal node, find the index of its room in the dungeon rooms list
            if(currentNode is GoalNode)
            {
                exitRoomIndex = dungeon.getDungeonRooms().IndexOf(nodesWithRoom[currentNode]);
            }
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
        dungeon.getDungeonRooms().Add(new Room(dungeon, topLeftCornerRow, topLeftCornerColumn, randomRoomHeight, randomRoomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        // Add node to lists
        nodesWithRoom.Add(currentNode.getConnection(dir), dungeon.getDungeonRooms()[dungeon.getDungeonRooms().Count - 1]);
        nodesToVisit.Add(currentNode.getConnection(dir));
        // Connect rooms
        dungeon.getDungeonCorridors().Add(new Corridor(dungeon, nodesWithRoom[currentNode], nodesWithRoom[currentNode.getConnection(dir)], floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
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
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, firstCell, lastCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, false));
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
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, firstCell, firstMiddleCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, true));
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, firstMiddleCell, secondMiddleCell, middleDirection, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, true));
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, secondMiddleCell, lastCell, Direction.Down, floorTileDimensions, floorMaterial, wallHeight, wallMaterial, false));
        }
    }
}
