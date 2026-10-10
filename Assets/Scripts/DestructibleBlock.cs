using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    [Header("Health")]
    public int health = 30;

    [Header("FX")]
    [Tooltip("Optional particle effect on destruction.")]
    public ParticleSystem destructionFX;

    [Header("Audio")]
    [Tooltip("Sound when the block is hit but not yet destroyed.")]
    public AudioClip hitSound;
    [Tooltip("Explosion sound when the block is destroyed.")]
    public AudioClip destroySound;

    [Header("Tag Filter")]
    [Tooltip("Only projectiles with these tags deal damage. Leave empty = all.")]
    public string[] damageSourceTags = { "PlayerTank", "EnemyTank" };

    private void OnCollisionEnter(Collision collision)
    {
        Projectile proj = collision.gameObject.GetComponent<Projectile>();
        if (proj == null) return;

        if (damageSourceTags != null && damageSourceTags.Length > 0)
        {
            bool allowed = false;
            foreach (string t in damageSourceTags)
            {
                if (string.IsNullOrEmpty(t)) continue;

                if (t == "PlayerTank" && proj.isPlayerProjectile) { allowed = true; break; }
                if (t == "EnemyTank"  && !proj.isPlayerProjectile) { allowed = true; break; }
            }
            if (!allowed) return;
        }

        TakeDamage(proj.damageAmount);
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            DestroyBlock();
        }
        else if (hitSound != null && AudioManager.Instance != null)
        {

            AudioManager.Instance.PlaySfx(hitSound);
        }
    }

    private void DestroyBlock()
    {
        if (destroySound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(destroySound);

        if (destructionFX != null)
        {
            ParticleSystem fx = Instantiate(destructionFX, transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + 1f);
        }

        Destroy(gameObject);
    }
}

