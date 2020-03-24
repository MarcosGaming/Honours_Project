using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // When the player can see the UI and presses enter, the next scene will be loaded
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Load last scene in the build
            UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
        }
        // If escape is pressed, exit the application
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
