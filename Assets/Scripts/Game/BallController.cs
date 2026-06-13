using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BallController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float lostY = -5.6f;
    [SerializeField] private float minimumVerticalSpeed = 1.5f;
    [SerializeField] private float paddleBounceMaxX = 0.85f;
    [SerializeField] private Vector2 paddleOffset = new Vector2(0f, 0.45f);
    [SerializeField] private Transform paddle;
    [SerializeField] private GameManager gameManager;

    private Rigidbody2D ballRigidbody;
    private bool waitingForLaunch = true;
    private bool lostReported;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody2D>();
        ballRigidbody.gravityScale = 0f;
        ballRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ballRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Start()
    {
        FindMissingReferences();
        ResetToPaddle();
    }

    private void Update()
    {
        if (waitingForLaunch)
        {
            FollowPaddle();

            if (Input.GetKeyDown(KeyCode.Space) && gameManager != null && gameManager.CanLaunchBall)
            {
                Launch();
            }

            return;
        }

        if (!lostReported && transform.position.y < lostY)
        {
            lostReported = true;
            ballRigidbody.velocity = Vector2.zero;

            if (gameManager != null)
            {
                gameManager.NotifyBallLost();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!waitingForLaunch && !lostReported)
        {
            NormalizeVelocity();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (waitingForLaunch || lostReported)
        {
            return;
        }

        Block block = collision.collider.GetComponent<Block>();
        if (block != null)
        {
            block.Break();
        }

        PaddleController paddleController = collision.collider.GetComponent<PaddleController>();
        if (paddleController != null)
        {
            ApplyPaddleBounce(paddleController.transform);
        }

        NormalizeVelocity();
    }

    public void ResetToPaddle()
    {
        waitingForLaunch = true;
        lostReported = false;
        ballRigidbody.simulated = true;
        ballRigidbody.velocity = Vector2.zero;
        ballRigidbody.angularVelocity = 0f;
        FollowPaddle();
    }

    public void StopBall()
    {
        waitingForLaunch = false;
        lostReported = true;
        ballRigidbody.velocity = Vector2.zero;
        ballRigidbody.angularVelocity = 0f;
    }

    public void Configure(Transform paddleTransform, GameManager manager)
    {
        paddle = paddleTransform;
        gameManager = manager;
    }

    private void Launch()
    {
        waitingForLaunch = false;
        lostReported = false;

        Vector2 launchDirection = new Vector2(0.35f, 1f).normalized;
        ballRigidbody.velocity = launchDirection * moveSpeed;

        if (gameManager != null)
        {
            gameManager.NotifyBallLaunched();
        }
    }

    private void FollowPaddle()
    {
        if (paddle == null)
        {
            return;
        }

        transform.position = (Vector2)paddle.position + paddleOffset;
    }

    private void ApplyPaddleBounce(Transform paddleTransform)
    {
        float halfWidth = 1f;
        Collider2D paddleCollider = paddleTransform.GetComponent<Collider2D>();

        if (paddleCollider != null)
        {
            halfWidth = Mathf.Max(0.01f, paddleCollider.bounds.extents.x);
        }

        float hitFactor = Mathf.Clamp((transform.position.x - paddleTransform.position.x) / halfWidth, -1f, 1f);
        Vector2 bounceDirection = new Vector2(hitFactor * paddleBounceMaxX, 1f).normalized;
        ballRigidbody.velocity = bounceDirection * moveSpeed;
    }

    private void NormalizeVelocity()
    {
        Vector2 velocity = ballRigidbody.velocity;

        if (velocity.sqrMagnitude <= 0.0001f)
        {
            velocity = Vector2.up;
        }

        float verticalSign = Mathf.Approximately(velocity.y, 0f) ? 1f : Mathf.Sign(velocity.y);
        if (Mathf.Abs(velocity.y) < minimumVerticalSpeed)
        {
            velocity.y = minimumVerticalSpeed * verticalSign;
        }

        ballRigidbody.velocity = velocity.normalized * moveSpeed;
    }

    private void FindMissingReferences()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }

        if (paddle == null)
        {
            PaddleController paddleController = FindObjectOfType<PaddleController>();
            if (paddleController != null)
            {
                paddle = paddleController.transform;
            }
        }
    }
}
