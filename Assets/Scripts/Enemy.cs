using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Enemy : MonoBehaviour, ILevelObject
{
    protected GameObject player;

    [SerializeField] protected GameObject attack;

    [SerializeField] protected float hp;

    public bool alive = true;
    protected bool active = false;

    protected bool waiting = true;
    protected bool attacking = false;
    [SerializeField] protected float waitTime;
    protected float waitTimer = 0;

    [SerializeField] protected float attackTime;
    protected float attackTimer = 0;

    protected Tilemap map;
    [SerializeField] protected Tile black;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        map = GameObject.FindGameObjectWithTag("Tilemap").GetComponentInChildren<Tilemap>();
    }

    private void Update()
    {

        if (active && alive)
        {
            if (waiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer > waitTime)
                {
                    waitTimer = 0;
                    waiting = false;
                    attacking = true;
                    Attack();
                }
            }
            else if (attacking)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer > attackTime)
                {
                    attackTimer = 0;
                    attacking = false;
                    waiting = true;
                    Move();
                }
            }
        }
    }

    public void Activate()
    {
        active = true;
        if (alive)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }

    public void Deactivate()
    {
        active = false;
        GetComponent<SpriteRenderer>().enabled = false;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public Vector3Int GetPos()
    {
        return new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    public void Damage()
    {
        print(hp);
        hp -= 1;
        if (hp <= 0)
        {
            alive = false;
            GetComponent<SpriteRenderer>().enabled = false;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    virtual public void Attack()
    {
        for (float x = transform.position.x - 1; x <= transform.position.x + 1; x++)
        {
            for (float y = transform.position.y - 1; y <= transform.position.y + 1; y++)
            {
                if (new Vector3(x,y) != transform.position)
                {
                    Instantiate(attack).transform.position = new Vector3(x, y);
                }
            }
        }
    }

    virtual public void Move()
    {
        Vector3 newPos = transform.position;
        if (player.transform.position.x > transform.position.x)
        {
            newPos.x += 1;
        }
        else if (player.transform.position.x < transform.position.x)
        {
            newPos.x -= 1;
        }

        if (player.transform.position.y > transform.position.y)
        {
            newPos.y += 1;
        }
        else if (player.transform.position.y < transform.position.y)
        {
            newPos.y -= 1;
        }

        bool canMove = true;
        var a = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in a)
        {
            if (enemy.alive && enemy.transform.position == newPos)
            {
                canMove = false;
                break;
            }
        }

        if (player.transform.position == newPos || player.GetComponent<PlayerBehaviour>().targetPosition == newPos)
        {
            canMove = false;
        }

        if (canMove && map.GetTile(new Vector3Int((int)newPos.x, (int)newPos.y)) != black)
        {
            transform.position = newPos;
        }
    }
}
