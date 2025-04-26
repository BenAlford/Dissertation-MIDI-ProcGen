using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour, ILevelObject
{
    bool collected = false;
    GameObject player;
    [SerializeField] CoinScriptable coinCounter;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // collects the coin when stepped on by the player
        if (!collected && player.transform.position == transform.position)
        {
            collected = true;
            GetComponent<SpriteRenderer>().enabled = false;
            coinCounter.coins++;
        }
    }

    public void Activate()
    {
        if (!collected)
            GetComponent<SpriteRenderer>().enabled = true;
    }

    public void Deactivate()
    {
        if (!collected)
            GetComponent<SpriteRenderer>().enabled = false;
    }

    public Vector3Int GetPos()
    {
        return new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }
}
