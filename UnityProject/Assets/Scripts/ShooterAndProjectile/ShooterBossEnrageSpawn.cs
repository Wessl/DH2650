using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterBossEnrageSpawn : MonoBehaviour
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
    private BoxCollider2D collider;
    public int shotsPerAttack;
    public float health;
    private Animator animator;
    private int positionIndex;
    private bool isBusy;
    public GameObject deathParticleSystem;
    public Sprite deathSprite;
    private SpriteRenderer spriteRenderer;
    private Material spriteMat, whiteMat;
    public float flashDuration;

    // Start is called before the first frame update
    void Start()
    {
        alive = true;
        isBusy = true;
        playerIsInRange = false;
       
        positionIndex = 0;
        timeSinceLastShot = 1;
        timePassedSinceLastFindAttempt = 0;
        
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponentInParent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        spriteMat = spriteRenderer.material;
        whiteMat = Resources.Load("WhiteMaterial", typeof(Material)) as Material;

        StartCoroutine(BossSpawnWaitPeriod());
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
            for (int i = -1; i < shotsPerAttack-1; i++)
            {
                var proj = Instantiate(projectile, projectileInstantiationPoint.position, Quaternion.identity);
                var projRB = proj.GetComponent<Rigidbody2D>(); 
                projRB.velocity = targ.normalized * projectileSpeed + new Vector3(i*5,Mathf.Abs(i)*2,0);
                projRB.rotation = proj.GetComponent<Rigidbody2D>().rotation;
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
            else if (health > 0)
            {
                animator.SetTrigger("pullback");
                isBusy = true;
                StartCoroutine(FlashSprite());
            }
        }
    }

    void Die()
    {
        animator.SetTrigger("death");
        playerIsInRange = false;
        alive = false;
        Instantiate(deathParticleSystem, transform.position, Quaternion.identity);
        spriteRenderer.sprite = deathSprite;
        //Destroy(gameObject);      
    }
    
    // Flash sprite white to indicate damage being taken
    IEnumerator FlashSprite()
    {
        spriteRenderer.material = whiteMat;
        //Color color = spriteMat.GetColor("_Color");
        //spriteMat.SetColor("_Color", new Color(1,0.55f,0.55f,0.88f));   // Arbitrary values
        yield return new WaitForSeconds(flashDuration);                                          // Arbitrary wait time, roughly four frames @60 fps
        // Reset to base value
        spriteRenderer.material = spriteMat;
        //spriteMat.SetColor("_Color", color);               // Alpha 0 = default sprite color
    }

    IEnumerator BossSpawnWaitPeriod()
    {
        yield return new WaitForSeconds(1f);
        isBusy = false;
    }

    public float Health
    {
        get => health;
        set => health = value;
    }

    public bool Alive
    {
        get => alive;
        set => alive = value;
    }
}
