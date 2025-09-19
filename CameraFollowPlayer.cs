using Unity.VisualScripting;
using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform target;
    public float speed = 20f;
    void Update()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, new Vector3(target.position.x, target.position.y, -10), Time.deltaTime * speed);
    }
}
