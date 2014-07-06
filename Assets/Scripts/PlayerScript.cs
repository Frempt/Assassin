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

    public GameObject mindSeedPrefab;
    public float mindSeedVelocity;
    public Vector2 mindSeedSpawnPosition;
    public float mindSeedRange;
    public LayerMask ignoreVisionMask;

    private float gravityScale;
    private GameObject groundedOn;
    private AnimatorStateInfo animState;
    private Animator animator;
    private Vector2 facingDirection = Vector2.zero;

    private Vector2 teleportEndPosition = Vector3.zero;
    private Vector2 teleportDirection = Vector3.zero;
    private float teleportSpeed = 40.0f;

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
        //get the velocity
        velocity = rigidbody2D.velocity;

        //get the animation state
        animState = animator.GetCurrentAnimatorStateInfo(0);

        //update the players movement and jumping
        UpdateMovement();
        UpdateJumping();

        //update the player's abilities and interactions
        UpdateInteraction();
        UpdateTeleportation();
        UpdateMindSeed();

        //update the music and animation
        UpdateMusic();
        UpdateAnimation();

        //update the player's velocity
        if (!isRooted)
        {
            rigidbody2D.velocity = velocity;
        }
        else rigidbody2D.velocity = Vector2.zero;

        //fix rotation to the identity
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
            //if the platform is horizontal, become grounded on it
            if (collision.gameObject.GetComponent<PlatformScript>().isHorizontal && collision.transform.position.y < transform.position.y)
            {
                isGrounded = true;
                groundedOn = collision.gameObject;

                if (velocity.y > 0.0f)
                {
                    velocity.y = -1.0f;
                }

                animator.SetBool("isFalling", false);
                animator.SetBool("isSliding", false);
            }
            //if the platform is vertical, slide down it
            else if(!isGrounded && !isRooted)
            {
                rigidbody2D.gravityScale = gravityScale / 2;
                animator.SetBool("isSliding", true);
            }
        }

        if (collision.gameObject.tag == "Enemy")
        {
            //TODO: alert enemy
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            //if the platform is vertical, slide down it
            if (!isGrounded && !isRooted && collision.gameObject.GetComponent<PlatformScript>().isHorizontal)
            {
                rigidbody2D.gravityScale = gravityScale / 2;
                animator.SetBool("isSliding", true);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (isGrounded && collision.gameObject == groundedOn)
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

    //play the death animation and reset the level
    public void Die()
    {
        animator.SetBool("isDying", true);

        GameControllerScript.instance.ResetLevel();
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

    public void UpdateMindSeed()
    {
        //allow the player to fire mind seeds
        if (Input.GetButtonDown("Fire1") && !animState.IsName("PhasingOut"))
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, facingDirection, mindSeedRange, ~LayerMask.NameToLayer("Enemy"));
            if (hitInfo)
            {
                //if the objective is targetted and in range
                if (hitInfo.collider.tag == "Objective")
                {
                    //fire a mind seed
                    GameObject newMindSeed = (GameObject)GameObject.Instantiate(mindSeedPrefab, transform.position + (Vector3)mindSeedSpawnPosition, Quaternion.identity);
                    newMindSeed.GetComponent<Rigidbody2D>().velocity = facingDirection *= mindSeedVelocity;
                }
            }
        }
    }

    //update teleportation controls
    public void UpdateTeleportation()
    {
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
                            teleportEndPosition = other.transform.position;
                            teleportDirection = teleportEndPosition - (Vector2)transform.position;

                            //TODO: set trail rendered colour to the teleport node's colour
                            GetComponent<TrailRenderer>().enabled = true;
                            renderer.enabled = false;
                        }
                    }
                }
            }
        }

        //if the player is teleporting, move towards the target position
        if (teleportDirection != Vector2.zero)
        {
            transform.Translate(teleportDirection.normalized * teleportSpeed * Time.deltaTime);

            //if the player is close to the target position, set its position to the target
            if ((teleportEndPosition - (Vector2)transform.position).magnitude < 0.5f)
            {
                transform.position = teleportEndPosition;

                //start phasing in
                animator.SetBool("isTeleporting", true);

                teleportEndPosition = Vector2.zero;
                teleportDirection = Vector2.zero;

                GetComponent<TrailRenderer>().enabled = false;
                renderer.enabled = true;
            }
        }
    }

    public void UpdateJumping()
    {
        //work out y axis movement
        if (Input.GetButtonDown("Jump"))
        {
            //if the player is grounded, jump straight up
            if (isGrounded && !isRooted)
            {
                velocity += new Vector2(0.0f, 20.0f);
            }

            //if the player is sliding down a wall, launch off it
            if (animState.IsName("Sliding"))
            {
                velocity = new Vector2(10.0f * -facingDirection.x, 20.0f);
                rigidbody2D.gravityScale = gravityScale;
                animator.SetBool("isSliding", false);
            }
        }

        if (!isGrounded && velocity.x != 0.0f)
        {
            velocity.x *= 0.99f;

            if (velocity.x < 1.0f && velocity.x > -1.0f)
            {
                velocity.x = 0.0f;
            }
        }
    }

    public void UpdateInteraction()
    {
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
                //if the player is rooted, turn release from the grab point
                if (isRooted)
                {
                    rigidbody2D.gravityScale = gravityScale;
                    isRooted = false;

                    if (Input.GetAxis("Horizontal") > 0.0f)
                    {
                        velocity.x += maxHorizontalSpeed * 5.0f;
                    }
                    if (Input.GetAxis("Horizontal") < 0.0f)
                    {
                        velocity.x -= maxHorizontalSpeed * 5.0f;
                    }

                    animator.SetBool("isHanging", false);
                }
                else
                {
                    //fix to the grab point
                    Vector2 newPosition = proxyGrabPoint.GetComponent<GrabPointScript>().hangPoint.position;
                    transform.position = newPosition;

                    velocity = Vector2.zero;
                    rigidbody2D.velocity = Vector2.zero;

                    rigidbody2D.gravityScale = 0.0f;
                    isRooted = true;

                    animator.SetBool("isHanging", true);
                }
            }
        }
    }

    public void UpdateAnimation()
    {
        //update the animation
        if (velocity.x != 0.0f)
        {
            animator.SetBool("isMoving", true);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }

        //tranistion out of the sliding state
        if (animState.IsName("Sliding") && isGrounded)
        {
            rigidbody2D.gravityScale = gravityScale;
            animator.SetBool("isSliding", false);
        }
    }

    public void UpdateMovement()
    {
        //work out x axis movement
        if (!isRooted && isGrounded)
        {
            if (Input.GetAxis("Horizontal") != 0.0f)
            {
                velocity.x += ((Input.GetAxis("Horizontal")));

                //update direction
                if (Input.GetAxis("Horizontal") > 0.0f)
                {
                    facingDirection.x = 1.0f;
                    if (mindSeedSpawnPosition.x <= 0.0f) mindSeedSpawnPosition *= -1.0f;
                    FaceRight(true);
                }
                if (Input.GetAxis("Horizontal") < 0.0f)
                {
                    facingDirection.x = -1.0f;
                    if (mindSeedSpawnPosition.x >= 0.0f) mindSeedSpawnPosition *= -1.0f;
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
        if (velocity.x > maxHorizontalSpeed && isGrounded)
        {
            velocity.x = maxHorizontalSpeed;
        }
        if (velocity.x < -maxHorizontalSpeed && isGrounded)
        {
            velocity.x = -maxHorizontalSpeed;
        }
    }
}
