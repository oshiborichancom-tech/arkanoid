using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class ItemController : MonoBehaviour
{
    private const int ItemSortingOrder = 15;
    private const int FillSortingOrderOffset = 1;
    private const int LabelSortingOrderOffset = 2;
    private const string BorderObjectName = "NeonBorder";
    private const string LabelObjectName = "Label";

    [SerializeField] private ItemType itemType = ItemType.PaddleExpand;
    [SerializeField] private float fallSpeed = 2.5f;
    [SerializeField] private float destroyY = -5.8f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer borderRenderer;
    [SerializeField] private TextMesh labelText;
    [SerializeField] private Color innerColor = new Color(0.10f, 0.05f, 0.14f, 0.96f);
    [SerializeField] private Color paddleExpandColor = new Color(1f, 0.31f, 0.85f, 1f);
    [SerializeField] private Color lifeUpColor = new Color(1f, 0.36f, 0.66f, 1f);
    [SerializeField] private Color addBallsColor = new Color(0.39f, 0.96f, 1f, 1f);
    [SerializeField] private Color labelColor = Color.white;
    [SerializeField] private int labelFontSize = 64;
    [SerializeField] private float labelCharacterSize = 0.12f;
    [SerializeField] private float borderScale = 1.18f;
    [SerializeField] private bool useGeneratedCircleSprite = true;

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
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (useGeneratedCircleSprite || spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = GetFallbackSprite();
        }

        spriteRenderer.sortingOrder = ItemSortingOrder + FillSortingOrderOffset;
        EnsureBorderVisual();
        EnsureLabelVisual();
    }

    private void CacheVisualReferences()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (borderRenderer == null)
        {
            Transform borderTransform = transform.Find(BorderObjectName);
            borderRenderer = borderTransform != null ? borderTransform.GetComponent<SpriteRenderer>() : null;
        }

        if (labelText == null)
        {
            labelText = GetComponentInChildren<TextMesh>(true);
        }
    }

    private void EnsureBorderVisual()
    {
        if (borderRenderer == null)
        {
            borderRenderer = CreateBorderRenderer();
        }

        if (borderRenderer == null)
        {
            return;
        }

        borderRenderer.sprite = spriteRenderer != null && spriteRenderer.sprite != null
            ? spriteRenderer.sprite
            : GetFallbackSprite();
        borderRenderer.sortingOrder = ItemSortingOrder;

        Transform borderTransform = borderRenderer.transform;
        borderTransform.localPosition = Vector3.zero;
        borderTransform.localRotation = Quaternion.identity;
        float safeScale = Mathf.Max(1f, borderScale);
        borderTransform.localScale = new Vector3(safeScale, safeScale, 1f);
    }

    private SpriteRenderer CreateBorderRenderer()
    {
        GameObject borderObject = new GameObject(BorderObjectName);
        borderObject.transform.SetParent(transform, false);
        return borderObject.AddComponent<SpriteRenderer>();
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
        labelText.characterSize = GetLabelCharacterSize(itemType);
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

        spriteRenderer.color = innerColor;

        if (borderRenderer != null)
        {
            borderRenderer.color = GetItemColor(itemType);
        }

        if (labelText != null)
        {
            labelText.text = GetItemLabel(itemType);
            labelText.color = labelColor;
            labelText.characterSize = GetLabelCharacterSize(itemType);
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
                return "\u2665";
            case ItemType.AddBalls:
                return "+2";
            default:
                return "?";
        }
    }

    private float GetLabelCharacterSize(ItemType type)
    {
        float safeCharacterSize = Mathf.Max(0.01f, labelCharacterSize);
        return type == ItemType.AddBalls ? safeCharacterSize * 0.78f : safeCharacterSize;
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
        borderScale = Mathf.Max(1f, borderScale);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = innerColor;
            spriteRenderer.sortingOrder = ItemSortingOrder + FillSortingOrderOffset;
        }

        if (borderRenderer != null)
        {
            borderRenderer.color = GetItemColor(itemType);
            borderRenderer.sortingOrder = ItemSortingOrder;
            borderRenderer.transform.localScale = new Vector3(borderScale, borderScale, 1f);
        }

        if (labelText != null)
        {
            labelText.text = GetItemLabel(itemType);
            labelText.color = labelColor;
            labelText.fontSize = labelFontSize;
            labelText.characterSize = GetLabelCharacterSize(itemType);
            UpdateLabelRenderer();
        }
    }
}
