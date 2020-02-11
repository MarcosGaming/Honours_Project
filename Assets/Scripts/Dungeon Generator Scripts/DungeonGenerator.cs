using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The dungeon generator is going to assume the following when building the dungeon: right is positive x, left is negative x, up is positive z and down is negative z
public abstract class DungeonGenerator : MonoBehaviour
{
    [Header("Top left corner of the dungeon")]
    [SerializeField] protected Vector3 dungeonTopLeftCellPosition;    // Top left position of dungeon cell [0,0]
    [Header("Room width and height in tiles")]
    [SerializeField] protected int roomMinTilesWidth;                 // Minimum number of column cells a room needs to have
    [SerializeField] protected int roomMaxTilesWidth;                 // Maximum number of column cells a room can have
    [SerializeField] protected int roomMinTilesHeight;                // Minimum number of row cells a room needs to have
    [SerializeField] protected int roomMaxTilesHeight;                // Maximum number of row cells a room can have
    [Header("Floor and wall materials")]
    [SerializeField] protected Material floorMaterial;                // Material that will be used for the floor
    [SerializeField] protected Material wallMaterial;                 // Material of the walls
    [Header("Dimensions of floor tiles and wall height")]
    [SerializeField] protected Vector3 floorTileDimensions;           // Dimensions of each floor tile
    [SerializeField] protected float wallHeight;                      // Height of each wall

    protected bool dungeonBuildingFinished;                           // Whether the dungeon generator has finished building the dungeon

    protected Dungeon dungeon;                                        // The dungeon to be built

    public abstract void BuildDungeon();                              // Method for building the dungeon
    protected abstract void AssertProperties();                       // Method for making sure that the properties entered by the user are feasible

    public void SaveDungeonAsPrefab() { dungeon.saveDungeonAsPrefab(); }                                        // Mehtod to save the dungeon generated as a prefab
    public bool isDungeonBuilt() { return dungeonBuildingFinished; }                                            // Method to check if the dungeon generator has finished building the dungeon
    public void DestroyDungeon() { dungeonBuildingFinished = false; dungeon.DestroyDungeon(); dungeon = null; } // Method to destroy the dungeon

}
