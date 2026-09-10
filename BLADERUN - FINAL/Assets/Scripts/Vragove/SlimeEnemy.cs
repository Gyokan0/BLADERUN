using System.Collections;
using UnityEngine;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2.3f;
    [SerializeField] private float aggroRange = 3f;
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private Transform player;

    [Header("Health / Damage")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float contactCooldown = 0.7f;
    [SerializeField] private float deathDelay = 0.2f;

    [Header("Player Knockback")]
    [SerializeField] private float playerKnockbackForce = 4f;

    [Header("Slime Knockback")]
    [SerializeField] private float slimeKnockbackForce = 3f;
    [SerializeField] private float slimeKnockbackUpForce = 0.3f;
    [SerializeField] private float slimeKnockbackTime = 0.2f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new(1f, 0.2f, 0.2f);
    [SerializeField] private float hitFlashTime = 0.15f;

    [Header("Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip hitSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Color normalColor;

    private int currentHealth;
    private float nextContactTime;
    private bool movingRight;
    private bool isKnockedBack;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startPosition = transform.position;
        normalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        movingRight = false;
        spriteRenderer.flipX = false;
    }

    private void Start()
    {
        if (player != null)
            return;

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
            player = foundPlayer.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || isKnockedBack || leftPoint == null || rightPoint == null)
            return;

        float left = Mathf.Min(leftPoint.position.x, rightPoint.position.x);
        float right = Mathf.Max(leftPoint.position.x, rightPoint.position.x);

        bool chasing = player != null && Vector2.Distance(transform.position, player.position) <= aggroRange;

        float direction;

        if (chasing)
        {
            direction = Mathf.Sign(player.position.x - transform.position.x);
        }
        else
        {
            direction = movingRight ? 1f : -1f;

            if (transform.position.x <= left)
                movingRight = true;
            else if (transform.position.x >= right)
                movingRight = false;
        }

        float speed = chasing ? chaseSpeed : patrolSpeed;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        spriteRenderer.flipX = direction > 0f;

        rb.position = new Vector2(Mathf.Clamp(rb.position.x, left, right), rb.position.y);
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

        StopCoroutine(nameof(SlimeKnockbackRoutine));
        StartCoroutine(SlimeKnockbackRoutine(hitDirection));
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
        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(deathDelay);

        gameObject.SetActive(false);
    }

    private IEnumerator SlimeKnockbackRoutine(Vector2 hitDirection)
    {
        isKnockedBack = true;

        float direction = Mathf.Sign(hitDirection.x);

        if (direction == 0f)
            direction = 1f;

        rb.linearVelocity = new Vector2(direction * slimeKnockbackForce, slimeKnockbackUpForce);

        yield return new WaitForSeconds(slimeKnockbackTime);

        isKnockedBack = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        DamagePlayer(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        DamagePlayer(collision);
    }

    private void DamagePlayer(Collision2D collision)
    {
        if (isDead ||
            Time.time < nextContactTime ||
            !collision.gameObject.CompareTag("Player"))
            return;

        nextContactTime = Time.time + contactCooldown;

        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

        PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();

        if (health != null)
            health.TakeDamage(contactDamage);

        if (movement != null)
        {
            float direction = collision.transform.position.x < transform.position.x ? -1f : 1f;

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
        movingRight = false;
        isKnockedBack = false;
        isDead = false;

        spriteRenderer.flipX = false;
        spriteRenderer.color = normalColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}