using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
    private GameObject teleport;    // Teleport the player needs to collide with
    private bool hasReachedEnd;     // Whether the player has collided with the teleport/reached the end of the dungeon

    // Start is called before the first frame update
    void Start()
    {
        hasReachedEnd = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject == teleport)
        {
            hasReachedEnd = true;
        }
    }

    public void setTeleport(GameObject teleport)
    {
        this.teleport = teleport; 
    }

    public bool getHasReachedEnd()
    {
        return hasReachedEnd;
    }
}
