using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpPower = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new(0.18f, 0.08f);

    [Header("Attack")]
    [SerializeField] private GameObject swordHitbox;
    [SerializeField] private float attackStartDelay = 0.08f;
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private AudioSource swordAudioSource;
    [SerializeField] private AudioClip swordSwingSound;

    [Header("Knockback")]
    [SerializeField] private float knockbackTime = 0.2f;
    [SerializeField] private float knockbackUpForce = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;

    private bool isFacingRight = true;
    private bool isGrounded;
    private bool isAttacking;
    private bool isKnockedBack;

    private int jumpCount;
    private const int MaxJumps = 2;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (swordHitbox != null)
            swordHitbox.SetActive(false);
    }

    private void Update()
    {
        ReadMovementInput();
        CheckGround();
        HandleJump();
        HandleAttack();
        FlipPlayer();
    }

    private void FixedUpdate()
    {
        if (!isKnockedBack)
            rb.linearVelocity = new Vector2(
                horizontalInput * moveSpeed,
                rb.linearVelocity.y
            );

        if (animator == null)
            return;

        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        bool isInAir =
            !isGrounded &&
            Mathf.Abs(rb.linearVelocity.y) > 0.05f;

        animator.SetBool("isJumping", isInAir);
    }

    private void ReadMovementInput()
    {
        if (Keyboard.current == null || isKnockedBack)
        {
            horizontalInput = 0f;
            return;
        }

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            horizontalInput = -1f;
        }
        else if (Keyboard.current.dKey.isPressed ||
                 Keyboard.current.rightArrowKey.isPressed)
        {
            horizontalInput = 1f;
        }
        else
        {
            horizontalInput = 0f;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        ) != null;

        if (isGrounded && !wasGrounded)
            jumpCount = 0;
    }

    private void HandleJump()
    {
        if (Keyboard.current == null || isKnockedBack)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame &&
            jumpCount < MaxJumps)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpPower
            );

            jumpCount++;
        }
    }

    private void HandleAttack()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.fKey.wasPressedThisFrame &&
            isGrounded &&
            !isAttacking &&
            !isKnockedBack)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Attack");

        if (swordAudioSource != null && swordSwingSound != null)
            swordAudioSource.PlayOneShot(swordSwingSound);

        yield return new WaitForSeconds(attackStartDelay);

        if (swordHitbox != null)
            swordHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        if (swordHitbox != null)
            swordHitbox.SetActive(false);

        yield return new WaitForSeconds(
            Mathf.Max(0f, attackCooldown - attackDuration)
        );

        isAttacking = false;
    }

    public void ApplyKnockback(float direction, float force)
    {
        if (!isKnockedBack)
            StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(float direction, float force)
    {
        isKnockedBack = true;
        horizontalInput = 0f;

        rb.linearVelocity = new Vector2(
            direction * force,
            knockbackUpForce
        );

        yield return new WaitForSeconds(knockbackTime);

        isKnockedBack = false;
    }

    private void FlipPlayer()
    {
        if (isKnockedBack)
            return;

        if ((isFacingRight && horizontalInput < 0f) ||
            (!isFacingRight && horizontalInput > 0f))
        {
            isFacingRight = !isFacingRight;

            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        isAttacking = false;
        isKnockedBack = false;

        if (swordHitbox != null)
            swordHitbox.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireCube(
                groundCheck.position,
                groundCheckSize
            );
    }
}