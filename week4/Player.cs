using Fusion;
using Fusion.Addons.Physics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public Vector2 movement;
    public float speed = 5f;
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    [Networked]
    private NetworkRigidbody2D Nrb2d { get; set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public override void Spawned()
    {
        // Get the NetworkRigidbody2D component from this object.
        // It's a networked version of Unity's Rigidbody2D.
        Nrb2d = GetComponent<NetworkRigidbody2D>();

        // Log a message to the console to confirm the object has been spawned.
        Debug.Log($"Player object spawned. Has Input Authority: {HasInputAuthority}");
    }
    void Update()
    {
        rb.linearVelocity = new Vector3(movement.x , rb.linearVelocityY, 0f);
        GoogleAPI.instance.myplayer.position = transform.position;
    }
    //public void OnMove(InputValue index) 
    //{
    //    movement = index.Get<Vector2>() * speed;
    //}
    //public void OnJump(InputValue index) 
    //{
    //   rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    //}

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            // แค่ Host/Client ที่มี InputAuthority เท่านั้นที่จะคุมตัวนี้
            Vector2 velocity = rb.linearVelocity;
            velocity.x = input.horizontal * speed;

            if (input.jump && Mathf.Abs(rb.linearVelocityY) < 0.01f)
                velocity.y = jumpForce;

            rb.linearVelocity = velocity;
        }
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
