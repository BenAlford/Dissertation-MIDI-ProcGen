using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProjectileBehaviour : MonoBehaviour
{
    Tilemap map;
    [SerializeField] Tile black;

    [SerializeField] SpriteRenderer warning;

    public Vector3 direction;

    [SerializeField] float speed;
    float moveTime;
    float moveTimer = 0;

    [SerializeField] float spawnTime;
    float spawnTimer = 0;

    bool spawned = false;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.FindGameObjectWithTag("Tilemap").GetComponentInChildren<Tilemap>();
        if (map.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y)) == black)
        {
            Destroy(gameObject);
        }
        moveTime = 1 / speed;
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // has a brief timer where a warning sign is shown before spawning
        if (!spawned)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnTime)
            {
                warning.enabled = false;
                GetComponent<SpriteRenderer>().enabled = true;
                spawned = true;
            }
        }
        // Moves constantly in one direction until it hits a wall or player
        else
        {
            moveTimer += Time.deltaTime;
            if (moveTimer > moveTime)
            {
                moveTimer -= moveTime;
                transform.position += direction;
                if (map.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y)) == black)
                {
                    Destroy(gameObject);
                }
            }

            // damages the player if hit
            if (transform.position == player.transform.position)
            {
                player.GetComponent<PlayerBehaviour>().Damage();
                Destroy(gameObject);
            }
        }
    }
}
