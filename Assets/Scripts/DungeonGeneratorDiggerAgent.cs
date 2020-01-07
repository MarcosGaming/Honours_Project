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
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void BuildDungeon()
    {
        // Create dungeon grid
        dungeon = new Dungeon(dungeonWidth, dungeonHeight);
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
        diggerAgent = new DiggerAgent(ref dungeon,ref dungeonRooms, ref dungeonCorridors, corridorMinTilesLength, corridorMaxTilesLength);
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

        private bool roomPlaced;        // Whether a room has succesfully been placed in the current iteration of the DigDungeon method
        private bool corridorPlaced;    // Whether a corridor has succesfully been placed in the current iteration of the DigDungeon method

        private int minCorridorsLength; // Minimum length of a corridor
        private int maxCorridorsLength; // Max length of a corridor

        public DiggerAgent(ref Dungeon dungeon, ref List<Room> rooms, ref List<Corridor> corridors, int minCorridorsLength, int maxCorridorsLength)
        {
            this.dungeon = dungeon;
            this.rooms = rooms;
            this.corridors = corridors;
            this.minCorridorsLength = minCorridorsLength;
            this.maxCorridorsLength = maxCorridorsLength;
            lastRoomPlacedPossibleTiles = new Dictionary<Direction, List<FloorTile>>();
            roomPlaced = false;
            corridorPlaced = false;
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
            // Check the number of empty tiles from the currentCell in the four directions
            int emptyCellsRight = CheckEmptyCellsRight(); ;
            int emptyCellsLeft = CheckEmptyCellsLeft(); ;
            int emptyCellsUp = CheckEmptyCellsUp();
            int emptyCellsDown = CheckEmptyCellsDown();
            // Check if there is enough space to create a room
            if(emptyCellsRight + emptyCellsLeft > roomMinWidth && emptyCellsUp + emptyCellsDown > roomMinHeight)
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
                // Create new room
                roomPlaced = true;
                int row = currentCell.getCellRowPositionInGrid();
                int column = currentCell.getCellColumnPositionInGrid();
                rooms.Add(new Room(ref dungeon, row, column, randomRoomHeight, randomRoomWidth, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                roomPlaced = true;
                // Get all possible tiles from which a corridor can be placed of the room currently added
                lastRoomPlacedPossibleTiles.Clear();
                lastRoomPlacedPossibleTiles.Add(Direction.Up, rooms[rooms.Count - 1].getTilesUpRow());
                lastRoomPlacedPossibleTiles.Add(Direction.Down, rooms[rooms.Count - 1].getTilesDownRow());
                lastRoomPlacedPossibleTiles.Add(Direction.Left, rooms[rooms.Count - 1].getTilesLeftColumn());
                lastRoomPlacedPossibleTiles.Add(Direction.Right, rooms[rooms.Count - 1].getTilesRightColumn());
            }
            // Reset corridor placed boolean
            corridorPlaced = false;
            // If a room has been created in the current iteration, choose one of its outer tiles to create the corridor from it
            if(roomPlaced)
            {
                // Try to place a corridor from one of the four outter walls of a room
                while(lastRoomPlacedPossibleTiles.Count > 0)
                {
                    // Choose random list
                    int i = Random.Range(0, lastRoomPlacedPossibleTiles.Count - 1);
                    Direction randomDirection = (Direction)Random.Range(0, System.Enum.GetValues(typeof(Direction)).Length);
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
                                    int length = Random.Range(minCorridorsLength, emptyCells);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - length + 1, currentCell.getCellColumnPositionInGrid()];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Down:
                                emptyCells = CheckEmptyCellsDown();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + length - 1, currentCell.getCellColumnPositionInGrid()];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Left:
                                emptyCells = CheckEmptyCellsLeft();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - length + 1];
                                    buildCorridor = true;
                                }
                                break;
                            case Direction.Right:
                                emptyCells = CheckEmptyCellsRight();
                                if (emptyCells > minCorridorsLength)
                                {
                                    // Choose random length
                                    int length = Random.Range(minCorridorsLength, emptyCells);
                                    length = Mathf.Min(length, maxCorridorsLength);
                                    // Set last cell
                                    lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + length - 1];
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
            }
            else
            {
                // Continue from last tile of last corridor

                // If no corridor can be placed from the last corridor try again from the last room placed

                // Remove corridors that lead to nowhere based on the boolean set by the user

            }
            // Try to create a corridor in any direction and with a length between the minimum and the maximum entered by the user

            // If a room or a corridor was placed, continue digging the dungeon
            if (roomPlaced || corridorPlaced)
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
                if (cellToCheck.getCellFloorTile() != null)
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
                if (cellToCheck.getCellFloorTile() != null)
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
                if (cellToCheck.getCellFloorTile() != null)
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
                if (cellToCheck.getCellFloorTile() != null)
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
