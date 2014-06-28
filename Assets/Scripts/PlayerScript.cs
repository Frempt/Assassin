using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    public Vector2 velocity;
    public float decelerationFactor = 0.9f;
    public float maxHorizontalSpeed = 8.0f;
    public bool isGrounded = false;
    public bool isRooted = false;

    public bool atDoor = false;
    public bool atGrabPoint = false;
    public bool atTeleporter = false;
    public GameObject proxyTeleporter;
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
        if (!isRooted)
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
        if (isGrounded && !isRooted)
        {
            if (Input.GetButtonDown("Jump"))
            {
                rigidbody2D.AddForce(new Vector2(0.0f, 10000.0f));
            }
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

            //if the player is at a teleporter
            if (atTeleporter)
            {
                //set the animation to phase out
                animator.SetBool("isPhasing", true);

                //prevent the player from moving
                isRooted = true;
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

        //allow the player to select a teleporter
        if (animState.IsName("PhasingOut"))
        {
            //if the player left clicks
            if (Input.GetMouseButtonDown(0))
            {
                //get the objects at the mouse point
                Collider2D[] colliders = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                //iterate through the colliders
                foreach (Collider2D other in colliders)
                {
                    //if the player has clicked a teleporter of the correct colour, set his position to that teleporter and begin phasing in
                    if (other.tag == "Teleporter")
                    {
                        if (other.GetComponent<TeleportNodeScript>().colour == proxyTeleporter.GetComponent<TeleportNodeScript>().colour)
                        {
                            transform.position = other.transform.position;
                            animator.SetBool("isTeleporting", true);
                        }
                    }
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

        //update the music
        UpdateMusic();

        //update the player's location
        if(!isRooted) rigidbody2D.velocity = velocity;
        transform.rotation = Quaternion.identity;
    }

    //make the player tangible
    public void PhaseIn()
    {
        collider2D.enabled = true;
        rigidbody2D.gravityScale = gravityScale;
        isRooted = false;
        animator.SetBool("isTeleporting", false);
    }

    //make the player untangible
    public void PhaseOut()
    {
        collider2D.enabled = false;
        rigidbody2D.gravityScale = 0.0f;
        animator.SetBool("isPhasing", false);
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
            //TODO: alert enemy
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

    //change which direction the character is facing
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
        //TODO: add player death
        animator.SetBool("isDying", true);
    }

    //update the music
    public void UpdateMusic()
    {
        //get the enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        //create a distance with the maximum distance
        float distance = MusicScript.instance.maximumEnemyDistance;

        //check if any enemies are closer
        foreach (GameObject enemy in enemies)
        {
            Vector2 thisDistance = enemy.transform.position - transform.position;

            if (thisDistance.magnitude < distance) distance = thisDistance.magnitude;
        }

        //set the distance to that of the closest enemy
        MusicScript.instance.enemyDistance = distance;
    }
}
