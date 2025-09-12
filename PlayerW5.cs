using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    public Vector2 movement;
    public float speed = 5f;
    public float jumpForce = 5f;
    public Rigidbody2D rb;
    public Animator anim;

    [Networked]
    private NetworkRigidbody2D Nrb2d { get; set; }

    [Networked]
    public int state { get; set; }



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Spawned()
    {
        Nrb2d = GetComponent<NetworkRigidbody2D>();
    }

    void Update()
    {
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocityY, 0f);
        GoogleAPI.instance.myplayer.position = transform.position;

        anim.SetInteger("state", state);
    }

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
        if (HasStateAuthority) 
        {
            Vector2 velocity = rb.linearVelocity;
            if (Mathf.Abs(velocity.y) > 0.1f)
            {
                state = velocity.y > 0f ? 2 : 3;
            }
            else if (Mathf.Abs(velocity.x) > 0.1f)
            {
                state = 1;
            }
            else 
            {
                state = 0;
            }
            
        }
    }



    /* public void OnMove(InputValue index)
     {
         movement = index.Get<Vector2>() * speed;
     }
     public void OnJump(InputValue index)
     {
         rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
     }*/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            GoogleAPI.instance.myplayer.score++;
            Destroy(collision.gameObject);
        }
    }
}
