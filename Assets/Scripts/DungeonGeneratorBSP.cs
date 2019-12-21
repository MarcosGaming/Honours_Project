using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The dungeon generator is going to assume the following when placing rooms: right is positive x, left is negative x, up is positive z and down is negative z
public class DungeonGeneratorBSP : MonoBehaviour
{
    [SerializeField] Vector3 dungeonTopLeftCellPosition;    // Top left position of dungeon cell [0,0]
    [SerializeField] int dungeonWidth;                      // Number of columns the dungeon grid is going to have
    [SerializeField] int dungeonHeight;                     // Number of rows the dungeon grid is going to have

    [Min(4)]
    [SerializeField] int roomMinTilesWidth;                 // Minimum number of column cells a room needs to have
    [SerializeField] int roomMaxTilesWidth;                 // Maximum number of column cells a room can have
    [Min(4)]
    [SerializeField] int roomMinTilesHeight;                // Minimum number of row cells a room needs to have
    [SerializeField] int roomMaxTilesHeight;                // Maximum number of row cells a room can have

    [Min(2)]
    [SerializeField] int roomAndPartitionBorderMargin;      // Margin between the rooms and the edge of a partition

    [SerializeField] Material floorMaterial;                // Material that will be used for the floor
    [SerializeField] Vector3 floorTileDimensions;           // Dimensions of each floor tile
    [SerializeField] Material wallMaterial;                 // Material of the walls
    [SerializeField] float wallHeight;                      // Height of each wall

    private int minPartitionWidth;                          // Minimum number of column cells a partition needs to have
    private int minPartitionHeight;                         // Minimum number of row cells a partition needs to have

    private Dungeon dungeon;                                // Dungeon class which basically consits in a 2D array of cells

    private BSPTree dungeonTree;                            // Binary Space Partitioning tree that represents the dungeon

    private List<Corridor> corridors;                       // List of the corridors

    // Start is called before the first frame update
    void Start()
    {
        // Initialise corridors list
        corridors = new List<Corridor>();
        // Modify dungeon width and height if less than the min room tiles width and height
        dungeonWidth = Mathf.Max(dungeonWidth, roomMinTilesWidth);
        dungeonHeight = Mathf.Max(dungeonHeight, roomMinTilesHeight);
        // Minimum partition width and height 
        minPartitionWidth = roomMinTilesWidth + roomAndPartitionBorderMargin;
        minPartitionHeight = roomMinTilesHeight + roomAndPartitionBorderMargin;
        // Build the dungeon
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildDungeon()
    {
        // Create dungeon grid
        dungeon = new Dungeon(dungeonWidth, dungeonHeight);
        ref DungeonCell[,] dungeonGrid = ref dungeon.getDungeonGrid();
        for (int i = 0; i < dungeonHeight; i++)
        {
            for (int j = 0; j < dungeonWidth; j++)
            {
                Vector3 cellPosition = dungeonTopLeftCellPosition + new Vector3(floorTileDimensions.x * j, floorTileDimensions.y, -floorTileDimensions.z * i);
                dungeonGrid[i, j] = new DungeonCell(cellPosition, i, j);
            }
        }
        // Create the root node of the tree
        dungeonTree = new BSPTree(new BSPNode(null, ref dungeonGrid[0,0], dungeonWidth, dungeonHeight));
        ref BSPNode root = ref dungeonTree.getRootNode();
        // Iteratively create partitions
        CreatePartition(ref root);
        // Once all partitions and rooms within those partitions are created, starting from the deepest nodes, connect rooms from nodes corresponding to children from the same parent
        ConnectRooms(ref dungeonTree.getRootNode().getLeftChildNode(), ref dungeonTree.getRootNode().getRightChildNode());
    }

    // Every partition is going to create two new nodes
    private void CreatePartition(ref BSPNode parent)
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
        parent.setLeftChildNode(new BSPNode(parent, ref dungeon.getDungeonGrid()[topLeftCellNewPartition1Row, topLeftCellNewPartition1Column], widthNewPartition1, heightNewPartition1));
        parent.setRightChildNode(new BSPNode(parent, ref dungeon.getDungeonGrid()[topLeftCellNewPartition2Row, topLeftCellNewPartition2Column], widthNewPartition2, heightNewPartition2));

        // Check if the left child can be further subdivided
        ref BSPNode leftChild = ref parent.getLeftChildNode();
        // Partition can only be subdivided if there is enough space for two sub partitions
        if (leftChild.getPartitionWidth() > (minPartitionWidth * 2.0f) && leftChild.getPartitionHeight() > (minPartitionHeight * 2.0f))
         {
             CreatePartition(ref leftChild);
         }
         // If the left child cannot be further subidivided create a room within its boundaries
         else
         {
            leftChild.createPartitionRoom(ref dungeon, roomMinTilesWidth, roomMinTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        }
        // Check if the right child can be further subdivided
        ref BSPNode rightChild = ref parent.getRightChildNode();
        // Partition can only be subdivided if there is enough space for two sub partitions
        if (rightChild.getPartitionWidth() > (minPartitionWidth * 2.0f) && rightChild.getPartitionHeight() > (minPartitionHeight * 2.0f))
        {
            CreatePartition(ref rightChild);
        }
        // If the right child cannot be further subidivided create a room within its boundaries
        else
        {
            rightChild.createPartitionRoom(ref dungeon, roomMinTilesWidth, roomMinTilesHeight, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        }
    }

    // Recursive function that will crate corridors between nodes that have a room
    private void ConnectRooms(ref BSPNode left, ref BSPNode right)
    {
        // Check if left node has children that need to be connected first
        if(left.hasChildren())
        {
            ConnectRooms(ref left.getLeftChildNode(), ref left.getRightChildNode());
        }
        // Check if right node has children that need to be connected first
        if(right.hasChildren())
        {
            ConnectRooms(ref right.getLeftChildNode(), ref right.getRightChildNode());
        }
        // Check which nodes have rooms
        if(left.getPartitionRoom() != null && right.getPartitionRoom() != null)
        {
            corridors.Add(new Corridor(ref dungeon, ref left.getPartitionRoom(), ref right.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else if(left.getPartitionRoom() != null && right.getPartitionRoom() == null)
        {
            // Find a room in one of the right node children
            ref BSPNode nodeWithRoom = ref findChildWithRoom(ref right);
            corridors.Add(new Corridor(ref dungeon, ref left.getPartitionRoom(), ref nodeWithRoom.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else if(left.getPartitionRoom() == null && right.getPartitionRoom() != null)
        {
            // Find a room in one of the left node children
            ref BSPNode nodeWithRoom = ref findChildWithRoom(ref left);
            corridors.Add(new Corridor(ref dungeon, ref nodeWithRoom.getPartitionRoom(), ref right.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
        else
        {
            // Find a room in one of the left and right children
            ref BSPNode nodeWithRoomLeft = ref findChildWithRoom(ref left);
            ref BSPNode nodeWithRoomRight = ref findChildWithRoom(ref right);
            corridors.Add(new Corridor(ref dungeon, ref nodeWithRoomLeft.getPartitionRoom(), ref nodeWithRoomRight.getPartitionRoom(), floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
    }

    // Recursive function to find a child node with a room
    private ref BSPNode findChildWithRoom(ref BSPNode node)
    {
        if(node.hasChildren())
        {
            // Randomly select child
            if(Random.Range(0.0f, 1.0f) > 0.5f)
            {
                return ref findChildWithRoom(ref node.getLeftChildNode());
            }
            else
            {
                return ref findChildWithRoom(ref node.getRightChildNode());
            }
        }
        return ref node;
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

        public BSPNode(BSPNode parent, ref DungeonCell topLeftCell, int width, int height)
        {
            this.parent = parent;
            this.topLeftCell = topLeftCell;
            this.width = width;
            this.height = height;
        }

        public ref BSPNode getParent()
        {
            return ref parent;
        }

        public ref DungeonCell getPartitionTopLeftCell()
        {
            return ref topLeftCell;
        }

        public int getPartitionWidth()
        {
            return width;
        }

        public int getPartitionHeight()
        {
            return height;
        }

        public ref Room getPartitionRoom()
        {
            return ref room;
        }

        private void setPartitionRoom(Room room)
        {
            this.room = room;
        }

        public ref BSPNode getRightChildNode()
        {
            return ref rightChild;
        }

        public void setRightChildNode(BSPNode node)
        {
            this.rightChild = node;
        }

        public ref BSPNode getLeftChildNode()
        {
            return ref leftChild;
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

        public void createPartitionRoom(ref Dungeon dungeon, int roomMinTilesWidth, int roomMinTilesHeight, Vector3 floorTileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
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
            int randomWidth = Random.Range(roomMinTilesWidth, maxWidth + 1);
            // Choose random height
            int maxHeight = this.topLeftCell.getCellRowPositionInGrid() + this.height - 1 - randomRow;
            int randomHeight = Random.Range(roomMinTilesHeight, maxHeight + 1);
            // Create room
            this.setPartitionRoom(new Room(ref dungeon, randomRow, randomColumn, randomHeight, randomWidth, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
        }
    }

    // A BSP tree is basically going to have a root node from which the tree is going to be traversed
    private class BSPTree
    {
        private BSPNode rootNode;

        public BSPTree(BSPNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public ref BSPNode getRootNode()
        {
            return ref rootNode;
        }
    }
}
