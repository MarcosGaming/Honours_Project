using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGeneratorGraphGrammar : MonoBehaviour
{
    Graph graph;
    StartNode node;
    // Start is called before the first frame update
    void Start()
    {
        node = new StartNode();
        graph = new Graph(node);
        ProductionRules.StartMission(node, graph);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Graph
{ 
    private AlphabetNode rootNode;      // The root node is going to be the start or entrance node

    public Graph(StartNode startNode)
    {
        rootNode = startNode;
    }

    public void setRootNode(AlphabetNode node)
    {
        this.rootNode = node;
    }

    public AlphabetNode getRootNode()
    {
        return rootNode;
    }
}
