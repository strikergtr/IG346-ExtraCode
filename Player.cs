using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 movement;
    public float speed = 5f;
    public float jumpForce = 5f;
    public Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        rb.linearVelocity = new Vector3(movement.x , rb.linearVelocityY, 0f);
        GoogleAPI.instance.myplayer.position = transform.position;
    }
    public void OnMove(InputValue index) 
    {
        movement = index.Get<Vector2>() * speed;
    }
    public void OnJump(InputValue index) 
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin") 
        {
            GoogleAPI.instance.myplayer.score++;
            Destroy(collision.gameObject);
        }
    }
}
