using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public enum Direction { Up, Down, Right, Left, Unknown};

public class Dungeon
{
    private GameObject dungeonObject;           // Game object that represents the dungeon
    private int dungeonWidth;                   // Number of columns in the grid
    private int dungeonHeight;                  // Number of rows in the grid
    private DungeonCell[,] dungeonGrid;         // 2D grid that represents the dungeon
    private List<Room> dungeonRooms;            // List with all the rooms in the dungeon
    private List<Corridor> dungeonCorridors;    // List with all the corridors in the dungeon

    public Dungeon(int dungeonWidth, int dungeonHeight)
    {
        dungeonObject = new GameObject();
        dungeonObject.name = "dungeon";
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

    public void setRoomsAndCorridorsAsDungeonChildren()
    {
        // Set rooms and corridors to be childs of the dungeon
        foreach (Room room in dungeonRooms)
        {
            room.getRoomGameObject().transform.SetParent(dungeonObject.transform);
        }
        foreach (Corridor corridor in dungeonCorridors)
        {
            corridor.getCorridorGameObject().transform.SetParent(dungeonObject.transform);
        }
    }

    public void saveDungeonAsPrefab()
    {
        #if (UNITY_EDITOR)
            // Create folder to store dungeon prefabs if there is not one already
            string folderPath = "Assets/DungeonPrefabs/";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            // Set path name
            string localPath = folderPath + dungeonObject.name + ".prefab";
            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
            // Store dungeon game object as a prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(dungeonObject, localPath, InteractionMode.UserAction);
        #endif
    }

    public void randomlyChooseEntranceRoomAndExitRoom()
    {
        // Choose entrance
        int entranceRandomIndex = Random.Range(0, dungeonRooms.Count - 1);
        // Instantiate the exit prefab in another room different from the entrance one
        while (true)
        {
            int exitRandomIndex = Random.Range(0, dungeonRooms.Count - 1);
            if (exitRandomIndex != entranceRandomIndex)
            {
                chooseEntranceRoomAndExitRoom(entranceRandomIndex, exitRandomIndex);
                break;
            }
        }
    }

    public void chooseEntranceRoomAndExitRoom(int entranceIndex, int exitIndex)
    {
        dungeonRooms[entranceIndex].getRoomGameObject().name = "Entrance";
        dungeonRooms[exitIndex].getRoomGameObject().name = "Exit";
    }

    public void DestroyDungeon()
    {
        foreach (Room room in dungeonRooms)
        {
            room.DestroyRoom();
        }
        foreach (Corridor corridor in dungeonCorridors)
        {
            corridor.DestroyCorridor();
        }
        for(int i = 0; i < dungeonHeight; i++)
        {
            for(int j = 0; j < dungeonWidth; j++)
            {
                dungeonGrid[i, j].removeFloorTile();
                dungeonGrid[i, j] = null;
            }
        }
        Object.Destroy(dungeonObject);
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