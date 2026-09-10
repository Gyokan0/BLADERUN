using System.Collections;
using UnityEngine;

public class WolfEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float aggroRange = 3.5f;
    [SerializeField] private Transform player;

    [Header("Health / Damage")]
    [SerializeField] private int maxHealth = 4;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactCooldown = 0.7f;
    [SerializeField] private float deathDelay = 0.2f;

    [Header("Player Knockback")]
    [SerializeField] private float playerKnockbackForce = 4f;

    [Header("Wolf Knockback")]
    [SerializeField] private float wolfKnockbackForce = 3f;
    [SerializeField] private float wolfKnockbackTime = 0.2f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new(1f, 0.2f, 0.2f);
    [SerializeField] private float hitFlashTime = 0.15f;

    [Header("Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private Vector3 startPosition;
    private Color normalColor;

    private int currentHealth;
    private float nextContactTime;

    private bool isKnockedBack;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        startPosition = transform.position;
        normalColor = spriteRenderer.color;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player != null)
        {
            bool playerInRange = Vector2.Distance(transform.position, player.position) <= aggroRange;

            if (animator != null)
                animator.SetBool("isMoving", playerInRange);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || isKnockedBack)
            return;

        bool chasing = player != null && Vector2.Distance(transform.position, player.position) <= aggroRange;

        if (!chasing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (animator != null)
                animator.SetBool("isMoving", false);

            return;
        }

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

        spriteRenderer.flipX = direction > 0f;

        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        StopCoroutine(nameof(HitFlashRoutine));
        StartCoroutine(HitFlashRoutine());

        if (hitAudioSource != null && hitSound != null)
            hitAudioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
        {
            StartCoroutine(DeathRoutine());
            return;
        }

        StopCoroutine(nameof(WolfKnockbackRoutine));
        StartCoroutine(WolfKnockbackRoutine(hitDirection));
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(hitFlashTime);

        if (!isDead)
            spriteRenderer.color = normalColor;
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetBool("isMoving", false);

        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(deathDelay);

        gameObject.SetActive(false);
    }

    private IEnumerator WolfKnockbackRoutine(Vector2 hitDirection)
    {
        isKnockedBack = true;

        if (animator != null)
            animator.SetBool("isMoving", false);

        float direction = Mathf.Sign(hitDirection.x);

        if (direction == 0f)
            direction = 1f;

        rb.linearVelocity = new Vector2(direction * wolfKnockbackForce, 0f);

        yield return new WaitForSeconds(wolfKnockbackTime);

        isKnockedBack = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DamagePlayer(collision.gameObject);
    }

    private void DamagePlayer(GameObject playerObject)
    {
        if (isDead ||
            Time.time < nextContactTime ||
            !playerObject.CompareTag("Player"))
            return;

        nextContactTime = Time.time + contactCooldown;

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();

        PlayerMovement movement = playerObject.GetComponent<PlayerMovement>();

        if (health != null)
            health.TakeDamage(contactDamage);

        if (movement != null)
        {
            float direction = playerObject.transform.position.x < transform.position.x ? -1f : 1f;

            movement.ApplyKnockback(direction, playerKnockbackForce);
        }
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();

        gameObject.SetActive(true);
        transform.position = startPosition;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        currentHealth = maxHealth;
        nextContactTime = 0f;

        isKnockedBack = false;
        isDead = false;

        spriteRenderer.color = normalColor;

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}