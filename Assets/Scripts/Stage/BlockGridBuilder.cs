using UnityEngine;

public class BlockGridBuilder : MonoBehaviour
{
    [SerializeField] private Block blockPrefab;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform blocksParent;
    [SerializeField] private int rows = 5;
    [SerializeField] private int columns = 10;
    [SerializeField] private float blockSize = 0.6f;
    [SerializeField] private float spacing = 0.01f;
    [SerializeField] private Vector2 startPosition = new Vector2(-3.24f, 3.25f);
    [SerializeField] private ItemController itemPrefab;
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.5f;
    [SerializeField] private ItemEffectManager itemEffectManager;
    [SerializeField] private bool useSingleBlockColor = true;
    [SerializeField] private Color singleBlockColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private bool useManualBlockLayout;
    [SerializeField]
    private string[] blockLayout =
    {
        "111111111",
        "111111111",
        "111111111",
        "111111111",
        "111111111"
    };
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
        bool useFallbackGrid = !TryBuildManualGrid(out createdCount);

        if (useFallbackGrid)
        {
            createdCount = BuildFullGrid();
        }

        if (gameManager != null)
        {
            gameManager.RegisterBlocks(createdCount);
        }
    }

    private int BuildFullGrid()
    {
        int createdCount = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Vector2 position = startPosition + new Vector2(
                    column * (blockSize + spacing),
                    -row * (blockSize + spacing));

                if (CreateBlock(row, column, position))
                {
                    createdCount++;
                }
            }
        }

        return createdCount;
    }

    private bool TryBuildManualGrid(out int createdCount)
    {
        createdCount = 0;

        if (!useManualBlockLayout)
        {
            return false;
        }

        if (blockLayout == null || blockLayout.Length <= 0)
        {
            Debug.LogWarning("Manual block layout is enabled, but Block Layout is empty. Falling back to blockRows/blockColumns.");
            return false;
        }

        int manualColumns = GetManualLayoutColumnCount();
        if (manualColumns <= 0 || !HasManualBlocks())
        {
            Debug.LogWarning("Manual block layout has no valid '1' entries. Falling back to blockRows/blockColumns.");
            return false;
        }

        bool loggedUnknownCharacter = false;
        for (int row = 0; row < blockLayout.Length; row++)
        {
            string rowText = blockLayout[row] ?? string.Empty;

            for (int column = 0; column < manualColumns; column++)
            {
                if (!ShouldCreateManualBlock(rowText, row, column, ref loggedUnknownCharacter))
                {
                    continue;
                }

                Vector2 position = startPosition + new Vector2(
                    column * (blockSize + spacing),
                    -row * (blockSize + spacing));

                if (CreateBlock(row, column, position))
                {
                    createdCount++;
                }
            }
        }

        return true;
    }

    private bool CreateBlock(int row, int column, Vector2 position)
    {
        Block block = Instantiate(blockPrefab, position, Quaternion.identity, blocksParent);
        if (block == null)
        {
            return false;
        }

        block.name = $"Block_{row + 1}_{column + 1}";
        block.gameObject.SetActive(true);
        block.transform.localScale = Vector3.one * blockSize;
        block.Initialize(gameManager);
        block.ConfigureItemDrop(itemPrefab, itemDropChance, itemEffectManager);

        SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = GetBlockColor(row, renderer.color);
        }

        return true;
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

    public void ConfigureBlockColor(bool useSingleColor, Color singleColor)
    {
        useSingleBlockColor = useSingleColor;
        singleBlockColor = singleColor;
    }

    public void ConfigureManualBlockLayout(bool useManualLayout, string[] layout)
    {
        useManualBlockLayout = useManualLayout;
        blockLayout = layout;
    }

    private Color GetBlockColor(int row, Color fallbackColor)
    {
        if (useSingleBlockColor)
        {
            return singleBlockColor;
        }

        if (rowColors != null && rowColors.Length > 0)
        {
            return rowColors[row % rowColors.Length];
        }

        return fallbackColor;
    }

    private int GetManualLayoutColumnCount()
    {
        if (blockLayout == null)
        {
            return 0;
        }

        int maxColumns = 0;
        for (int i = 0; i < blockLayout.Length; i++)
        {
            string rowText = blockLayout[i];
            if (rowText != null)
            {
                maxColumns = Mathf.Max(maxColumns, rowText.Length);
            }
        }

        return maxColumns;
    }

    private bool HasManualBlocks()
    {
        if (blockLayout == null)
        {
            return false;
        }

        for (int row = 0; row < blockLayout.Length; row++)
        {
            string rowText = blockLayout[row];
            if (string.IsNullOrEmpty(rowText))
            {
                continue;
            }

            for (int column = 0; column < rowText.Length; column++)
            {
                if (rowText[column] == '1')
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool ShouldCreateManualBlock(string rowText, int row, int column, ref bool loggedUnknownCharacter)
    {
        if (string.IsNullOrEmpty(rowText) || column >= rowText.Length)
        {
            return false;
        }

        char value = rowText[column];
        switch (value)
        {
            case '1':
                return true;
            case '0':
            case '.':
            case ' ':
                return false;
            default:
                if (!loggedUnknownCharacter)
                {
                    Debug.LogWarning($"Manual block layout contains unsupported character '{value}' at row {row + 1}, column {column + 1}. It will be treated as empty.");
                    loggedUnknownCharacter = true;
                }

                return false;
        }
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
