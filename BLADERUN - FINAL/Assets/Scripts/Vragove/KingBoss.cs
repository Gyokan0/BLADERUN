using System.Collections;
using UnityEngine;

public class KingBoss : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.8f;
    [SerializeField] private float aggroRange = 6f;
    [SerializeField] private float attackRange = 1.4f;
    [SerializeField] private Transform player;

    [Header("Attack")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private float attackStartDelay = 0.08f;
    [SerializeField] private float attackDuration = 0.45f;
    [SerializeField] private float attackCooldown = 1.4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float deathDelay = 0.3f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackTime = 0.2f;
    [SerializeField] private float knockbackCooldown = 0.8f;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new(1f, 0.2f, 0.2f);
    [SerializeField] private float hitFlashTime = 0.15f;

    [Header("Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip hitSound;

    [Header("Victory Sound")]
    [SerializeField] private AudioSource victoryAudioSource;
    [SerializeField] private AudioClip victorySound;

    [Header("Victory")]
    [SerializeField] private VictoryUI victoryUI;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Collider2D bodyCollider;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Color normalColor;

    private int currentHealth;

    private bool isFacingRight = false;
    private bool isAttacking;
    private bool isKnockedBack;
    private bool isDead;

    private float nextAttackTime;
    private float nextKnockbackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();

        startPosition = transform.position;
        startScale = transform.localScale;
        normalColor = spriteRenderer.color;

        currentHealth = maxHealth;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
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
        if (isDead || isKnockedBack || isAttacking || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        FlipKing(direction);

        if (distance > aggroRange)
        {
            StopMoving();
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();

            if (Time.time >= nextAttackTime)
                StartCoroutine(AttackRoutine());

            return;
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (animator != null)
            animator.SetBool("isMoving", true);
    }

    private void FlipKing(float direction)
    {
        if (direction == 0f)
            return;

        bool wantsToFaceRight = direction > 0f;

        if (wantsToFaceRight == isFacingRight)
            return;

        isFacingRight = wantsToFaceRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        StopMoving();

        if (player != null)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);

            FlipKing(direction);
        }

        if (animator != null)
            animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackStartDelay);

        if (attackHitbox != null)
            attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        isAttacking = false;
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

        if (Time.time >= nextKnockbackTime)
        {
            nextKnockbackTime = Time.time + knockbackCooldown;

            StopCoroutine(nameof(KnockbackRoutine));
            StartCoroutine(KnockbackRoutine(hitDirection));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 hitDirection)
    {
        isKnockedBack = true;

        float direction = Mathf.Sign(hitDirection.x);

        if (direction == 0f)
            direction = 1f;

        rb.linearVelocity = new Vector2(direction * knockbackForce, rb.linearVelocity.y);

        yield return new WaitForSeconds(knockbackTime);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        isKnockedBack = false;
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
        isAttacking = false;
        isKnockedBack = true;

        rb.linearVelocity = Vector2.zero;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (animator != null)
            animator.SetBool("isMoving", false);

        spriteRenderer.color = hitColor;

        yield return new WaitForSeconds(deathDelay);

        spriteRenderer.enabled = false;

        if (bodyCollider != null)
            bodyCollider.enabled = false;

        if (victoryAudioSource != null && victorySound != null)
        {
            victoryAudioSource.PlayOneShot(victorySound);

            yield return new WaitForSeconds(Mathf.Max(0f, victorySound.length - 1f));
        }

        if (victoryUI != null)
            victoryUI.ShowEndScreen();
    }

    public void ResetEnemy()
    {
        StopAllCoroutines();

        gameObject.SetActive(true);

        transform.position = startPosition;
        transform.localScale = startScale;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        currentHealth = maxHealth;
        nextAttackTime = 0f;
        nextKnockbackTime = 0f;

        isFacingRight = false;
        isDead = false;
        isAttacking = false;
        isKnockedBack = false;

        spriteRenderer.enabled = true;
        spriteRenderer.color = normalColor;

        if (bodyCollider != null)
            bodyCollider.enabled = true;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.SetBool("isMoving", false);
            animator.ResetTrigger("Attack");
            animator.Play("KingIdle", 0, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}