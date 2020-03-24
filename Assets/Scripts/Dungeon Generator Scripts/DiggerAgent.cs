using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiggerAgent : DungeonGenerator
{
    [Header("Dungeon width and height in tiles")]
    [SerializeField] int dungeonWidth;                      // Number of columns the dungeon grid is going to have
    [SerializeField] int dungeonHeight;                     // Number of rows the dungeon grid is going to have
    [Header("Corridor length in tiles")]
    [SerializeField] int corridorMinTilesLength;            // Minimum number of tiles of length a corridor can be
    [SerializeField] int corridorMaxTilesLength;            // Maximum number of tiles of length a corridor can be
    [Header("Remove corridors that lead to no room")]
    [SerializeField] bool removeDirtyCorridors;             // Wheter to remove corridors that lead to no room or not

    private Digger digger;                                  // DiggerAgent objet that is going to build the dungeon

    public override void BuildDungeon()
    {
        AssertProperties();
        // Create dungeon grid
        dungeon = new Dungeon(dungeonHeight, dungeonWidth);
        dungeon.createDungeonGrid(dungeonTopLeftCellPosition, floorTileDimensions);
        // Initialise digger
        digger = new Digger(dungeon, corridorMinTilesLength, corridorMaxTilesLength, removeDirtyCorridors);
        // Place digger in a random dungeon cell leaving a margin on the right and the bottom parts of the grid for the first room
        digger.SetDiggerInitialPosition(roomMinTilesWidth, roomMinTilesHeight);
        // Create dungeon
        digger.DigDungeon(roomMinTilesWidth, roomMaxTilesWidth, roomMinTilesHeight, roomMaxTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        // Set entrance and exit rooms
        dungeon.chooseEntranceRoomAndExitRoom(0, dungeon.getDungeonRooms().Count - 1);
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
        // Make sure that the min corridor width is at least 2
        corridorMinTilesLength = Mathf.Max(corridorMinTilesLength, 2);
        // Make sure that the dungeon width and height selected by the user is at least four times bigger than the minimum room width and height
        dungeonWidth = Mathf.Max(dungeonWidth, roomMinTilesWidth * 4);
        dungeonHeight = Mathf.Max(dungeonHeight, roomMinTilesHeight * 4);
        // Make sure that the maximum width and height of a room is greater or equal to the minimum height and width of a room
        roomMaxTilesWidth = Mathf.Max(roomMinTilesWidth, roomMaxTilesWidth);
        roomMaxTilesHeight = Mathf.Max(roomMinTilesHeight, roomMaxTilesHeight);
        // Make sure that the maximum corridor length is greater or equal to the minimum length of a corridor
        corridorMaxTilesLength = Mathf.Max(corridorMinTilesLength, corridorMaxTilesLength);
    }

    private class Digger
    {
        private Dungeon dungeon;            // Dungeon

        private DungeonCell currentCell;    // Dungeon cell in which the digger agent is currently at
        private Direction direction;        // Direction of the digger agent

        private int minCorridorsLength;     // Minimum length of a corridor
        private int maxCorridorsLength;     // Max length of a corridor

        private bool roomPlaced;            // Whether a room has succesfully been placed in the current iteration of the DigDungeon method
        private bool corridorPlaced;        // Whether a corridor has succesfully been placed in the current iteration of the DigDungeon method
        private bool tryAgain;              // When a corridor cannot be built from another corridor this boolean will be set to true to try to build a corridor from the last room created instead
        private bool placeFirstRoom;        // Boolean to place the first room without the need of a corridor

        private Dictionary<Direction, List<FloorTile>> lastRoomPlacedPossibleTiles; // Dictionary that contains the list of tiles of a room from which a corridor can be created  

        private List<Corridor> dirtyCorridors;                                      // List of connected corridors that one of their ends do not connect with a room
        private bool removeDirtyCorridors;                                          // Whether to remove or not the corridors that have one end not ending in a room
        private FloorTile lastTileUsedAsCorridorConnection;                         // Last tile from which corridors where created
        private Direction directionOfWallToRebuild;                                 // Direction of the last tile wall that was removed to connect it with the corridors

        public Digger(Dungeon dungeon, int minCorridorsLength, int maxCorridorsLength, bool removeDirtyCorridors)
        {
            this.dungeon = dungeon;

            this.minCorridorsLength = minCorridorsLength;
            this.maxCorridorsLength = maxCorridorsLength;

            roomPlaced = false;
            corridorPlaced = false;
            tryAgain = false;
            placeFirstRoom = true;

            lastRoomPlacedPossibleTiles = new Dictionary<Direction, List<FloorTile>>();

            dirtyCorridors = new List<Corridor>();
            this.removeDirtyCorridors = removeDirtyCorridors;
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
            // Try to build a room from the current digger position only when a corridor has been placed, otherwise rooms can be disconnected
            if (corridorPlaced || placeFirstRoom)
            {
                TryToPlaceRoom(roomMinWidth, roomMaxWidth, roomMinHeight, roomMaxHeight, tileDimensions, floorMaterial, wallHeight, wallMaterial);
                placeFirstRoom = false;
            }
            // Reset corridor placed boolean
            corridorPlaced = false;
            // If a room has been created in the current iteration or no more corridors could be placed, choose one of the last room placed outer tiles to create the corridor from it
            if(roomPlaced || tryAgain)
            {
                TryToBuildCorridorFromRoom(tileDimensions, floorMaterial, wallHeight, wallMaterial);
            }
            else if(dirtyCorridors.Count > 0)
            {
                // If no room was created in the previous iteration and there are dirty corridors placed the digger is in the last tile of a corridor
                TryToBuildCorridorFromCorridor(tileDimensions, floorMaterial, wallHeight, wallMaterial);
                if (!corridorPlaced)
                {
                    // If no corridor can be placed from the last corridor try again from the last room placed
                    tryAgain = true;
                    // Remove corridors that lead to nowhere based on the boolean set by the user
                    if(removeDirtyCorridors)
                    {
                        foreach(Corridor corridor in dirtyCorridors)
                        {
                            dungeon.getDungeonCorridors().Remove(corridor);
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

        // Method that will try to build a room from the cell the digger is currently at
        private void TryToPlaceRoom(int roomMinWidth, int roomMaxWidth, int roomMinHeight, int roomMaxHeight, Vector3 tileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
        {
            // If a corridor was placed in the previous iteration, the room needs to start from the next cell to the actual current cell based on the direcion the digger agent was going
            DungeonCell previousCurrent = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid()];
            Direction directionToRemoveWall = Direction.Unknown;
            if (corridorPlaced)
            {
                UpdateCurrentCellToNextCell(ref directionToRemoveWall);
            }
            // Check the number of empty tiles from the currentCell in the four directions
            int emptyCellsRight = CheckEmptyCellsRight();
            int emptyCellsLeft = CheckEmptyCellsLeft();
            int emptyCellsUp = CheckEmptyCellsUp();
            int emptyCellsDown = CheckEmptyCellsDown();
            // Check if there is enough space to create a room from current cell
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
                if (AllTilesAreEmpty(topLeftCornerRandomRow, randomRoomHeight, topLeftCornerRandomColumn, randomRoomWidth))
                {
                    // Create new room
                    dungeon.getDungeonRooms().Add(new Room(dungeon, topLeftCornerRandomRow, topLeftCornerRandomColumn, randomRoomHeight, randomRoomWidth, tileDimensions, floorMaterial, wallHeight, wallMaterial));
                    roomPlaced = true;
                    // Get all possible tiles from which a corridor can be placed of the room currently added
                    lastRoomPlacedPossibleTiles.Clear();
                    lastRoomPlacedPossibleTiles.Add(Direction.Up, dungeon.getDungeonRooms()[dungeon.getDungeonRooms().Count - 1].getTilesUpRow());
                    lastRoomPlacedPossibleTiles.Add(Direction.Down, dungeon.getDungeonRooms()[dungeon.getDungeonRooms().Count - 1].getTilesDownRow());
                    lastRoomPlacedPossibleTiles.Add(Direction.Left, dungeon.getDungeonRooms()[dungeon.getDungeonRooms().Count - 1].getTilesLeftColumn());
                    lastRoomPlacedPossibleTiles.Add(Direction.Right, dungeon.getDungeonRooms()[dungeon.getDungeonRooms().Count - 1].getTilesRightColumn());
                    // Remove wall from the previous current cell to connect the room and the corridor, if a corridor was placed in the previous iteration
                    if (corridorPlaced)
                    {
                        previousCurrent.getCellFloorTile().removeWall(direction);
                        currentCell.getCellFloorTile().removeWall(directionToRemoveWall);
                    }
                }
            }
            // If room could not be placed, then set the current to be the previous current to properly connect a corridor with another corridor
            if (!roomPlaced)
            {
                currentCell = previousCurrent;
            }
        }

        // Method that will try to build a corridor from of the outer tiles of the last room placed
        private void TryToBuildCorridorFromRoom(Vector3 tileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
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
                    DungeonCell lastCell = randomTile.getCorrespondingDungeonCell();
                    // Check if corridor can be created from random tile
                    int emptyCells = 0;
                    int rowLengthMultiplier = 0;
                    int columnLengthMultiplier = 0;
                    switch (randomDirection)
                    {
                        case Direction.Up:
                            emptyCells = CheckEmptyCellsUp();
                            rowLengthMultiplier = -1;
                            break;
                        case Direction.Down:
                            emptyCells = CheckEmptyCellsDown();
                            rowLengthMultiplier = 1;
                            break;
                        case Direction.Left:
                            emptyCells = CheckEmptyCellsLeft();
                            columnLengthMultiplier = -1;
                            break;
                        case Direction.Right:
                            emptyCells = CheckEmptyCellsRight();
                            columnLengthMultiplier = 1;
                            break;
                    }
                    if (emptyCells > minCorridorsLength)
                    {
                        // Choose random length
                        int length = Random.Range(minCorridorsLength, emptyCells - 1);
                        length = Mathf.Min(length, maxCorridorsLength);
                        // Set last cell
                        lastCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + (rowLengthMultiplier * length), currentCell.getCellColumnPositionInGrid() + (columnLengthMultiplier * length)];
                        // Build corridor between first and last cell
                        dungeon.getDungeonCorridors().Add(new Corridor(dungeon, currentCell, lastCell, randomDirection, tileDimensions, floorMaterial, wallHeight, wallMaterial, true));
                        corridorPlaced = true;
                        // Add the corridor to the dirty corridors list
                        dirtyCorridors.Add(dungeon.getDungeonCorridors()[dungeon.getDungeonCorridors().Count - 1]);
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
                if (corridorPlaced)
                {
                    break;
                }
                // Remove list
                lastRoomPlacedPossibleTiles.Remove(randomDirection);
            }
            // Reset try again boolean
            tryAgain = false;
        }

        // Method that will try to build a corridor from another corridor
        private void TryToBuildCorridorFromCorridor(Vector3 tileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
        {
            int emptyCells = 0;
            int rowLengthMultiplier = 0;
            int columnLengthMultiplier = 0;
            Direction directionToDig = direction;
            ref DungeonCell lastCell = ref currentCell;
            // Corridor direction depends on the direction of the digger
            if (direction == Direction.Up || direction == Direction.Down)
            {
                // Try to go right or left
                int emptyTilesRight = CheckEmptyCellsRight();
                int emptyTilesLeft = CheckEmptyCellsLeft();
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
                else if (emptyTilesRight > minCorridorsLength)
                {
                    // Digger can only go right
                    directionToDig = Direction.Right;
                }
                else if (emptyTilesLeft > minCorridorsLength)
                {
                    // Digger can only go left
                    directionToDig = Direction.Left;
                }
                // Dig corridor if direction is either right or left
                switch (directionToDig)
                {
                    case Direction.Right:
                        emptyCells = emptyTilesRight;
                        columnLengthMultiplier = 1;
                        break;
                    case Direction.Left:
                        emptyCells = emptyTilesLeft;
                        columnLengthMultiplier = -1;
                        break;
                }
            }
            else
            {
                // Try to go up or down
                int emptyTilesUp = CheckEmptyCellsUp();
                int emptyTilesDown = CheckEmptyCellsDown();
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
                switch (directionToDig)
                {
                    case Direction.Up:
                        emptyCells = emptyTilesUp;
                        rowLengthMultiplier = -1;
                        break;
                    case Direction.Down:
                        emptyCells = emptyTilesDown;
                        rowLengthMultiplier = 1;
                        break;
                }
            }
            if (emptyCells > minCorridorsLength)
            {
                // Choose random length
                int length = Random.Range(minCorridorsLength, emptyCells - 1);
                length = Mathf.Min(length, maxCorridorsLength);
                // Set last cell
                lastCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + (rowLengthMultiplier * length), currentCell.getCellColumnPositionInGrid() + (columnLengthMultiplier * length)];
                // Build corridor between first and last cell
                dungeon.getDungeonCorridors().Add(new Corridor(dungeon, currentCell, lastCell, directionToDig, tileDimensions, floorMaterial, wallHeight, wallMaterial, true));
                corridorPlaced = true;
                // Add the corridor to the dirty corridors list
                dirtyCorridors.Add(dungeon.getDungeonCorridors()[dungeon.getDungeonCorridors().Count - 1]);
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

        // Method that will update the current cell to the next based on the direction the digger agent was going
        private void UpdateCurrentCellToNextCell(ref Direction directionToRemoveWall)
        {
            switch (direction)
            {
                case Direction.Up:
                    if (currentCell.getCellRowPositionInGrid() - 1 >= 0)
                    {
                        currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - 1, currentCell.getCellColumnPositionInGrid()];
                        directionToRemoveWall = Direction.Down;
                    }
                    break;
                case Direction.Down:
                    if (currentCell.getCellRowPositionInGrid() + 1 <= dungeon.getDungeonHeight() - 1)
                    {
                        currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + 1, currentCell.getCellColumnPositionInGrid()];
                        directionToRemoveWall = Direction.Up;
                    }
                    break;
                case Direction.Left:
                    if (currentCell.getCellColumnPositionInGrid() - 1 >= 0)
                    {
                        currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - 1];
                        directionToRemoveWall = Direction.Right;
                    }
                    break;
                case Direction.Right:
                    if (currentCell.getCellColumnPositionInGrid() + 1 <= dungeon.getDungeonWidth() - 1)
                    {
                        currentCell = dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + 1];
                        directionToRemoveWall = Direction.Left;
                    }
                    break;
            }
        }

        // Method that will check that all the tiles needed to build the room with the random parameters are empty
        private bool AllTilesAreEmpty(int topLeftCornerRow, int height, int topLeftCornerColumn, int width)
        {
            for (int i = topLeftCornerRow; i < (topLeftCornerRow + height); i++)
            {
                for (int j = topLeftCornerColumn; j < (topLeftCornerColumn + width); j++)
                {
                    if (dungeon.getDungeonGrid()[i, j].getCellFloorTile() != null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Method that will check the number of dungeon cell that are empty from the current upwards
        private int CheckEmptyCellsUp()
        {
            DungeonCell cellToCheck = currentCell;
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
                cellToCheck = dungeon.getDungeonGrid()[previousRow, cellToCheck.getCellColumnPositionInGrid()];
            }
            return emptyCellsUp;
        }

        // Method that will check the number of dungeon cell that are empty from the current downwards
        private int CheckEmptyCellsDown()
        {
            DungeonCell cellToCheck = currentCell;
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
                cellToCheck = dungeon.getDungeonGrid()[nextRow, cellToCheck.getCellColumnPositionInGrid()];
            }
            return emptyCellsDown;
        }

        // Method that will check the number of dungeon cell that are empty from the current rightwards
        private int CheckEmptyCellsRight()
        {
            DungeonCell cellToCheck = currentCell;
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
                cellToCheck = dungeon.getDungeonGrid()[cellToCheck.getCellRowPositionInGrid(), nextColumn];
            }
            return emptyCellsRight;
        }

        // Method that will check the number of dungeon cell that are empty from the current leftwards
        private int CheckEmptyCellsLeft()
        {
            DungeonCell cellToCheck = currentCell;
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
                cellToCheck = dungeon.getDungeonGrid()[cellToCheck.getCellRowPositionInGrid(), previousColumn];
            }
            return emptyCellsLeft;
        }
    }
}
