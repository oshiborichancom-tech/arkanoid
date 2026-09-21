using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Block : MonoBehaviour
{
    private const string BorderObjectName = "Border";
    private const string StateLabelObjectName = "StateLabel";
    private const int BorderSortingOrderOffset = -1;
    private const int LabelSortingOrderOffset = 2;
    private const int DurableHitPoints = 2;

    private static readonly ItemType[] DropItemTypes = (ItemType[])System.Enum.GetValues(typeof(ItemType));

    [SerializeField] private bool countAsTarget = true;
    [SerializeField] private BlockType blockType = BlockType.Normal;
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
    [SerializeField] private float borderThickness = 0.015f;
    [SerializeField] private SpriteRenderer borderRenderer;
    [Header("Block Type Visuals")]
    [SerializeField] private Color durableFullColor = new Color(0.66f, 0.25f, 0.85f, 1f);
    [SerializeField] private Color durableDamagedColor = new Color(1f, 0.45f, 0.82f, 1f);
    [SerializeField] private Color durableBorderColor = new Color(1f, 0.31f, 0.85f, 1f);
    [SerializeField] private Color indestructibleColor = new Color(0.18f, 0.15f, 0.22f, 1f);
    [SerializeField] private Color indestructibleBorderColor = new Color(0.39f, 0.96f, 1f, 1f);
    [SerializeField] private float durableBorderThickness = 0.025f;
    [SerializeField] private float indestructibleBorderThickness = 0.02f;
    [SerializeField] private float stateLabelCharacterSize = 0.34f;
    [SerializeField] private TextMesh stateLabel;

    private bool isBroken;
    private int remainingHitPoints = 1;
    private int lastHitFrame = -1;
    private int lastHitterInstanceId;
    private Color normalBlockColor = Color.white;
    private Color normalBorderColor;
    private float normalBorderThickness;

    public BlockType Type => blockType;
    public bool CountsForClear => countAsTarget && blockType != BlockType.Indestructible;
    public int RemainingHitPoints => blockType == BlockType.Indestructible ? -1 : remainingHitPoints;

    private void Awake()
    {
        SpriteRenderer bodyRenderer = GetComponent<SpriteRenderer>();
        if (bodyRenderer != null)
        {
            normalBlockColor = GetOpaqueColor(bodyRenderer.color);
        }

        normalBorderColor = borderColor;
        normalBorderThickness = borderThickness;
        remainingHitPoints = GetInitialHitPoints(blockType);
        ApplyTypeVisual();
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

        ApplyTypeVisual();
    }

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    public void ConfigureType(BlockType type, Color normalColor)
    {
        blockType = type;
        normalBlockColor = GetOpaqueColor(normalColor);
        countAsTarget = blockType != BlockType.Indestructible;
        remainingHitPoints = GetInitialHitPoints(blockType);
        isBroken = false;
        lastHitFrame = -1;
        lastHitterInstanceId = 0;
        ApplyTypeVisual();
    }

    public void ConfigureItemDrop(ItemController prefab, float dropChance, ItemEffectManager effectManager)
    {
        itemPrefab = prefab;
        itemDropChance = Mathf.Clamp01(dropChance);
        itemEffectManager = effectManager;
    }

    public void Break()
    {
        Hit(null);
    }

    public void Hit(BallController hitter)
    {
        if (isBroken)
        {
            return;
        }

        int hitterInstanceId = hitter != null ? hitter.GetInstanceID() : 0;
        if (lastHitFrame == Time.frameCount && lastHitterInstanceId == hitterInstanceId)
        {
            return;
        }

        lastHitFrame = Time.frameCount;
        lastHitterInstanceId = hitterInstanceId;

        if (blockType == BlockType.Indestructible)
        {
            return;
        }

        if (blockType == BlockType.Durable && remainingHitPoints > 1)
        {
            remainingHitPoints--;
            ApplyTypeVisual();
            return;
        }

        DestroyBlock();
    }

    private void DestroyBlock()
    {
        isBroken = true;
        remainingHitPoints = 0;

        SpawnBreakEffect();
        TryDropItem();

        if (countAsTarget && gameManager != null)
        {
            gameManager.NotifyBlockDestroyed();
        }

        Destroy(gameObject);
    }

    private void ApplyTypeVisual()
    {
        SpriteRenderer bodyRenderer = GetComponent<SpriteRenderer>();
        if (bodyRenderer == null)
        {
            SetBorderVisible(false);
            SetStateLabelVisible(false);
            return;
        }

        Color bodyColor;
        string label;
        Color labelColor;

        switch (blockType)
        {
            case BlockType.Durable:
                bool isDamaged = remainingHitPoints <= 1;
                bodyColor = isDamaged ? durableDamagedColor : durableFullColor;
                borderColor = durableBorderColor;
                borderThickness = durableBorderThickness;
                label = isDamaged ? "1" : "2";
                labelColor = Color.white;
                break;
            case BlockType.Indestructible:
                bodyColor = indestructibleColor;
                borderColor = indestructibleBorderColor;
                borderThickness = indestructibleBorderThickness;
                label = "X";
                labelColor = indestructibleBorderColor;
                break;
            default:
                bodyColor = normalBlockColor;
                borderColor = normalBorderColor;
                borderThickness = normalBorderThickness;
                label = string.Empty;
                labelColor = Color.white;
                break;
        }

        bodyRenderer.color = GetOpaqueColor(bodyColor);
        EnsureBorderVisual();
        UpdateStateLabel(bodyRenderer, label, labelColor);
    }

    private void UpdateStateLabel(SpriteRenderer bodyRenderer, string label, Color labelColor)
    {
        if (string.IsNullOrEmpty(label))
        {
            SetStateLabelVisible(false);
            return;
        }

        if (stateLabel == null)
        {
            Transform labelTransform = transform.Find(StateLabelObjectName);
            if (labelTransform != null)
            {
                stateLabel = labelTransform.GetComponent<TextMesh>();
            }
        }

        if (stateLabel == null)
        {
            GameObject labelObject = new GameObject(StateLabelObjectName);
            labelObject.transform.SetParent(transform, false);
            stateLabel = labelObject.AddComponent<TextMesh>();
        }

        stateLabel.transform.localPosition = Vector3.zero;
        stateLabel.transform.localRotation = Quaternion.identity;
        stateLabel.transform.localScale = Vector3.one;
        stateLabel.text = label;
        stateLabel.anchor = TextAnchor.MiddleCenter;
        stateLabel.alignment = TextAlignment.Center;
        stateLabel.fontSize = 64;
        stateLabel.characterSize = Mathf.Max(0.01f, stateLabelCharacterSize);
        stateLabel.color = GetOpaqueColor(labelColor);

        MeshRenderer labelRenderer = stateLabel.GetComponent<MeshRenderer>();
        if (labelRenderer != null)
        {
            labelRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
            labelRenderer.sortingOrder = bodyRenderer.sortingOrder + LabelSortingOrderOffset;
        }

        stateLabel.gameObject.SetActive(true);
    }

    private void SetStateLabelVisible(bool isVisible)
    {
        if (stateLabel == null)
        {
            Transform labelTransform = transform.Find(StateLabelObjectName);
            if (labelTransform != null)
            {
                stateLabel = labelTransform.GetComponent<TextMesh>();
            }
        }

        if (stateLabel != null)
        {
            stateLabel.gameObject.SetActive(isVisible);
        }
    }

    private static int GetInitialHitPoints(BlockType type)
    {
        return type == BlockType.Durable ? DurableHitPoints : 1;
    }

    private static Color GetOpaqueColor(Color color)
    {
        color.a = 1f;
        return color;
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
        durableBorderThickness = Mathf.Max(0f, durableBorderThickness);
        indestructibleBorderThickness = Mathf.Max(0f, indestructibleBorderThickness);
        stateLabelCharacterSize = Mathf.Max(0.01f, stateLabelCharacterSize);

        if (borderRenderer != null)
        {
            borderRenderer.color = borderColor;
            borderRenderer.enabled = showBorder;
        }
    }
}
