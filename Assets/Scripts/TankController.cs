using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Tank_Inputs))]
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float tankSpeed = 15f;
    public float tankRotationSpeed = 20f;

    [Header("Turret")]
    public Transform turretTransform;
    [Range(1f, 20f)]
    public float turretLagSpeed = 6f;

    [Header("Reticle")]
    public Transform reticleTransform;

    [Header("Engine Audio")]
    [Tooltip("Looping engine/tread sound. Plays while driving, fades out when idle.")]
    public AudioClip engineLoopClip;
    [Range(0f, 1f)] public float engineVolume = 0.5f;
    [Tooltip("How fast the engine fades in/out (higher = snappier).")]
    public float engineFadeSpeed = 5f;

    private Rigidbody _rb;
    private Tank_Inputs _input;
    private Vector3 _finalTurretLookDir;

    private AudioSource _engineSource;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<Tank_Inputs>();

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
        Debug.Log($"[TankController] '{name}' physics check -> " +
                  $"Rigidbody: {(_rb != null)} | Collider: {(col != null ? col.GetType().Name : "MISSING")} | " +
                  $"ColliderEnabled: {(col != null && col.enabled)} | IsTrigger: {(col != null && col.isTrigger)} | " +
                  $"Gravity: {(_rb != null && _rb.useGravity)} | Detect: {(_rb != null ? _rb.collisionDetectionMode.ToString() : "-")}. " +
                  (col == null ? ">>> NO COLLIDER: the tank cannot hit walls. Add a BoxCollider." : "OK"));

        if (turretTransform != null)
            _finalTurretLookDir = turretTransform.forward;

        SetupEngineAudio();
    }

    private void SetupEngineAudio()
    {
        if (engineLoopClip == null) return;

        _engineSource = gameObject.AddComponent<AudioSource>();
        _engineSource.clip = engineLoopClip;
        _engineSource.loop = true;
        _engineSource.playOnAwake = false;
        _engineSource.volume = 0f;
        _engineSource.spatialBlend = 0f;

        if (AudioManager.Instance != null && AudioManager.Instance.SfxGroup != null)
            _engineSource.outputAudioMixerGroup = AudioManager.Instance.SfxGroup;

        _engineSource.Play();
    }

    private void FixedUpdate()
    {
        if (_rb == null || _input == null) return;

        HandleMovement();
        HandleTurret();
        HandleReticle();
        HandleEngineAudio();
    }

    private void HandleEngineAudio()
    {
        if (_engineSource == null) return;

        bool driving = Mathf.Abs(_input.ForwardInput) > 0.05f || Mathf.Abs(_input.RotationInput) > 0.05f;
        float targetVol = driving ? engineVolume : 0f;

        _engineSource.volume = Mathf.MoveTowards(_engineSource.volume, targetVol, engineFadeSpeed * Time.fixedDeltaTime);
    }

    protected virtual void HandleMovement()
    {

        float drive = _input.ForwardInput * tankSpeed;
        Vector3 desired = transform.forward * drive;

        Vector3 v = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(desired.x, v.y, desired.z);

        float turn = _input.RotationInput * tankRotationSpeed * Time.fixedDeltaTime;
        Quaternion turnRot = Quaternion.Euler(Vector3.up * turn);
        _rb.MoveRotation(_rb.rotation * turnRot);
    }

    protected virtual void HandleTurret()
    {
        if (turretTransform == null) return;

        Vector3 lookDir = _input.reticlePosition - turretTransform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude < 0.001f) return;

        _finalTurretLookDir = Vector3.Lerp(
            _finalTurretLookDir,
            lookDir.normalized,
            Time.fixedDeltaTime * turretLagSpeed);

        if (_finalTurretLookDir.sqrMagnitude > 0.001f)
            turretTransform.rotation = Quaternion.LookRotation(_finalTurretLookDir);
    }

    protected virtual void HandleReticle()
    {
        if (reticleTransform != null)
            reticleTransform.position = _input.reticlePosition;
    }
}
