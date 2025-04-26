using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Exit : MonoBehaviour, ILevelObject
{
    GameObject player;
    PlayMIDIFile playmidi;

    private void Start()
    {
        playmidi = GameObject.FindGameObjectWithTag("MIDIPlayer").GetComponent<PlayMIDIFile>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // Loads the win screen when stepped on
        if (player.transform.position == transform.position)
        {
            print("You win!");
            playmidi.Stop();
            SceneManager.LoadScene(2);
        }
    }

    public void Activate()
    {
        GetComponent<SpriteRenderer>().enabled = true;
    }

    public void Deactivate()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    public Vector3Int GetPos()
    {
        return new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }
}
