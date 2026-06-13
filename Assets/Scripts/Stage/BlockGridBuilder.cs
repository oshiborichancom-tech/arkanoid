using UnityEngine;

public class BlockGridBuilder : MonoBehaviour
{
    [SerializeField] private Block blockPrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform blocksParent;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 10;
    [SerializeField] private float blockSize = 0.6f;
    [SerializeField] private float spacing = 0.12f;
    [SerializeField] private Vector2 startPosition = new Vector2(-3.24f, 3.25f);
    [SerializeField] private ItemController itemPrefab;
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.5f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField]
    private Color[] rowColors =
    {
        new Color(0.95f, 0.25f, 0.28f),
        new Color(0.96f, 0.56f, 0.20f),
        new Color(0.98f, 0.86f, 0.28f),
        new Color(0.28f, 0.76f, 0.45f),
        new Color(0.24f, 0.62f, 0.92f)
    };

    private void Start()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        if (blockPrefab == null)
        {
            Debug.LogWarning("BlockGridBuilder needs a block prefab.");
            return;
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }

        if (blocksParent == null)
        {
            blocksParent = transform;
        }

        if (itemEffectManager == null)
        {
            itemEffectManager = ItemEffectManager.Instance != null
                ? ItemEffectManager.Instance
                : FindObjectOfType<ItemEffectManager>();
        }

        int createdCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector2 position = startPosition + new Vector2(
                    column * (blockSize + spacing),
                    -row * (blockSize + spacing));

                Block block = Instantiate(blockPrefab, position, Quaternion.identity, blocksParent);
                block.name = $"Block_{row + 1}_{column + 1}";
                block.gameObject.SetActive(true);
                block.transform.localScale = Vector3.one * blockSize;
                block.Initialize(gameManager);
                block.ConfigureItemDrop(itemPrefab, itemDropChance, itemEffectManager);

                SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();
                if (renderer != null && rowColors != null && rowColors.Length > 0)
                {
                    renderer.color = rowColors[row % rowColors.Length];
                }

                createdCount++;
            }
        }

        if (gameManager != null)
        {
            gameManager.RegisterBlocks(createdCount);
        }
    }

    public void Configure(
        Block prefab,
        GameManager manager,
        Transform parent,
        int rowCount,
        int columnCount,
        float size,
        float gap,
        Vector2 firstPosition)
    {
        blockPrefab = prefab;
        gameManager = manager;
        blocksParent = parent;
        rows = Mathf.Max(1, rowCount);
        columns = Mathf.Max(1, columnCount);
        blockSize = Mathf.Max(0.1f, size);
        spacing = Mathf.Max(0f, gap);
        startPosition = firstPosition;
    }

    public void ConfigureItemDrops(ItemController prefab, float dropChance, ItemEffectManager effectManager)
    {
        itemPrefab = prefab;
        itemDropChance = Mathf.Clamp01(dropChance);
        itemEffectManager = effectManager;
    }

    private void OnValidate()
    {
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);
        blockSize = Mathf.Max(0.1f, blockSize);
        spacing = Mathf.Max(0f, spacing);
        itemDropChance = Mathf.Clamp01(itemDropChance);
    }
}
