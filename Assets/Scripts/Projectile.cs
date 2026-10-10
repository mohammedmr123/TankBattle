using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Ownership")]
    [Tooltip("Set to true for player-fired projectiles, false for enemy-fired.")]
    public bool isPlayerProjectile = true;

    [Header("Damage")]
    public int damageAmount = 20;

    [Header("Lifetime")]
    [Tooltip("Auto-return to pool after this many seconds if no collision occurs.")]
    public float lifetime = 5f;

    private float _spawnTime;
    private Rigidbody _rb;
    private bool _hasHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _spawnTime = Time.time;
        _hasHit = false;

        LevelManager.ActiveProjectileCount++;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnDisable()
    {

        LevelManager.ActiveProjectileCount = Mathf.Max(0, LevelManager.ActiveProjectileCount - 1);
    }

    private void Update()
    {
        if (!_hasHit && Time.time - _spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        _hasHit = true;

        if (ProjectilePoolManager.Instance != null)
        {
            ParticleSystem fx = ProjectilePoolManager.Instance.GetImpactFX(
                transform.position, Quaternion.identity);
            if (fx != null)
            {
                fx.Play();

                AutoReturnFX autoReturn = fx.GetComponent<AutoReturnFX>();
                if (autoReturn == null) autoReturn = fx.gameObject.AddComponent<AutoReturnFX>();
                autoReturn.duration = fx.main.duration;
            }
        }

        if (isPlayerProjectile)
        {
            if (collision.gameObject.CompareTag("EnemyTank"))
            {
                EnemyTankHealth eh = collision.gameObject.GetComponent<EnemyTankHealth>();
                if (eh != null) eh.takeDamage(damageAmount);
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("PlayerTank"))
            {
                TankHealth th = collision.gameObject.GetComponent<TankHealth>();
                if (th != null) th.takeDamage(damageAmount);
            }
        }

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        _hasHit = true;
        if (ProjectilePoolManager.Instance != null)
        {
            if (isPlayerProjectile)
                ProjectilePoolManager.Instance.ReturnPlayerProjectile(_rb);
            else
                ProjectilePoolManager.Instance.ReturnEnemyProjectile(_rb);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
