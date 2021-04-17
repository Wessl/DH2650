using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    public Rigidbody2D rb, targetRB;
    public float jumpForce = 20f;
    public Transform groundCheck, wallCheck, ceilingCheck, tonguePoint;
    public float speed, groundCheckRadius, wallCheckRadius, ceilingCheckRadius, wallSlideSpeed;
    public Animator animator;
   
    public LayerMask groundLayers, ceilingLayers;
    public float tongueSpeed = 50;
    public float pullSpeed = 40;
    public bool touchingCeiling, touchingWall, grounded;

    public GameObject tongueInit;
    GameObject tongue, target;
    public LineRenderer line;

    public float weight;
    public int isFacingRight = 1;
    public float mx, my, stickTimer;
    Vector2 worldPos, grappleDirection;
    bool gettingPulled, pulling;
    CircleCollider2D tongueCollider;
    Vector2 tonguePos, targetPos, tongueRelPos;
    float stickiness = 1;

    public GameObject achievementPanel;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        stickTimer = stickiness;
        Physics2D.IgnoreLayerCollision(9, 10);
    }

    // Update is called once per frame
    void Update()
    {
        IsGrounded();
        IsTouchingWall();
        IsTouchingCeiling();
        CheckInput();
        if(!animator.GetBool("LockedDirection"))
            FixDirection();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
        if (mx * isFacingRight == 1)
            animator.SetBool("Forward", true);
        else if (mx * isFacingRight == -1)
            animator.SetBool("Forward", false);
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
            else if (grounded)
                Jump();
            else if (touchingWall)
                WallJump();
        }

        if (Input.GetMouseButtonDown(0) && tongue == null)   // can only shoot if not already shooting
        {
            grappleDirection = (worldPos - getMouthPos()).normalized;
            if ((grappleDirection.x > 0 && isFacingRight > 0) || (grappleDirection.x < 0 && isFacingRight < 0))     // limits tongueshooting to direction frog is facing (not really needed with mouse tracking but w/e)
            {
                ShootTongue();  // shoot that thang
            }
        }
        else if (tongue != null && (!Input.GetMouseButton(0) || animator.GetBool("LockedMovement"))) // retract tongue if not holding button
        {
            RetractTongue();
        }

    }

    void FixedUpdate()
    {
        if (pulling || gettingPulled)         // keep on pulling
            Pull();
        if (!animator.GetBool("LockedMovement") && !gettingPulled)
        {
            
            if (grounded)  // snappy movement if player is grounded 
            {
                if (!animator.GetBool("Hurt"))
                    rb.velocity = new Vector2(mx * speed, rb.velocity.y);
                else
                    rb.velocity = new Vector2(mx * speed/2, rb.velocity.y);
            }
            else if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.x + mx) || Mathf.Abs(rb.velocity.x) < speed / 1.1)    // more limited control while in the air
                rb.velocity = new Vector2(rb.velocity.x + mx / 2, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
        if (touchingWall && !grounded)
        {   // frog can wall run for a short time with enough momentum, otherwise he slides down wall
            if (rb.velocity.y < -wallSlideSpeed && my >= 0)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            else if(rb.velocity.y > jumpForce)  // he shouldn't go too fast tho
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            else if(my > 0)
                rb.velocity = new Vector2(rb.velocity.x, (float)(rb.velocity.y+0.5));
        }
        else if (touchingCeiling && !gettingPulled)
        {
            if (my > 0 && stickTimer > 0)   // check if player wants to stick and if they can stick
            {
                print(Physics2D.gravity.y);
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.05), 1);    // slow down horizontal movement a bit and force body up towards ceiling
                animator.speed = rb.velocity.x / 10;
                stickTimer -= Time.deltaTime;
            }
            else
            {
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.1), rb.velocity.y);     // if player doesn't want to stick, still slow down horizontal movement (friction yo)
            }
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
        Vector2 direction = ((targetPos + tongueRelPos) - getMouthPos()).normalized;
        if (dist < 1.6 )   // the tongue has almost returned back to the mouth
        {
            RetractTongue();
        }
        else if (gettingPulled)
        {
            tonguePoint.localPosition = new Vector2(0.2f, 0.96f);
            float rot = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (direction.x < 0)
            {
                transform.localRotation = Quaternion.Euler(0, 180, -rot + 90);
            } else
                transform.localRotation = Quaternion.Euler(0, 0, rot-90);
            rb.velocity = direction * pullSpeed/1.5f;
        }
        else if (pulling)
        {
            targetRB.velocity = -direction * pullSpeed;
        }
    }
    public void RetractTongue()
    {
        Destroy(tongue);
        if (pulling && target != null)
            target.GetComponent<Animator>().SetBool("Pulled", false);
        tonguePoint.localPosition = new Vector2(0.3f, 0.76f);
        if (isFacingRight == 1)
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        else
            transform.localRotation = Quaternion.Euler(0, -180, 0);
        GetPulled(false);
        pulling = false;
    }

    // Tongue has hit something
    // TODO: mass comparison
    public void TargetHit(GameObject hitTarget, Vector2 pos, string tag)
    {
        print("Target hit");
        tonguePos = pos;
        target = hitTarget;
        targetPos = target.transform.position;
        tongueRelPos = tonguePos - targetPos;
        print(tag);
        switch(tag)
        {
            case "Enemy":
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
            case "Ground":
                GetPulled(true);
                break;
            case "SnakeBoss":
                GetPulled(true);
                break;
            case "Platform":
                GetPulled(true);
                break;
            default:
                print("Maybe do something for layer: " + tag);
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
        tongue = Instantiate(tongueInit,tonguePoint.position,Quaternion.identity);

        tongueCollider = tongue.GetComponent<CircleCollider2D>();

        Physics2D.IgnoreCollision(tongueCollider, GetComponent<CapsuleCollider2D>());
        tongue.GetComponent<Rigidbody2D>().AddForce(grappleDirection * (tongueSpeed + rb.velocity.magnitude));
    }


    void Jump()
    {
        Vector2 movement = new Vector2(rb.velocity.x, jumpForce);

        rb.velocity = movement;
    }

    void WallJump()
    {
        Vector2 movement = new Vector2(-isFacingRight*jumpForce/2, jumpForce);

        //Flip(); //flips character after wall jump (not needed with mouse tracking)
        rb.velocity = movement;
    }

    void IsGrounded()
    {
        Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        if (ground != null)
            grounded = true;
        else
            grounded = false;
        animator.SetBool("Grounded", grounded);
    }

    void IsTouchingWall()
    {
        Collider2D wallTouch = Physics2D.OverlapCapsule(wallCheck.position, new Vector2(1, 1.5f), CapsuleDirection2D.Vertical, 0, ceilingLayers);
        if (wallTouch != null)
            touchingWall = true;
        else
            touchingWall = false;
    }

    void IsTouchingCeiling()
    {
        Collider2D ceilingTouch = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayers);

        if (ceilingTouch != null)
        {
            print(ceilingTouch.tag);
            touchingCeiling = true;
            animator.speed = Mathf.Abs(rb.velocity.x)/speed;
        }
        else
        {
            stickTimer = stickiness;        // reset ceiling stick timer
            animator.speed = 1;
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
        if (touchingWall && !grounded)
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
    
    // Use this property to get the current orientation (facing right or left) of the frog.
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

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawWireCube(wallCheck.position, new Vector2(1, 1.5f));

        Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);

        //Gizmos.DrawLine(ceilingCheck.position, new Vector3(ceilingCheck.position.x, ceilingCheck.position.y + ceilingCheckDist, ceilingCheck.position.z));
    }
}
