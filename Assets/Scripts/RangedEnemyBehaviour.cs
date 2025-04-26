using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyBehaviour : Enemy
{
    override public void Attack()
    {
        Vector3 projectileDirection = Vector3.zero;
        Vector3 direction = (player.transform.position - transform.position).normalized;
        

        // calculates the closest direction (of 8 directions) to the player to shoot the projectile at
        float angle = (180/Mathf.PI) * -Mathf.Atan2(direction.x,direction.y);

        if (angle >= -22.5 && angle < 22.5)
        {
            projectileDirection = new Vector3(0,1);
        }
        else if (angle >= 22.5 && angle < 67.5)
        {
            projectileDirection = new Vector3(-1,1);
        }
        else if (angle >= 67.5 && angle < 112.5)
        {
            projectileDirection = new Vector3(-1, 0);
        }
        else if (angle >= 112.5 && angle < 157.5)
        {
            projectileDirection = new Vector3(-1, -1);
        }
        else if (angle >= 157.5 || angle < -157.5)
        {
            projectileDirection = new Vector3(0, -1);
        }
        else if (angle >= -157.5 && angle < -112.5)
        {
            projectileDirection = new Vector3(1, -1);
        }
        else if (angle >= -112.5 && angle < -67.5)
        {
            projectileDirection = new Vector3(1, 0);
        }
        else
        {
            projectileDirection = new Vector3(1, 1);
        }

        // Spawns the projectile
        GameObject proj = Instantiate(attack);
        proj.transform.position = transform.position + projectileDirection;
        proj.GetComponent<ProjectileBehaviour>().direction = projectileDirection;
    }

    public override void Move()
    {
        // moves to align the enemy with the player in the closest direction
        Vector3 newPos = transform.position;

        if (Mathf.Abs(player.transform.position.x - transform.position.x) <
            Mathf.Abs(player.transform.position.y - transform.position.y))
        {
            if (player.transform.position.x < transform.position.x)
            {
                newPos.x -= 1;
            }
            else if (player.transform.position.x > transform.position.x)
            {
                newPos.x += 1;
            }
        }
        else
        {
            if (player.transform.position.y < transform.position.y)
            {
                newPos.y -= 1;
            }
            else if (player.transform.position.y > transform.position.y)
            {
                newPos.y += 1;
            }
        }

        // stops the movement if it is blocked by an enemy
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

        // stops the movement if it is blocked by the player
        if (player.transform.position == newPos || player.GetComponent<PlayerBehaviour>().targetPosition == newPos)
        {
            canMove = false;
        }

        // stops the movement if it is blocked by terrain
        if (canMove && map.GetTile(new Vector3Int((int)newPos.x, (int)newPos.y)) != black)
        {
            transform.position = newPos;
        }
    }
}
