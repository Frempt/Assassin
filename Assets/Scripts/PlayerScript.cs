using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    public Vector2 velocity;
    public float decelerationFactor = 0.9f;
    public float maxHorizontalSpeed = 8.0f;
    public float attackTimer = 0.0f;
    public float attackDelay = 1.0f;
    public float attackDistance = 2.0f;
    public bool isGrounded = false;
    public bool isRooted = false;

    public bool atDoor = false;
    public bool atGrabPoint = false;
    public GameObject proxyGrabPoint;
    public GameObject proxyDoor;

    private float gravityScale;
    private GameObject groundedOn;
    private AnimatorStateInfo animState;
    private Animator animator;
    private Vector2 facingDirection = Vector2.zero;

    // Use this for initialization
    void Start()
    {
        facingDirection.x = 1.0f;

        animator = GetComponent<Animator>();

        gravityScale = rigidbody2D.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        velocity = rigidbody2D.velocity;

        animState = animator.GetCurrentAnimatorStateInfo(0);

        //work out x axis movement
        if (!animState.IsName("Attacking") && !isRooted)
        {
            if (Input.GetAxis("Horizontal") != 0.0f)
            {
                velocity.x += ((Input.GetAxis("Horizontal")));

                //update direction
                if (Input.GetAxis("Horizontal") > 0.0f)
                {
                    facingDirection.x = 1.0f;
                    FaceRight(true);
                }
                if (Input.GetAxis("Horizontal") < 0.0f)
                {
                    facingDirection.x = -1.0f;
                    FaceRight(false);
                }
            }
            else if (velocity.x != 0.0f)
            {
                velocity.x *= decelerationFactor;

                if (velocity.x < 1.0f && velocity.x > -1.0f)
                {
                    velocity.x = 0.0f;
                }
            }
        }

        //check X velocity is legal
        if (velocity.x > maxHorizontalSpeed)
        {
            velocity.x = maxHorizontalSpeed;
        }
        if (velocity.x < -maxHorizontalSpeed)
        {
            velocity.x = -maxHorizontalSpeed;
        }

        //work out y axis movement
        if (!animState.IsName("isAttacking") && isGrounded && !isRooted)
        {
            if (Input.GetButtonDown("Jump"))
            {
                rigidbody2D.AddForce(new Vector2(0.0f, 10000.0f));
            }
        }

        animator.SetBool("isAttacking", false);

        //player attack
        if (Input.GetButtonDown("Fire1") && !isRooted)
        {
            //if the attack isn't cooling down and the player isn't falling
            if (attackTimer >= attackDelay)
            {
                //start attack animation
                animator.SetBool("isAttacking", true);

                //add some attack momentum
                velocity.x = maxHorizontalSpeed * facingDirection.x;
            }
        }

        //increment the attack cooldown timer
        if (attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;
        }

        //allow for player interactions
        if (Input.GetButtonDown("Action"))
        {
            //if the player is at a door and is grounded
            if (atDoor && isGrounded)
            {
                //move the player to the door's exit
                Vector2 newPosition = proxyDoor.GetComponent<DoorScript>().exit.transform.position;
                transform.position = newPosition;
            }

            //if the player is at a grab point and is not grounded
            if (atGrabPoint && !isGrounded)
            {
                //move the player to the hang position, and keep him there
                Vector2 newPosition = proxyGrabPoint.GetComponent<GrabPointScript>().hangPoint.position;
                transform.position = newPosition;

                velocity = Vector2.zero;

                if (isRooted)
                {
                    rigidbody2D.gravityScale = gravityScale;
                    isRooted = false;
                }
                else
                {
                    rigidbody2D.gravityScale = 0.0f;
                    isRooted = true;
                }
            }
        }

        //update the animation
        if (velocity.x != 0.0f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        //update the player's location
        rigidbody2D.velocity = velocity;
        transform.rotation = Quaternion.identity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            if (collision.transform.position.y < transform.position.y)
            {
                isGrounded = true;
                groundedOn = collision.gameObject;
                animator.SetBool("isFalling", false);
            }
        }

        if (collision.gameObject.tag == "Enemy")
        {
            Vector2 collisionDirection = collision.transform.position - transform.position;
            collisionDirection.y = 0.0f;
            collisionDirection.Normalize();

            if (animState.IsName("Attacking") && collisionDirection == facingDirection)
            {
                //kill enemy
                collision.gameObject.GetComponent<EnemyScript>().Die();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Vector2 collisionDirection = collision.transform.position - transform.position;
            collisionDirection.y = 0.0f;
            collisionDirection.Normalize();

            if (animState.IsName("Attacking") && collisionDirection == facingDirection)
            {
                //kill enemy
                collision.gameObject.GetComponent<EnemyScript>().Die();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (groundedOn && collision.gameObject == groundedOn)
        {
            isGrounded = false;
            groundedOn = null;
            animator.SetBool("isFalling", true);
        }
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
        //todo: add player death
        animator.SetBool("isDying", true);
    }
}
