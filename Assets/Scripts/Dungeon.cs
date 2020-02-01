using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Down, Right, Left, Unknown};

public class Dungeon
{
    private int dungeonWidth;                   // Number of columns in the grid
    private int dungeonHeight;                  // Number of rows in the grid
    private DungeonCell[,] dungeonGrid;         // 2D grid that represents the dungeon
    private List<Room> dungeonRooms;            // List with all the rooms in the dungeon
    private List<Corridor> dungeonCorridors;    // List with all the corridors in the dungeon

    public Dungeon(int dungeonWidth, int dungeonHeight)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonHeight = dungeonHeight;
        dungeonGrid = new DungeonCell[this.dungeonHeight, this.dungeonWidth];
        dungeonRooms = new List<Room>();
        dungeonCorridors = new List<Corridor>();

    }

    public int getDungeonWidth()
    {
        return dungeonWidth;
    }

    public int getDungeonHeight()
    {
        return dungeonHeight;
    }

    public DungeonCell[,] getDungeonGrid()
    {
        return dungeonGrid;
    }

    public void createDungeonGrid(Vector3 dungeonTopLeftCellPosition, Vector3 floorTileDimensions)
    {
        for (int i = 0; i < dungeonHeight; i++)
        {
            for (int j = 0; j < dungeonWidth; j++)
            {
                Vector3 cellPosition = dungeonTopLeftCellPosition + new Vector3(floorTileDimensions.x * j, floorTileDimensions.y, -floorTileDimensions.z * i);
                dungeonGrid[i, j] = new DungeonCell(cellPosition, i, j);
            }
        }
    }

    public List<Room> getDungeonRooms()
    {
        return dungeonRooms;
    }

    public List<Corridor> getDungeonCorridors()
    {
        return dungeonCorridors;
    }
}

public class DungeonCell
{
    private Vector3 cellPosition;       // Position of the cell in world space
    private FloorTile floorTile;        // A dungeon cell might containt a floor tile
    private int rowPositionInGrid;      // Row position of the cell in the dungeon grid
    private int columnPositionInGrid;   // Column position of the cell in the dungeon grid

    public DungeonCell(Vector3 cellPosition, int rowPositionInGrid, int columnPositionInGrid)
    {
        this.cellPosition = cellPosition;
        this.rowPositionInGrid = rowPositionInGrid;
        this.columnPositionInGrid = columnPositionInGrid;
    }

    public Vector3 getCellWorldPosition()
    {
        return cellPosition;
    }

    public int getCellRowPositionInGrid()
    {
        return rowPositionInGrid;
    }

    public int getCellColumnPositionInGrid()
    {
        return columnPositionInGrid;
    }

    public void setCellFloorTile(FloorTile floorTile)
    {
        this.floorTile = floorTile;
    }

    public FloorTile getCellFloorTile()
    {
        return floorTile;
    }

    public void removeFloorTile()
    {
        floorTile = null;
    }
}