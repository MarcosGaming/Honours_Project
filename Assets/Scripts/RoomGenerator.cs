using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction{ Forward, Backward, Right, Left};

// The room generator class is going to assume the following when placing rooms: right is positive x, left is negative x, forward is positive z and backward is negative z
public class RoomGenerator : MonoBehaviour
{
    private List<Room> rooms;                       // List of the rooms in the dungeon
    [SerializeField] Material floorMaterial;        // Material that will be used for the floor
    [SerializeField] Vector3 floorTileDimensions;   // Dimensions of each floor tile
    [SerializeField] Material wallMaterial;         // Material of the walls
    [SerializeField] float wallHeight;              // Height of each wall
    [SerializeField] int roomWidth;                 // How big a room can be
    [SerializeField] int roomHeight;

    // Start is called before the first frame update
    void Start()
    {
        rooms = new List<Room>();
        createRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void createRoom()
    {
        rooms.Add(new Room(new Vector3(0.0f,0.0f,0.0f), roomWidth, roomHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
    }
}
