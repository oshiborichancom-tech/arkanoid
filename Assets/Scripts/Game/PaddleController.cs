using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PaddleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float screenPadding = 0.25f;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool usePlayAreaBounds;
    [SerializeField] private Vector2 playAreaXLimits = new Vector2(-8f, 8f);
    [SerializeField] private bool logMovementBounds = true;

    private Rigidbody2D paddleRigidbody;
    private Collider2D paddleCollider;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseLocalScale;
    private Coroutine expandCoroutine;
    private float moveInput;
    private bool baseScaleCaptured;

    private void Awake()
    {
        CaptureBaseScaleIfNeeded();

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

    private void OnDisable()
    {
        if (expandCoroutine != null)
        {
            StopCoroutine(expandCoroutine);
            expandCoroutine = null;
        }

        RestoreBaseScale();
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
        nextX = ClampXToScreen(nextX);

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

    public void ConfigurePlayArea(float minX, float maxX)
    {
        playAreaXLimits = new Vector2(Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
        usePlayAreaBounds = true;
        ClampCurrentPositionToScreen();
        LogMovementBounds("configured");
    }

    public void ApplyTemporaryExpand(float scaleMultiplier, float duration)
    {
        CaptureBaseScaleIfNeeded();

        float safeMultiplier = Mathf.Max(1f, scaleMultiplier);
        float safeDuration = Mathf.Max(0f, duration);

        if (expandCoroutine != null)
        {
            StopCoroutine(expandCoroutine);
            expandCoroutine = null;
        }

        Vector3 expandedScale = baseLocalScale;
        expandedScale.x = baseLocalScale.x * safeMultiplier;
        transform.localScale = expandedScale;
        ClampCurrentPositionToScreen();
        LogMovementBounds("expanded");

        if (safeDuration <= 0f)
        {
            RestoreBaseScale();
            return;
        }

        expandCoroutine = StartCoroutine(RestoreScaleAfterDelay(safeDuration));
    }

    private IEnumerator RestoreScaleAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        RestoreBaseScale();
        expandCoroutine = null;
    }

    private void CaptureBaseScaleIfNeeded()
    {
        if (baseScaleCaptured)
        {
            return;
        }

        baseLocalScale = transform.localScale;
        baseScaleCaptured = true;
    }

    private void RestoreBaseScale()
    {
        if (!baseScaleCaptured)
        {
            return;
        }

        transform.localScale = baseLocalScale;
        ClampCurrentPositionToScreen();
        LogMovementBounds("restored");
    }

    private void ClampCurrentPositionToScreen()
    {
        Vector2 currentPosition = paddleRigidbody != null
            ? paddleRigidbody.position
            : (Vector2)transform.position;

        Vector2 clampedPosition = new Vector2(ClampXToScreen(currentPosition.x), currentPosition.y);

        if (paddleRigidbody != null)
        {
            paddleRigidbody.position = clampedPosition;
            return;
        }

        transform.position = clampedPosition;
    }

    private float ClampXToScreen(float x)
    {
        float minX = GetMinX();
        float maxX = GetMaxX();

        if (minX > maxX)
        {
            return (minX + maxX) * 0.5f;
        }

        return Mathf.Clamp(x, minX, maxX);
    }

    private float GetMinX()
    {
        if (usePlayAreaBounds)
        {
            return playAreaXLimits.x + GetHalfWidth();
        }

        if (targetCamera == null)
        {
            return -8f;
        }

        return targetCamera.transform.position.x - targetCamera.orthographicSize * targetCamera.aspect + GetHalfWidth() + screenPadding;
    }

    private float GetMaxX()
    {
        if (usePlayAreaBounds)
        {
            return playAreaXLimits.y - GetHalfWidth();
        }

        if (targetCamera == null)
        {
            return 8f;
        }

        return targetCamera.transform.position.x + targetCamera.orthographicSize * targetCamera.aspect - GetHalfWidth() - screenPadding;
    }

    private float GetHalfWidth()
    {
        float halfWidth = 0f;

        if (paddleCollider != null)
        {
            halfWidth = Mathf.Max(halfWidth, paddleCollider.bounds.extents.x);
        }

        if (spriteRenderer != null)
        {
            halfWidth = Mathf.Max(halfWidth, spriteRenderer.bounds.extents.x);
        }

        return halfWidth > 0f ? halfWidth : Mathf.Abs(transform.lossyScale.x) * 0.5f;
    }

    private void LogMovementBounds(string context)
    {
        if (!logMovementBounds || !usePlayAreaBounds)
        {
            return;
        }

        float halfWidth = GetHalfWidth();
        float minCenterX = playAreaXLimits.x + halfWidth;
        float maxCenterX = playAreaXLimits.y - halfWidth;
        Debug.Log(
            $"Paddle movement bounds ({context}): playArea=[{playAreaXLimits.x:F3}, {playAreaXLimits.y:F3}], " +
            $"halfWidth={halfWidth:F3}, centerX=[{minCenterX:F3}, {maxCenterX:F3}]");
    }
}
