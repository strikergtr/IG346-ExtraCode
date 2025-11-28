using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;
    [Networked]
    public bool attack { get; set; }
    public float moveSpeed = 4f;
    public float chaseRange = 10f;
    public float leashRange = 15f;

    private bool _isSpawned = false;

    private NetworkRigidbody2D _nrb;
    private Vector2 _startPosition;
    private Transform _target;
    private float _searchTimer;
    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        _isSpawned = true;
        if (HasStateAuthority)
        {
            _startPosition = transform.position;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _nrb = GetComponent<NetworkRigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        _searchTimer += Runner.DeltaTime;
        if (_searchTimer > 0.5f)
        {
            _searchTimer = 0;
            FindTarget();
        }

        // 3. execute Movement Logic
        ProcessMovement();
    }
    private void FindTarget()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position, chaseRange);
        Transform closest = null;
        float closeDist = Mathf.Infinity;

        foreach (var hit in colliders)
        {
            if (hit.CompareTag("Player"))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < closeDist)
                {
                    closeDist = d;
                    closest = hit.transform;
                }
            }
        }
        _target = closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Vector2 center = Application.isPlaying ? _startPosition : (Vector2)transform.position;
        Gizmos.DrawWireSphere(center, leashRange);
    }

    private void ProcessMovement()
    {
        Vector2 destination = _startPosition;


        float distToStart = Vector2.Distance(transform.position, _startPosition);
        float distToTarget = _target != null ? Vector2.Distance(transform.position, _target.position) : Mathf.Infinity;

        bool isLeashing = distToStart > leashRange;
        bool isChasing = _target != null && distToTarget < chaseRange && !isLeashing;

        if (isChasing)
        {
            destination = _target.position;
        }

        MoveRigidbody(destination, isChasing || isLeashing);
    }

    private void MoveRigidbody(Vector2 destination, bool isMoving)
    {
        if (!isMoving && Vector2.Distance(transform.position, _startPosition) < 0.5f)
        {
            _nrb.Rigidbody.linearVelocity = new Vector2(0, _nrb.Rigidbody.linearVelocity.y);
            return;
        }
        Vector2 dir = (destination - (Vector2)transform.position).normalized;
        float moveX = 0;

        if (destination.x > transform.position.x + 0.1f) moveX = moveSpeed;
        else if (destination.x < transform.position.x - 0.1f) moveX = -moveSpeed;
        _nrb.Rigidbody.linearVelocity = new Vector2(moveX, _nrb.Rigidbody.linearVelocity.y);
        if (moveX != 0)
        {
            float facing = Mathf.Sign(moveX);
            transform.localScale = new Vector3(facing, 1, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        attack = true;
        if (other.gameObject.tag == "Player")
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                player.GetComponent<NetworkRigidbody2D>().Teleport(player.origin);
            }
        }
    }

    public

    // Update is called once per frame
    void Update()
    {

        if (_isSpawned == true && attack) 
        {
            anim.SetTrigger("attack");
            if (HasStateAuthority) 
            {
                attack = false;
            }
        }
    }
}
