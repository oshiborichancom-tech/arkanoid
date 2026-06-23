using UnityEngine;

public enum BackgroundFitMode
{
    Fit,
    Cover,
    Stretch
}

[CreateAssetMenu(fileName = "StageData", menuName = "Arkanoid/Stage Data")]
public class StageData : ScriptableObject
{
    [SerializeField, Min(1)] private int stageId = 1;
    [SerializeField] private string stageName = "Stage 1";

    [Header("Background")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private BackgroundFitMode backgroundFitMode = BackgroundFitMode.Stretch;
    [SerializeField] private Vector2 backgroundOffset = Vector2.zero;
    [SerializeField] private Vector2 backgroundScaleMultiplier = Vector2.one;

    [Header("Blocks")]
    [SerializeField, Min(1)] private int blockRows = 5;
    [SerializeField, Min(1)] private int blockColumns = 10;
    [SerializeField, Min(0.1f)] private float blockSize = 0.6f;
    [SerializeField, Min(0f)] private float blockSpacing = 0.12f;
    [SerializeField] private Vector2 blockStartPosition = new Vector2(-3.24f, 3.25f);

    [Header("Player")]
    [SerializeField, Min(0.1f)] private float ballSpeed = 7f;
    [SerializeField, Min(0.1f)] private float paddleSpeed = 9f;
    [SerializeField, Min(1)] private int initialLives = 3;

    [Header("Items")]
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.5f;
    [SerializeField, Min(1f)] private float paddleExpandMultiplier = 1.5f;
    [SerializeField, Min(0f)] private float paddleExpandDuration = 8f;
    [SerializeField, Min(1)] private int addBallsCount = 2;
    [SerializeField, Min(0f)] private float addBallLaunchAngle = 25f;
    [SerializeField, Min(0.1f)] private float addBallSpeed = 7f;

    public int StageId => Mathf.Max(1, stageId);
    public string StageName => string.IsNullOrWhiteSpace(stageName) ? $"Stage {StageId}" : stageName;
    public Sprite BackgroundSprite => backgroundSprite;
    public BackgroundFitMode BackgroundFitMode => backgroundFitMode;
    public Vector2 BackgroundOffset => backgroundOffset;
    public Vector2 BackgroundScaleMultiplier => GetSafeScaleMultiplier(backgroundScaleMultiplier);
    public int BlockRows => Mathf.Max(1, blockRows);
    public int BlockColumns => Mathf.Max(1, blockColumns);
    public float BlockSize => Mathf.Max(0.1f, blockSize);
    public float BlockSpacing => Mathf.Max(0f, blockSpacing);
    public Vector2 BlockStartPosition => blockStartPosition;
    public float BallSpeed => Mathf.Max(0.1f, ballSpeed);
    public float PaddleSpeed => Mathf.Max(0.1f, paddleSpeed);
    public int InitialLives => Mathf.Max(1, initialLives);
    public float ItemDropChance => Mathf.Clamp01(itemDropChance);
    public float PaddleExpandMultiplier => Mathf.Max(1f, paddleExpandMultiplier);
    public float PaddleExpandDuration => Mathf.Max(0f, paddleExpandDuration);
    public int AddBallsCount => Mathf.Max(1, addBallsCount);
    public float AddBallLaunchAngle => Mathf.Max(0f, addBallLaunchAngle);
    public float AddBallSpeed => Mathf.Max(0.1f, addBallSpeed);

    private void OnValidate()
    {
        stageId = Mathf.Max(1, stageId);
        if (string.IsNullOrWhiteSpace(stageName))
        {
            stageName = $"Stage {stageId}";
        }

        backgroundScaleMultiplier = GetSafeScaleMultiplier(backgroundScaleMultiplier);
        blockRows = Mathf.Max(1, blockRows);
        blockColumns = Mathf.Max(1, blockColumns);
        blockSize = Mathf.Max(0.1f, blockSize);
        blockSpacing = Mathf.Max(0f, blockSpacing);
        ballSpeed = Mathf.Max(0.1f, ballSpeed);
        paddleSpeed = Mathf.Max(0.1f, paddleSpeed);
        initialLives = Mathf.Max(1, initialLives);
        itemDropChance = Mathf.Clamp01(itemDropChance);
        paddleExpandMultiplier = Mathf.Max(1f, paddleExpandMultiplier);
        paddleExpandDuration = Mathf.Max(0f, paddleExpandDuration);
        addBallsCount = Mathf.Max(1, addBallsCount);
        addBallLaunchAngle = Mathf.Max(0f, addBallLaunchAngle);
        addBallSpeed = Mathf.Max(0.1f, addBallSpeed);
    }

    private static Vector2 GetSafeScaleMultiplier(Vector2 value)
    {
        return new Vector2(Mathf.Max(0.01f, value.x), Mathf.Max(0.01f, value.y));
    }
}
