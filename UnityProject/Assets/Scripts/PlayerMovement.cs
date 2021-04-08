using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb, targetRB;
    public float jumpForce = 20f;
    public Transform groundCheck, wallCheck, ceilingCheck, tonguePoint;
    public float speed, groundCheckRadius, wallCheckRadius, ceilingCheckRadius, wallSlideSpeed;
   
    public LayerMask groundLayers;
    public float tongueSpeed = 50;
    public float pullSpeed = 40;

    public GameObject tongueInit;
    GameObject tongue, target;
    public LineRenderer line;

    int isFacingRight = 1;
    float mx, my, stickTimer;
    Vector2 startPos, worldPos, grappleDirection;
    bool shooting, gettingPulled, pulling;
    float pullCharge = 0;
    CircleCollider2D tongueCollider;
    Vector2 tonguePos, targetPos, tongueRelPos;
    float stickiness = 1;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        shooting = false;
        stickTimer = stickiness;
        Physics2D.IgnoreLayerCollision(9, 10);
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        FixDirection();
    }

    void CheckInput()
    {
        worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mx = Input.GetAxisRaw("Horizontal");
        my = Input.GetAxisRaw("Vertical");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            Jump();
        } 
        else if (Input.GetButtonDown("Jump") && IsTouchingWall())
        {
            WallJump();
        }

        if (Input.GetMouseButtonDown(0) && !shooting)   // can only shoot if not already shooting
        {
            grappleDirection = (worldPos - getMouthPos()).normalized;
            if ((grappleDirection.x > 0 && isFacingRight > 0) || (grappleDirection.x < 0 && isFacingRight < 0))     // limits tongueshooting to direction frog is facing (not really needed with mouse tracking but w/e)
            {
                shooting = true;    
                ShootTongue();  // shoot that thang
            }
        }
        else if (!Input.GetMouseButton(0) && !shooting && (gettingPulled || pulling))
        {
            pullCharge = -1;
        }
    }

    void FixedUpdate()
    {
        if (pullCharge > 0)         // keep on pulling
        {
            Pull();
        } else if (pullCharge < 0)  // one last pull
        {
            Pull();
            gettingPulled = false;
            pulling = false;
            RetractTongue();
            pullCharge = 0;
        }
        else if (IsGrounded())  // snappy movement if player is grounded
            rb.velocity = new Vector2(mx * speed, rb.velocity.y);
        else if (Mathf.Abs(rb.velocity.x) > Mathf.Abs(rb.velocity.x + mx) || Mathf.Abs(rb.velocity.x) < speed / 1.1)    // more limited control while in the air
            rb.velocity = new Vector2(rb.velocity.x + mx / 2, rb.velocity.y);

        if (IsWallSliding())
        {   // frog can wall run for a short time with enough momentum, otherwise he slides down wall
            if (rb.velocity.y < -wallSlideSpeed && my >= 0)
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            else if(rb.velocity.y > jumpForce)  // he shouldn't go too fast tho
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            else if(my > 0)
                rb.velocity = new Vector2(rb.velocity.x, (float)(rb.velocity.y+0.5));
        }
        else if (IsCeilingSticking() && !gettingPulled)
        {
            if (my > 0 && stickTimer > 0)   // check if player wants to stick and if they can stick
            {
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.05), speed);    // slow down horizontal movement a bit and force body up towards ceiling
                stickTimer -= Time.deltaTime;
            }
            else
                rb.velocity = new Vector2((float)(rb.velocity.x / 1.1), rb.velocity.y);     // if player doesn't want to stick, still slow down horizontal movement (friction yo)
        }
    }
    
    void Pull()
    {
        targetPos = target.transform.position;
        float dist = Vector2.Distance(getMouthPos(), targetPos + tongueRelPos);
        Vector2 direction = ((targetPos + tongueRelPos) - getMouthPos()).normalized;
        if (dist < 1)   // the tongue has almost returned back to the mouth
        {
            gettingPulled = false;
            pulling = false;
            RetractTongue();
            pullCharge = 0;
        }
        else if (gettingPulled)
        {
            rb.velocity = direction * pullSpeed;
        }
        else if (pulling)
        {
            targetRB.velocity = -direction * pullSpeed;
        }
    }
    public void RetractTongue()
    {
        Destroy(tongue);
        shooting = false;
    }

    // Tongue has hit something
    // TODO: mass comparison
    public void TargetHit(GameObject hitTarget, Vector2 pos, string tag)
    {
        print("Target hit");
        tonguePos = pos;
        target = hitTarget;
        Vector2 targetdPos = target.transform.position;
        tongueRelPos = tonguePos - targetPos;
        targetPos = target.transform.position;
        tongueRelPos = tonguePos - targetPos;
        print(tag);
        if (tag.Equals("Enemy"))
        {
            print("yes");
            targetRB = target.GetComponent<Rigidbody2D>();
            pulling = true;
        }
        else if (tag.Equals("Ground"))
            gettingPulled = true;
        shooting = false;
        pullCharge = 1;
    }

    void ShootTongue()
    {
        tongue = Instantiate(tongueInit,tonguePoint.position,Quaternion.identity);

        tongueCollider = tongue.GetComponent<CircleCollider2D>();

        Physics2D.IgnoreCollision(tongueCollider, GetComponent<BoxCollider2D>());
        tongue.GetComponent<Rigidbody2D>().AddForce(grappleDirection * (tongueSpeed + rb.velocity.magnitude));
    }

    bool IsWallSliding()
    {
        if (IsTouchingWall() && !IsGrounded())
            return true;
        return false;
    }

    bool IsCeilingSticking()
    {
        if (IsTouchingCeiling()) 
            return true;
        stickTimer = stickiness;
        return false;
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

    bool IsGrounded()
    {
        Collider2D grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        if (grounded != null)
            return true;

        return false;
    }

    bool IsTouchingWall()
    {
        Collider2D touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayers);

        if (touchingWall != null)
            return true;

        return false;
    }

    bool IsTouchingCeiling()
    {
        Collider2D touchingCeiling = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, groundLayers);

        if (touchingCeiling != null)
            return true;

        return false;
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
        if (!IsWallSliding())
        {
            Vector2 direction = worldPos - rb.position;
            if (direction.x > 0 && isFacingRight == -1)
                Flip();
            else if (direction.x < 0 && isFacingRight == 1)
                Flip();
        }
    }
    
    // Flip character to face other direction (HE'S AMBIDEXTROUS OK)
    void Flip()
    {
        isFacingRight = -isFacingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    // What that mouth do?? It shoots a tongue
    public Vector2 getMouthPos()
    {
        return tonguePoint.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);

        Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);

        //Gizmos.DrawLine(ceilingCheck.position, new Vector3(ceilingCheck.position.x, ceilingCheck.position.y + ceilingCheckDist, ceilingCheck.position.z));
    }
}
