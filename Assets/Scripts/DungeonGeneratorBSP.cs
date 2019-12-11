using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The dungeon generator is going to assume the following when placing rooms: right is positive x, left is negative x, forward is positive z and backward is negative z
public class DungeonGeneratorBSP : MonoBehaviour
{
    [SerializeField] Vector3 dungeonTopLeftCorner;  // Top left point of the dungeon
    [SerializeField] float dungeonWidthX;           // Width of dungeon - X
    [SerializeField] float dungeonHeightZ;          // Height of dungeon - Z
    private float minPartitionWidthX;               // Minimum width of a partition
    private float minPartitionHeightZ;              // Minimum height of a partition

    [Min(4)]
    [SerializeField] int roomMinTilesWidthX;        // Minimum room width - X
    [SerializeField] int roomMaxTilesWidthX;        // Maximum room width - X
    [Min(4)]
    [SerializeField] int roomMinTilesHeightZ;       // Minimum room height - Z
    [SerializeField] int roomMaxTilesHeightZ;       // Maximum room height - Z

    [SerializeField] Material floorMaterial;        // Material that will be used for the floor
    [SerializeField] Vector3 floorTileDimensions;   // Dimensions of each floor tile
    [SerializeField] Material wallMaterial;         // Material of the walls
    [SerializeField] float wallHeight;              // Height of each wall

    private float floorTileAndWallMarginX;          // Margin at width to keep at each partition
    private float floorTileAndWallMarginZ;          // Margin at height to keep at each partition

    private BSPTree dungeonTree;                    // Binary Space Partitioning tree that represents the dungeon

    public static int counter = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Additional margin to keep when performing the partitions
        floorTileAndWallMarginX = floorTileDimensions.x * 2.0f;
        floorTileAndWallMarginZ = floorTileDimensions.z * 2.0f;
        // Minimum partition width and height 
        minPartitionWidthX = (roomMinTilesWidthX * floorTileDimensions.x * 2.0f) + (floorTileAndWallMarginX * 2.0f);
        minPartitionHeightZ = roomMinTilesHeightZ * floorTileDimensions.z * 2.0f + (floorTileAndWallMarginZ * 2.0f);
        // Build the dungeon
        BuildDungeon();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void BuildDungeon()
    {
        // Create the root node of the tree
        dungeonTree = new BSPTree(new BSPNode(null, dungeonTopLeftCorner, dungeonWidthX, dungeonHeightZ));
        ref BSPNode root = ref dungeonTree.getRootNode();
        // Iteratively create partitions
        CreatePartition(ref root);
        // Once all partitions and rooms within those partitions are created, starting from the deepest nodes, connect rooms from nodes corresponding to children from the same parent


    }

    // Every partition is going to create two new nodes
    private void CreatePartition(ref BSPNode parent)
    {
        // Data of the new partitions
        Vector3 topLeftCornerNewPartition1 = parent.getPartitionTopLeftCorner();
        float widthXNewPartition1 = 0;
        float heightZNewPartition1 = 0;
        Vector3 topLeftCornerNewPartition2 = new Vector3();
        float widthXNewPartition2 = 0;
        float heightZNewPartition2 = 0;
        // Divide the area vertically
        if (Random.Range(0.0f, 1.0f) > 0.5f)
        {
            // Select a random position from the parent top left corner towards the x positive axis taking into account the room min width for the top left corner of the new partition 2
            float min = parent.getPartitionTopLeftCorner().x + (roomMinTilesWidthX * floorTileDimensions.x) + floorTileAndWallMarginX;
            float max = parent.getPartitionTopLeftCorner().x + parent.getPartitionWidthX() - (roomMinTilesWidthX * floorTileDimensions.x) - floorTileAndWallMarginX;
            float randomPoisitionX = Random.Range(min, max);
            topLeftCornerNewPartition2 = new Vector3(randomPoisitionX, topLeftCornerNewPartition1.y, topLeftCornerNewPartition1.z);
            // The width of the new partition 1 is going to be the distance from its top left corner to the top left corner of new partition 2
            widthXNewPartition1 = Vector3.Distance(topLeftCornerNewPartition1, topLeftCornerNewPartition2);
            // The width of the new partition 2 is going to be the distance between its top left corner and the top right corner of the parent partition
            Vector3 parentTopRightCorner = parent.getPartitionTopLeftCorner() + new Vector3(parent.getPartitionWidthX(), 0.0f, 0.0f);
            widthXNewPartition2 = Vector3.Distance(topLeftCornerNewPartition2, parentTopRightCorner);
            // When dividing vertically, the height of the new partitions 1 and 2 is the same as the parent height
            heightZNewPartition1 = parent.getPartitionHeightZ();
            heightZNewPartition2 = parent.getPartitionHeightZ();
        }
        // Divide the area horizontally
        else
        {
            // Select a random position form the top left corner towards the z negative axis taking into account the room min height for the top left corner of the new partition 2
            float min = parent.getPartitionTopLeftCorner().z - (roomMinTilesHeightZ * floorTileDimensions.z) - floorTileAndWallMarginZ;
            float max = parent.getPartitionTopLeftCorner().z - parent.getPartitionHeightZ() + (roomMinTilesHeightZ * floorTileDimensions.z) + floorTileAndWallMarginZ;
            float randomPoisitionZ = Random.Range(min, max);
            topLeftCornerNewPartition2 = new Vector3(topLeftCornerNewPartition1.x, topLeftCornerNewPartition1.y, randomPoisitionZ);
            // When dividing horizontally, the width of the new partitions 1 and 2 is going to be the same as the parent width
            widthXNewPartition1 = parent.getPartitionWidthX();
            widthXNewPartition2 = parent.getPartitionWidthX();
            // The height of the new partition 1 is going to be the distance between its top left corner and the top left corner of new partition 2
            heightZNewPartition1 = Vector3.Distance(topLeftCornerNewPartition1, topLeftCornerNewPartition2);
            // The height of the new partition 2 is going to be distance between its top left corner and the down left corner of the parent partition
            Vector3 parentDownLeftCorner = parent.getPartitionTopLeftCorner() - new Vector3(0.0f, 0.0f, parent.getPartitionHeightZ());
            heightZNewPartition2 = Vector3.Distance(topLeftCornerNewPartition2, parentDownLeftCorner);
        }
        // Create new nodes with the partitions data
        parent.setLeftChildNode(new BSPNode(parent, topLeftCornerNewPartition1, widthXNewPartition1, heightZNewPartition1));
        parent.setRightChildNode(new BSPNode(parent, topLeftCornerNewPartition2, widthXNewPartition2, heightZNewPartition2));
        // Check if the left child can be further subdivided
        ref BSPNode leftChild = ref parent.getLeftChildNode();
        if (leftChild.getPartitionWidthX() > minPartitionWidthX && leftChild.getPartitionHeightZ() > minPartitionHeightZ)
         {
             CreatePartition(ref leftChild);
         }
         // If the left child cannot be further subidivided create a room within its boundaries
         else
         {
            GameObject obj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj1.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
            obj1.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            obj1.transform.localScale = new Vector3(leftChild.getPartitionWidthX(), 0.0f, leftChild.getPartitionHeightZ());
            obj1.transform.position = leftChild.getPartitionTopLeftCorner() + new Vector3(leftChild.getPartitionWidthX() * 0.5f, 0.0f, -leftChild.getPartitionHeightZ() * 0.5f);
            leftChild.createPartitionRoom(floorTileAndWallMarginX, floorTileAndWallMarginZ, roomMinTilesWidthX, roomMaxTilesWidthX, roomMinTilesHeightZ, roomMaxTilesHeightZ, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        }
        // Check if the right child can be further subdivided
        ref BSPNode rightChild = ref parent.getRightChildNode();
        if (rightChild.getPartitionWidthX() > minPartitionWidthX && rightChild.getPartitionHeightZ() > minPartitionHeightZ)
        {
            CreatePartition(ref rightChild);
        }
        // If the right child cannot be further subidivided create a room within its boundaries
        else
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<Renderer>().material.shader = Shader.Find("Unlit/Color");
            obj.GetComponent<Renderer>().material.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            obj.transform.localScale = new Vector3(rightChild.getPartitionWidthX(), 0.0f, rightChild.getPartitionHeightZ());
            obj.transform.position = rightChild.getPartitionTopLeftCorner() + new Vector3(rightChild.getPartitionWidthX() * 0.5f, 0.0f, -rightChild.getPartitionHeightZ() * 0.5f);
            rightChild.createPartitionRoom(floorTileAndWallMarginX, floorTileAndWallMarginZ, roomMinTilesWidthX, roomMaxTilesWidthX, roomMinTilesHeightZ, roomMaxTilesHeightZ, floorTileDimensions, floorMaterial, wallHeight, wallMaterial);
        }
    }

    // Node of the BSP tree
    private class BSPNode
    {
        // Partition data
        private Vector3 topLeftCorner;
        private float widthX;
        private float heightZ;
        // A node might have a room
        private Room room;
        // Node connections
        private BSPNode parent;
        private BSPNode rightChild;
        private BSPNode leftChild;

        public BSPNode(BSPNode parent, Vector3 topLeftCorner, float widthX, float heightZ)
        {
            this.parent = parent;
            this.topLeftCorner = topLeftCorner;
            this.widthX = widthX;
            this.heightZ = heightZ;
        }

        public BSPNode getParent()
        {
            return parent;
        }

        public Vector3 getPartitionTopLeftCorner()
        {
            return topLeftCorner;
        }

        public float getPartitionWidthX()
        {
            return widthX;
        }

        public float getPartitionHeightZ()
        {
            return heightZ;
        }

        public Room getPartitionRoom()
        {
            return room;
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

        public void createPartitionRoom(float widthMarginX, float heightMarginZ, int minTilesX, int maxTilesX, int minTilesZ, int maxTilesZ, Vector3 floorTileDimensions, Material floorMaterial, float wallHeight, Material wallMaterial)
        {
            // Get min and max values for x and z taking into account the min room width, min room height, size of floor tiles and size of walls
            float minX = this.getPartitionTopLeftCorner().x + widthMarginX;
            float maxX = this.getPartitionTopLeftCorner().x + this.getPartitionWidthX() - (minTilesX * floorTileDimensions.x) - widthMarginX;
            float maxZ = this.getPartitionTopLeftCorner().z - heightMarginZ;
            float minZ = this.getPartitionTopLeftCorner().z - this.getPartitionHeightZ() + (minTilesZ * floorTileDimensions.z) + heightMarginZ;
            // Max x being smaller than min x means that the width is quite small and the value that needs to be used is the min one as using other can cause the room to be placed outside the partition
            if(minX > maxX)
            {
                maxX = minX;
            }
            // Min Z being greater (more positive) than max z means that the height is quite small and the value that needs to be used is the max one as using other can cause the room to be placed outside the partition
            if(minZ > maxZ)
            {
                minZ = maxZ;
            }
            // Select top left corner
            Vector3 randomTopLeftCorner = new Vector3(Random.Range(minX, maxX), this.getPartitionTopLeftCorner().y, Random.Range(minZ, maxZ));
            // Select random width
            int distanceToEndOfPartitionX = Mathf.RoundToInt(Vector3.Distance(randomTopLeftCorner, new Vector3(this.getPartitionTopLeftCorner().x + this.getPartitionWidthX() - widthMarginX, randomTopLeftCorner.y, randomTopLeftCorner.z)));
            int widhtInTiles = Mathf.Abs(Mathf.RoundToInt(Random.Range(minTilesX, (distanceToEndOfPartitionX / floorTileDimensions.x))));
            widhtInTiles = Mathf.Min(widhtInTiles, maxTilesX);
            // Select random height
            int distanceToEndOfPartitionZ = Mathf.RoundToInt(Vector3.Distance(randomTopLeftCorner, new Vector3(randomTopLeftCorner.x, randomTopLeftCorner.y, this.getPartitionTopLeftCorner().z - this.getPartitionHeightZ() + heightMarginZ)));
            int heightInTiles = Mathf.Abs(Mathf.RoundToInt(Random.Range(minTilesZ, (distanceToEndOfPartitionZ / floorTileDimensions.z))));
            heightInTiles = Mathf.Min(heightInTiles, maxTilesZ);
            // Create room
            this.setPartitionRoom(new Room(randomTopLeftCorner, widhtInTiles, heightInTiles, floorTileDimensions, floorMaterial, wallHeight, wallMaterial));
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
