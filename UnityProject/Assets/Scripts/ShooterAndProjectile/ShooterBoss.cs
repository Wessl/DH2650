using System;
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
    private BoxCollider2D collider;
    public int shotsPerAttack;
    public float health;
    private Animator animator;
    public Transform[] existPositions;
    private int positionIndex;
    private bool isBusy;
    public GameObject deathParticleSystem;
    public Sprite deathSprite;
    private SpriteRenderer spriteRenderer;
    private Material spriteMat, whiteMat;
    public float flashDuration;
    public GameObject shooterBossPrefab;
    public float enrageSpawnHealth = 20f;
    public bool canCauseEnrage;
    

    // Start is called before the first frame update
    void Start()
    {
        alive = true;
        isBusy = false;
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
            else if (health > 0 && canCauseEnrage)
            {
                animator.SetTrigger("pullback");
                isBusy = true;
                StartCoroutine(FlashSprite());
                StartCoroutine(MovePositions());
            }

            if (health <= enrageSpawnHealth && canCauseEnrage)
            {
                Enrage();
            }
            
        }
        
    }

    IEnumerator MovePositions()
    {
        collider.isTrigger = true;
        Debug.Log("positions before moving: " + transform.position);
        yield return new WaitForSeconds(1f);
        positionIndex++;
        transform.parent.position = existPositions[positionIndex % (existPositions.Length)].position;
        Debug.Log(transform.parent.position);
        Debug.Log("positions before moving: " + transform.position);
        animator.SetTrigger("moveout");
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
        collider.isTrigger = false;
    }
    
    void Die()
    {
        animator.SetTrigger("death");
        playerIsInRange = false;
        alive = false;
        collider.isTrigger = true;
        //rb.gravityScale = 1f;
        rb.bodyType = RigidbodyType2D.Static;
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

    private void Enrage()
    {
        // Enrage phase will cause three shooterboyes to appear
        for (int i = 1; i < existPositions.Length; i++)
        {
            var otherPosition = (positionIndex + i + 1) % existPositions.Length;    // yes it's supposed to be +1
            var sb = Instantiate(shooterBossPrefab, existPositions[otherPosition].position + new Vector3(0,2,0), Quaternion.identity);
        }
        // Also make sure this guy can't cause enrage more than once
        canCauseEnrage = false;
    }

    public float Health
    {
        get => health;
        set => health = value;
    }
}
