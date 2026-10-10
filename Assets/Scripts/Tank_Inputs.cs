using UnityEngine;

public class Tank_Inputs : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera camera1;

    [Header("Aiming LayerMask")]
    [Tooltip("Assign the Ground layer here so the mouse raycast ignores tank colliders.")]
    public LayerMask groundLayerMask = (1 << 0);

    [Header("Gamepad Deadzone")]
    [Range(0.05f, 0.5f)]
    public float aimDeadzone = 0.19f;

    [Header("Aim Inversion")]
    [Tooltip("Invert the gamepad right-stick vertical aim axis (set by Options menu).")]
    public bool invertAimY = false;

    public Vector3 reticlePosition { get; private set; }
    public Vector3 ReticlePosition => reticlePosition;

    private Vector3 _reticleNormal;
    public Vector3 ReticleNormal => _reticleNormal;

    private float _forwardInput;
    public float ForwardInput => _forwardInput;

    private float _rotationInput;
    public float RotationInput => _rotationInput;

    private bool _firePressed;
    public bool FirePressed => _firePressed;

    private bool _fireHeld;
    public bool FireHeld => _fireHeld;

    private bool _usingGamepadAim;

    private Vector3 _gamepadAimWorldPos;

    private void Update()
    {
        if (camera1 != null)
            HandleInputs();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(reticlePosition, 0.5f);
    }

    protected virtual void HandleInputs()
    {

        _forwardInput = Input.GetAxis("Vertical");
        _rotationInput = Input.GetAxis("Horizontal");

        float aimH = 0f, aimV = 0f;
        try { aimH = Input.GetAxis("AimHorizontal"); } catch { }
        try { aimV = Input.GetAxis("AimVertical"); } catch { }

        Vector2 stickAim = new Vector2(aimH, aimV);
        _usingGamepadAim = stickAim.magnitude > aimDeadzone;

        if (_usingGamepadAim)
        {

            float aimZ = invertAimY ? -stickAim.y : stickAim.y;
            Vector3 aimDir = new Vector3(stickAim.x, 0f, aimZ).normalized;
            _gamepadAimWorldPos = transform.position + aimDir * 20f;
            reticlePosition = _gamepadAimWorldPos;
            _reticleNormal = Vector3.up;
        }
        else
        {

            Ray screenRay = camera1.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(screenRay, out RaycastHit hit, 200f, groundLayerMask))
            {
                reticlePosition = hit.point;
                _reticleNormal = hit.normal;
            }
        }

        bool mouseDown = Input.GetMouseButtonDown(0);
        bool mouseHeld = Input.GetMouseButton(0);

        bool gpBtnDown = false;
        bool gpBtnHeld = false;
        try { gpBtnDown = Input.GetButtonDown("FireGamepad"); } catch { }
        try { gpBtnHeld = Input.GetButton("FireGamepad"); } catch { }

        float triggerVal = 0f;
        try { triggerVal = Input.GetAxis("FireTrigger"); } catch { }
        bool triggerFire = triggerVal > 0.3f;
        bool triggerDown = triggerFire && !_prevTriggerFire;
        _prevTriggerFire = triggerFire;

        _firePressed = mouseDown || gpBtnDown || triggerDown;
        _fireHeld = mouseHeld || gpBtnHeld || triggerFire;
    }

    private bool _prevTriggerFire = false;

    public void SetAimConfig(float deadzone, bool invertY)
    {
        aimDeadzone = Mathf.Clamp(deadzone, 0.05f, 0.5f);
        invertAimY = invertY;
    }
}
