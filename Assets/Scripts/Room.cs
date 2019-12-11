using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    private GameObject room;            // Empty game object that represents a room
    private FloorTile[,] floorTiles;    // Grid of floor tiles
    private Vector3 roomCenter;         // Center of room
    private float roomWidth;            // Width of the room
    private float roomHeight;           // Height of the room

    public Room(Vector3 topLeftCorner, int roomWidth, int roomHeight, Vector3 floorTileDimensions, Material floorTileMaterial, float wallHeight, Material wallMaterial)
    {
        // Create empty game object for the room
        room = new GameObject();
        room.name = "Room " + DungeonGeneratorBSP.counter;
        BoxCollider collider = room.AddComponent<BoxCollider>();
        // Top left corner is center of top left floor tile, to find the true top left corner the dimensions of the tile need to be considered
        Vector3 corner = topLeftCorner + new Vector3(-floorTileDimensions.x * 0.5f, 0.0f, floorTileDimensions.z * 0.5f);
        // Place room game object in center of room
        room.transform.position = corner + new Vector3(floorTileDimensions.x * (float)roomWidth * 0.5f, wallHeight * 0.5f, -floorTileDimensions.z * (float)roomHeight * 0.5f);
        collider.size = new Vector3(roomWidth * floorTileDimensions.x + floorTileDimensions.x, wallHeight, roomHeight * floorTileDimensions.z + floorTileDimensions.z);
        // Set room properties
        roomCenter = room.transform.position;
        this.roomWidth = roomWidth * floorTileDimensions.x;
        this.roomHeight = roomHeight * floorTileDimensions.z;
        // Place floor tiles of room
        floorTiles = new FloorTile[roomHeight, roomWidth];
        for (int i = 0; i < roomHeight; i++)
        {
            for (int j = 0; j < roomWidth; j++)
            {
                Vector3 tilePosition = topLeftCorner + new Vector3(floorTileDimensions.x * j, 0.0f, -floorTileDimensions.z * i);
                floorTiles[i, j] = new FloorTile(floorTileMaterial, floorTileDimensions, tilePosition);
            }
        }
        // Place walls in the uper row of tiles
        for (int column = 0; column < roomWidth; column++)
        {
            floorTiles[0, column].placeWall(wallMaterial, wallHeight, Direction.Forward);
        }
        // Place walls in the lower row of tiles
        for (int column = 0; column < roomWidth; column++)
        {
            floorTiles[roomHeight - 1, column].placeWall(wallMaterial, wallHeight, Direction.Backward);
        }
        // Place walls in the left column of tiles
        for (int row = 0; row < roomHeight; row++)
        {
            floorTiles[row, 0].placeWall(wallMaterial, wallHeight, Direction.Left);
        }
        // Place walls in the right column of tiles
        for (int row = 0; row < roomHeight; row++)
        {
            floorTiles[row, roomWidth - 1].placeWall(wallMaterial, wallHeight, Direction.Right);
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

    public float getRoomWidth()
    {
        return roomWidth;
    }

    public float getRoomHeight()
    {
        return roomHeight;
    }

    public ref FloorTile[,] getFloorTiles()
    {
        return ref floorTiles;
    }
}

