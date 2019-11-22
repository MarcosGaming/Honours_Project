using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    private GameObject room;            // Empty game object that represents a room
    private FloorTile[,] floorTiles;    // Grid of floor tiles

    public Room(int roomMaxDimensions, Vector3 topLeftCorner, Vector3 floorTileDimensions, Material floorTileMaterial, float wallHeight, Material wallMaterial)
    {
        // Randomly choose a room dimension smaller or equal than the maximum
        int roomDimensions = roomMaxDimensions;
        // Create empty game object for the room
        room = new GameObject();
        room.name = "Room";
        BoxCollider collider = room.AddComponent<BoxCollider>();
        // Top left corner is center of top left floor tile, to find the true top left corner the dimensions of the tile need to be considered
        Vector3 corner = topLeftCorner + new Vector3(-floorTileDimensions.x * 0.5f, 0.0f, floorTileDimensions.z * 0.5f);
        // Place room game object in center of room
        room.transform.position = corner + new Vector3(floorTileDimensions.x * (float)roomDimensions * 0.5f, wallHeight * 0.5f, -floorTileDimensions.z * (float)roomDimensions * 0.5f);
        collider.size = new Vector3(roomDimensions * floorTileDimensions.x + floorTileDimensions.x, wallHeight, roomDimensions * floorTileDimensions.z + floorTileDimensions.z);
        // Place floor tiles of room
        floorTiles = new FloorTile[roomDimensions, roomDimensions];
        for (int i = 0; i < roomDimensions; i++)
        {
            for (int j = 0; j < roomDimensions; j++)
            {
                Vector3 tilePosition = topLeftCorner + new Vector3(floorTileDimensions.x * j, 0.0f, -floorTileDimensions.z * i);
                floorTiles[i, j] = new FloorTile(floorTileMaterial, floorTileDimensions, tilePosition);
            }
        }
        // Place walls in the uper row of tiles
        for (int column = 0; column < roomDimensions; column++)
        {
            floorTiles[0, column].placeWall(wallMaterial, wallHeight, Direction.Forward);
        }
        // Place walls in the lower row of tiles
        for (int column = 0; column < roomDimensions; column++)
        {
            floorTiles[roomDimensions - 1, column].placeWall(wallMaterial, wallHeight, Direction.Backward);
        }
        // Place walls in the left column of tiles
        for (int row = 0; row < roomDimensions; row++)
        {
            floorTiles[row, 0].placeWall(wallMaterial, wallHeight, Direction.Left);
        }
        // Place walls in the right column of tiles
        for (int row = 0; row < roomDimensions; row++)
        {
            floorTiles[row, roomDimensions - 1].placeWall(wallMaterial, wallHeight, Direction.Right);
        }
        // Set parent of floor tiles to be the room
        for (int i = 0; i < roomDimensions; i++)
        {
            for (int j = 0; j < roomDimensions; j++)
            {
                floorTiles[i, j].setParent(room);
            }
        }
    }
}

