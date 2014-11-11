using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{
    public Vector2 velocity;
    public float decelerationFactor = 0.9f;
    public float maxHorizontalSpeed = 8.0f;
    public bool isBesideWall = false;
    public bool isGrounded = false;
    public bool isRooted = false;
    public bool isHanging = false;
    public GameObject cursorPrefab;
    public GameObject grabberPrefab;

    public bool atDoor = false;
    public bool atLever = false;
    public bool atGrabPoint = false;
    public bool atTeleporter = false;
    public TeleportNodeScript.NodeColour teleportColour;
    public GameObject proxyTeleporter;
    public GameObject selectedTeleporter;
    public GameObject proxyGrabPoint;
    public GameObject targetedGrabPoint;
    public GameObject proxyDoor;
    public GameObject proxyLever;

    public GameObject mindSeedPrefab;
    public float mindSeedVelocity;
    public Vector2 mindSeedSpawnPosition;
    public float mindSeedRange;
    public LayerMask ignoreVisionMask;

    public AudioClip stepClip01;
    public AudioClip stepClip02;
    public AudioClip jumpClip;
    public AudioClip teleportInClip;
    public AudioClip teleportOutClip;
    public AudioClip teleportFlyClip;

    private float gravityScale;
    private GameObject groundedOn;
    private GameObject beside;
    private AnimatorStateInfo animState;
    private Animator animator;
    private Vector2 facingDirection = Vector2.zero;
    private GameObject grabDestination;

    private Vector2 teleportEndPosition = Vector3.zero;
    private Vector2 teleportDirection = Vector3.zero;
    private float teleportSpeed = 40.0f;
    private int teleporterIndex = 0;

    private float previousHorizontalAxis = 0.0f;
    private Vector2 previousMousePosition = new Vector2(0.0f, 0.0f);
    private bool mouseMoved = false;

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

        //update the mouse state
        mouseMoved = false;
        if (previousMousePosition != (Vector2)Input.mousePosition) mouseMoved = true;

        if (!GameControllerScript.instance.levelComplete && !animState.IsName("Dying"))
        {
            //update the players movement and jumping
            UpdateMovement();
            UpdateJumping();

            //update the player's abilities and interactions
            UpdateInteraction();
            UpdateTeleportation();
            UpdateMindSeed();
        }

        //update the animation
        UpdateAnimation();

        //update the player's velocity
        if (!isRooted)
        {
            rigidbody2D.velocity = velocity;
        }
        else rigidbody2D.velocity = Vector2.zero;

        //fix rotation to the identity
        transform.rotation = Quaternion.identity;

        previousMousePosition = Input.mousePosition;
        previousHorizontalAxis = Input.GetAxis("Horizontal");
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
        renderer.enabled = false;
        collider2D.enabled = false;
        animator.SetBool("isPhasing", false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            //if the platform is horizontal, become grounded on it
            if (collision.gameObject.GetComponent<PlatformScript>().isHorizontal)
            {
                if(collision.transform.position.y < transform.position.y)
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
            }
            //if the platform is vertical, slide down it
            else 
            {
                if (!isGrounded && !isRooted)
                {
                    rigidbody2D.gravityScale = gravityScale / 2;
                    animator.SetBool("isSliding", true);
                }

                beside = collision.gameObject;
                isBesideWall = true;
            }
        }

        //alert the enemy of the player's position
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<EnemyScript>().Alert(transform.position);
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

        if (isBesideWall && collision.gameObject == beside)
        {
            beside = null;
            isBesideWall = false;
            animator.SetBool("isSliding", false);
            rigidbody2D.gravityScale = gravityScale;
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
        isRooted = false;
        if (rigidbody2D.gravityScale != gravityScale) rigidbody2D.gravityScale = gravityScale;
    }

    public void ResetLevel()
    {
        GameControllerScript.instance.ResetLevel();
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
                    isRooted = true;
                    animator.SetBool("isShooting", true);
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
            //if the player is invisible/intangible and has not started teleporting
            if(!GetComponent<Renderer>().enabled && !GetComponent<TrailRenderer>().enabled)
            {
                //if the mouse has moved
                if (mouseMoved)
                {
                    Collider2D[] colliders;
                    //get the objects at the mouse point
                    if (Application.platform == RuntimePlatform.PSMPlayer)
                    {
                        colliders = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position));
                    }
                    else
                    {
                        colliders = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    }

                    //iterate through the colliders
                    foreach (Collider2D other in colliders)
                    {
                        //if the mouse is at a teleporter of the correct colour, it is the selected teleporter
                        if (other.tag == "Teleporter")
                        {
                            if (other.GetComponent<TeleportNodeScript>().colour == teleportColour)
                            {
                                selectedTeleporter = other.gameObject;
                            }
                        }
                    }
                }

                //get all the teleporters
                GameObject[] teleporters = GameObject.FindGameObjectsWithTag("Teleporter");
                List<GameObject> colourTeleporters = new List<GameObject>();

                //select the first teleporter of the correct colour
                foreach (GameObject teleporter in teleporters)
                {
                    if (teleporter.GetComponent<TeleportNodeScript>().colour == teleportColour)
                    {
                        colourTeleporters.Add(teleporter);
                    }
                }

                //if the axis has changed
                if (previousHorizontalAxis == 0.0f && Input.GetAxis("Horizontal") != 0.0f)
                {
                    //decrement the index
                    if (Input.GetAxis("Horizontal") < 0.0f)
                    {
                        teleporterIndex--;
                    }

                    //increment the index
                    if (Input.GetAxis("Horizontal") > 0.0f)
                    {
                        teleporterIndex++;
                    }

                    //ensure the index is within the bounds
                    if (teleporterIndex >= colourTeleporters.Count) teleporterIndex = 0;
                    if (teleporterIndex < 0) teleporterIndex = (colourTeleporters.Count-1);

                    //set the selected teleporter
                    selectedTeleporter = colourTeleporters[teleporterIndex];
                }

                //make sure the cursor is at the correct position
                if (selectedTeleporter) CursorScript.CreateNewCursor(selectedTeleporter.transform.position, cursorPrefab);

                //if the player presses action
                if ((Input.GetButtonDown("Action") || Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)) && selectedTeleporter)
                {
                    //player is teleporting to the selected teleporter
                    teleportEndPosition = selectedTeleporter.transform.position;
                    teleportDirection = teleportEndPosition - (Vector2)transform.position;

                    selectedTeleporter = null;

                    //TODO: set trail rendered colour to the teleport node's colour
                    GetComponent<TrailRenderer>().enabled = true;

                    audio.loop = true;
                    audio.clip = teleportFlyClip;
                    audio.Play();

                    //remove the cursor
                    CursorScript.RemoveCursors();
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

                audio.Stop();
                audio.PlayOneShot(teleportInClip);
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
                Jump();
            }

            //if the player is sliding down a wall, launch off it
            if (animState.IsName("Sliding"))
            {
                velocity = new Vector2(10.0f * -facingDirection.x, 25.0f);
                rigidbody2D.gravityScale = gravityScale;
                animator.SetBool("isSliding", false);
                animator.SetBool("isFalling", true);
                audio.PlayOneShot(jumpClip);
                facingDirection.x *= -1.0f;
                if (facingDirection.x < 0.0f) FaceRight(false);
                else FaceRight(true);
            }
        }

        //if not grounded and moving, slow down
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
            //if the player is at a teleporter
            if (atTeleporter && !isHanging)
            {
                if (!animState.IsName("PhasingOut") && !animState.IsName("PhasingIn"))
                {
                    //set the animation to phase out
                    animator.SetBool("isPhasing", true);

                    //prevent the player from moving
                    isRooted = true;
                    rigidbody2D.gravityScale = 0.0f;
                    teleporterIndex = 0;
                    teleportColour = proxyTeleporter.GetComponent<TeleportNodeScript>().colour;

                    audio.PlayOneShot(teleportOutClip);
                }
            }

            //if the player is at a door and is grounded
            else if (atDoor && isGrounded)
            {
                //move the player to the door's exit
                Vector2 newPosition = proxyDoor.GetComponent<DoorScript>().exit.transform.position;
                transform.position = newPosition;
            }

            else if (atLever && isGrounded)
            {
                //switch the lever
                proxyLever.GetComponent<LeverScript>().Switch();
            }

            //if the player is at a grab point and is not grounded
            else if (atGrabPoint && !isGrounded)
            {
                if (!animState.IsName("PhasingOut") && !animState.IsName("PhasingIn"))
                {
                    //if the player is hanging already, release from the grab point
                    if (isHanging)
                    {
                        isRooted = false;

                        //if targetting another grab point, set it to the destination
                        if (targetedGrabPoint)
                        {
                            grabDestination = targetedGrabPoint;
                        }
                        else
                        {
                            //if not targetting a grab point, stop hanging
                            animator.SetBool("isHanging", false);
                            isHanging = false;
                            rigidbody2D.gravityScale = gravityScale;

                            //remove the old grabber
                            GameObject oldGrabber = GameObject.FindGameObjectWithTag("Grabber");
                            if (oldGrabber)
                            {
                                Destroy(oldGrabber);
                            }

                            //remove the old cursor
                            CursorScript.RemoveCursors();
                        }
                    }
                    else
                    {
                        //if not currently hanging, set the proxy grab point to the destination and start hanging
                        velocity = Vector2.zero;
                        rigidbody2D.velocity = Vector2.zero;
                        rigidbody2D.gravityScale = 0.0f;

                        grabDestination = proxyGrabPoint;

                        animator.SetBool("isHanging", true);
                        isHanging = true;
                    }
                }
            }
        }

        //if the player is hanging
        if (isHanging)
        {
            //if not yet at the destination
            if ((Vector2)transform.position != (Vector2)grabDestination.GetComponent<GrabPointScript>().hangPoint.position)
            {
                //move towards the destination
                velocity = (Vector2)grabDestination.GetComponent<GrabPointScript>().hangPoint.position - (Vector2)transform.position;
                velocity.Normalize();
                velocity *= maxHorizontalSpeed;

                if(((Vector2)transform.position - (Vector2)grabDestination.GetComponent<GrabPointScript>().hangPoint.position).magnitude < 0.5f)
                {
                    transform.position = (Vector2)grabDestination.GetComponent<GrabPointScript>().hangPoint.position;
                }

                //remove the old grabber
                GameObject oldGrabber = GameObject.FindGameObjectWithTag("Grabber");
                if (oldGrabber)
                {
                    Destroy(oldGrabber);
                }

                //create a new grabber
                GameObject grabber = (GameObject)GameObject.Instantiate(grabberPrefab, transform.position, Quaternion.identity);
                LineRenderer lr = grabber.GetComponent<LineRenderer>();
                lr.SetPosition(0, grabber.transform.position);
                lr.SetPosition(1, grabDestination.transform.position);
                lr.SetWidth(0.1f, 0.1f);
                grabber.tag = "Grabber";
            }
            else
            {
                //if destination reached, root the player
                velocity = Vector2.zero;
                rigidbody2D.velocity = Vector2.zero;
                isRooted = true;
            }

            //if the player is pressing right
            if (Input.GetAxis("Horizontal") > 0.0f)
            {
                //look for a grab point to the right of the player
                Collider2D[] colliders = Physics2D.OverlapPointAll((Vector2)grabDestination.transform.position + new Vector2(5.0f, 0.0f));

                targetedGrabPoint = null;

                facingDirection.x = 1.0f;
                FaceRight(true);
                if (mindSeedSpawnPosition.x <= 0.0f) mindSeedSpawnPosition.x *= -1.0f;

                foreach (Collider2D collider in colliders)
                {
                    //if grab point found
                    if (collider.tag == "GrabPoint")
                    {
                        //update the cursor
                        CursorScript.CreateNewCursor(collider.transform.position, cursorPrefab);

                        //set the grab point to the target
                        if (targetedGrabPoint != collider.gameObject)
                        {
                            targetedGrabPoint = collider.gameObject;
                        }
                    }
                }
            }
            //if the player is pressing left
            else if (Input.GetAxis("Horizontal") < 0.0f)
            {
                //look for a grab point to the left of the player
                Collider2D[] colliders = Physics2D.OverlapPointAll((Vector2)transform.position + new Vector2(-5.0f, 0.0f));

                targetedGrabPoint = null;

                facingDirection.x = -1.0f;
                FaceRight(false);
                if (mindSeedSpawnPosition.x >= 0.0f) mindSeedSpawnPosition.x *= -1.0f;

                foreach (Collider2D collider in colliders)
                {
                    //if grab point found
                    if (collider.tag == "GrabPoint")
                    {
                        //update the cursor
                        CursorScript.CreateNewCursor(collider.transform.position, cursorPrefab);

                        //set the grab point to the target
                        if (targetedGrabPoint != collider.gameObject)
                        {
                            targetedGrabPoint = collider.gameObject;
                        }
                    }
                }
            }
            else
            {
                //remove the cursor
                CursorScript.RemoveCursors();
                targetedGrabPoint = null;
            }
        }
        else
        {
            //remove the old grabber
            GameObject oldGrabber = GameObject.FindGameObjectWithTag("Grabber");
            if (oldGrabber)
            {
                Destroy(oldGrabber);
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
                    if (mindSeedSpawnPosition.x <= 0.0f) mindSeedSpawnPosition.x *= -1.0f;
                    FaceRight(true);
                }
                if (Input.GetAxis("Horizontal") < 0.0f)
                {
                    facingDirection.x = -1.0f;
                    if (mindSeedSpawnPosition.x >= 0.0f) mindSeedSpawnPosition.x *= -1.0f;
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

    public void Jump()
    {
        //apply vertical 
        audio.PlayOneShot(jumpClip);
        velocity += new Vector2(0.0f, 20.0f);

        //if the player is at a wall, slide down it
        if (isBesideWall)
        {
            rigidbody2D.gravityScale = gravityScale / 2;
            animator.SetBool("isSliding", true);
        }
    }

    public void Shoot()
    {
        //fire a mind seed
        GameObject newMindSeed = (GameObject)GameObject.Instantiate(mindSeedPrefab, transform.position + (Vector3)mindSeedSpawnPosition, Quaternion.identity);
        newMindSeed.GetComponent<Rigidbody2D>().velocity = facingDirection *= mindSeedVelocity;

        isRooted = false;
        animator.SetBool("isShooting", false);
    }

    //return true if the player is not dying
    public bool IsAlive()
    {
        return !animState.IsName("Dying");
    }

    public void SoundEffectFootstepLeft()
    {
        audio.PlayOneShot(stepClip01);
    }

    public void SoundEffectFootstepRight()
    {
        audio.PlayOneShot(stepClip02);
    }
}
