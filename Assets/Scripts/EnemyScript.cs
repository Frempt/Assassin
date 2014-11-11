using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Vector2 bulletSpawnPosition = new Vector2(2.0f, 2.0f);
    public float bulletVelocity = 20.0f;

    public Transform[] patrolPoints;

    public float maxHorizontalSpeed = 5.0f;

    public LayerMask visionIgnoreMask;

    protected Animator animator;

    protected int patrolIndex = 0;
    protected Vector2 target = Vector3.zero;

    protected enum EnemyState { PATROLLING = 0, ATTACKING = 1, ROOTED = 2};
    protected EnemyState state = EnemyState.PATROLLING;

    // Use this for initialization
    public void Start()
    {
        if (patrolPoints[0])
        {
            target = patrolPoints[patrolIndex].position;
        }

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void Update()
    {
        //set movement direction
        target.y = transform.position.y;
        Vector2 direction = target - (Vector2)transform.position;
        direction.y = 0.0f;

        //update animation
        animator.SetBool("isShooting", false);
        animator.SetBool("isMoving", false);

        if (state != EnemyState.ROOTED)
        {
            //look for player
            bool playerFound = false;
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, (target - (Vector2)transform.position).normalized, 4000.0f, ~visionIgnoreMask);
            if (hitInfo)
            {
                //if the player is in sight, start shooting
                if (hitInfo.collider.tag == "Player" && hitInfo.collider.gameObject.GetComponent<PlayerScript>().IsAlive())
                {
                    state = EnemyState.ATTACKING;
                    target = hitInfo.transform.position;
                    playerFound = true;
                    animator.SetBool("isShooting", true);
                }
            }

            //if the player isn't in sight, search for them
            if (!playerFound && state == EnemyState.ATTACKING)
            {
                state = EnemyState.PATROLLING;
                target = patrolPoints[patrolIndex].position;
            }
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
                            patrolIndex = 0;
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

            case EnemyState.ATTACKING:

                //attack logic primarily carried out by animator
                animator.SetBool("isShooting", true);
                transform.rotation = Quaternion.identity;
                break;

            case EnemyState.ROOTED:
                //enemy rooted state
                //primarily used by the objective character
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
        if (target.x < transform.position.x)
        {
            if (bulletSpawnPosition.x > 0.0f) bulletSpawnPosition.x *= -1.0f;
        }

        if (target.x > transform.position.x)
        {
            if (bulletSpawnPosition.x < 0.0f) bulletSpawnPosition.x *= -1.0f;
        }

        audio.Play();

        GameObject newBullet = (GameObject)GameObject.Instantiate(bulletPrefab, transform.position + (Vector3)bulletSpawnPosition, Quaternion.identity);
        newBullet.rigidbody2D.velocity = (target - (Vector2)transform.position).normalized * bulletVelocity;
    }

    //update the direction the character is facing
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

    public void Alert(Vector3 playerPosition)
    {
        //face the player and start shooting
        target = playerPosition;
        state = EnemyState.ATTACKING;
        animator.SetBool("isShooting", true);
    }
}
