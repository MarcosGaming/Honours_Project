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
        diggerAgent = new DiggerAgent(ref dungeon,ref dungeonRooms, ref dungeonCorridors);
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

        private bool roomPlaced;            // Whether a room has succesfully been placed in the current iteration of the DigDungeon method
        private bool corridorPlaced;        // Whether a corridor has succesfully been placed in the current iteration of the DigDungeon method

        public DiggerAgent(ref Dungeon dungeon, ref List<Room> rooms, ref List<Corridor> corridors)
        {
            this.dungeon = dungeon;
            this.rooms = rooms;
            this.corridors = corridors;
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
            int emptyCellsRight = 0;
            int emptyCellsLeft = 0;
            int emptyCellsUp = 0;
            int emptyCellsDown = 0;
            CheckEmptyCellsInFourDirections(ref emptyCellsRight, ref emptyCellsLeft, ref emptyCellsUp, ref emptyCellsDown);
            // Keep track of the room cell which wall would need to be removed if a corridor was placed in the last iteration of this method
            ref DungeonCell cellThatNeedsWallRemoval = ref currentCell;
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
                // Remove wall if needed based on the direction the digger agent was going
                if(corridorPlaced)
                {
                    switch(this.direction)
                    {
                        case Direction.Up:
                            currentCell.getCellFloorTile().removeWall(Direction.Down);
                            break;
                        case Direction.Down:
                            currentCell.getCellFloorTile().removeWall(Direction.Up);
                            break;
                        case Direction.Right:
                            currentCell.getCellFloorTile().removeWall(Direction.Left);
                            break;
                        case Direction.Left:
                            currentCell.getCellFloorTile().removeWall(Direction.Right);
                            break;
                    }
                }
            }
            // Reset corridor placed boolean
            corridorPlaced = false;
            // Try to create a corridor in any direction and with a length between the minimum and the maximum entered by the user

            // If a room or a corridor was placed, continue digging the dungeon
            if (roomPlaced || corridorPlaced)
            {
                DigDungeon(roomMinWidth, roomMaxWidth, roomMinHeight, roomMaxHeight, tileDimensions, floorMaterial, wallHeight, wallMaterial);
            }
        }

        private void CheckEmptyCellsInFourDirections(ref int emptyCellsRight, ref int emptyCellsLeft, ref int emptyCellsUp, ref int emptyCellsDown)
        {
            ref DungeonCell cellToCheck = ref currentCell;
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
            cellToCheck = ref currentCell;
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
            cellToCheck = ref currentCell;
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
        }
    }


}
