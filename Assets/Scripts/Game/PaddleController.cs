using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PaddleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float screenPadding = 0.25f;
    [SerializeField] private Camera targetCamera;

    private Rigidbody2D paddleRigidbody;
    private Collider2D paddleCollider;
    private SpriteRenderer spriteRenderer;
    private float moveInput;

    private void Awake()
    {
        paddleRigidbody = GetComponent<Rigidbody2D>();
        paddleCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (paddleRigidbody != null)
        {
            paddleRigidbody.bodyType = RigidbodyType2D.Kinematic;
            paddleRigidbody.gravityScale = 0f;
            paddleRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        moveInput = 0f;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            moveInput -= 1f;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            moveInput += 1f;
        }
    }

    private void FixedUpdate()
    {
        Vector2 currentPosition = paddleRigidbody != null
            ? paddleRigidbody.position
            : (Vector2)transform.position;

        float nextX = currentPosition.x + moveInput * moveSpeed * Time.fixedDeltaTime;
        nextX = Mathf.Clamp(nextX, GetMinX(), GetMaxX());

        Vector2 nextPosition = new Vector2(nextX, currentPosition.y);

        if (paddleRigidbody != null)
        {
            paddleRigidbody.MovePosition(nextPosition);
            return;
        }

        transform.position = nextPosition;
    }

    public void Configure(Camera camera, float speed)
    {
        targetCamera = camera;
        moveSpeed = speed;
    }

    private float GetMinX()
    {
        if (targetCamera == null)
        {
            return -8f;
        }

        return targetCamera.transform.position.x - targetCamera.orthographicSize * targetCamera.aspect + GetHalfWidth() + screenPadding;
    }

    private float GetMaxX()
    {
        if (targetCamera == null)
        {
            return 8f;
        }

        return targetCamera.transform.position.x + targetCamera.orthographicSize * targetCamera.aspect - GetHalfWidth() - screenPadding;
    }

    private float GetHalfWidth()
    {
        if (paddleCollider != null)
        {
            return paddleCollider.bounds.extents.x;
        }

        if (spriteRenderer != null)
        {
            return spriteRenderer.bounds.extents.x;
        }

        return transform.lossyScale.x * 0.5f;
    }
}
