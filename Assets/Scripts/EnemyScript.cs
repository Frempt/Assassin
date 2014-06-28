using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpawnDistance = 2.0f;
    public float bulletVelocity = 20.0f;

    public Transform[] patrolPoints;

    public float maxHorizontalSpeed = 5.0f;

    private Animator animator;

    private int patrolIndex = 0;
    private Vector2 target = Vector3.zero;

    private float searchTimer = 0.0f;
    private float searchDelay = 5.0f;

    private enum EnemyState { PATROLLING = 0, SEARCHING = 1, ATTACKING = 2, DEAD = 3 };
    private EnemyState state = EnemyState.PATROLLING;

    // Use this for initialization
    void Start()
    {
        if (patrolPoints[0])
        {
            target = patrolPoints[patrolIndex].position;
        }

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //set movement direction
        target.y = transform.position.y;
        Vector2 direction = target - (Vector2)transform.position;
        direction.y = 0.0f;

        //update animation
        animator.SetBool("isShooting", false);
        animator.SetBool("isMoving", false);

        //look for player
        bool playerFound = false;
        RaycastHit2D[] hitInfo = Physics2D.RaycastAll(transform.position, (target - (Vector2)transform.position).normalized, 4000.0f);
        foreach (RaycastHit2D ray in hitInfo)
        {
            if (ray)
            {
                //if the player is in sight, start shooting
                if (ray.collider.tag == "Player")
                {
                    state = EnemyState.ATTACKING;
                    target = ray.transform.position;
                    playerFound = true;
                    animator.SetBool("isShooting", true);
                    break;
                }
            }
        }

        //if the player isn't in sight, search for them
        if (!playerFound && state == EnemyState.ATTACKING)
        {
            state = EnemyState.SEARCHING;
            target = patrolPoints[patrolIndex].position;
        }

        //state based behaviours
        switch (state)
        {
            case EnemyState.PATROLLING:
                //turn around
                if (direction.magnitude < 1.0f)
                {
                    if (patrolPoints.Length > 1)
                    {
                        if (patrolIndex < (patrolPoints.Length - 1))
                        {
                            patrolIndex++;
                            target = patrolPoints[patrolIndex].position;
                        }
                        else
                        {
                            patrolIndex--;
                            target = patrolPoints[patrolIndex].position;
                        }
                    }
                }

                animator.SetBool("isMoving", true);

                //update the enemy position
                direction.Normalize();
                transform.Translate(direction * maxHorizontalSpeed * Time.deltaTime);
                transform.rotation = Quaternion.identity;
                break;

            case EnemyState.SEARCHING:

                //increment the timer
                if (searchTimer < searchDelay)
                {
                    searchTimer += Time.deltaTime;
                }
                else
                {
                    //return to the patrol state
                    state = EnemyState.PATROLLING;
                    target = patrolPoints[patrolIndex].position;
                    searchTimer = 0.0f;
                }

                animator.SetBool("isMoving", true);

                if (direction.magnitude < 1.0f)
                {
                    target = -direction.normalized * 5.0f;
                }

                //update the enemy position
                direction.Normalize();
                transform.Translate(direction * maxHorizontalSpeed * Time.deltaTime);
                transform.rotation = Quaternion.identity;
                break;

            case EnemyState.ATTACKING:

                //attack logic primarily carried out by animator
                animator.SetBool("isShooting", true);
                transform.rotation = Quaternion.identity;
                break;

            case EnemyState.DEAD:

                animator.SetBool("isDying", true);

                transform.rotation = Quaternion.identity;
                break;

            default:
                transform.rotation = Quaternion.identity;
                break;
        }

        if (direction.x > 0.0f)
        {
            FaceRight(true);
        }
        else
        {
            FaceRight(false);
        }
    }

    public void ShootBullet()
    {
        GameObject newBullet = (GameObject)GameObject.Instantiate(bulletPrefab, transform.position + ((Vector3)target-transform.position).normalized * bulletSpawnDistance, Quaternion.identity);
        newBullet.rigidbody2D.velocity = (target - (Vector2)transform.position).normalized * bulletVelocity;

        Debug.Log("shot bullet");
    }

    public void FaceRight(bool facingRight)
    {
        if (facingRight && transform.localScale.x < 0.0f)
        {
            Vector2 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        if (!facingRight && transform.localScale.x > 0.0f)
        {
            Vector2 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
