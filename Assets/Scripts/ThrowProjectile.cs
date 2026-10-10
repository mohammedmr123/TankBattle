using UnityEngine;

public class ThrowProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [Tooltip("Drag your Rigidbody-based projectile prefab here (also set in ProjectilePoolManager).")]
    public Rigidbody projectilePrefab;

    [Tooltip("Where the projectile spawns (turret barrel tip).")]
    public Transform throwPoint;

    [Tooltip("Initial speed applied to the projectile.")]
    public float launchForce = 15f;

    [Header("Fire Rate")]
    [Tooltip("Minimum time (seconds) between consecutive shots.")]
    public float timeBetweenShots = 0.5f;

    [Header("Camera Shake")]
    [Tooltip("Optional: assign a CinemachineImpulseSource component for camera shake on fire.")]
    public MonoBehaviour cinemachineImpulseSource;

    private float _nextTimeToFire = 0f;
    private Tank_Inputs _inputs;

    private void Start()
    {
        _inputs = GetComponent<Tank_Inputs>();
    }

    private void Update()
    {

        if (OptionsMenuUI.IsOpen) return;

        bool wantFire = false;

        if (_inputs != null)
        {
            wantFire = _inputs.FirePressed;
        }
        else
        {

            wantFire = Input.GetMouseButtonDown(0);
        }

        if (wantFire && Time.time >= _nextTimeToFire)
        {
            FireProjectile();
            _nextTimeToFire = Time.time + timeBetweenShots;
        }
    }

    private void FireProjectile()
    {
        if (throwPoint == null)
        {
            Debug.LogWarning("[ThrowProjectile] No throwPoint assigned.");
            return;
        }

        Rigidbody proj = null;

        if (ProjectilePoolManager.Instance != null)
        {
            proj = ProjectilePoolManager.Instance.GetPlayerProjectile(
                throwPoint.position, throwPoint.rotation);
        }
        else if (projectilePrefab != null)
        {

            proj = Instantiate(projectilePrefab, throwPoint.position, throwPoint.rotation);
        }

        if (proj == null) return;

        Projectile p = proj.GetComponent<Projectile>();
        if (p != null) p.isPlayerProjectile = true;

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
