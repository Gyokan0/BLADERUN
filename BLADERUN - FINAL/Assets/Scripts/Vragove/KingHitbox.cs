using UnityEngine;

public class KingHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();

        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (health != null)
            health.TakeDamage(damage);

        if (movement != null)
        {
            float kingX = transform.parent.position.x;
            float playerX = other.transform.position.x;

            float direction = playerX < kingX ? -1f : 1f;

            movement.ApplyKnockback(direction, knockbackForce);
        }
    }
}