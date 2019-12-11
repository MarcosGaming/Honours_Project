using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{
    public Corridor(Room room1, Room room2, Vector3 floorTileDimensions, Material floorTileMaterial, float wallHeight, Material wallMaterial)
    {
        // Get room1 limits
        float room1LeftLimit = room1.getRoomCenter().x - (floorTileDimensions.x * room1.getRoomWidth() * 0.5f);
        float room1RightLimit = room1.getRoomCenter().x + (floorTileDimensions.x * room1.getRoomWidth() * 0.5f);
        float room1UpLimit = room1.getRoomCenter().z + (floorTileDimensions.z * room1.getRoomHeight() * 0.5f);
        float room1DownLimit = room1.getRoomCenter().z - (floorTileDimensions.z * room1.getRoomHeight() * 0.5f);
        // Get room2 limits
        float room2LeftLimit = room2.getRoomCenter().x - (floorTileDimensions.x * room2.getRoomWidth() * 0.5f);
        float room2RightLimit = room2.getRoomCenter().x + (floorTileDimensions.x * room2.getRoomWidth() * 0.5f);
        float room2UpLimit = room2.getRoomCenter().z + (floorTileDimensions.z * room2.getRoomHeight() * 0.5f);
        float room2DownLimit = room2.getRoomCenter().z - (floorTileDimensions.z * room2.getRoomHeight() * 0.5f);
        // Check if room 1 is between the right and left limits of room 2

        // Check if room2 is between the right and left limits of room1
        if (room1LeftLimit <= room2LeftLimit && room2LeftLimit <= room1RightLimit || room1LeftLimit <= room2RightLimit && room2RightLimit <= room1RightLimit)
        {
            // Check if room2 has at least two tiles that can be connected directly to room1
            if(Mathf.Abs(room1RightLimit - room2LeftLimit) > (floorTileDimensions.x * 2.0f) || Mathf.Abs(room2RightLimit - room1LeftLimit) > (floorTileDimensions.x * 2.0f))
            {

            }
        }
        // Check if room2 is between up and down limits of room1
        else if(room1DownLimit <= room2UpLimit && room2UpLimit <= room1UpLimit || room1DownLimit <= room2DownLimit && room2DownLimit <= room1UpLimit)
        {

        }
        else
        {
            // Connect rooms with an L shape corridor

        }

    }
}
