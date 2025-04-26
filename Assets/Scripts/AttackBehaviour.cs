using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AttackBehaviour : MonoBehaviour
{
    Tilemap map;
    [SerializeField] Tile black;
    [SerializeField] SpriteRenderer spike;

    bool attacking = true;
    public float attackWindUpTime;
    float attackWindUpTimer = 0;

    public float attackLingerTime;
    float attackLingerTimer = 0;

    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.FindGameObjectWithTag("Tilemap").GetComponentInChildren<Tilemap>();
        if (map.GetTile(new Vector3Int((int)transform.position.x, (int)transform.position.y)) == black)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // attacks with spikes after a set delay
        if (attacking)
        {
            attackWindUpTimer += Time.deltaTime;
            if (attackWindUpTimer > attackWindUpTime)
            {
                // shows the spike and hides the warning
                GetComponent<SpriteRenderer>().enabled = false;
                spike.enabled = true;
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                attacking = false;

                Vector3 playerPos;
                playerPos.x = Mathf.Round(player.transform.position.x);
                playerPos.y = Mathf.Round(player.transform.position.y);
                playerPos.z = Mathf.Round(player.transform.position.z);

                // damages the player if they are on the spike
                if (playerPos == transform.position)
                {
                    player.GetComponent<PlayerBehaviour>().Damage();
                }
            }
        }
        // removes the spike after a short delay
        else
        {
            attackLingerTimer += Time.deltaTime;
            if (attackLingerTimer > attackLingerTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
