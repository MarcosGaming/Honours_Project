using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesHandler
{
    private List<Object> scenes;            // Scenes to load

    private static ScenesHandler instance;  // Singleton instance

    private ScenesHandler() { }

    public static ScenesHandler Instance()
    {
        if(instance == null)
        {
            instance = new ScenesHandler();
        }
        return instance;
    }

    public void setScenes(List<Object>scenes)
    {
        if(this.scenes == null)
        {
            this.scenes = new List<Object>();
            this.scenes = scenes.GetRange(0, scenes.Count);
        }
    }
    
    public Object getRandomScene()
    {
        Object randomScene = null;
        if(scenes.Count > 0)
        {
            int random = Random.Range(0, scenes.Count);
            randomScene = scenes[random];
            scenes.Remove(randomScene);
        }
        return randomScene;
    }
}
