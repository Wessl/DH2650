using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public static Combat instance;
    public Animator animator;
    public LayerMask enemyLayer;
    bool attacking, canAttack;
    Rigidbody2D rb;
    [SerializeField]
    GameObject slash;
    [SerializeField]
    Transform attackPoint;
    [SerializeField]
    float slash1height, slash2range;
    PlayerMovement playerMovement;
    public LayerMask rayMask;
    public float attackDamage;
    Animation slashAnimation;
    float attackMove;
    int attacked;
    Vector2 attackOrigin;
    string slashStr;

    void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        rayMask = ~(1 << LayerMask.NameToLayer("Enemy"));
        slashAnimation = slash.GetComponent<Animation>();
        foreach (AnimationState anim in slashAnimation)
        {
            anim.speed = 0.5f;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }


    private void FixedUpdate()
    {
        AttackMovement();
    }

    public void Attack()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (playerMovement.IsGrounded())
            {
                attacking = true;
                canAttack = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {

            slash.transform.localRotation = Quaternion.Euler(180, 0, 180);
            slash.GetComponent<Animation>().Play("rotatingslash");
        }
    }
    public void SwitchAttackStatus()
    {
            canAttack = !canAttack;
    }

    public bool IsAttacking()
    {
        return attacking;
    }

    public void SetAttack(bool atk)
    {
            attacking = atk;
    }

    public void Attack(string name, float moveAmount)
    {
        slashStr = name;
        attackMove = playerMovement.Orientation * moveAmount;
        attackOrigin = attackPoint.position;
        if (moveAmount > 0)
            attacked = 2;
        else
            ExecuteAttack();
    }

    void ExecuteAttack()
    {
        float distance = Vector2.Distance(attackPoint.position, attackOrigin);
        float damage = 0;
        Vector2 point = new Vector2(0, 0);
        float radius = 0;
        Vector2 direction = new Vector2(-playerMovement.Orientation, 0);
        // play slash FX
        slash.GetComponent<Animation>().Play(slashStr);

        // assign damage and attackpoint
        switch(slashStr)
        {
            case "slash1":
                damage = attackDamage;
                point = new Vector3(0.3f, -0.3f, 0) + attackPoint.position;
                radius = slash1height;
                break;
            case "altslash1":
                damage = attackDamage;
                point = new Vector3(0.3f, -0.3f, 0) + attackPoint.position;
                radius = slash1height;
                break;
            case "slash2":
                damage = attackDamage * 1.5f;
                point = attackPoint.position;
                radius = slash2range;
                break;
        }

        // raycast to start point of attack
        RaycastHit2D[] hitCollider = Physics2D.CircleCastAll(point, radius, direction, distance, enemyLayer);
        foreach (RaycastHit2D target in hitCollider)
        {
            Enemy enemy = target.collider.GetComponent<Enemy>();
            if (enemy.GetComponent<Animator>().GetBool("PulledEffect"))
                damage *= 2;
            enemy.TakeDamage(damage);
            print("enemy hit");
        }
    }

    public void AttackMovement()
    {
        if (attacked > 1)
        {
            // add very fast movement for one frame
            rb.velocity += new Vector2(attackMove, 0);
            attacked--;
        }
        else if (attacked > 0)
        {
            // stop movement
            rb.velocity = new Vector2(0, rb.velocity.y);
            ExecuteAttack();
            attacked--;
        }
    }

    public void Transition()
    {
        //rb.position += new Vector2(0.0025f*playerMovement.Orientation, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(0.3f, -0.3f, 0) + attackPoint.position, slash1height);
        Gizmos.DrawWireSphere(attackPoint.position, slash2range);
    }
}