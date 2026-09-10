using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [SerializeField] private int attackDamage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform player = transform.parent;

        if (player == null)
            return;

        Vector2 hitDirection = other.transform.position - player.position;

        SlimeEnemy slime = other.GetComponent<SlimeEnemy>();
        if (slime != null)
        {
            slime.TakeDamage(attackDamage, hitDirection.normalized);
            return;
        }

        BatEnemy bat = other.GetComponent<BatEnemy>();
        if (bat != null)
        {
            bat.TakeDamage(attackDamage, hitDirection.normalized);
            return;
        }

        WolfEnemy wolf = other.GetComponent<WolfEnemy>();
        if (wolf != null)
        {
            wolf.TakeDamage(attackDamage, hitDirection.normalized);
            return;
        }

        KingBoss king = other.GetComponent<KingBoss>();
        if (king != null)
        {
            king.TakeDamage(attackDamage, hitDirection.normalized);
        }
    }
}