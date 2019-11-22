using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTile
{
    private GameObject tile;            // The floor tile itself
    private List<GameObject> walls;     // A floor tile might have up to four walls

    public FloorTile(Material material, Vector3 dimensions, Vector3 position)
    {
        // Create primitive will attach to the game object a collider, a mesh filter and a mesh renderer
        tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tile.name = "FloorTile";
        tile.GetComponent<MeshRenderer>().material = material;
        tile.transform.localScale = dimensions;
        tile.transform.position = new Vector3(0.0f, dimensions.y * 0.5f, 0.0f) + position;
        walls = new List<GameObject>();
    }
    public void setParent(GameObject parent)
    {
        tile.transform.SetParent(parent.transform);
    }
    // Method to create a wall
    public void placeWall(Material material, float wallHeight, Direction direction)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.GetComponent<MeshRenderer>().material = material;
        // Set the scale based on which direction the wall will be moved
        if (direction == Direction.Forward || direction == Direction.Backward)
        {
            wall.transform.localScale = new Vector3(tile.transform.localScale.x, wallHeight, tile.transform.localScale.z * 0.5f);
        }
        else
        {
            wall.transform.localScale = new Vector3(tile.transform.localScale.x * 0.5f, wallHeight, tile.transform.localScale.z);
        }
        // Set the position based on the direction
        Vector3 position = tile.transform.position;
        position.y = wallHeight * 0.5f;
        switch (direction)
        {
            case Direction.Forward:
                position.z += tile.transform.localScale.z * 0.5f + wall.transform.localScale.z * 0.5f;
                break;
            case Direction.Backward:
                position.z -= tile.transform.localScale.z * 0.5f + wall.transform.localScale.z * 0.5f;
                break;
            case Direction.Right:
                position.x += tile.transform.localScale.x * 0.5f + wall.transform.localScale.x * 0.5f;
                break;
            case Direction.Left:
                position.x -= tile.transform.localScale.x * 0.5f + wall.transform.localScale.x * 0.5f;
                break;
        }
        wall.transform.position = position;
        wall.transform.SetParent(tile.transform);
        walls.Add(wall);
    }
}
