using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;

    [Header("Camera offset")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Follow smoothness")]
    [Range(0f, 1f)]
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        // Desired camera position (target + offset)
        Vector3 desiredPosition = target.position + offset;

        // Smooth transition between current and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply position
        transform.position = smoothedPosition;
    }
}