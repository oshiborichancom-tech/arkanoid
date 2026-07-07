using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Block : MonoBehaviour
{
    private const string BorderObjectName = "Border";
    private const int BorderSortingOrderOffset = -1;

    private static readonly ItemType[] DropItemTypes = (ItemType[])System.Enum.GetValues(typeof(ItemType));

    [SerializeField] private bool countAsTarget = true;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ItemController itemPrefab;
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.5f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField] private bool spawnBreakEffect = true;
    [SerializeField] private BlockBreakEffect breakEffectPrefab;
    [SerializeField] private bool useBlockColorForBreakEffect = true;
    [SerializeField] private Color breakEffectColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private float breakEffectDuration = 0.3f;
    [SerializeField] private float breakEffectStartScale = 0.8f;
    [SerializeField] private float breakEffectEndScale = 1.35f;
    [SerializeField] private bool showBorder = true;
    [SerializeField] private Color borderColor = new Color(0f, 0f, 0f, 0.65f);
    [SerializeField] private float borderThickness = 0.04f;
    [SerializeField] private SpriteRenderer borderRenderer;

    private bool isBroken;

    private void Awake()
    {
        EnsureBorderVisual();
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }

        if (itemEffectManager == null)
        {
            itemEffectManager = ItemEffectManager.Instance != null
                ? ItemEffectManager.Instance
                : FindObjectOfType<ItemEffectManager>();
        }

        EnsureBorderVisual();
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void ConfigureItemDrop(ItemController prefab, float dropChance, ItemEffectManager effectManager)
    {
        itemPrefab = prefab;
        itemDropChance = Mathf.Clamp01(dropChance);
        itemEffectManager = effectManager;
    }

    public void Break()
    {
        if (isBroken)
        {
            return;
        }

        isBroken = true;

        SpawnBreakEffect();
        TryDropItem();

        if (countAsTarget && gameManager != null)
        {
            gameManager.NotifyBlockDestroyed();
        }

        Destroy(gameObject);
    }

    private void TryDropItem()
    {
        if (itemPrefab == null || itemDropChance <= 0f || Random.value > itemDropChance)
        {
            return;
        }

        ItemController item = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        item.Initialize(GetRandomItemType(), itemEffectManager);
        item.gameObject.SetActive(true);
    }

    private void SpawnBreakEffect()
    {
        if (!spawnBreakEffect)
        {
            return;
        }

        SpriteRenderer blockRenderer = GetComponent<SpriteRenderer>();
        Color effectColor = GetBreakEffectColor(blockRenderer);
        int sortingOrder = blockRenderer != null ? blockRenderer.sortingOrder + 7 : 12;

        BlockBreakEffect effect = null;
        if (breakEffectPrefab != null)
        {
            effect = Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            GameObject effectObject = new GameObject("BlockBreakEffect");
            effectObject.transform.position = transform.position;
            effectObject.AddComponent<SpriteRenderer>();
            effect = effectObject.AddComponent<BlockBreakEffect>();
        }

        if (effect == null)
        {
            return;
        }

        effect.transform.localScale = transform.lossyScale;
        effect.Configure(
            effectColor,
            breakEffectDuration,
            breakEffectStartScale,
            breakEffectEndScale,
            sortingOrder);
    }

    private Color GetBreakEffectColor(SpriteRenderer blockRenderer)
    {
        if (!useBlockColorForBreakEffect || blockRenderer == null)
        {
            return breakEffectColor;
        }

        Color color = blockRenderer.color;
        color.a = breakEffectColor.a;
        return color;
    }

    private void EnsureBorderVisual()
    {
        SpriteRenderer bodyRenderer = GetComponent<SpriteRenderer>();
        if (bodyRenderer == null)
        {
            SetBorderVisible(false);
            return;
        }

        if (!showBorder)
        {
            SetBorderVisible(false);
            return;
        }

        if (borderRenderer == null)
        {
            borderRenderer = FindBorderRenderer();
        }

        if (borderRenderer == null)
        {
            borderRenderer = CreateBorderRenderer();
        }

        if (borderRenderer == null)
        {
            return;
        }

        Transform borderTransform = borderRenderer.transform;
        borderTransform.SetParent(transform, false);
        borderTransform.localPosition = Vector3.zero;
        borderTransform.localRotation = Quaternion.identity;
        float safeThickness = Mathf.Max(0f, borderThickness);
        float borderScale = 1f + safeThickness * 2f;
        borderTransform.localScale = new Vector3(borderScale, borderScale, 1f);

        borderRenderer.sprite = bodyRenderer.sprite;
        borderRenderer.color = borderColor;
        borderRenderer.flipX = bodyRenderer.flipX;
        borderRenderer.flipY = bodyRenderer.flipY;
        borderRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
        borderRenderer.sortingOrder = bodyRenderer.sortingOrder + BorderSortingOrderOffset;
        borderRenderer.enabled = true;
    }

    private SpriteRenderer FindBorderRenderer()
    {
        Transform borderTransform = transform.Find(BorderObjectName);
        return borderTransform != null ? borderTransform.GetComponent<SpriteRenderer>() : null;
    }

    private SpriteRenderer CreateBorderRenderer()
    {
        GameObject borderObject = new GameObject(BorderObjectName);
        borderObject.transform.SetParent(transform, false);
        return borderObject.AddComponent<SpriteRenderer>();
    }

    private void SetBorderVisible(bool isVisible)
    {
        if (borderRenderer == null)
        {
            borderRenderer = FindBorderRenderer();
        }

        if (borderRenderer != null)
        {
            borderRenderer.enabled = isVisible;
        }
    }

    private static ItemType GetRandomItemType()
    {
        return DropItemTypes[Random.Range(0, DropItemTypes.Length)];
    }

    private void OnValidate()
    {
        itemDropChance = Mathf.Clamp01(itemDropChance);
        breakEffectDuration = Mathf.Max(0.01f, breakEffectDuration);
        breakEffectStartScale = Mathf.Max(0.01f, breakEffectStartScale);
        breakEffectEndScale = Mathf.Max(breakEffectStartScale, breakEffectEndScale);
        borderThickness = Mathf.Max(0f, borderThickness);

        if (borderRenderer != null)
        {
            borderRenderer.color = borderColor;
            borderRenderer.enabled = showBorder;
        }
    }
}
