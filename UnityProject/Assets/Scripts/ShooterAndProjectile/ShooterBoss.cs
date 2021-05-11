using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBoss : MonoBehaviour
{
    private Transform playerTransform;
    private float timeSinceLastShot;
    public float shootDelay;
    public GameObject projectile;
    public Transform projectileInstantiationPoint;
    public float projectileSpeed;
    public float damagePerShot;
    public float radius;
    private bool playerIsInRange;
    private float timePassedSinceLastFindAttempt;
    public LayerMask playerLayer;
    public float detectionDelay;
    private bool alive;
    private Rigidbody2D rb;
    private CapsuleCollider2D collider;
    public int shotsPerAttack;
    public float health;
    private Animator animator;
    public Transform[] existPositions;
    private int positionIndex;
    private bool isBusy;

    
    // Start is called before the first frame update
    void Start()
    {
        isBusy = false;
        positionIndex = 0;
        rb = GetComponent<Rigidbody2D>();
        timePassedSinceLastFindAttempt = 0;
        timeSinceLastShot = 1;
        playerIsInRange = false;
        collider = GetComponent<CapsuleCollider2D>();
        alive = true;
        animator = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timePassedSinceLastFindAttempt += Time.deltaTime;
        if (timePassedSinceLastFindAttempt > detectionDelay && alive && !isBusy)
        {
            FindPlayer();
            timePassedSinceLastFindAttempt = 0;
        }

        if (playerIsInRange && !isBusy && alive)
        {
            FaceAndAttack();
        }
        Debug.Log(transform.position);
    }
    
    private void FindPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, playerLayer);
        foreach (var hit in hits)
        {
            if (hit.transform.CompareTag("Player"))
            {
                playerTransform = hit.transform;
                playerIsInRange = true;
            }
        }

        if (hits.Length == 0)
        {
            playerIsInRange = false;
        }
    }
    
    void FaceAndAttack()
    {
        // Face player
        Vector3 targ = playerTransform.position;
        targ.z = 0f;
        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;
 
        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        
        // Attack player
        if (timeSinceLastShot > shootDelay)
        {
            for (int i = 0; i < shotsPerAttack; i++)
            {
                var proj = Instantiate(projectile, projectileInstantiationPoint.position, Quaternion.identity);
                proj.GetComponent<Rigidbody2D>().velocity = targ.normalized * projectileSpeed;
                proj.GetComponent<Rigidbody2D>().rotation = proj.GetComponent<Rigidbody2D>().rotation + i*5;
                proj.GetComponent<Projectile>().DamageToDeal = damagePerShot;
                //StartCoroutine(DestroyProjectile(proj));
                timeSinceLastShot = 0;
            }
            
        }
        else
        {
            timeSinceLastShot += Time.deltaTime;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    public void TakeDamage(float damage)
    {
        if (!isBusy)
        {
            Debug.Log("I've been hit!");
            playerIsInRange = false;
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
            animator.SetTrigger("pullback");
            isBusy = true;
            StartCoroutine(MovePositions());
        }
        
    }

    IEnumerator MovePositions()
    {
        Debug.Log("positions before moving: " + transform.position);
        yield return new WaitForSeconds(1f);
        positionIndex++;
        transform.parent.position = existPositions[positionIndex % (existPositions.Length)].position;
        Debug.Log("positions before moving: " + transform.position);
        animator.SetTrigger("moveout");
        isBusy = false;
    }
    
    void Die()
    {
        playerIsInRange = false;
        alive = false;
        //Instantiate(deathParticleSystem, body.transform.position, Quaternion.identity);
        //Destroy(body);      // Don't need to destroy the foundation of the shooter boye, only body
    }
}
