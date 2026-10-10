using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 10f, 0f);

    [Header("Rotation Offset")]
    public float RotationX;
    public float RotationY;
    public float RotationZ;

    [Header("Optional Smoothing")]
    [Tooltip("If zero or negative, camera will snap directly. Higher = slower follow.")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;

        if (smoothSpeed > 0f)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                smoothSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = desiredPos;
        }

        transform.rotation = Quaternion.Euler(RotationX, RotationY, RotationZ);
    }
}
