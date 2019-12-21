using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { Up, Down, Right, Left };

public enum TileType { RoomInnerTile, RoomOuterTile, CorridorTile };

public class FloorTile
{
    private GameObject tile;            // The floor tile itself
    private GameObject UpperWall;       // Upper wall of tile - towards positive Z
    private GameObject DownWall;        // Down wall of tile - towards negative Z
    private GameObject RightWall;       // Right wall of tile - towards positive X
    private GameObject LeftWall;        // left wall of tile - towards negative X
    private DungeonCell dungeonCell;    // Dungeon cell where the floor tile is
    private TileType tileType;          // The type of floor tile

    public FloorTile(ref DungeonCell cell, Material material, Vector3 dimensions, Vector3 position, TileType tileType)
    {
        // Create primitive will attach to the game object a collider, a mesh filter and a mesh renderer
        this.dungeonCell = cell;
        tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "FloorTile";
        tile.GetComponent<MeshRenderer>().material = material;
        tile.transform.localScale = dimensions;
        tile.transform.position = new Vector3(0.0f, dimensions.y * 0.5f, 0.0f) + position;
        this.tileType = tileType;
    }
    public void setParent(GameObject parent, bool worldPositionStays)
    {
        tile.transform.SetParent(parent.transform, worldPositionStays);
    }
    // Method to create a wall
    public void placeWall(Material material, float wallHeight, Direction direction)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.GetComponent<MeshRenderer>().material = material;
        // Set the scale based on which direction the wall will be moved
        if (direction == Direction.Up || direction == Direction.Down)
        {
            wall.transform.localScale = new Vector3(tile.transform.localScale.x, wallHeight, 0.01f);
        }
        else
        {
            wall.transform.localScale = new Vector3(0.01f, wallHeight, tile.transform.localScale.z);
        }
        // Set the position based on the direction
        Vector3 position = tile.transform.position;
        position.y = wallHeight * 0.5f + tile.transform.localScale.y * 2.0f;
        switch (direction)
        {
            case Direction.Up:
                position.z += tile.transform.localScale.z * 0.5f - wall.transform.localScale.z * 0.5f;
                wall.transform.position = position;
                wall.transform.SetParent(tile.transform);
                wall.name = "UpperWall";
                UpperWall = wall;
                break;
            case Direction.Down:
                position.z -= tile.transform.localScale.z * 0.5f - wall.transform.localScale.z * 0.5f;
                wall.transform.position = position;
                wall.transform.SetParent(tile.transform);
                wall.name = "DownWall";
                DownWall = wall;
                break;
            case Direction.Right:
                position.x += tile.transform.localScale.x * 0.5f - wall.transform.localScale.x * 0.5f;
                wall.transform.position = position;
                wall.transform.SetParent(tile.transform);
                wall.name = "RightWall";
                RightWall = wall;
                break;
            case Direction.Left:
                position.x -= tile.transform.localScale.x * 0.5f - wall.transform.localScale.x * 0.5f;
                wall.transform.position = position;
                wall.transform.SetParent(tile.transform);
                wall.name = "LeftWall";
                LeftWall = wall;
                break;
        }
    }

    public void removeWall(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
                if(UpperWall != null)
                {
                    Object.Destroy(UpperWall);
                    UpperWall = null;
                }
                break;
            case Direction.Down:
                if(DownWall != null)
                {
                    Object.Destroy(DownWall);
                    DownWall = null;
                }
                break;
            case Direction.Right:
                if (RightWall != null)
                {
                    Object.Destroy(RightWall);
                    RightWall = null;
                }
                break;
            case Direction.Left:
                if (LeftWall != null)
                {
                    Object.Destroy(LeftWall);
                    LeftWall = null;
                }
                break;
        }
    }
    
    public ref DungeonCell getCorrespondingDungeonCell()
    {
        return ref dungeonCell;
    }

    public TileType getTileType()
    {
        return this.tileType;
    }

    public void setTileType(TileType type)
    {
        this.tileType = type;
    }
}
