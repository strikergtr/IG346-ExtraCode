using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;
    void Update()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }
}
