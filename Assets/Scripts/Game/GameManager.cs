using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        ReadyToLaunch,
        Playing,
        Clear,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    [SerializeField] private int initialLives = 3;
    [SerializeField] private int stageId = 1;
    [SerializeField] private string stageName = "Stage 1";
    [SerializeField] private BallController ball;
    [SerializeField] private BallController ballPrefab;
    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballsParent;
    [SerializeField] private float extraBallLaunchAngle = 25f;
    [SerializeField] private float extraBallSpeed = 7f;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool hasNextStage = true;
    [SerializeField] private bool autoFindReferences = true;
    [SerializeField] private int scorePerBlock = 100;
    [SerializeField] private string icon1Label = "C";
    [SerializeField] private string icon2Label = "N";
    [SerializeField] private string icon3Label = "S";
    [SerializeField] private string icon1Description = "Clear";
    [SerializeField] private string icon2Description = "No Miss";
    [SerializeField] private string icon3Description = "Score";
    [SerializeField] private int iconScoreTarget = 3000;

    private int lives;
    private int missCount;
    private int remainingBlocks;
    private int totalBlocks;
    private int destroyedBlocks;
    private int currentScore;
    private readonly List<BallController> activeBalls = new List<BallController>();

    public GameState CurrentState { get; private set; }
    public bool CanLaunchBall => CurrentState == GameState.ReadyToLaunch;
    public bool IsStageFinished => CurrentState == GameState.Clear || CurrentState == GameState.GameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances found. The latest one will be used.");
        }

        Instance = this;

        if (autoFindReferences)
        {
            FindMissingReferences();
        }

        lives = Mathf.Max(1, initialLives);
        CurrentState = GameState.ReadyToLaunch;
    }

    private void Start()
    {
        if (uiManager != null)
        {
            uiManager.SetStageName(stageName);
            uiManager.SetLives(lives);
            uiManager.SetScore(currentScore);
            RefreshStageIconDisplay();
            uiManager.ShowPlaying();
        }

        if (ball != null)
        {
            ball.ResetToPaddle();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Configure(BallController ballController, UIManager manager, string displayStageName, int livesCount)
    {
        Configure(ballController, manager, displayStageName, livesCount, 1, true);
    }

    public void Configure(BallController ballController, UIManager manager, string displayStageName, int livesCount, int currentStageId)
    {
        Configure(ballController, manager, displayStageName, livesCount, currentStageId, true);
    }

    public void Configure(BallController ballController, UIManager manager, string displayStageName, int livesCount, int currentStageId, bool stageHasNextStage)
    {
        ball = ballController;
        uiManager = manager;
        stageName = displayStageName;
        stageId = Mathf.Max(1, currentStageId);
        hasNextStage = stageHasNextStage;
        initialLives = Mathf.Max(1, livesCount);
        lives = initialLives;
        ResetResultStats();
        CurrentState = GameState.ReadyToLaunch;
    }

    public void ConfigureStageIcons(string clearIconLabel, string noMissIconLabel, string scoreIconLabel, int scoreTarget)
    {
        ConfigureStageIcons(
            clearIconLabel,
            "Clear",
            noMissIconLabel,
            "No Miss",
            scoreIconLabel,
            "Score",
            scoreTarget);
    }

    public void ConfigureStageIcons(
        string clearIconLabel,
        string clearIconDescription,
        string noMissIconLabel,
        string noMissIconDescription,
        string scoreIconLabel,
        string scoreIconDescription,
        int scoreTarget)
    {
        icon1Label = GetSafeIconLabel(clearIconLabel, "C");
        icon1Description = GetSafeIconDescription(clearIconDescription, "Clear");
        icon2Label = GetSafeIconLabel(noMissIconLabel, "N");
        icon2Description = GetSafeIconDescription(noMissIconDescription, "No Miss");
        icon3Label = GetSafeIconLabel(scoreIconLabel, "S");
        icon3Description = GetSafeIconDescription(scoreIconDescription, "Score");
        iconScoreTarget = Mathf.Max(0, scoreTarget);
        RefreshStageIconDisplay();
    }

    public void ConfigureBallSpawning(
        BallController prefab,
        Transform paddleTransform,
        Transform parent,
        float launchAngle,
        float launchSpeed)
    {
        ballPrefab = prefab != null ? prefab : ball;
        paddle = paddleTransform;
        ballsParent = parent;
        extraBallLaunchAngle = Mathf.Max(0f, launchAngle);
        extraBallSpeed = Mathf.Max(0.1f, launchSpeed);
    }

    public void RegisterBlocks(int blockCount)
    {
        int safeCount = Mathf.Max(0, blockCount);
        totalBlocks += safeCount;
        remainingBlocks += safeCount;
    }

    public void NotifyBallLaunched()
    {
        NotifyBallLaunched(ball);
    }

    public void NotifyBallLaunched(BallController launchedBall)
    {
        if (!CanLaunchBall)
        {
            return;
        }

        RegisterActiveBall(launchedBall != null ? launchedBall : ball);
        CurrentState = GameState.Playing;

        if (uiManager != null)
        {
            uiManager.ShowPlaying();
        }
    }

    public void NotifyBlockDestroyed()
    {
        if (IsStageFinished)
        {
            return;
        }

        destroyedBlocks = Mathf.Min(GetSafeTotalBlocks(), destroyedBlocks + 1);
        remainingBlocks = Mathf.Max(0, remainingBlocks - 1);
        AddScore(scorePerBlock);

        if (uiManager != null)
        {
            uiManager.SetScore(currentScore);
        }

        if (remainingBlocks <= 0)
        {
            SetClear();
        }
    }

    public void NotifyBallLost()
    {
        NotifyBallLost(ball);
    }

    public void NotifyBallLost(BallController lostBall)
    {
        if (IsStageFinished)
        {
            return;
        }

        CleanActiveBalls();

        if (lostBall != null)
        {
            activeBalls.Remove(lostBall);
        }

        bool hasOtherBalls = activeBalls.Count > 0;

        if (lostBall != null && lostBall != ball)
        {
            Destroy(lostBall.gameObject);
        }
        else if (lostBall == ball && hasOtherBalls)
        {
            lostBall.DeactivateAfterLoss();
        }

        if (hasOtherBalls)
        {
            return;
        }

        HandleAllBallsLost();
    }

    public void AddExtraBalls(int count)
    {
        if (count <= 0)
        {
            Debug.LogWarning($"AddExtraBalls ignored because count must be positive. Count: {count}");
            return;
        }

        if (CurrentState != GameState.Playing)
        {
            Debug.Log($"AddBalls ignored while game state is {CurrentState}.");
            return;
        }

        FindMissingReferences();

        BallController template = ballPrefab != null ? ballPrefab : ball;
        if (template == null)
        {
            Debug.LogWarning("Ball prefab not found. AddBalls could not be applied.");
            return;
        }

        Vector2 spawnPosition = GetExtraBallSpawnPosition();
        int addedCount = 0;

        for (int i = 0; i < count; i++)
        {
            BallController extraBall = Instantiate(template, spawnPosition, Quaternion.identity, ballsParent);
            if (extraBall == null)
            {
                continue;
            }

            extraBall.gameObject.name = $"ExtraBall_{i + 1}";
            extraBall.gameObject.SetActive(true);
            extraBall.Configure(paddle, this);
            extraBall.LaunchFrom(spawnPosition, GetExtraBallDirection(i, count), extraBallSpeed);
            RegisterActiveBall(extraBall);
            addedCount++;
        }

        if (addedCount <= 0)
        {
            Debug.LogWarning("AddBalls could not create any extra balls.");
            return;
        }

        Debug.Log($"AddBalls effect applied. Added: {addedCount}");
    }

    private void HandleAllBallsLost()
    {
        missCount++;
        lives = Mathf.Max(0, lives - 1);

        if (uiManager != null)
        {
            uiManager.SetLives(lives);
        }

        if (lives <= 0)
        {
            SetGameOver();
            return;
        }

        CurrentState = GameState.ReadyToLaunch;

        if (uiManager != null)
        {
            uiManager.ShowPlaying();
        }

        if (ball != null)
        {
            activeBalls.Clear();
            ball.ResetToPaddle();
        }
    }

    public void AddLife(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"AddLife ignored because amount must be positive. Amount: {amount}");
            return;
        }

        if (CurrentState != GameState.Playing)
        {
            Debug.Log($"Life up ignored while game state is {CurrentState}.");
            return;
        }

        lives += amount;

        if (uiManager == null && autoFindReferences)
        {
            FindMissingReferences();
        }

        if (uiManager != null)
        {
            uiManager.SetLives(lives);
        }

        Debug.Log($"Life up applied. Lives: {lives}");
    }

    public void RestartScene()
    {
        CleanupStageObjects();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToStageSelect()
    {
        SceneLoader.LoadScene(SceneLoader.StageSelectSceneName);
    }

    private void SetClear()
    {
        CurrentState = GameState.Clear;
        Debug.Log("Stage clear.");
        bool unlockedNextStage = hasNextStage;
        bool isFinalStage = !hasNextStage;
        bool achievedClearIcon = true;
        bool achievedNoMissIcon = missCount == 0;
        bool achievedScoreIcon = currentScore >= iconScoreTarget;

        if (hasNextStage)
        {
            StageUnlockManager.UnlockNextStage(stageId);
        }

        AwardStageIconsOnClear(achievedClearIcon, achievedNoMissIcon, achievedScoreIcon);
        CleanupStageObjects();

        if (uiManager != null)
        {
            uiManager.ShowClear(
                unlockedNextStage,
                isFinalStage,
                currentScore,
                destroyedBlocks,
                GetSafeTotalBlocks(),
                lives,
                GetClearRank(),
                icon1Label,
                achievedClearIcon,
                icon2Label,
                achievedNoMissIcon,
                icon3Label,
                achievedScoreIcon);
        }
    }

    private void SetGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("Game over.");

        CleanupStageObjects();

        if (uiManager != null)
        {
            uiManager.ShowGameOver(
                currentScore,
                destroyedBlocks,
                GetSafeTotalBlocks(),
                lives,
                GetGameOverRank());
        }
    }

    private void ResetResultStats()
    {
        totalBlocks = 0;
        remainingBlocks = 0;
        destroyedBlocks = 0;
        currentScore = 0;
        missCount = 0;
    }

    private int GetSafeTotalBlocks()
    {
        return Mathf.Max(totalBlocks, destroyedBlocks);
    }

    private string GetClearRank()
    {
        if (lives >= 3)
        {
            return "S";
        }

        if (lives == 2)
        {
            return "A";
        }

        return "B";
    }

    private string GetGameOverRank()
    {
        int safeTotalBlocks = GetSafeTotalBlocks();
        if (safeTotalBlocks <= 0)
        {
            return "C";
        }

        float destroyedRate = destroyedBlocks / (float)safeTotalBlocks;
        if (destroyedRate >= 0.8f)
        {
            return "A";
        }

        if (destroyedRate >= 0.5f)
        {
            return "B";
        }

        return "C";
    }

    private void AddScore(int amount)
    {
        currentScore = Mathf.Max(0, currentScore + Mathf.Max(0, amount));
    }

    private void AwardStageIconsOnClear(bool achievedClearIcon, bool achievedNoMissIcon, bool achievedScoreIcon)
    {
        bool updated = false;

        if (achievedClearIcon)
        {
            updated |= StageIconProgressManager.SetIconAcquiredIfNeeded(stageId, StageIconProgressManager.ClearIconIndex);
        }

        if (achievedNoMissIcon)
        {
            updated |= StageIconProgressManager.SetIconAcquiredIfNeeded(stageId, StageIconProgressManager.NoMissIconIndex);
        }

        if (achievedScoreIcon)
        {
            updated |= StageIconProgressManager.SetIconAcquiredIfNeeded(stageId, StageIconProgressManager.ScoreIconIndex);
        }

        if (updated)
        {
            StageIconProgressManager.Save();
        }

        RefreshStageIconDisplay();
    }

    private void RefreshStageIconDisplay()
    {
        if (uiManager == null && autoFindReferences)
        {
            FindMissingReferences();
        }

        if (uiManager == null)
        {
            return;
        }

        uiManager.SetStageIcons(
            icon1Label,
            StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.ClearIconIndex),
            icon1Description,
            icon2Label,
            StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.NoMissIconIndex),
            icon2Description,
            icon3Label,
            StageIconProgressManager.IsIconAcquired(stageId, StageIconProgressManager.ScoreIconIndex),
            icon3Description,
            iconScoreTarget);
    }

    private static string GetSafeIconLabel(string label, string fallback)
    {
        return string.IsNullOrWhiteSpace(label) ? fallback : label.Trim();
    }

    private void FindMissingReferences()
    {
        if (ball == null)
        {
            ball = FindObjectOfType<BallController>();
        }

        if (ballPrefab == null)
        {
            ballPrefab = ball;
        }

        if (paddle == null)
        {
            PaddleController paddleController = FindObjectOfType<PaddleController>();
            if (paddleController != null)
            {
                paddle = paddleController.transform;
            }
        }

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void RegisterActiveBall(BallController activeBall)
    {
        if (activeBall == null)
        {
            return;
        }

        CleanActiveBalls();

        if (!activeBalls.Contains(activeBall))
        {
            activeBalls.Add(activeBall);
        }
    }

    private void CleanActiveBalls()
    {
        activeBalls.RemoveAll(activeBall => activeBall == null);
    }

    private Vector2 GetExtraBallSpawnPosition()
    {
        BallController sourceBall = GetExtraBallSource();
        if (sourceBall != null)
        {
            return sourceBall.transform.position;
        }

        if (paddle != null)
        {
            return (Vector2)paddle.position + new Vector2(0f, 0.45f);
        }

        if (ball != null)
        {
            return ball.transform.position;
        }

        return Vector2.zero;
    }

    private BallController GetExtraBallSource()
    {
        CleanActiveBalls();

        for (int i = 0; i < activeBalls.Count; i++)
        {
            BallController activeBall = activeBalls[i];
            if (activeBall != null && activeBall.gameObject.activeInHierarchy && !activeBall.IsLost)
            {
                return activeBall;
            }
        }

        if (ball != null && ball.gameObject.activeInHierarchy)
        {
            return ball;
        }

        return null;
    }

    private Vector2 GetExtraBallDirection(int index, int count)
    {
        if (count <= 1)
        {
            return Vector2.up;
        }

        int pairIndex = index / 2;
        float side = index % 2 == 0 ? -1f : 1f;
        float angle = extraBallLaunchAngle * (pairIndex + 1) * side;
        return Quaternion.Euler(0f, 0f, angle) * Vector2.up;
    }

    private void CleanupStageObjects()
    {
        ItemController[] items = Resources.FindObjectsOfTypeAll<ItemController>();

        for (int i = 0; i < items.Length; i++)
        {
            ItemController currentItem = items[i];
            if (!IsRuntimeSceneObject(currentItem))
            {
                continue;
            }

            currentItem.gameObject.SetActive(false);
            Destroy(currentItem.gameObject);
        }

        BallController[] balls = Resources.FindObjectsOfTypeAll<BallController>();

        for (int i = 0; i < balls.Length; i++)
        {
            BallController currentBall = balls[i];
            if (!IsRuntimeSceneObject(currentBall))
            {
                continue;
            }

            currentBall.StopBall();
            currentBall.gameObject.SetActive(false);
            Destroy(currentBall.gameObject);
        }

        activeBalls.Clear();
    }

    private static bool IsRuntimeSceneObject(Component component)
    {
        return component != null
            && component.gameObject.scene.IsValid()
            && component.gameObject.scene.isLoaded;
    }

    private void OnValidate()
    {
        stageId = Mathf.Max(1, stageId);
        scorePerBlock = Mathf.Max(0, scorePerBlock);
        icon1Label = GetSafeIconLabel(icon1Label, "C");
        icon1Description = GetSafeIconDescription(icon1Description, "Clear");
        icon2Label = GetSafeIconLabel(icon2Label, "N");
        icon2Description = GetSafeIconDescription(icon2Description, "No Miss");
        icon3Label = GetSafeIconLabel(icon3Label, "S");
        icon3Description = GetSafeIconDescription(icon3Description, "Score");
        iconScoreTarget = Mathf.Max(0, iconScoreTarget);
        extraBallLaunchAngle = Mathf.Max(0f, extraBallLaunchAngle);
        extraBallSpeed = Mathf.Max(0.1f, extraBallSpeed);
    }

    private static string GetSafeIconDescription(string description, string fallback)
    {
        return string.IsNullOrWhiteSpace(description) ? fallback : description.Trim();
    }
}
