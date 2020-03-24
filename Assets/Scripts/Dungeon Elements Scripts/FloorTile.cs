using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType { RoomInnerTile, RoomOuterTile, CorridorTile };

public class FloorTile
{
    private GameObject tile;            // The floor tile itself
    private GameObject upperWall;       // Upper wall of tile - towards positive Z
    private GameObject downWall;        // Down wall of tile - towards negative Z
    private GameObject rightWall;       // Right wall of tile - towards positive X
    private GameObject leftWall;        // left wall of tile - towards negative X
    private DungeonCell dungeonCell;    // Dungeon cell where the floor tile is
    private TileType tileType;          // The type of floor tile

    public FloorTile(DungeonCell cell, Material material, Vector3 dimensions, TileType tileType)
    {
        // Create primitive will attach to the game object a collider, a mesh filter and a mesh renderer
        tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        this.dungeonCell = cell;
        tile.GetComponent<MeshRenderer>().material = material;
        tile.transform.localScale = dimensions;
        tile.transform.position = new Vector3(0.0f, dimensions.y * 0.5f, 0.0f) + cell.getCellWorldPosition();
        this.tileType = tileType;
        tile.name = "FloorTile";
    }

    public void setParent(GameObject parent, bool worldPositionStays)
    {
        tile.transform.SetParent(parent.transform, worldPositionStays);
    }

    public void placeWall(Material material, float wallHeight, Direction direction)
    {
        // Do not place a wall twice
        if(direction == Direction.Up && upperWall != null || direction == Direction.Down && downWall != null || direction == Direction.Right && rightWall != null || direction == Direction.Left && leftWall != null)
        {
            return;
        }
        // Create wall primitive and assign a material to it
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.GetComponent<MeshRenderer>().material = material;
        // Set the scale based on which direction the wall will be moved, value of 0.01f is used to make the wall thin so it can be placed above the tile itself
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
        // Calculate wall position and assign the new wall to the corresponding wall property
        switch (direction)
        {
            case Direction.Up:
                position.z += tile.transform.localScale.z * 0.5f - wall.transform.localScale.z * 0.5f;
                wall.name = "UpperWall";
                upperWall = wall;
                break;
            case Direction.Down:
                position.z -= tile.transform.localScale.z * 0.5f - wall.transform.localScale.z * 0.5f;
                wall.name = "DownWall";
                downWall = wall;
                break;
            case Direction.Right:
                position.x += tile.transform.localScale.x * 0.5f - wall.transform.localScale.x * 0.5f;
                wall.name = "RightWall";
                rightWall = wall;
                break;
            case Direction.Left:
                position.x -= tile.transform.localScale.x * 0.5f - wall.transform.localScale.x * 0.5f;
                wall.name = "LeftWall";
                leftWall = wall;
                break;
        }
        // Set wall position, as walls are game objects (class type) they are references, so a change in the original wall object will also change the corresponding wall property
        wall.transform.position = position;
        wall.transform.SetParent(tile.transform);
    }

    public void removeWall(Direction direction)
    {
        switch(direction)
        {
            case Direction.Up:
                if(upperWall != null)
                {
                    upperWall.transform.parent = null;
                    Object.Destroy(upperWall);
                    upperWall = null;
                }
                break;
            case Direction.Down:
                if(downWall != null)
                {
                    downWall.transform.parent = null;
                    Object.Destroy(downWall);
                    downWall = null;
                }
                break;
            case Direction.Right:
                if (rightWall != null)
                {
                    rightWall.transform.parent = null;
                    Object.Destroy(rightWall);
                    rightWall = null;
                }
                break;
            case Direction.Left:
                if (leftWall != null)
                {
                    leftWall.transform.parent = null;
                    Object.Destroy(leftWall);
                    leftWall = null;
                }
                break;
        }
    }

    public DungeonCell getCorrespondingDungeonCell()
    {
        return dungeonCell;
    }

    public TileType getTileType()
    {
        return this.tileType;
    }

    public void setTileType(TileType type)
    {
        this.tileType = type;
    }

    public void DestroyFloorTile()
    {
        dungeonCell.removeFloorTile();
        removeWall(Direction.Up);
        removeWall(Direction.Down);
        removeWall(Direction.Right);
        removeWall(Direction.Left);
        tile.transform.parent = null;
        Object.Destroy(tile);
    }
}
