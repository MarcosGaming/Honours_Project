using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{
    private GameObject corridor;            // Game object that represents the corridor
    private List<FloorTile> corridorTiles;  // List of the tiles that form the corridor

    public Corridor(ref Dungeon dungeon, ref Room room1, ref Room room2, Vector3 floorTileDimensions, Material floorTileMaterial, float wallHeight, Material wallMaterial)
    {
        // Initialize list
        corridorTiles = new List<FloorTile>();
        // Initialize game object
        corridor = new GameObject();
        corridor.name = "Corridor";
        // Choose one random tile from room 1 and room 2
        ref FloorTile randomRoom1Tile = ref room1.getFloorTiles()[Random.Range(0,room1.getRoomHeight() - 1),Random.Range(0, room1.getRoomWidth() - 1)];
        ref FloorTile randomRoom2Tile = ref room2.getFloorTiles()[Random.Range(0, room2.getRoomHeight() - 1), Random.Range(0, room2.getRoomWidth() - 1)];
        // Get the corresponding cell of each tile in the dungeon grid
        ref DungeonCell startCell = ref randomRoom1Tile.getCorrespondingDungeonCell();
        ref DungeonCell currentCell = ref randomRoom1Tile.getCorrespondingDungeonCell();
        ref DungeonCell nextCell = ref randomRoom1Tile.getCorrespondingDungeonCell();
        ref DungeonCell endCell = ref randomRoom2Tile.getCorrespondingDungeonCell();
        // Traverse columns until the end cell column is reached
        while(true)
        {
            // If grid cell is empty place floor tile normally
            if (currentCell.getCellFloorTile() == null && currentCell.getCellColumnPositionInGrid() != endCell.getCellColumnPositionInGrid())
            {
                // Create tile
                FloorTile tile = new FloorTile(ref currentCell, floorTileMaterial, floorTileDimensions, currentCell.getCellWorldPosition(), TileType.CorridorTile);
                // When traversing right or left the tile is going to need upper and down walls
                tile.placeWall(wallMaterial, wallHeight, Direction.Up);
                tile.placeWall(wallMaterial, wallHeight, Direction.Down);
                // Add tile to the list of corridor tiles
                corridorTiles.Add(tile);
                currentCell.setCellFloorTile(ref tile);
            }
            // Traverse right
            if (currentCell.getCellColumnPositionInGrid() < endCell.getCellColumnPositionInGrid())
            {
                // If the grid cell has a tile with walls remove the ones that are not needed
                if (currentCell.getCellFloorTile().getTileType() == TileType.RoomOuterTile || currentCell.getCellFloorTile().getTileType() == TileType.CorridorTile)
                {
                    // When traversing right always remove the right wall of current tile if there is one
                    currentCell.getCellFloorTile().removeWall(Direction.Right);
                }
                // Get next cell
                nextCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() + 1];
                // Remove left wall of next cell if there is one
                if(nextCell.getCellFloorTile() != null)
                {
                    nextCell.getCellFloorTile().removeWall(Direction.Left);
                }
            }
            // Traverse left
            else if(currentCell.getCellColumnPositionInGrid() > endCell.getCellColumnPositionInGrid())
            {
                // If the grid cell has a tile with walls remove the ones that are not needed
                if (currentCell.getCellFloorTile().getTileType() == TileType.RoomOuterTile || currentCell.getCellFloorTile().getTileType() == TileType.CorridorTile)
                {
                    // When traversing left always remove the left wall of current tile if there is one
                    currentCell.getCellFloorTile().removeWall(Direction.Left);
                }
                nextCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid(), currentCell.getCellColumnPositionInGrid() - 1];
                // Remove right wall of next cell if there is one
                if (nextCell.getCellFloorTile() != null)
                {
                    nextCell.getCellFloorTile().removeWall(Direction.Right);
                }
            }
            // Current cell column and end cell column are the same
            else
            {
                // If grid cell is empty place floor tile with walls forming a corner
                if (currentCell.getCellFloorTile() == null)
                {
                    // Check in which row direction (up or down) the corridor will go next
                    Direction cornerWall1;
                    if(currentCell.getCellRowPositionInGrid() < endCell.getCellRowPositionInGrid())
                    {
                        // If the corridor needs to go down the corner wall needs to be placed up the current tile
                        cornerWall1 = Direction.Up;
                    }
                    else
                    {
                        // If the corridor needs to go up the corner wall needs to be placed down the current tile
                        cornerWall1 = Direction.Down;
                    }
                    // Check if the start cell is at the right or left of the current cell
                    Direction cornerWall2;
                    if (startCell.getCellColumnPositionInGrid() < currentCell.getCellColumnPositionInGrid())
                    {
                        // When coming from the left, the wall of the corner corridor cell needs to be placed on the right
                        cornerWall2 = Direction.Right;
                    }
                    else
                    {
                        // When coming from the right, the wall of the corner corridor cell needs to be placed on the left
                        cornerWall2 = Direction.Left;
                    }
                    // Create tile
                    FloorTile tile = new FloorTile(ref currentCell, floorTileMaterial, floorTileDimensions, currentCell.getCellWorldPosition(), TileType.CorridorTile);
                    // Place corner tile walls
                    tile.placeWall(wallMaterial, wallHeight, cornerWall1);
                    tile.placeWall(wallMaterial, wallHeight, cornerWall2);
                    // Add tile to the list of corridor tiles
                    corridorTiles.Add(tile);
                    currentCell.setCellFloorTile(ref tile);
                }
                // Exit loop
                break;
            }
            // Set current cell to next cell
            currentCell = ref nextCell;
        }
        // Traverse rows until the end cell row is reached
        while(true)
        {
            // If grid cell is empty place floor tile normally
            if (currentCell.getCellFloorTile() == null)
            {
                // Create tile
                FloorTile tile = new FloorTile(ref currentCell, floorTileMaterial, floorTileDimensions, currentCell.getCellWorldPosition(), TileType.CorridorTile);
                // When traversing up or down the tile is going to need right and left walls
                tile.placeWall(wallMaterial, wallHeight, Direction.Right);
                tile.placeWall(wallMaterial, wallHeight, Direction.Left);
                // Add tile to the list of corridor tiles
                corridorTiles.Add(tile);
                currentCell.setCellFloorTile(ref tile);
            }
            // Traverse down
            if (currentCell.getCellRowPositionInGrid() < endCell.getCellRowPositionInGrid())
            {
                // If the grid cell has a tile with walls remove the ones that are not needed
                if (currentCell.getCellFloorTile().getTileType() == TileType.RoomOuterTile || currentCell.getCellFloorTile().getTileType() == TileType.CorridorTile)
                {
                    // When traversing down always remove the down wall of current tile if there is one
                    currentCell.getCellFloorTile().removeWall(Direction.Down);
                }
                // Get next cell
                nextCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() + 1, currentCell.getCellColumnPositionInGrid()];
                // Remove upper wall of next cell if there is one
                if (nextCell.getCellFloorTile() != null)
                {
                    nextCell.getCellFloorTile().removeWall(Direction.Up);
                }
            }
            // Traverse up
            else if (currentCell.getCellRowPositionInGrid() > endCell.getCellRowPositionInGrid())
            {
                // If the grid cell has a tile with walls remove the ones that are not needed
                if (currentCell.getCellFloorTile().getTileType() == TileType.RoomOuterTile || currentCell.getCellFloorTile().getTileType() == TileType.CorridorTile)
                {
                    // When traversing up always remove the upper wall of current tile if there is one
                    currentCell.getCellFloorTile().removeWall(Direction.Up);
                }
                nextCell = ref dungeon.getDungeonGrid()[currentCell.getCellRowPositionInGrid() - 1, currentCell.getCellColumnPositionInGrid()];
                // Remove down wall of next cell if there is one
                if (nextCell.getCellFloorTile() != null)
                {
                    nextCell.getCellFloorTile().removeWall(Direction.Down);
                }
            }
            // Current cell column and end cell column are the same, as the up or down traversal is done after the right or left, at this point the start and end cells are connected and no corners are needed
            else
            {
                // Exit loop
                break;
            }
            // Set current cell to next cell
            currentCell = ref nextCell;
        }
        // Set corridor game object in the tile at the middle of the tiles list
        corridor.transform.position = corridorTiles[corridorTiles.Count/2].getCorrespondingDungeonCell().getCellWorldPosition();
        // Set the tiles as children of the corridor game object
        foreach(FloorTile tile in corridorTiles)
        {
            tile.setParent(corridor, true);
        }
    }
}
