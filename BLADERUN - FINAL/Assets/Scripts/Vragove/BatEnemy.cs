using System.Collections;
using UnityEngine;

public class BatEnemy : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float aggroRange = 4f;
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

    [Header("Bat Knockback")]
    [SerializeField] private float batKnockbackForce = 3f;
    [SerializeField] private float batKnockbackTime = 0.2f;

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

    private bool movingRight = false;
    private bool isKnockedBack;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        startPosition = transform.position;
        normalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        rb.gravityScale = 0f;
        movingRight = false;
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

        float directionX;
        float speed;

        if (chasing)
        {
            directionX = Mathf.Sign(player.position.x - transform.position.x);

            speed = chaseSpeed;
        }
        else
        {
            Transform targetPoint = movingRight ? rightPoint : leftPoint;

            directionX = Mathf.Sign(targetPoint.position.x - transform.position.x);

            speed = patrolSpeed;
        }

        if (rb.position.x <= left && directionX < 0f)
        {
            movingRight = true;
            directionX = 1f;
        }

        if (rb.position.x >= right && directionX > 0f)
        {
            movingRight = false;
            directionX = -1f;
        }

        rb.linearVelocity = new Vector2(
            directionX * speed,
            0f
        );

        if (Mathf.Abs(directionX) > 0.01f)
            spriteRenderer.flipX = directionX > 0f;
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

        StopCoroutine(nameof(BatKnockbackRoutine));
        StartCoroutine(BatKnockbackRoutine(hitDirection));
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

    private IEnumerator BatKnockbackRoutine(Vector2 hitDirection)
    {
        isKnockedBack = true;

        Vector2 direction = hitDirection.normalized;

        if (direction == Vector2.zero)
            direction = Vector2.right;

        rb.linearVelocity = direction * batKnockbackForce;

        yield return new WaitForSeconds(batKnockbackTime);

        isKnockedBack = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == leftPoint)
        {
            movingRight = true;
            return;
        }

        if (other.transform == rightPoint)
        {
            movingRight = false;
            return;
        }

        DamagePlayer(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DamagePlayer(other.gameObject);
    }

    private void DamagePlayer(GameObject playerObject)
    {
        if (isDead ||
            Time.time < nextContactTime ||
            !playerObject.CompareTag("Player"))
            return;

        nextContactTime = Time.time + contactCooldown;

        PlayerHealth health =
            playerObject.GetComponent<PlayerHealth>();

        PlayerMovement movement =
            playerObject.GetComponent<PlayerMovement>();

        if (health != null)
            health.TakeDamage(contactDamage);

        if (movement != null)
        {
            float direction =
                playerObject.transform.position.x < transform.position.x
                    ? -1f
                    : 1f;

            movement.ApplyKnockback(
                direction,
                playerKnockbackForce
            );
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

        spriteRenderer.color = normalColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}