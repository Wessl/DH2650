using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    public Rigidbody2D rb, targetRB;
    public float jumpForce = 20f;
    public Transform groundCheck, wallCheck, ceilingCheck, tonguePoint, backWallCheck;
    public float speed, groundCheckRadius, wallCheckRadius, ceilingCheckRadius, wallSlideSpeed;
    public Animator animator;
   
    public LayerMask groundLayers, ceilingLayers;
    public float tongueSpeed = 50;
    public float pullSpeed = 40;
    public bool touchingCeiling, touchingWall, grounded, backTouchingWall;

    public GameObject tongueInit;
    GameObject tongue, target;

    public float weight;
    public int isFacingRight = 1;
    public float mx, my, stickTimer;
    Vector2 worldPos, grappleDirection;
    bool gettingPulled, pulling, jump;
    CircleCollider2D tongueCollider;
    Vector2 hitPos, targetPos, tongueRelPos;
    float stickiness = 1;
    public LayerMask playerMask, enemyMask;
    float tongueSize;
    public float airLerp, groundLerp, jumpMemory;
    private int tongueMouseKeyCode;
    public float dashTime, dashSpeed, dashCooldown, imageDistance, pulledSlash;
    float dashTimer;
    Vector2 lastImagePos;
    bool dashUnlocked, dashing, dashed;
    Vector2 dashDirection, pullDirection;
    public GameObject achievementPanel;

    private void Awake()
    {
        Application.targetFrameRate = 144;
    }

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        stickTimer = stickiness;
        Physics2D.IgnoreLayerCollision(10, 9);
        Physics2D.IgnoreLayerCollision(10, 14);
        Physics2D.IgnoreLayerCollision(10, 15);
        tongueSize = tongueInit.GetComponent<CircleCollider2D>().radius*2;
        UpdateTongueButtonMapping();
        dashTimer = dashTime;
    }

    // Update is called once per frame
    void Update()
    {
        IsGrounded();
        IsTouchingWall();
        IsBackTouchingWall();
        IsTouchingCeiling();
        CheckInput();
        if(tongue != null && !pulling && !gettingPulled)
        {
            CheckTongueCollision();
        }
        if(!animator.GetBool("LockedDirection"))
            FixDirection();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
        if (mx * isFacingRight == 1)
            animator.SetBool("Forward", true);
        else if (mx * isFacingRight == -1)
            animator.SetBool("Forward", false);
        if (jumpMemory > 0)
            jumpMemory -= Time.deltaTime;
        else if(jump)
            jump = false;
        if(gettingPulled && tongue == null)
        {
            if ((pullDirection.normalized / rb.velocity.normalized).sqrMagnitude > 4)
            {
                ResetRotation();
                GetPulled(false);
            }
        }
    }


    void CheckInput()
    {
        worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!(Input.GetKey("left") && Input.GetKey("right")) && !(Input.GetKey("a") && Input.GetKey("d")))
        {
            mx = Input.GetAxisRaw("Horizontal");
        }
        my = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump"))
        {
            if (animator.GetBool("LockedMovement"))
                animator.SetBool("MoveInput", true);
            jump = true;
            jumpMemory = 0.1f;
        }

        if (Input.GetMouseButtonDown(tongueMouseKeyCode) && !animator.GetBool("LockedMovement") && !TimeController.Instance.slowedTime)
        {
            grappleDirection = (worldPos - getMouthPos()).normalized;
            if (tongue == null &&                                                               // can only shoot if not already shooting
                ((grappleDirection.x + tonguePoint.localPosition.x >= 0 && isFacingRight > 0)
                || (grappleDirection.x - tonguePoint.localPosition.x <= 0 && isFacingRight < 0)))     // limits tongueshooting to direction frog is facing (not really needed with mouse tracking but w/e)

            {
                ShootTongue();  // shoot that thang
            }
        }
        else if (tongue != null && (!Input.GetMouseButton(tongueMouseKeyCode) || animator.GetBool("LockedMovement"))) // retract tongue if not holding button
        {
            RetractTongue();
        }

        Dash();
    }

    void FixedUpdate()
    {
        if(pulledSlash > 0)
        {
            pulledSlash -= Time.deltaTime;
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0,0), Time.deltaTime * 10);
            if(rb.velocity.magnitude > 20)
                AfterImagePool.Instance.GetFromPool(isFacingRight);
            return;
        } else if (pulledSlash > -99)
        {
            ResetRotation();
            pulledSlash = -99;
        }
        if (tongue && (pulling || gettingPulled))         // keep on pulling
            Pull();
        if (jump)
        {
            if (grounded)
                Jump();
            else if (touchingWall || backTouchingWall)
                WallJump();
        }
        if (!animator.GetBool("LockedMovement") && !(gettingPulled && tongue))
        {

            if (grounded)  // snappy movement if player is grounded 
            {
                if (!animator.GetBool("Hurt"))
                {
                    if (mx != 0)
                        rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(mx * speed, rb.velocity.y), Time.deltaTime * groundLerp);
                    else
                        rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(mx * speed, rb.velocity.y), Time.deltaTime * groundLerp * 2);
                }
                else
                    rb.velocity = new Vector2(mx * speed / 2, rb.velocity.y);
            }
            else   // more limited control while in the air
                rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(mx * speed, rb.velocity.y), Time.deltaTime * airLerp);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        if ((touchingWall || backTouchingWall) && !grounded)
        {   // frog can wall run for a short time with enough momentum, otherwise he slides down wall
            if (rb.velocity.y < -wallSlideSpeed && my >= 0)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            else if(rb.velocity.y > jumpForce)  // he shouldn't go too fast tho
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            /*else if(my > 0)
                rb.velocity = new Vector2(rb.velocity.x, (float)(rb.velocity.y+0.5));*/
        }
        else if (touchingCeiling && !gettingPulled)
        {
            if (my > 0 && stickTimer > 0)   // check if player wants to stick and if they can stick
            {
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.05), 1);    // slow down horizontal movement a bit and force body up towards ceiling
                animator.SetFloat("ClimbSpeed", Mathf.Abs(rb.velocity.x) / 10);
                stickTimer -= Time.deltaTime;
            }
            else
            {
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.1), rb.velocity.y);     // if player doesn't want to stick, still slow down horizontal movement (friction yo)
            }
        }
    }

    void Dash()
    {
        if (dashUnlocked)
            return;
        if(dashed)
        {
            if (dashTimer > -dashCooldown)
            {
                if (dashTimer > -0.5f && rb.velocity.sqrMagnitude > 400)
                {
                    if (((Vector2)transform.position - lastImagePos).sqrMagnitude > imageDistance)
                    {
                        AfterImagePool.Instance.GetFromPool(isFacingRight);
                        lastImagePos = transform.position;
                    }
                }
                dashTimer -= Time.deltaTime;
                return;
            }
            else
                dashed = false;
        }

        if (!dashing)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                dashDirection = (worldPos - (Vector2)transform.position).normalized;
                dashing = true;
                AfterImagePool.Instance.GetFromPool(isFacingRight);
                lastImagePos = transform.position;
                dashTimer = dashTime;
            }
        }
        else
        {
            if (dashTimer <= 0)
            {
                dashed = true;
                dashing = false;
                float yVel = rb.velocity.y;
                if (yVel > jumpForce*0.8f)
                    yVel = jumpForce * 0.8f;
                rb.velocity = new Vector2(rb.velocity.x, yVel);
            }
            else
            {
                dashTimer -= Time.deltaTime;
                rb.velocity = dashDirection * dashSpeed;

                if(((Vector2)transform.position - lastImagePos).sqrMagnitude > imageDistance)
                {
                    AfterImagePool.Instance.GetFromPool(isFacingRight);
                    lastImagePos = transform.position;
                }
            }
        }
        
    }

    void CheckTongueCollision()
    {
        RaycastHit2D hit = Physics2D.Raycast(tonguePoint.position, (tongue.transform.position - tonguePoint.position).normalized, Vector2.Distance(tonguePoint.position, tongue.transform.position) + tongueSize, playerMask);

        if (hit)
        {
            TargetHit(hit.collider.gameObject, hit.point, hit.collider.tag);
        }
    }
    
    void Pull()
    {
        if(target == null)
        {
            RetractTongue();
            return;
        }
        targetPos = target.transform.position;
        float dist = Vector2.Distance(getMouthPos(), targetPos + tongueRelPos);
        pullDirection = ((targetPos + tongueRelPos) - getMouthPos()).normalized;
        if (dist < 1.6 )   // the tongue has almost returned back to the mouth
        {
            RetractTongue();
        }
        else if (gettingPulled)
        {
            tonguePoint.localPosition = new Vector2(0.2f, 0.96f);
            float rot = Mathf.Atan2(pullDirection.y, pullDirection.x) * Mathf.Rad2Deg;
            if (pullDirection.x < 0)
            {
                transform.localRotation = Quaternion.Euler(0, 180, -rot + 90);
            } else
                transform.localRotation = Quaternion.Euler(0, 0, rot-90);
            rb.velocity = pullDirection * pullSpeed/1.5f;
        }
        else if (pulling)
        {
            if (targetRB != null)
                targetRB.velocity = -pullDirection * pullSpeed;
            else
                target.transform.position = Vector2.MoveTowards(target.transform.position, getMouthPos(), Time.deltaTime * pullSpeed);
        }
    }
    public void RetractTongue()
    {
        TongueScript.Instance.Reset();
        TonguePool.Instance.AddToPool(tongue);
        tongue = null;
        if (pulling && target != null && !target.CompareTag("PickUp"))
            target.GetComponent<Animator>().SetBool("Pulled", false);
        targetRB = null;
        target = null;
        tonguePoint.localPosition = new Vector2(0.3f, 0.76f);
        pulling = false;
    }

    // Tongue has hit something
    // TODO: mass comparison
    public void TargetHit(GameObject hitTarget, Vector2 pos, string tag)
    {
        hitPos = pos;
        target = hitTarget;
        targetPos = target.transform.position;
        tongueRelPos = hitPos - targetPos;
        switch(tag)
        {
            case "OldStuff":
                targetRB = target.GetComponent<Rigidbody2D>();
                if (target.GetComponent<Enemy>().weight < weight)
                {
                    target.GetComponent<Animator>().SetBool("Pulled", true);
                    pulling = true;
                }
                else
                {
                    GetPulled(true);
                }
                break;
            case "StationaryShootingEnemy":
                tongue.GetComponent<TongueScript>().Target(target, pos);
                targetRB = target.GetComponent<Rigidbody2D>();
                pulling = true;
                target.GetComponentInParent<ShooterBoye>().Unstuck();
                break;
            case "Ground":
            case "Platform":
            case "Ceiling":
            case "GrapplePoint":
                tongue.GetComponent<TongueScript>().Target(target, pos);
                GetPulled(true);
                break;
            case "Enemy":
            case "FlyingEnemy":
            case "GroundEnemy":
            case "NewGroundEnemy":
                targetRB = target.GetComponent<Rigidbody2D>();
                tongue.GetComponent<TongueScript>().Target(target, pos);
                if (target.GetComponent<Enemy>().weight < weight)
                {
                    target.GetComponent<Animator>().SetBool("Pulled", true);
                    pulling = true;
                }
                else
                {
                    GetPulled(true);
                }
                break;
            case "SnakeBoss":
            case "Larva":
            case "WaspQueen":
                //Combat.instance.PulledSlash(target, tag, hitPos);
                break;
            case "PickUp":
                targetRB = target.GetComponent<Rigidbody2D>();
                targetRB.bodyType = RigidbodyType2D.Dynamic;
                tongue.GetComponent<TongueScript>().Target(target, pos);
                break;
            case "Nongrappable":
                break;
            default:
                print("Maybe do something for tag: " + tag);
                break;
        }
    }

    void GetPulled(bool pulled)
    {
        gettingPulled = pulled;
        animator.SetBool("GettingPulled", pulled);
        if(pulled)
        {
            Vector2 direction = ((targetPos + tongueRelPos) - getMouthPos());
        }
    }

    void ShootTongue()
    {
        ResetRotation();
        GetPulled(false);
        tongue = TonguePool.Instance.GetFromPool();
        tongueCollider = tongue.GetComponent<CircleCollider2D>();

        Physics2D.IgnoreCollision(tongueCollider, GetComponent<CapsuleCollider2D>());
        tongue.GetComponent<Rigidbody2D>().AddForce(grappleDirection * tongueSpeed);
    }


    void Jump()
    {
        Vector2 movement = new Vector2(rb.velocity.x, jumpForce);

        rb.velocity = movement;
        jump = false;
    }

    void WallJump()
    {
        Vector2 movement = new Vector2(0, 0);
        if(touchingWall)
            movement = new Vector2(-isFacingRight*jumpForce, jumpForce);
        else
            movement = new Vector2(isFacingRight * jumpForce, jumpForce);

        //Flip(); //flips character after wall jump (not needed with mouse tracking)
        rb.velocity = movement;
        jump = false;
    }

    void IsGrounded()
    {
        Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        if (ground != null)
        {
            grounded = true;
        }
        else
            grounded = false;
        animator.SetBool("Grounded", grounded);
    }

    void IsTouchingWall()
    {
        Collider2D wallTouch = Physics2D.OverlapCapsule(wallCheck.position, new Vector2(0.5f, 1.5f), CapsuleDirection2D.Vertical, 0, ceilingLayers);
        if (wallTouch != null)
            touchingWall = true;
        else
            touchingWall = false;
    }

    void IsBackTouchingWall()
    {
        Collider2D wallTouch = Physics2D.OverlapCapsule(backWallCheck.position, new Vector2(0.5f, 1.5f), CapsuleDirection2D.Vertical, 0, ceilingLayers);
        if (wallTouch != null)
            backTouchingWall = true;
        else
            backTouchingWall = false;
    }


    void IsTouchingCeiling()
    {
        Collider2D ceilingTouch = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayers);

        if (ceilingTouch != null)
        {
            touchingCeiling = true;
            animator.SetFloat("ClimbSpeed", Mathf.Abs(rb.velocity.x)/speed);
        }
        else
        {
            stickTimer = stickiness;        // reset ceiling stick timer
            touchingCeiling = false;
        }
        animator.SetBool("TouchingCeiling", touchingCeiling);
    }
    
    void FixDirection()
    {
        //follow movement or follow mouse?
        /*
        if (isFacingRight==1 && mx < 0)
            Flip();
        else if (isFacingRight==-1 && mx > 0)
            Flip();
        */
        if (pulledSlash > 0 || TimeController.Instance.bulletSlashing)
            return;
        Vector2 direction = worldPos - rb.position;
        if (direction.x > 0 && isFacingRight == -1)
            Flip();
        else if (direction.x < 0 && isFacingRight == 1)
            Flip();
    }
    
    // Flip character to face other direction (HE'S AMBIDEXTROUS OK)
    void Flip()
    {
        isFacingRight = -isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    
    // Use this property to get the current orientation (facing right or left) of the frog. Example.
    public int Orientation
    {
        get => isFacingRight;
        set => isFacingRight = value;
    }

    // What that mouth do?? It shoots a tongue
    public Vector2 getMouthPos()
    {
        return tonguePoint.position;
    }
    
    public void UpdateTongueButtonMapping()
    {
        if (PlayerPrefs.GetInt("LeftMouseIsTongue") == 1)
        {
            tongueMouseKeyCode = 0; // If mouse left click is tongue, then set tongue to be 0, i.e. left mouse button
        }
        else
        {
            tongueMouseKeyCode = 1; // If left mouse is not tongue, then right mouse must be tongue. 
        }
    }

    void ResetRotation()
    {
        if (isFacingRight == 1)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        else
            transform.localRotation = Quaternion.Euler(0, -180, 0);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Spikes"))
            Combat.instance.TakeDamage(150);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawWireCube(wallCheck.position, new Vector2(0.5f, 1.5f));

        Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);

        //Gizmos.DrawLine(ceilingCheck.position, new Vector3(ceilingCheck.position.x, ceilingCheck.position.y + ceilingCheckDist, ceilingCheck.position.z));
    }
}
