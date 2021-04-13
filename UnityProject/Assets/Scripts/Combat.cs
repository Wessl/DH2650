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

    public void Attack()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            attacking = true;
            canAttack = false;
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

    public void PrintAnimator(Animator anim)
    {
        print(anim.name);
    }

    public void Attack(string name, float moveAmount)
    {
        slash.GetComponent<Animation>().Play(name);
        int direction = playerMovement.Orientation;
        float movement = playerMovement.Orientation * moveAmount;
        if (name.Equals("slash1") || name.Equals("altslash1"))
        {
            RaycastHit2D[] hitCollider = Physics2D.CircleCastAll(new Vector3(0.1f, -0.35f, 0) + attackPoint.position, slash1height,new Vector2(direction, 0), moveAmount, enemyLayer);
            foreach (RaycastHit2D enemy in hitCollider)
            {
                enemy.collider.GetComponent<Enemy>().TakeDamage(attackDamage);
                print("enemy hit");
            }
        }
        else if (name.Equals("slash2"))
        {
            Collider2D[] hitCollider = Physics2D.OverlapCircleAll(attackPoint.position, slash2range, enemyLayer);
            foreach (Collider2D enemy in hitCollider)
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage*1.5f);
            }
        }
        rb.position += new Vector2(movement,0);
    }

    public void Transition()
    {
        //rb.position += new Vector2(0.0025f*playerMovement.Orientation, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(new Vector3(0.1f, -0.35f, 0) + attackPoint.position, slash1height);
        Gizmos.DrawWireSphere(attackPoint.position, slash2range);
    }
}