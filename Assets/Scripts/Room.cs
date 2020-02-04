using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RoomType {startRoom, normalRoom, exitRoom};

public class Room
{
    private GameObject room;                    // Empty game object that represents a room
    private RoomType roomType;                  // The type of the room

    private FloorTile[,] floorTiles;            // Grid of floor tiles
    private List<FloorTile> tilesUpRow;         // List of the floor tiles in the most up row of the room
    private List<FloorTile> tilesDownRow;       // List of the floor tiles in the most down row of the room
    private List<FloorTile> tilesLeftColumn;    // List of the floor tiles in the most left column of the room
    private List<FloorTile> tilesRightColumn;   // List of the floor tiles in the most right column of the room

    private Vector3 roomCenter;                 // Center of room

    private int roomWidth;                      // Number of tile columns the room has
    private int roomHeight;                     // Number of tile rows the room has

    public Room(Dungeon dungeon, int roomTopLeftCellRow, int roomTopLeftCellColumn, int roomHeight, int roomWidth, Vector3 floorTileDimensions, Material floorTileMaterial, float wallHeight, Material wallMaterial)
    {
        // Create empty game object for the room
        room = new GameObject();
        room.name = "Room";
        // Initialise lists
        tilesUpRow = new List<FloorTile>();
        tilesDownRow = new List<FloorTile>();
        tilesLeftColumn = new List<FloorTile>();
        tilesRightColumn = new List<FloorTile>();
        // Get corner position of the room in the world to place the empty game object in the middle of where the room is going to be
        DungeonCell[,] grid = dungeon.getDungeonGrid();
        Vector3 corner = grid[roomTopLeftCellRow, roomTopLeftCellColumn].getCellWorldPosition() + new Vector3(-floorTileDimensions.x * 0.5f, 0.0f, floorTileDimensions.z * 0.5f);
        room.transform.position = corner + new Vector3(floorTileDimensions.x * (float)roomWidth * 0.5f, wallHeight * 0.5f, -floorTileDimensions.z * (float)roomHeight * 0.5f);
        // Set room properties
        roomCenter = room.transform.position;
        this.roomWidth = roomWidth;
        this.roomHeight = roomHeight;
        // Place floor tiles of room
        floorTiles = new FloorTile[roomHeight, roomWidth];
        int tileRowPos = 0;
        for(int i = roomTopLeftCellRow; i < roomTopLeftCellRow + roomHeight; i++)
        {
            int tileColumnPos = 0;
            for (int j = roomTopLeftCellColumn; j < roomTopLeftCellColumn + roomWidth; j++)
            {
                floorTiles[tileRowPos, tileColumnPos] = new FloorTile(grid[i,j] ,floorTileMaterial, floorTileDimensions, grid[i, j].getCellWorldPosition(), TileType.RoomInnerTile);
                grid[i, j].setCellFloorTile(floorTiles[tileRowPos, tileColumnPos]); 
                tileColumnPos++;
            }
            tileRowPos++;
        }
        // Place walls in the uper row of tiles
        for (int column = 0; column < roomWidth; column++)
        {
            floorTiles[0, column].placeWall(wallMaterial, wallHeight, Direction.Up);
            floorTiles[0, column].setTileType(TileType.RoomOuterTile);
            tilesUpRow.Add(floorTiles[0, column]);
        }
        // Place walls in the lower row of tiles
        for (int column = 0; column < roomWidth; column++)
        {
            floorTiles[roomHeight - 1, column].placeWall(wallMaterial, wallHeight, Direction.Down);
            floorTiles[roomHeight - 1, column].setTileType(TileType.RoomOuterTile);
            tilesDownRow.Add(floorTiles[roomHeight - 1, column]);
        }
        // Place walls in the left column of tiles
        for (int row = 0; row < roomHeight; row++)
        {
            floorTiles[row, 0].placeWall(wallMaterial, wallHeight, Direction.Left);
            floorTiles[row, 0].setTileType(TileType.RoomOuterTile);
            tilesLeftColumn.Add(floorTiles[row, 0]);
        }
        // Place walls in the right column of tiles
        for (int row = 0; row < roomHeight; row++)
        {
            floorTiles[row, roomWidth - 1].placeWall(wallMaterial, wallHeight, Direction.Right);
            floorTiles[row, roomWidth - 1].setTileType(TileType.RoomOuterTile);
            tilesRightColumn.Add(floorTiles[row, roomWidth - 1]);
        }
        // Set parent of floor tiles to be the room
        for (int i = 0; i < roomHeight; i++)
        {
            for (int j = 0; j < roomWidth; j++)
            {
                floorTiles[i, j].setParent(room, true);
            }
        }
    }

    public Vector3 getRoomCenter()
    {
        return roomCenter;
    }

    public int getRoomWidth()
    {
        return roomWidth;
    }

    public int getRoomHeight()
    {
        return roomHeight;
    }

    public FloorTile[,] getFloorTiles()
    {
        return floorTiles;
    }

    public List<FloorTile> getTilesUpRow()
    {
        return tilesUpRow;
    }

    public List<FloorTile> getTilesDownRow()
    {
        return tilesDownRow;
    }

    public List<FloorTile> getTilesLeftColumn()
    {
        return tilesLeftColumn;
    }

    public List<FloorTile> getTilesRightColumn()
    {
        return tilesRightColumn;
    }
}

