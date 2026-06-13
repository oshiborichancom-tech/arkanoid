using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ItemController : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.PaddleExpand;
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float destroyY = -5.8f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static Sprite fallbackSprite;

    private Rigidbody2D itemRigidbody;
    private Collider2D itemCollider;
    private bool collected;

    private void Awake()
    {
        itemRigidbody = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ConfigurePhysics();
        EnsureVisual();
        ApplyTypeVisual();
    }

    private void Start()
    {
        ResolveItemEffectManager();
    }

    private void FixedUpdate()
    {
        if (itemRigidbody != null)
        {
            itemRigidbody.velocity = Vector2.down * Mathf.Max(0f, fallSpeed);
        }
    }

    private void Update()
    {
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectFrom(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollectFrom(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryCollectFrom(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryCollectFrom(collision.collider);
    }

    public void Initialize(ItemType type, ItemEffectManager manager)
    {
        itemType = type;
        itemEffectManager = manager;
        ApplyTypeVisual();
    }

    private void TryCollectFrom(Collider2D other)
    {
        if (collected || other == null || other.GetComponentInParent<PaddleController>() == null)
        {
            return;
        }

        Debug.Log($"Item touched paddle: {itemType}");
        Collect();
    }

    private void Collect()
    {
        collected = true;
        ResolveItemEffectManager();

        if (itemEffectManager != null)
        {
            itemEffectManager.ApplyItemEffect(itemType);
        }
        else
        {
            Debug.LogWarning("ItemEffectManager not found.");
            Debug.Log($"Item acquired: {itemType}");
        }

        Destroy(gameObject);
    }

    private void ResolveItemEffectManager()
    {
        if (itemEffectManager != null)
        {
            return;
        }

        itemEffectManager = ItemEffectManager.GetOrCreateInstance();
    }

    private void ConfigurePhysics()
    {
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
        }

        if (itemRigidbody == null)
        {
            return;
        }

        itemRigidbody.bodyType = RigidbodyType2D.Dynamic;
        itemRigidbody.gravityScale = 0f;
        itemRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        itemRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        itemRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void EnsureVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetFallbackSprite();
        }

        spriteRenderer.sortingOrder = 15;
    }

    private void ApplyTypeVisual()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        switch (itemType)
        {
            case ItemType.PaddleExpand:
                spriteRenderer.color = new Color(0.38f, 0.95f, 0.70f, 1f);
                break;
            case ItemType.LifeUp:
                spriteRenderer.color = new Color(1f, 0.82f, 0.32f, 1f);
                break;
            case ItemType.AddBalls:
                spriteRenderer.color = new Color(0.55f, 0.78f, 1f, 1f);
                break;
        }
    }

    private static Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        fallbackSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        fallbackSprite.name = "Runtime_ItemSprite";
        return fallbackSprite;
    }

    private void OnValidate()
    {
        fallSpeed = Mathf.Max(0f, fallSpeed);
    }
}
