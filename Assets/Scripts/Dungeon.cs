using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Down, Right, Left, Unknown};

public class Dungeon
{
    private DungeonCell[,] dungeonGrid; // 2D grid that represents the dungeon
    private int dungeonWidth;           // Number of columns in the grid
    private int dungeonHeight;          // Number of rows in the grid

    public Dungeon(int dungeonWidth, int dungeonHeight)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonHeight = dungeonHeight;
        dungeonGrid = new DungeonCell[this.dungeonHeight, this.dungeonWidth];
    }

    public ref DungeonCell[,] getDungeonGrid()
    {
        return ref dungeonGrid;
    }

    public int getDungeonWidth()
    {
        return dungeonWidth;
    }

    public int getDungeonHeight()
    {
        return dungeonHeight;
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

    public void setCellFloorTile(ref FloorTile floorTile)
    {
        this.floorTile = floorTile;
    }

    public ref FloorTile getCellFloorTile()
    {
        return ref floorTile;
    }

    public void removeFloorTile()
    {
        floorTile = null;
    }
}