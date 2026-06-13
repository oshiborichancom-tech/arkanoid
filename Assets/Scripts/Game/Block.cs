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

    private static ItemType GetRandomItemType()
    {
        return DropItemTypes[Random.Range(0, DropItemTypes.Length)];
    }
}
