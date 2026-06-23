using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Block : MonoBehaviour
{
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

    private bool isBroken;

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
    }
}
