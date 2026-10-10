using System.Collections.Generic;
using UnityEngine;

public class EnemyTankAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Auto-assigned to the player tank on Start.")]
    public Transform playerTransform;

    [Header("Range Settings")]
    public float followRange = 30f;
    public float minDistance = 10f;

    [Header("Movement Settings")]
    public float tankSpeed = 8f;
    public float tankRotationSpeed = 120f;

    private Rigidbody _rb;

    [Header("Turret Settings")]
    public Transform turretTransform;
    public float turretLagSpeed = 6f;
    private Vector3 _finalTurretLookDir;

    [Header("Projectile Settings")]
    public Rigidbody projectilePrefab;
    public Transform throwPoint;
    public float launchForce = 15f;

    [Header("Fire Rate")]
    public float timeBetweenShots = 1.5f;
    private float _nextTimeToFire = 0f;

    [Header("Camera Shake")]
    [Tooltip("Optional: assign a CinemachineImpulseSource here for shake on enemy shots.")]
    public MonoBehaviour cinemachineImpulseSource;

    private void Start()
    {

        TankController playerCtrl = FindAnyObjectByType<TankController>();
        if (playerCtrl != null)
            playerTransform = playerCtrl.transform;

        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezePositionY
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationZ;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Collider col = GetComponent<Collider>();
        Debug.Log($"[EnemyTankAI] '{name}' physics check -> " +
                  $"Rigidbody: {(_rb != null)} | Collider: {(col != null ? col.GetType().Name : "MISSING")} | " +
                  $"Enabled: {(col != null && col.enabled)} | IsTrigger: {(col != null && col.isTrigger)}. " +
                  (col == null ? ">>> NO COLLIDER: add a BoxCollider." : "OK"));

        if (turretTransform != null)
            _finalTurretLookDir = turretTransform.forward;

        LevelManager.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        LevelManager.UnregisterEnemy(this);
    }

    private void FixedUpdate()
    {
        if (_rb == null || playerTransform == null) return;

        HandleMovement();
        HandleTurret();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (distance > followRange) return;

        if (distance > minDistance)
        {

            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
                Quaternion newRot = Quaternion.RotateTowards(
                    transform.rotation, targetRot,
                    tankRotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newRot);
            }

            Vector3 v = _rb.linearVelocity;
            Vector3 desired = transform.forward * tankSpeed;
            _rb.linearVelocity = new Vector3(desired.x, v.y, desired.z);
        }
        else
        {

            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
                Quaternion newRot = Quaternion.RotateTowards(
                    transform.rotation, targetRot,
                    tankRotationSpeed * Time.fixedDeltaTime);
                _rb.MoveRotation(newRot);
            }

            Vector3 stop = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(0f, stop.y, 0f);
        }
    }

    private void HandleTurret()
    {
        if (turretTransform == null) return;

        Vector3 turretDir = playerTransform.position - turretTransform.position;
        turretDir.y = 0f;
        if (turretDir.sqrMagnitude < 0.001f) return;

        _finalTurretLookDir = Vector3.Lerp(
            _finalTurretLookDir,
            turretDir.normalized,
            Time.fixedDeltaTime * turretLagSpeed);

        if (_finalTurretLookDir.sqrMagnitude > 0.001f)
            turretTransform.rotation = Quaternion.LookRotation(_finalTurretLookDir);
    }

    private void HandleShooting()
    {
        if (throwPoint == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        if (distance <= minDistance && Time.time >= _nextTimeToFire)
        {
            FireProjectile();
            _nextTimeToFire = Time.time + timeBetweenShots;
        }
    }

    private void FireProjectile()
    {
        Rigidbody proj = null;

        if (ProjectilePoolManager.Instance != null)
        {
            proj = ProjectilePoolManager.Instance.GetEnemyProjectile(
                throwPoint.position, throwPoint.rotation);
        }
        else if (projectilePrefab != null)
        {
            proj = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);
        }

        if (proj == null) return;

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.isPlayerProjectile = false;

        proj.linearVelocity = throwPoint.forward * launchForce;

        TriggerCameraShake();
    }

    private void TriggerCameraShake()
    {

        if (!SettingsManager.CameraShakeEnabled) return;
        if (cinemachineImpulseSource == null) return;
        var method = cinemachineImpulseSource.GetType().GetMethod("GenerateImpulse",
            System.Type.EmptyTypes);
        method?.Invoke(cinemachineImpulseSource, null);
    }
}
