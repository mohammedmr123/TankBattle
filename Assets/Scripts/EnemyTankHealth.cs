using UnityEngine;

public class EnemyTankHealth : MonoBehaviour
{
    public int maxHealth = 60;
    private int _currentHealth;

    [Header("Audio")]
    [Tooltip("Explosion sound played when this enemy is destroyed.")]
    public AudioClip deathSound;

    private EnemyTankAI _ai;

    private void Start()
    {
        _currentHealth = maxHealth;
        _ai = GetComponent<EnemyTankAI>();
    }

    public void takeDamage(int damage)
    {
        _currentHealth -= damage;

        if (_currentHealth <= 0)
        {

            if (deathSound != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(deathSound);

            if (_ai != null) _ai.enabled = false;
            Destroy(gameObject);
        }
    }
}
