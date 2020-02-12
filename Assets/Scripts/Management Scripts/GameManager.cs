using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Next scene to load")]
    [SerializeField] Object scene;                  // The next scene to load
    [SerializeField] List<Object> randomScenes;     // Scenes that will be loaded randomly
    [Header("Player object")]
    [SerializeField] GameObject player;             // Player
    [Header("Teleporter game objects")]
    [SerializeField] GameObject entranceTeleporter; // Teleporter from which the player starts
    [SerializeField] GameObject exitTeleporter;     // Teleporter from which the player proceeds to the next level
    [Header("UI Game Objects")]
    [SerializeField] GameObject canvas;             // UI canvas
    [SerializeField] Text sceneNameText;            // Name of the current scene text
    [SerializeField] Text timeText;                 // Time the player has needed to complete the dungeon text

    private static ScenesHandler scenesHandler;     // Script containing all the scenes to load

    private GameObject entrance;                    // Entrance room
    private GameObject exit;                        // Exit room

    private PlayerTeleport playerTeleport;          // Playerteleport script to catch the collision between the player and the exit teleporter
    private bool showingUI;                         // Whether the UI is being shown

    private float timer;                            // Timer valie

    // Start is called before the first frame update
    void Start()
    {
        // Disable cursor
        Cursor.visible = false;
        // Set timer to 0
        timer = 0.0f;
        // Set showing UI to false and disable canvas so the player can't see it
        showingUI = false;
        canvas.SetActive(false);
        // Get scene name from active scene
        sceneNameText.text = "SCENE NAME: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // Get entrance and exit rooms from the dungeon
        entrance = GameObject.Find("Entrance");
        exit = GameObject.Find("Exit");
        // Instantiate entrance and exit teleporters
        entranceTeleporter = Instantiate(entranceTeleporter, entrance.transform.position + new Vector3(0.0f, -1.5f, 0.0f), entranceTeleporter.transform.rotation);
        exitTeleporter = Instantiate(exitTeleporter, exit.transform.position + new Vector3(0.0f, -1.5f, 0.0f), exitTeleporter.transform.rotation);
        // Set player position and the exit teleport
        player.transform.position = entranceTeleporter.transform.position;
        playerTeleport = player.GetComponent<PlayerTeleport>();
        playerTeleport.setTeleport(exitTeleporter);
        // Get scenes handler
        scenesHandler = ScenesHandler.Instance();
        scenesHandler.setScenes(randomScenes);
    }

    // Update is called once per frame
    void Update()
    {
        // Increase time
        timer += Time.deltaTime;
        // Player collides with the exit teleporter
        if (playerTeleport.getHasReachedEnd() && !showingUI)
        {
            // Disable player
            player.SetActive(false);
            // Enable UI elements
            canvas.SetActive(true);
            showingUI = true;
            // Set time
            System.TimeSpan ts = System.TimeSpan.FromSeconds(timer);
            timeText.text = "DUNGEON END REACHED IN: " + string.Format("{0:00}:{1:00}", ts.TotalMinutes, ts.Seconds);
        }
        // When the player can see the UI and presses enter, the next scene will be loaded
        if(Input.GetKeyDown(KeyCode.Return) && showingUI)
        {
            // If there is no next scene to load, try getting one random scene from the scenes handles
            if(scene == null)
            {
                Object randomScene = scenesHandler.getRandomScene();
                if(randomScene == null)
                {
                    Application.Quit();
                }
                // Load level
                UnityEngine.SceneManagement.SceneManager.LoadScene(randomScene.name);
            }
            else
            {
                // Load level
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene.name);
            }
        }
        // If escape is pressed, exit the application
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
