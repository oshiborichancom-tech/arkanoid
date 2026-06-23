using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ItemController : MonoBehaviour
{
    private const int ItemSortingOrder = 15;
    private const int LabelSortingOrderOffset = 1;
    private const string LabelObjectName = "Label";

    [SerializeField] private ItemType itemType = ItemType.PaddleExpand;
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float destroyY = -5.8f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMesh labelText;
    [SerializeField] private Color paddleExpandColor = new Color(0.30f, 0.68f, 1f, 1f);
    [SerializeField] private Color lifeUpColor = new Color(0.35f, 0.92f, 0.55f, 1f);
    [SerializeField] private Color addBallsColor = new Color(1f, 0.72f, 0.22f, 1f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private int labelFontSize = 64;
    [SerializeField] private float labelCharacterSize = 0.12f;

    private static Sprite fallbackSprite;
    private static Font labelFont;

    private Rigidbody2D itemRigidbody;
    private Collider2D itemCollider;
    private bool collected;

    private void Awake()
    {
        itemRigidbody = GetComponent<Rigidbody2D>();
        itemCollider = GetComponent<Collider2D>();

        ConfigurePhysics();
        EnsureVisual();
        ApplyVisual();
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
        ApplyVisual();
    }

    private void TryCollectFrom(Collider2D other)
    {
        if (collected || other == null || other.GetComponentInParent<PaddleController>() == null)
        {
            return;
        }

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
        CacheVisualReferences();

        if (spriteRenderer == null)
        {
            return;
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetFallbackSprite();
        }

        spriteRenderer.sortingOrder = ItemSortingOrder;
        EnsureLabelVisual();
    }

    private void CacheVisualReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (labelText == null)
        {
            labelText = GetComponentInChildren<TextMesh>(true);
        }
    }

    private void EnsureLabelVisual()
    {
        if (labelText == null)
        {
            labelText = CreateLabelText();
        }

        if (labelText == null)
        {
            return;
        }

        Transform labelTransform = labelText.transform;
        labelTransform.localPosition = Vector3.zero;
        labelTransform.localRotation = Quaternion.identity;
        labelTransform.localScale = Vector3.one;

        labelText.anchor = TextAnchor.MiddleCenter;
        labelText.alignment = TextAlignment.Center;
        labelText.fontSize = Mathf.Max(1, labelFontSize);
        labelText.characterSize = Mathf.Max(0.01f, labelCharacterSize);
        labelText.color = labelColor;

        Font font = GetLabelFont();
        if (font != null)
        {
            labelText.font = font;
        }

        UpdateLabelRenderer();
    }

    private TextMesh CreateLabelText()
    {
        GameObject labelObject = new GameObject(LabelObjectName);
        labelObject.transform.SetParent(transform, false);
        return labelObject.AddComponent<TextMesh>();
    }

    private void ApplyVisual()
    {
        EnsureVisual();

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = GetItemColor(itemType);

        if (labelText != null)
        {
            labelText.text = GetItemLabel(itemType);
            labelText.color = labelColor;
            UpdateLabelRenderer();
        }
    }

    private void UpdateLabelRenderer()
    {
        if (labelText == null)
        {
            return;
        }

        MeshRenderer labelRenderer = labelText.GetComponent<MeshRenderer>();
        if (labelRenderer == null)
        {
            labelRenderer = labelText.gameObject.AddComponent<MeshRenderer>();
        }

        labelRenderer.sortingOrder = spriteRenderer != null
            ? spriteRenderer.sortingOrder + LabelSortingOrderOffset
            : ItemSortingOrder + LabelSortingOrderOffset;

        if (labelText.font != null)
        {
            labelRenderer.sharedMaterial = labelText.font.material;
        }
    }

    private Color GetItemColor(ItemType type)
    {
        switch (type)
        {
            case ItemType.PaddleExpand:
                return paddleExpandColor;
            case ItemType.LifeUp:
                return lifeUpColor;
            case ItemType.AddBalls:
                return addBallsColor;
            default:
                return Color.white;
        }
    }

    private static string GetItemLabel(ItemType type)
    {
        switch (type)
        {
            case ItemType.PaddleExpand:
                return "P";
            case ItemType.LifeUp:
                return "L";
            case ItemType.AddBalls:
                return "B";
            default:
                return "?";
        }
    }

    private static Font GetLabelFont()
    {
        if (labelFont != null)
        {
            return labelFont;
        }

        try
        {
            labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Built-in item label font could not be loaded. {exception.Message}");
        }

        return labelFont;
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
        labelFontSize = Mathf.Max(1, labelFontSize);
        labelCharacterSize = Mathf.Max(0.01f, labelCharacterSize);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = GetItemColor(itemType);
        }

        if (labelText != null)
        {
            labelText.text = GetItemLabel(itemType);
            labelText.color = labelColor;
            labelText.fontSize = labelFontSize;
            labelText.characterSize = labelCharacterSize;
            UpdateLabelRenderer();
        }
    }
}
