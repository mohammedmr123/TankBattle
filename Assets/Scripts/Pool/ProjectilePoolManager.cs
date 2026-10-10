using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

    [Header("Player Projectile Pool")]
    public Rigidbody playerProjectilePrefab;
    [Range(1, 50)] public int playerInitialSize = 10;
    [Range(1, 100)] public int playerMaxSize = 30;

    [Header("Enemy Projectile Pool")]
    public Rigidbody enemyProjectilePrefab;
    [Range(1, 50)] public int enemyInitialSize = 15;
    [Range(1, 100)] public int enemyMaxSize = 50;

    [Header("Impact FX Pool")]
    public ParticleSystem impactFXPrefab;
    [Range(1, 50)] public int fxInitialSize = 10;
    [Range(1, 100)] public int fxMaxSize = 40;

    private ObjectPool<Rigidbody> _playerPool;
    private ObjectPool<Rigidbody> _enemyPool;
    private ObjectPool<ParticleSystem> _fxPool;

    private Transform _poolRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _poolRoot = new GameObject("[Pool Root]").transform;
        _poolRoot.SetParent(transform);

        if (playerProjectilePrefab != null)
            _playerPool = new ObjectPool<Rigidbody>(playerProjectilePrefab, playerInitialSize, playerMaxSize, _poolRoot);

        if (enemyProjectilePrefab != null)
            _enemyPool = new ObjectPool<Rigidbody>(enemyProjectilePrefab, enemyInitialSize, enemyMaxSize, _poolRoot);

        if (impactFXPrefab != null)
            _fxPool = new ObjectPool<ParticleSystem>(impactFXPrefab, fxInitialSize, fxMaxSize, _poolRoot);
    }

    public Rigidbody GetPlayerProjectile(Vector3 pos, Quaternion rot)
        => _playerPool?.Get(pos, rot);

    public Rigidbody GetEnemyProjectile(Vector3 pos, Quaternion rot)
        => _enemyPool?.Get(pos, rot);

    public void ReturnPlayerProjectile(Rigidbody rb)
        => _playerPool?.Return(rb);

    public void ReturnEnemyProjectile(Rigidbody rb)
        => _enemyPool?.Return(rb);

    public ParticleSystem GetImpactFX(Vector3 pos, Quaternion rot)
        => _fxPool?.Get(pos, rot);

    public void ReturnImpactFX(ParticleSystem ps)
        => _fxPool?.Return(ps);

    public bool IsEnemyProjectile(Rigidbody rb)
    {

        return rb != null && rb.CompareTag("EnemyProjectile");
    }
}

