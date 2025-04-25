using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] float hp;
    bool attacking = false;
    bool retreating = false;
    bool canAttack = true;
    float attackTimer = 0;
    [SerializeField] float attackTime = 0.2f;
    float attackCdTimer = 0;
    public float attackCd = 1;
    Vector3 attackStart = Vector3.zero;
    public float speed;
    bool moving = false;
    bool canMove = true;
    float movingTimer;
    float movingTime;
    Vector3 direction = Vector3.zero;
    [HideInInspector] public Vector3 targetPosition = Vector3.zero;
    float movingCdTimer;
    public float movingCdTime;

    [SerializeField] Tilemap tilemap;
    [SerializeField] ProcGenTest proc;
    [SerializeField] PlayMIDIFile playmidi;

    [SerializeField] List<Tile> walkaleTiles;

    [SerializeField] LightingManager lightingmanager;

    [SerializeField] GameObject playerShow;

    [SerializeField] TextMeshProUGUI text;

    bool moveStart = true;
    float moveStartTimer = 0;
    float moveStartTime = 0.02f;

    bool recovering = false;
    [SerializeField] float recoveryTime;
    float recoveryTimer;

    public bool active = false;


    // Start is called before the first frame update
    void Start()
    {
        // calculates movement speed
        movingTime = 1/speed;
        movingTimer = movingTime;
        movingCdTimer = movingCdTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            // returns to the menu and stops the midi when escape is pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                playmidi.Stop();
                SceneManager.LoadScene(0);
            }

            // provides a set time of invincibility when the player is hit
            if (recovering)
            {
                recoveryTimer += Time.deltaTime;
                if (recoveryTimer > recoveryTime)
                {
                    recovering = false;
                    recoveryTimer = 0;

                    Color colour = GetComponentInChildren<SpriteRenderer>().color;
                    colour.a = 1;
                    GetComponentInChildren<SpriteRenderer>().color = colour;
                }
            }

            // gives a cooldown to attacking
            if (!canAttack)
            {
                attackCdTimer += Time.deltaTime;
                if (attackCdTimer > attackCd)
                {
                    canAttack = true;
                    attackCdTimer = 0;
                }
            }

            // provides a slight delay before the player moves when an input is pressed, to allow diagonal movement
            if (moveStart)
            {
                moveStartTimer += Time.deltaTime;
                if (moveStartTimer > moveStartTime)
                {
                    moveStartTimer = 0;
                    moveStart = false;

                    // if the player wants to moves
                    if (direction.magnitude > 0)
                    {
                        moving = true;
                        targetPosition = transform.position + direction;

                        // rotates the player to face that direction
                        playerShow.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * (180 / Mathf.PI));

                        // if the player is trying to move into a wall, don't let them
                        if (!walkaleTiles.Contains(tilemap.GetTile(new Vector3Int((int)targetPosition.x, (int)targetPosition.y, 0))))
                        {
                            moving = false;
                        }
                        else
                        {
                            // if the player is trying to move into an enemy, don't let them
                            var enemies = FindObjectsOfType<Enemy>();
                            foreach (Enemy enemy in enemies)
                            {
                                if (enemy.alive && enemy.transform.position == targetPosition)
                                {
                                    moving = false;
                                }
                            }

                            // if the player's movement has passed all the checks, increase the time needed to move for diagonal movement
                            // (to avoid it being faster)
                            if (moving)
                            {
                                movingTimer *= direction.magnitude;
                            }
                        }
                    }
                }
            }

            // moves the player over a duration to their desired position
            if (moving)
            {
                movingTimer -= Time.deltaTime;

                // once the time is up
                if (movingTimer <= 0)
                {
                    // set their position to the target position
                    transform.position = targetPosition;

                    // reset variables
                    movingTimer = movingTime;
                    moving = false;
                    canMove = false;

                    // tell the lighting manager of this change
                    lightingmanager.UpdateLighting((int)transform.position.x, (int)transform.position.y);
                }
                else
                {
                    // moves the player gradually to the target
                    transform.position += direction.normalized * speed * Time.deltaTime;
                }
            }
            // a small cooldown on moving
            else if (!canMove)
            {
                movingCdTimer -= Time.deltaTime;
                if (movingCdTimer <= 0)
                {
                    canMove = true;
                    movingCdTimer = movingCdTime;
                }
            }

            // if the player is able to move and is not moving, record the movement keys they press
            if (!attacking && !moving && canMove)
            {
                // attacks if space is pressed and they are able to
                if (canAttack && Input.GetKey(KeyCode.Space))
                {
                    attacking = true;
                    attackStart = playerShow.transform.position;
                }
                else
                {
                    direction = Vector3.zero;
                    if (Input.GetKey(KeyCode.W))
                    {
                        direction += new Vector3(0, 1, 0);
                        moveStart = true;
                    }
                    if (Input.GetKey(KeyCode.S))
                    {
                        direction += new Vector3(0, -1, 0);
                        moveStart = true;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        direction += new Vector3(-1, 0, 0);
                        moveStart = true;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        direction += new Vector3(1, 0, 0);
                        moveStart = true;
                    }
                }
            }
            // moves the player forward and back again in an attack motion
            if (attacking)
            {
                attackTimer += Time.deltaTime;

                // moves forward
                if (!retreating)
                {
                    playerShow.transform.position += playerShow.transform.up * Time.deltaTime * 6;
                    if (attackTimer > attackTime)
                    {
                        retreating = true;
                        attackTimer = 0;

                        //calculates the tile the player is attacking
                        Vector3 playerDirection = playerShow.transform.up;
                        if (playerDirection.x > 0.1f)
                        {
                            playerDirection.x = 1;
                        }
                        else if (playerDirection.x < -0.1f)
                        {
                            playerDirection.x = -1;
                        }

                        if (playerDirection.y > 0.1f)
                        {
                            playerDirection.y = 1;
                        }
                        else if (playerDirection.y < -0.1f)
                        {
                            playerDirection.y = -1;
                        }
                        Vector3 attackPos = attackStart + playerDirection;

                        // damages any enemy on that tile
                        var enemies = FindObjectsOfType<Enemy>();
                        print(playerDirection);
                        foreach (Enemy enemy in enemies)
                        {
                            if (enemy.alive && enemy.transform.position == attackPos)
                            {
                                enemy.Damage();
                            }
                        }
                    }
                }
                // retreats backwards
                else
                {
                    playerShow.transform.position -= playerShow.transform.up * Time.deltaTime * 6;

                    // resets variables when done
                    if (attackTimer > attackTime)
                    {
                        retreating = false;
                        attacking = false;
                        canAttack = false;
                        attackTimer = 0;
                        playerShow.transform.position = attackStart;
                    }
                }
            }
        }
    }

    public void Damage()
    {
        // damages the player if they aren't invincible
        if (!recovering)
        {
            hp -= 1;
            text.text = "HP: " + hp.ToString();

            // returns to title screen if the player dies
            if (hp <= 0)
            {
                playmidi.Stop();
                SceneManager.LoadScene(0);
            }
            // else sets the player to be invincible
            else
            {
                recovering = true;
                Color colour = GetComponentInChildren<SpriteRenderer>().color;
                colour.a = 0.5f;
                GetComponentInChildren<SpriteRenderer>().color = colour;
            }
        }
    }
}
