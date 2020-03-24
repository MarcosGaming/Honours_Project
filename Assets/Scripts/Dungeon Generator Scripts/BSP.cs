using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BSP : DungeonGenerator
{
    [Header("Dungeon width and height in tiles")]
    [SerializeField] int dungeonWidth;                      // Number of columns the dungeon grid is going to have
    [SerializeField] int dungeonHeight;                     // Number of rows the dungeon grid is going to have
    [Header("Margin between rooms and edge of partition")]
    [SerializeField] int roomAndPartitionBorderMargin;      // Margin between the rooms and the edge of a partition

    private int minPartitionWidth;                          // Minimum number of column cells a partition needs to have
    private int minPartitionHeight;                         // Minimum number of row cells a partition needs to have

    private BSPTree dungeonTree;                            // Binary Space Partitioning tree that represents the dungeon

    public override void BuildDungeon()
    {
        AssertProperties();
        // Create dungeon grid
        dungeon = new Dungeon(dungeonWidth, dungeonHeight);
        dungeon.createDungeonGrid(dungeonTopLeftCellPosition, floorTileDimensions);
        // Create the root node of the tree
        dungeonTree = new BSPTree(new BSPNode(null, dungeon.getDungeonGrid()[0, 0], dungeonWidth, dungeonHeight));
        BSPNode root = dungeonTree.getRootNode();
        // Iteratively create partitions
        CreatePartition(root);
        // Once all partitions and rooms within those partitions are created, starting from the deepest nodes, connect rooms from nodes corresponding to children from the same parent
        ConnectRooms(dungeonTree.getRootNode().getLeftChildNode(), dungeonTree.getRootNode().getRightChildNode());
        // Set the entrance and exit rooms randomly
        dungeon.randomlyChooseEntranceRoomAndExitRoom();
        // Set the corridors and rooms to be children of the dungeon game object
        dungeon.setRoomsAndCorridorsAsDungeonChildren();
        // Set that the dungeon has finished building
        dungeonBuildingFinished = true;
    }

    protected override void AssertProperties()
    {
        // Make sure that the wall height is at least one
        wallHeight = Mathf.Max(1.0f, wallHeight);
        // Make sure that the tile dimensions are at least 1,0.5,1
        floorTileDimensions.x = Mathf.Max(1.0f, floorTileDimensions.x);
        floorTileDimensions.y = Mathf.Max(0.5f, floorTileDimensions.y);
        floorTileDimensions.z = Mathf.Max(1.0f, floorTileDimensions.z);
        // Make sure that the minimum width and height of a room is at least four tiles
        roomMinTilesWidth = Mathf.Max(roomMinTilesWidth, 4);
        roomMinTilesHeight = Mathf.Max(roomMinTilesHeight, 4);
        // Make sure that the margin between a room and the partition border is at least two
        roomAndPartitionBorderMargin = Mathf.Max(roomAndPartitionBorderMargin, 2);
        // Make sure that the dungeon width and height selected by the user is at least twice as big the minimum room width and height with margins
        dungeonWidth = Mathf.Max(dungeonWidth, roomMinTilesWidth * 2 + roomAndPartitionBorderMargin * 2);
        dungeonHeight = Mathf.Max(dungeonHeight, roomMinTilesHeight * 2 + roomAndPartitionBorderMargin * 2);
        // Make sure that the maximum width and height of a room is greater or equal to the minimum height and width of a room
        roomMaxTilesWidth = Mathf.Max(roomMinTilesWidth, roomMaxTilesWidth);
        roomMaxTilesHeight = Mathf.Max(roomMinTilesHeight, roomMaxTilesHeight);
        // Minimum partition width and height 
        minPartitionWidth = roomMinTilesWidth + roomAndPartitionBorderMargin;
        minPartitionHeight = roomMinTilesHeight + roomAndPartitionBorderMargin;
    }

    // Every partition is going to create two new nodes
    private void CreatePartition(BSPNode parent)
    {
        // Get row and column of the top left cell of the partition
        int firstRow = parent.getPartitionTopLeftCell().getCellRowPositionInGrid();
        int firstColumn = parent.getPartitionTopLeftCell().getCellColumnPositionInGrid();
        // Data of the new partitions
        int topLeftCellNewPartition1Row = firstRow;
        int topLeftCellNewPartition1Column = firstColumn;
        int widthNewPartition1 = 0;
        int heightNewPartition1 = 0;
        int topLeftCellNewPartition2Row = 0;
        int topLeftCellNewPartition2Column = 0;
        int widthNewPartition2 = 0;
        int heightNewPartition2 = 0;
        // Divide the area vertically
        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            // Select a random cell from the parent top left corner first row taking into account the min partition dimensions
            int min = firstColumn + minPartitionWidth;
            int max = firstColumn + parent.getPartitionWidth() - 1 - minPartitionWidth + 1;
            int randomCellColumn = Random.Range(min, max + 1);
            topLeftCellNewPartition2Row = firstRow;
            topLeftCellNewPartition2Column = randomCellColumn;
            // Width of new partition 1 is going to be from its top left cell to the top left cell of new partition 2
            widthNewPartition1 = randomCellColumn - parent.getPartitionTopLeftCell().getCellColumnPositionInGrid();
            // Width of new partition 2 is going to be from its top left cell to the top right cell of the parent partition
            widthNewPartition2 = (firstColumn + parent.getPartitionWidth() - 1) - randomCellColumn + 1;
            // When dividing vertically, the height of the new partitions 1 and 2 is the same as the parent height
            heightNewPartition1 = parent.getPartitionHeight();
            heightNewPartition2 = parent.getPartitionHeight();
        }
        // Divide the area horizontally
        else
        {
            // Select a random cell from the parent top left first column taking into account the min partition dimensions
            int min = firstRow + minPartitionHeight;
            int max = firstRow + parent.getPartitionHeight() - 1 - minPartitionHeight + 1;
            int randomCellRow = Random.Range(min, max + 1);
            topLeftCellNewPartition2Row = randomCellRow;
            topLeftCellNewPartition2Column = firstColumn;
            // When dividing horizontally, the width of the new partitions 1 and 2 is going to be the same as the parent width
            widthNewPartition1 = parent.getPartitionWidth();
            widthNewPartition2 = parent.getPartitionWidth();
            // The height of new partition 1 is going to be from its top left cell to the top left cell of new partition 2
            heightNewPartition1 = randomCellRow - parent.getPartitionTopLeftCell().getCellRowPositionInGrid();
            // The height of new partition 2 is going to be from its top left cell to the bottom left cell of the parent partitiom
            heightNewPartition2 = (firstRow + parent.getPartitionHeight() - 1) - randomCellRow + 1;
        }
        // Create new nodes with the partitions data
        parent.setLeftChildNode(new BSPNode(parent, dungeon.getDungeonGrid()[topLeftCellNewPartition1Row, topLeftCellNewPartition1Column], widthNewPartition1, heightNewPartition1));
        parent.setRightChildNode(new BSPNode(parent, dungeon.getDungeonGrid()[topLeftCellNewPartition2Row, topLeftCellNewPartition2Column], widthNewPartition2, heightNewPartition2));

        // Check if the left child can be further subdivided
        BSPNode leftChild = parent.getLeftChildNode();
        // Partition can only be subdivided if there is enough space for two sub partitions
        if (leftChild.getPartitionWidth() > (minPartitionWidth * 2) && leftChild.getPartitionHeight() > (minPartitionHeight * 2))
         {
             CreatePartition(leftChild);
         }
         // If the left child cannot be further subidivided create a room within its boundaries
         else
         {
            leftChild.createPartitionRoom(dungeon, roomMinTilesWidth, roomMaxTilesWidth, roomMinTilesHeight, roomMaxTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
         }
        // Check if the right child can be further subdivided
        BSPNode rightChild = parent.getRightChildNode();
        // Partition can only be subdivided if there is enough space for two sub partitions
        if (rightChild.getPartitionWidth() > (minPartitionWidth * 2) && rightChild.getPartitionHeight() > (minPartitionHeight * 2))
        {
            CreatePartition(rightChild);
        }
        // If the right child cannot be further subidivided create a room within its boundaries
        else
        {
            rightChild.createPartitionRoom(dungeon, roomMinTilesWidth, roomMaxTilesWidth, roomMinTilesHeight, roomMaxTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        }
    }

    // Recursive function that will crate corridors between nodes that have a room
    private void ConnectRooms(BSPNode left, BSPNode right)
    {
        // Check if left node has children that need to be connected first
        if(left.hasChildren())
        {
            ConnectRooms(left.getLeftChildNode(), left.getRightChildNode());
        }
        // Check if right node has children that need to be connected first
        if(right.hasChildren())
        {
            ConnectRooms(right.getLeftChildNode(), right.getRightChildNode());
        }
        // Check which nodes have rooms
        if(left.getPartitionRoom() != null && right.getPartitionRoom() != null)
        {
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, left.getPartitionRoom(), right.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else if(left.getPartitionRoom() != null && right.getPartitionRoom() == null)
        {
            // Find a room in one of the right node children
            BSPNode nodeWithRoom = findChildWithRoom(right);
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, left.getPartitionRoom(), nodeWithRoom.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else if(left.getPartitionRoom() == null && right.getPartitionRoom() != null)
        {
            // Find a room in one of the left node children
            BSPNode nodeWithRoom = findChildWithRoom(left);
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, nodeWithRoom.getPartitionRoom(), right.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else
        {
            // Find a room in one of the left and right children
            BSPNode nodeWithRoomLeft = findChildWithRoom(left);
            BSPNode nodeWithRoomRight = findChildWithRoom(right);
            dungeon.getDungeonCorridors().Add(new Corridor(dungeon, nodeWithRoomLeft.getPartitionRoom(), nodeWithRoomRight.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
    }

    // Recursive function to find a child node with a room
    private BSPNode findChildWithRoom(BSPNode node)
    {
        if(node.hasChildren())
        {
            // Randomly select child
            if(Random.Range(0.0f, 1.0f) > 0.5f)
            {
                return findChildWithRoom(node.getLeftChildNode());
            }
            else
            {
                return findChildWithRoom(node.getRightChildNode());
            }
        }
        return node;
    }

    // A BSP tree is basically going to have a root node from which the tree is going to be traversed
    private class BSPTree
    {
        private BSPNode rootNode;

        public BSPTree(BSPNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public BSPNode getRootNode()
        {
            return rootNode;
        }
    }

    // Node of the BSP tree
    private class BSPNode
    {
        // Partition data
        private DungeonCell topLeftCell;
        private int width;
        private int height;
        // A node might have a room
        private Room room;
        // Node connections
        private BSPNode parent;
        private BSPNode rightChild;
        private BSPNode leftChild;

        public BSPNode(BSPNode parent, DungeonCell topLeftCell, int width, int height)
        {
            this.parent = parent;
            this.topLeftCell = topLeftCell;
            this.width = width;
            this.height = height;
        }

        public BSPNode getParent()
        {
            return parent;
        }

        public DungeonCell getPartitionTopLeftCell()
        {
            return topLeftCell;
        }

        public int getPartitionWidth()
        {
            return width;
        }

        public int getPartitionHeight()
        {
            return height;
        }

        public Room getPartitionRoom()
        {
            return room;
        }

        private void setPartitionRoom(Room room)
        {
            this.room = room;
        }

        public BSPNode getRightChildNode()
        {
            return rightChild;
        }

        public void setRightChildNode(BSPNode node)
        {
            this.rightChild = node;
        }

        public BSPNode getLeftChildNode()
        {
            return leftChild;
        }

        public void setLeftChildNode(BSPNode node)
        {
            this.leftChild = node;
        }

        public bool hasChildren()
        {
            if(this.rightChild == null && this.leftChild == null)
            {
                return false;
            }
            return true;
        }

        public void createPartitionRoom(Dungeon dungeon, int roomMinTilesWidth, int roomMaxTilesWidth, int roomMinTilesHeight, int roomMaxTilesHeight, Vector3 floorTileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
        {
            // Get min and max values for the top left cell row and column for the room, taking into account the min room dimensions and that one row and one column are used as margins
            int minRow = this.topLeftCell.getCellRowPositionInGrid() + 1;
            int maxRow = this.topLeftCell.getCellRowPositionInGrid() + this.height - roomMinTilesHeight - 1;
            int minColumn = this.topLeftCell.getCellColumnPositionInGrid() + 1;
            int maxColumn = this.topLeftCell.getCellColumnPositionInGrid() + this.width - roomMinTilesWidth - 1;
            // Choose random row and column
            int randomRow = Random.Range(minRow, maxRow + 1);
            int randomColumn = Random.Range(minColumn, maxColumn + 1);
            // Choose random width
            int maxWidth = this.topLeftCell.getCellColumnPositionInGrid() + this.width - 1 - randomColumn;
            maxWidth = Mathf.Min(maxWidth, roomMaxTilesWidth);
            int randomWidth = Random.Range(roomMinTilesWidth, maxWidth + 1);
            // Choose random height
            int maxHeight = this.topLeftCell.getCellRowPositionInGrid() + this.height - 1 - randomRow;
            maxHeight = Mathf.Min(maxHeight, roomMaxTilesHeight);
            int randomHeight = Random.Range(roomMinTilesHeight, maxHeight + 1);
            // Create room
            this.setPartitionRoom(new Room(dungeon, randomRow, randomColumn, randomHeight, randomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
            dungeon.getDungeonRooms().Add(this.getPartitionRoom());
        }
    }
}
