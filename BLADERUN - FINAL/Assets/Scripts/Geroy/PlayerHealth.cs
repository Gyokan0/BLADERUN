using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 4;
    public int currentHealth;
    public float damageCooldown = 1f;

    [Header("UI")]
    public HealthUI healthUI;
    public GameOverUI gameOverUI;

    [Header("Hit Flash")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Color hitColor = new(1f, 0.2f, 0.2f);
    [SerializeField] private float hitFlashTime = 0.15f;
    [SerializeField] private float deathDelay = 0.2f;

    [Header("Hit Sound")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip hitSound;

    private Vector3 checkpointPosition;
    private Color normalColor;
    private float nextDamageTime;
    private bool isDead;

    private void Start()
    {
        if (playerSprite == null)
            playerSprite = GetComponent<SpriteRenderer>();

        if (playerSprite != null)
            normalColor = playerSprite.color;

        currentHealth = maxHealth;
        checkpointPosition = transform.position;

        if (healthUI != null)
            healthUI.UpdateHealth(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + damageCooldown;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (healthUI != null)
            healthUI.UpdateHealth(currentHealth);

        StopCoroutine(nameof(HitFlashRoutine));
        StartCoroutine(HitFlashRoutine());

        if (hitAudioSource != null && hitSound != null)
            hitAudioSource.PlayOneShot(hitSound);

        if (currentHealth <= 0)
            StartCoroutine(DeathRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        if (playerSprite == null)
            yield break;

        playerSprite.color = hitColor;

        yield return new WaitForSeconds(hitFlashTime);

        if (!isDead)
            playerSprite.color = normalColor;
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;

        if (playerSprite != null)
            playerSprite.color = hitColor;

        yield return new WaitForSeconds(deathDelay);

        if (gameOverUI != null)
            gameOverUI.ShowGameOver();
    }

    public void SetCheckpoint(Vector3 newPosition)
    {
        checkpointPosition = newPosition;
    }

    public void RespawnAtCheckpoint()
    {
        StopAllCoroutines();

        transform.position = checkpointPosition;

        currentHealth = maxHealth;
        nextDamageTime = 0f;
        isDead = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerSprite != null)
            playerSprite.color = normalColor;

        if (healthUI != null)
            healthUI.UpdateHealth(currentHealth);
    }
}