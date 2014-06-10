using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour 
{
    public Vector2 velocity;
    public float decelerationFactor = 0.9f;
    public float maxHorizontalSpeed = 5.0f;
    public float attackTimer = 0.0f;
    public float attackDelay = 1.0f;
    public float attackDistance = 2.0f;
    public float mass = 1.0f;
    public bool isGrounded = false;

    private Animator animator;
    private Vector2 facingDirection = Vector2.zero;

	// Use this for initialization
	void Start () 
    {
        facingDirection.x = 1.0f;

        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //work out x axis movement
        if (Input.GetAxis("Horizontal") != 0.0f)
        {
            velocity.x += ((Input.GetAxis("Horizontal")));
        }
        else if (velocity.x != 0.0f)
        {
            velocity.x *= decelerationFactor;

            if (velocity.x < 0.2f && velocity.x > -0.2)
            {
                velocity.x = 0.0f;
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

        //update direction
        if (velocity.x > 0.0f) facingDirection.x = 1.0f;
        if (velocity.x < 0.0f) facingDirection.x = -1.0f;

        //work out y axis movement
        if (isGrounded)
        {
            velocity.y = 0.0f;
            animator.SetBool("isFalling", false);
        }
        else
        {
            animator.SetBool("isFalling", true);
        }

        //player attack
        if (Input.GetButtonDown("Fire1"))
        {
            //if the attack isn't cooling down and the player isn't falling
            if (attackTimer >= attackDelay && velocity.y == 0.0f)
            {
                //start attack animation
                animator.SetBool("isAttacking", true);

                //stop the player from moving
                velocity.x = 0.0f;

                //check if the attack hits
                RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, attackDistance);

                if(hit.transform.tag == "Enemy")
                {
                    //kill enemy
                }
            }
        }

        //increment the attack cooldown timer
        if (attackTimer < attackDelay)
        {
            attackTimer += Time.deltaTime;
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
        transform.Translate(Time.deltaTime * velocity);
	}

    void OnCollisionEnter2D(Collision2D collision)
    {
    }

    void OnCollisionStay2D(Collision2D collision)
    {
    }
}
