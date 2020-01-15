using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneratorDiggerAgent : MonoBehaviour
{
    [SerializeField] Vector3 dungeonTopLeftCellPosition;    // Top left position of dungeon cell [0,0]
    [SerializeField] int dungeonWidth;                      // Number of columns the dungeon grid is going to have
    [SerializeField] int dungeonHeight;                     // Number of rows the dungeon grid is going to have

    [SerializeField] int roomMinTilesWidth;                 // Minimum number of column cells a room needs to have
    [SerializeField] int roomMaxTilesWidth;                 // Maximum number of column cells a room can have
    [SerializeField] int roomMinTilesHeight;                // Minimum number of row cells a room needs to have
    [SerializeField] int roomMaxTilesHeight;                // Maximum number of row cells a room can have

    [SerializeField] int corridorMinTilesLength;            // Minimum number of tiles of length a corridor can be
    [SerializeField] int corridorMaxTilesLength;            // Maximum number of tiles of length a corridor can be

    [SerializeField] Material floorMaterial;                // Material that will be used for the floor
    [SerializeField] Vector3 floorTileDimensions;           // Dimensions of each floor tile
    [SerializeField] Material wallMaterial;                 // Material of the walls
    [SerializeField] float wallHeight;                      // Height of each wall

    private Dungeon dungeon;                                // Dungeon class which basically consits in a 2D array of cells
    private List<Room> dungeonRooms;                        // List with all the rooms in the dungeon
    private List<Corridor> dungeonCorridors;                // List with all the corridors in the dungeon

    [SerializeField] bool removeDirtyCorridors;             // Wheter to remove corridors that lead to no room or not

    private DiggerAgent diggerAgent;                        // DiggerAgent objet that is going to build the dungeon

    // Start is called before the first frame update
    void Start()
    {
        dungeonRooms = new List<Room>();
        dungeonCorridors = new List<Corridor>();
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void BuildDungeon()
    {
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
        // Initialise digger
        diggerAgent = new DiggerAgent(ref dungeon,ref dungeonRooms, ref dungeonCorridors, corridorMinTilesLength, corridorMaxTilesLength, removeDirtyCorridors);
        // Place digger in a random dungeon cell leaving a margin on the right and the bottom parts of the grid for the first room
        diggerAgent.SetDiggerInitialPosition(roomMinTilesWidth, roomMinTilesHeight);
        // Create dungeon
        diggerAgent.DigDungeon(roomMinTilesWidth, roomMaxTilesWidth, roomMinTilesHeight, roomMaxTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
    }

    private class DiggerAgent
    {
        private Dungeon dungeon;            // Dungeon grid
        private List<Room> rooms;           // List of the rooms in the dungeon
        private List<Corridor> corridors;   // List of the corridors in the dungeon

        private DungeonCell currentCell;    // Dungeon cell in which the digger agent is currently at
        private Direction direction;        // Direction of the digger agent

        private Dictionary<Direction, List<FloorTile>> lastRoomPlacedPossibleTiles; // Dictionary that contains the list of tiles of a room from which a corridor can be created  
        private List<Corridor> dirtyCorridors;                                      // List of corridors that one of their ends do not connect with a room
        private bool removeDirtyCorridors;                                          // Whether to remove or not the corridors that have one end not ending in a room
        private FloorTile lastTileUsedAsCorridorConnection;                         // Last tile from which corridors where created
        private Direction directionOfWallToRebuild;                                 // Direction of the last tile wall that was removed to connect it with the corridors

        private bool roomPlaced;        // Whether a room has succesfully been placed in the current iteration of the DigDungeon method
        private bool corridorPlaced;    // Whether a corridor has succesfully been placed in the current iteration of the DigDungeon method
        private bool tryAgain;          // When a corridor cannot be built from another corridor this boolean will be set to true to try to build a corridor from the last room created instead

        private int minCorridorsLength; // Minimum length of a corridor
        private int maxCorridorsLength; // Max length of a corridor

        public DiggerAgent(ref Dungeon dungeon, ref List<Room> rooms, ref List<Corridor> corridors, int minCorridorsLength, int maxCorridorsLength, bool removeDirtyCorridors)
        {
            this.dungeon = dungeon;
            this.rooms = rooms;
            this.corridors = corridors;
            this.minCorridorsLength = minCorridorsLength;
            this.maxCorridorsLength = maxCorridorsLength;
            this.removeDirtyCorridors = removeDirtyCorridors;
            dirtyCorridors = new List<Corridor>();
            lastRoomPlacedPossibleTiles = new Dictionary<Direction, List<FloorTile>>();
            roomPlaced = false;
            corridorPlaced = false;
            tryAgain = false;
        }

        public void SetDiggerInitialPosition(int roomMinTilesWidth, int roomMinTilesHeight)
        {
            // Get margins
            int minRow = 0;
            int maxRow = dungeon.getDungeonHeight() - 1 - roomMinTilesHeight;
            int minColumn = 0;
            int maxColumn = dungeon.getDungeonWidth() - 1 - roomMinTilesWidth;
            // Place digger in random cell that is inside those margins
            currentCell = dungeon.getDungeonGrid()[Random.Range(minRow, maxRow), Random.Range(minColumn, maxColumn)];  
        }

        public void DigDungeon(int roomMinWidth, int roomMaxWidth, int roomMinHeight, int roomMaxHeight, Vector3 tileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
        {
            // Reset room placed boolean
            roomPlaced = false;
            // If a corridor was placed in the previous iteration, the room needs to start from the next cell to the actual current cell based on the direcion the digger agent was going
            DungeonCell previousCurrent = currentCell;
            Direction directionToRemoveWall = Direction.Unknown;
            if (corridorPlaced)
            {
                switch(direction)
                {
                    case Direction.Up:
                        if(currentCell.getCellRowPositionInGrid() - 1 >= 0)
                        {
                            currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - 1, currentCell.getCellColumnPositionInGrid()];
                            directionToRemoveWall = Direction.Down;
                        }
                        break;
                    case Direction.Down:
                        if(currentCell.getCellRowPositionInGrid() + 1 <= dungeon.getDungeonHeight() - 1)
                        {
                            currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + 1, currentCell.getCellColumnPositionInGrid()];
                            directionToRemoveWall = Direction.Up;
                        }
                        break;
                    case Direction.Left:
                        if(currentCell.getCellColumnPositionInGrid() - 1 >= 0)
                        {
                            currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - 1];
                            directionToRemoveWall = Direction.Right;
                        }
                        break;
                    case Direction.Right:
                        if(currentCell.getCellColumnPositionInGrid() + 1 <= dungeon.getDungeonWidth() - 1)
                        {
                            currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + 1];
                            directionToRemoveWall = Direction.Left;
                        }
                        break;
                }
            }
            // Check the number of empty tiles from the currentCell in the four directions
            int emptyCellsRight = CheckEmptyCellsRight();
            int emptyCellsLeft = CheckEmptyCellsLeft();
            int emptyCellsUp = CheckEmptyCellsUp();
            int emptyCellsDown = CheckEmptyCellsDown();
            // Check if there is enough space to create a room
            if (emptyCellsRight + emptyCellsLeft > roomMinWidth && emptyCellsUp + emptyCellsDown > roomMinHeight)
            {
                // To make sure that one of the room tiles is placed in the current cell, the maximum distance to move the starting cell is the room minimum tiles height/width
                int maximumDistanceUp = Mathf.Min(emptyCellsUp, roomMinHeight);
                int maximumDistanceLeft = Mathf.Min(emptyCellsLeft, roomMinWidth);
                // Rooms are created from the top left corner, meaning that the starting cell can be moved from the current cell up or left if there is space
                int topLeftCornerMinRow = currentCell.getCellRowPositionInGrid() - maximumDistanceUp + 1;
                int topLeftCornerMinColumn = currentCell.getCellColumnPositionInGrid() - maximumDistanceLeft + 1;
                // The max column and row are going to depend on the available cells at the right and down of the current cell
                int maxRow = currentCell.getCellRowPositionInGrid() + emptyCellsDown - 1;
                int topLeftCornerMaxRow = maxRow - roomMinHeight + 1;
                topLeftCornerMaxRow = topLeftCornerMaxRow > currentCell.getCellRowPositionInGrid() ? currentCell.getCellRowPositionInGrid() : topLeftCornerMaxRow;
                int maxColumn = currentCell.getCellColumnPositionInGrid() + emptyCellsRight - 1;
                int topLeftCornerMaxColumn = maxColumn - roomMinWidth + 1;
                topLeftCornerMaxColumn = topLeftCornerMaxColumn > currentCell.getCellColumnPositionInGrid() ? currentCell.getCellColumnPositionInGrid() : topLeftCornerMaxColumn;
                // Choose random row and column for the top left corner of the room
                int topLeftCornerRandomRow = Random.Range(topLeftCornerMinRow, topLeftCornerMaxRow);
                int topLeftCornerRandomColumn = Random.Range(topLeftCornerMinColumn, topLeftCornerMaxColumn);
                // Get max width and height from the row and column selected to be the top left corner of the room
                int maxWidth = Mathf.Min(maxColumn - topLeftCornerRandomColumn + 1, roomMaxWidth);
                int maxHeight = Mathf.Min(maxRow - topLeftCornerRandomRow + 1, roomMaxHeight);
                // Get random width and height
                int randomRoomWidth = Random.Range(roomMinWidth, maxWidth);
                int randomRoomHeight = Random.Range(roomMinHeight, maxHeight);
                // Given the top left corner, width and height, make sure that the room can still be placed
                bool allTilesEmpty = true;
                for(int i = topLeftCornerRandomRow; i < (topLeftCornerRandomRow + randomRoomHeight); i++)
                {
                    for(int j = topLeftCornerRandomColumn; j < (topLeftCornerRandomColumn + randomRoomWidth); j++)
                    {
                        if(dungeon.getDungeonGrid()[i,j].getCellFloorTile() != null)
                        {
                            allTilesEmpty = false;
                            break;
                        }
                    }
                    if(!allTilesEmpty)
                    {
                        break;
                    }
                }
                if(allTilesEmpty)
                {
                    // Create new room
                    rooms.Add(new Room(ref dungeon, topLeftCornerRandomRow, topLeftCornerRandomColumn, randomRoomHeight, randomRoomWidth, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                    roomPlaced = true;
                    // Get all possible tiles from which a corridor can be placed of the room currently added
                    lastRoomPlacedPossibleTiles.Clear();
                    lastRoomPlacedPossibleTiles.Add(Direction.Up, rooms[rooms.Count - 1].getTilesUpRow());
                    lastRoomPlacedPossibleTiles.Add(Direction.Down, rooms[rooms.Count - 1].getTilesDownRow());
                    lastRoomPlacedPossibleTiles.Add(Direction.Left, rooms[rooms.Count - 1].getTilesLeftColumn());
                    lastRoomPlacedPossibleTiles.Add(Direction.Right, rooms[rooms.Count - 1].getTilesRightColumn());
                    // Remove wall from the previous current cell to connect the room and the corridor, if a corridor was placed in the previous iteration
                    if (corridorPlaced)
                    {
                        previousCurrent.getCellFloorTile().removeWall(direction);
                        currentCell.getCellFloorTile().removeWall(directionToRemoveWall);
                    }
                }
            }
            if(!roomPlaced)
            {
                // If room could not be placed, then set the current to be the previous current to properly connect a corridor with another corridor
                currentCell = previousCurrent;
            }
            // Reset corridor placed boolean
            corridorPlaced = false;
            // If a room has been created in the current iteration or no more corridors could be placed, choose one of the last room placed outer tiles to create the corridor from it
            if(roomPlaced || tryAgain)
            {
                // Reset dirty corridors variables
                dirtyCorridors.Clear();
                lastTileUsedAsCorridorConnection = null;
                directionOfWallToRebuild = Direction.Unknown;
                // Try to place a corridor from one of the four outter walls of a room
                while (lastRoomPlacedPossibleTiles.Count > 0)
                {
                    // Choose random list
                    int i = Random.Range(0, lastRoomPlacedPossibleTiles.Count - 1);
                    // Choose random list
                    Direction randomDirection = (Direction)Random.Range(0, System.Enum.GetValues(typeof(Direction)).Length);
                    while (!lastRoomPlacedPossibleTiles.ContainsKey(randomDirection))
                    {
                        randomDirection = (Direction)Random.Range(0, System.Enum.GetValues(typeof(Direction)).Length);
                    }
                    List<FloorTile> randomList = lastRoomPlacedPossibleTiles[randomDirection];
                    // Choose random tile until all tiles have been checked
                    while (randomList.Count > 0 && !corridorPlaced)
                    {
                        // Randomly select tile
                        int j = Random.Range(0, randomList.Count - 1);
                        FloorTile randomTile = randomList[j];
                        currentCell = randomTile.getCorrespondingDungeonCell();
                        ref DungeonCell lastCell = ref randomTile.getCorrespondingDungeonCell();
                        bool buildCorridor = false;
                        // Check if corridor can be created from random tile
                        int emptyCells = 0;
                        switch (randomDirection)
                        {
                            case Direction.Up:
                                emptyCells = CheckEmptyCellsUp();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells - 1);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - length, currentCell.getCellColumnPositionInGrid()];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Down:
                                emptyCells = CheckEmptyCellsDown();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells - 1);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + length, currentCell.getCellColumnPositionInGrid()];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Left:
                                emptyCells = CheckEmptyCellsLeft();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells - 1);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - length];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Right:
                                emptyCells = CheckEmptyCellsRight();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells - 1);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + length];
                                    buildCorridor = true;
                                }
                                break;
                        }
                        // If there is enough space for a corridor
                        if (buildCorridor)
                        {
                            // Build corridor between first and last cell
                            corridors.Add(new Corridor(ref dungeon, ref currentCell, ref lastCell, randomDirection, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                            corridorPlaced = true;
                            // Add the corridor to the dirty corridors list
                            dirtyCorridors.Add(corridors[corridors.Count - 1]);
                            // Set the current cell (outer room cell) as the last cell used as connection
                            lastTileUsedAsCorridorConnection = currentCell.getCellFloorTile();
                            // Set the direction of the tile wall that will need to be removed
                            directionOfWallToRebuild = randomDirection;
                            // Set direction to randomDirection
                            direction = randomDirection;
                            // Set current cell to the last cell of the corridor
                            currentCell = lastCell;
                        }
                        // Remove tile from list
                        randomList.Remove(randomTile);
                    }
                    // Exit loop if corridor was placed
                    if(corridorPlaced)
                    {
                        break;
                    }
                    // Remove list
                    lastRoomPlacedPossibleTiles.Remove(randomDirection);
                }
                // Reset try again boolean
                tryAgain = false;
            }
            else
            {
                // Continue from last tile of last corridor which is the current cell based on the previous direction
                if(direction == Direction.Up || direction == Direction.Down)
                {
                    // Try to go right or left
                    int emptyTilesRight = CheckEmptyCellsRight();
                    int emptyTilesLeft = CheckEmptyCellsLeft();
                    Direction directionToDig = direction;
                    if (emptyTilesRight > minCorridorsLength && emptyTilesLeft > minCorridorsLength)
                    {
                        // If digger can go in both directions choose one randomly
                        if (Random.Range(0.0f, 1.0f) > 0.5f)
                        {
                            // Dig right
                            directionToDig = Direction.Right;
                        }
                        else
                        {
                            // Dig left
                            directionToDig = Direction.Left;
                        }
                    }
                    else if(emptyTilesRight > minCorridorsLength)
                    {
                        // Digger can only go right
                        directionToDig = Direction.Right;
                    }
                    else if(emptyTilesLeft > minCorridorsLength)
                    {
                        // Digger can only go left
                        directionToDig = Direction.Left;
                    }
                    // Dig corridor if direction is either right or left
                    bool buildCorridor = false;
                    ref DungeonCell lastCell = ref currentCell;
                    switch (directionToDig)
                    {
                        case Direction.Right:
                            // Choose random length to dig right
                            int randomLength = Random.Range(minCorridorsLength, emptyTilesRight - 1);
                            randomLength = Mathf.Min(maxCorridorsLength, randomLength);
                            // Set last cell
                            lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + randomLength];
                            buildCorridor = true;
                            break;
                        case Direction.Left:
                            // Choose random length to dig left
                            randomLength = Random.Range(minCorridorsLength, emptyTilesLeft - 1);
                            randomLength = Mathf.Min(maxCorridorsLength, randomLength);
                            // Set last cell
                            lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - randomLength];
                            buildCorridor = true;
                            break;
                    }
                    if(buildCorridor)
                    {
                        // Build corridor between first and last cell
                        corridors.Add(new Corridor(ref dungeon, ref currentCell, ref lastCell, directionToDig, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                        corridorPlaced = true;
                        // Add the corridor to the dirty corridors list
                        dirtyCorridors.Add(corridors[corridors.Count - 1]);
                        // Set direction
                        direction = directionToDig;
                        // Set current cell to the last cell of the corridor
                        currentCell = lastCell;
                    }
                    else
                    {
                        // There is no space for the digger to place a new corridor
                        corridorPlaced = false;
                    }
                }
                else
                {
                    // Try to go up or down
                    int emptyTilesUp = CheckEmptyCellsUp();
                    int emptyTilesDown = CheckEmptyCellsDown();
                    Direction directionToDig = direction;
                    if (emptyTilesUp > minCorridorsLength && emptyTilesDown > minCorridorsLength)
                    {
                        // If digger can go in both directions choose one randomly
                        if (Random.Range(0.0f, 1.0f) > 0.5f)
                        {
                            // Dig up
                            directionToDig = Direction.Up;
                        }
                        else
                        {
                            // Dig down
                            directionToDig = Direction.Down;
                        }
                    }
                    else if (emptyTilesUp > minCorridorsLength)
                    {
                        // Digger can only go right
                        directionToDig = Direction.Up;
                    }
                    else if (emptyTilesDown > minCorridorsLength)
                    {
                        // Digger can only go left
                        directionToDig = Direction.Down;
                    }
                    // Dig corridor if direction is either up or down
                    bool buildCorridor = false;
                    ref DungeonCell lastCell = ref currentCell;
                    switch (directionToDig)
                    {
                        case Direction.Up:
                            // Choose random length to dig up
                            int randomLength = Random.Range(minCorridorsLength, emptyTilesUp - 1);
                            randomLength = Mathf.Min(maxCorridorsLength, randomLength);
                            // Set last cell
                            lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - randomLength, currentCell.getCellColumnPositionInGrid()];
                            buildCorridor = true;
                            break;
                        case Direction.Down:
                            // Choose random length to dig down
                            randomLength = Random.Range(minCorridorsLength, emptyTilesDown - 1);
                            randomLength = Mathf.Min(maxCorridorsLength, randomLength);
                            // Set last cell
                            lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + randomLength, currentCell.getCellColumnPositionInGrid()];
                            buildCorridor = true;
                            break;
                    }
                    if (buildCorridor)
                    {
                        // Build corridor between first and last cell
                        corridors.Add(new Corridor(ref dungeon, ref currentCell, ref lastCell, directionToDig, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                        corridorPlaced = true;
                        // Add the corridor to the dirty corridors list
                        dirtyCorridors.Add(corridors[corridors.Count - 1]);
                        // Set direction
                        direction = directionToDig;
                        // Set current cell to the last cell of the corridor
                        currentCell = lastCell;
                    }
                    else
                    {
                        // There is no space for the digger to place a new corridor
                        corridorPlaced = false;
                    }
                }
                if(!corridorPlaced)
                {
                    // If no corridor can be placed from the last corridor try again from the last room placed
                    tryAgain = true;
                    // Remove corridors that lead to nowhere based on the boolean set by the user
                    if(removeDirtyCorridors)
                    {
                        foreach(Corridor corridor in dirtyCorridors)
                        {
                            corridor.DestroyCorridor();
                        }
                        dirtyCorridors.Clear();
                        // Rebuild the wall destroyed to connect the room with the corridors
                        if(lastTileUsedAsCorridorConnection != null)
                        {
                            lastTileUsedAsCorridorConnection.placeWall(wallMaterial, wallHeight, directionOfWallToRebuild);
                        }
                        // Reset dirty variables
                        lastTileUsedAsCorridorConnection = null;
                        directionOfWallToRebuild = Direction.Unknown;
                    }
                }
            }
            // If a room or a corridor was placed or the algorithm is going to try again from the last room placed, continue digging the dungeon
            if (roomPlaced || corridorPlaced || tryAgain)
            {
                DigDungeon(roomMinWidth, roomMaxWidth, roomMinHeight, roomMaxHeight, tileDimensions, floorMaterial, wallHeight, wallMaterial);
            }
        }

        private int CheckEmptyCellsUp()
        {
            ref DungeonCell cellToCheck = ref currentCell;
            int emptyCellsUp = 0;
            // Check up cells
            while (true)
            {
                // Check if cell is empty
                if (cellToCheck.getCellFloorTile() != null && cellToCheck != currentCell)
                {
                    break;
                }
                emptyCellsUp++;
                // Check if the end of the grid has been reached
                int previousRow = cellToCheck.getCellRowPositionInGrid() - 1;
                if (previousRow < 0)
                {
                    break;
                }
                cellToCheck = ref dungeon.getDungeonGrid()[previousRow, cellToCheck.getCellColumnPositionInGrid()];
            }
            return emptyCellsUp;
        }

        private int CheckEmptyCellsDown()
        {
            ref DungeonCell cellToCheck = ref currentCell;
            int emptyCellsDown = 0;
            // Check down cells
            while (true)
            {
                // Check if cell is empty
                if (cellToCheck.getCellFloorTile() != null && cellToCheck != currentCell)
                {
                    break;
                }
                emptyCellsDown++;
                // Check if the end of the grid has been reached
                int nextRow = cellToCheck.getCellRowPositionInGrid() + 1;
                if (nextRow >= dungeon.getDungeonHeight())
                {
                    break;
                }
                cellToCheck = ref dungeon.getDungeonGrid()[nextRow, cellToCheck.getCellColumnPositionInGrid()];
            }
            return emptyCellsDown;
        }

        private int CheckEmptyCellsRight()
        {
            ref DungeonCell cellToCheck = ref currentCell;
            int emptyCellsRight = 0;
            // Check right cells
            while (true)
            {
                // Check if the cell is empty
                if (cellToCheck.getCellFloorTile() != null && cellToCheck != currentCell)
                {
                    break;
                }
                emptyCellsRight++;
                // Check if the end of the grid has been reached
                int nextColumn = cellToCheck.getCellColumnPositionInGrid() + 1;
                if (nextColumn >= dungeon.getDungeonWidth())
                {
                    break;
                }
                cellToCheck = ref dungeon.getDungeonGrid()[cellToCheck.getCellRowPositionInGrid(), nextColumn];
            }
            return emptyCellsRight;
        }

        private int CheckEmptyCellsLeft()
        {
            ref DungeonCell cellToCheck = ref currentCell;
            int emptyCellsLeft = 0;
            // Check left cells
            while (true)
            {
                // Check if the cell is empty
                if (cellToCheck.getCellFloorTile() != null && cellToCheck != currentCell)
                {
                    break;
                }
                emptyCellsLeft++;
                // Check if the beginning of the grid has been reached
                int previousColumn = cellToCheck.getCellColumnPositionInGrid() - 1;
                if (previousColumn < 0)
                {
                    break;
                }
                cellToCheck = ref dungeon.getDungeonGrid()[cellToCheck.getCellRowPositionInGrid(), previousColumn];
            }
            return emptyCellsLeft;
        }
    }
}
