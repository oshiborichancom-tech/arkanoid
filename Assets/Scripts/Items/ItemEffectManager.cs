using UnityEngine;

public class ItemEffectManager : MonoBehaviour
{
    public static ItemEffectManager Instance { get; private set; }

    [SerializeField] private PaddleController paddle;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float paddleExpandMultiplier = 1.5f;
    [SerializeField] private float paddleExpandDuration = 8f;
    [SerializeField] private int lifeUpAmount = 1;
    [SerializeField] private int addBallsCount = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple ItemEffectManager instances found. The latest one will be used.");
        }

        Instance = this;
    }

    private void Start()
    {
        FindMissingReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public static ItemEffectManager GetOrCreateInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        ItemEffectManager existingManager = UnityEngine.Object.FindObjectOfType<ItemEffectManager>();
        if (existingManager != null)
        {
            Instance = existingManager;
            return existingManager;
        }

        return new GameObject("ItemEffectManager").AddComponent<ItemEffectManager>();
    }

    public void Configure(
        PaddleController paddleController,
        GameManager manager,
        float expandMultiplier,
        float expandDuration,
        int extraBallCount)
    {
        paddle = paddleController;
        gameManager = manager;
        paddleExpandMultiplier = Mathf.Max(1f, expandMultiplier);
        paddleExpandDuration = Mathf.Max(0f, expandDuration);
        lifeUpAmount = Mathf.Max(1, lifeUpAmount);
        addBallsCount = Mathf.Max(1, extraBallCount);
    }

    public void ApplyItemEffect(ItemType itemType)
    {
        Debug.Log($"Item acquired: {itemType}");

        switch (itemType)
        {
            case ItemType.PaddleExpand:
                ApplyPaddleExpand();
                break;
            case ItemType.LifeUp:
                ApplyLifeUp();
                break;
            case ItemType.AddBalls:
                ApplyAddBalls();
                break;
            default:
                Debug.LogWarning($"Unknown item type: {itemType}");
                break;
        }
    }

    private void ApplyPaddleExpand()
    {
        FindMissingReferences();

        if (paddle == null)
        {
            Debug.LogWarning("PaddleController not found. Paddle expand could not be applied.");
            return;
        }

        paddle.ApplyTemporaryExpand(paddleExpandMultiplier, paddleExpandDuration);
        Debug.Log("Paddle expand applied.");
    }

    private void ApplyLifeUp()
    {
        FindMissingReferences();

        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found. Life up could not be applied.");
            return;
        }

        gameManager.AddLife(lifeUpAmount);
    }

    private void ApplyAddBalls()
    {
        FindMissingReferences();

        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found. AddBalls could not be applied.");
            return;
        }

        Debug.Log("AddBalls effect requested.");
        gameManager.AddExtraBalls(addBallsCount);
    }

    private void FindMissingReferences()
    {
        if (paddle == null)
        {
            paddle = FindObjectOfType<PaddleController>();
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance != null ? GameManager.Instance : FindObjectOfType<GameManager>();
        }
    }

    private void OnValidate()
    {
        paddleExpandMultiplier = Mathf.Max(1f, paddleExpandMultiplier);
        paddleExpandDuration = Mathf.Max(0f, paddleExpandDuration);
        lifeUpAmount = Mathf.Max(1, lifeUpAmount);
        addBallsCount = Mathf.Max(1, addBallsCount);
    }
}
