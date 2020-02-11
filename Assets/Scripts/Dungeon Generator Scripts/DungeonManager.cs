using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    [Header("Keys to control the dungeon generation")]
    [SerializeField] KeyCode keyToGenerateDungeonsUsingBSP = KeyCode.Alpha1;
    [SerializeField] KeyCode keyToGenerateDungeonsUsingDiggerAgent = KeyCode.Alpha2;
    [SerializeField] KeyCode keyToGenerateDungeonsUsingGraphGrammars = KeyCode.Alpha3;
    [SerializeField] KeyCode keyToSaveTheGeneratedDungeonAsPrefab = KeyCode.Alpha4;

    private BSP dungeonGeneratorBSP;
    private DiggerAgent dungeonGeneratorDiggerAgent;
    private GraphGrammars dungeonGeneratorGraphGrammar;

    private DungeonGenerator[] generators;

    // Start is called before the first frame update
    void Start()
    {
        // Get dungeon generator components
        dungeonGeneratorBSP = GetComponent<BSP>();
        dungeonGeneratorDiggerAgent = GetComponent<DiggerAgent>();
        dungeonGeneratorGraphGrammar = GetComponent<GraphGrammars>();
        // If any of the components is null, create a new one
        if (dungeonGeneratorBSP == null)
        {
            dungeonGeneratorBSP = new BSP();
        }
        if (dungeonGeneratorDiggerAgent == null)
        {
            dungeonGeneratorDiggerAgent = new DiggerAgent();
        }
        if (dungeonGeneratorGraphGrammar == null)
        {
            dungeonGeneratorGraphGrammar = new GraphGrammars();
        }
        // Initialize array
        generators = new DungeonGenerator[3];
        // Add generator to the array
        generators[0] = dungeonGeneratorBSP;
        generators[1] = dungeonGeneratorDiggerAgent;
        generators[2] = dungeonGeneratorGraphGrammar;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(keyToGenerateDungeonsUsingBSP))
        {
            DestroyCurrentDungeon();
            dungeonGeneratorBSP.BuildDungeon();
        }
        else if (Input.GetKeyDown(keyToGenerateDungeonsUsingDiggerAgent))
        {
            DestroyCurrentDungeon();
            dungeonGeneratorDiggerAgent.BuildDungeon();
        }
        else if (Input.GetKeyDown(keyToGenerateDungeonsUsingGraphGrammars))
        {
            DestroyCurrentDungeon();
            dungeonGeneratorGraphGrammar.BuildDungeon();
        }
        else if (Input.GetKeyDown(keyToSaveTheGeneratedDungeonAsPrefab))
        {
            SaveCurrentDungeonAsPrefab();
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    // Method to remove any dungeon generated
    private void DestroyCurrentDungeon()
    {
        foreach(DungeonGenerator generator in generators)
        {
            if(generator.isDungeonBuilt())
            {
                generator.DestroyDungeon();
            }
        }
    }

    // Mehtod to save the current dungeon
    private void SaveCurrentDungeonAsPrefab()
    {
        foreach (DungeonGenerator generator in generators)
        {
            if (generator.isDungeonBuilt())
            {
                generator.SaveDungeonAsPrefab();
            }
        }
    }
}
