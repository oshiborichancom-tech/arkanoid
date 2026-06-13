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
    [SerializeField] private string stageName = "Stage 1";
    [SerializeField] private BallController ball;
    [SerializeField] private BallController ballPrefab;
    [SerializeField] private Transform paddle;
    [SerializeField] private Transform ballsParent;
    [SerializeField] private float extraBallLaunchAngle = 25f;
    [SerializeField] private float extraBallSpeed = 7f;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool autoFindReferences = true;

    private int lives;
    private int remainingBlocks;
    private readonly List<BallController> activeBalls = new List<BallController>();

    public GameState CurrentState { get; private set; }
    public bool CanLaunchBall => CurrentState == GameState.ReadyToLaunch;

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
        ball = ballController;
        uiManager = manager;
        stageName = displayStageName;
        initialLives = Mathf.Max(1, livesCount);
        lives = initialLives;
        CurrentState = GameState.ReadyToLaunch;
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
        remainingBlocks += safeCount;
        Debug.Log($"Registered {safeCount} blocks. Remaining blocks: {remainingBlocks}");
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
        if (CurrentState == GameState.Clear || CurrentState == GameState.GameOver)
        {
            return;
        }

        remainingBlocks = Mathf.Max(0, remainingBlocks - 1);

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
        if (CurrentState == GameState.Clear || CurrentState == GameState.GameOver)
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

        StopAllBalls();

        if (uiManager != null)
        {
            uiManager.ShowClear();
        }
    }

    private void SetGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("Game over.");

        StopAllBalls();

        if (uiManager != null)
        {
            uiManager.ShowGameOver();
        }
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

    private void StopAllBalls()
    {
        CleanActiveBalls();

        for (int i = activeBalls.Count - 1; i >= 0; i--)
        {
            BallController activeBall = activeBalls[i];
            if (activeBall == null)
            {
                continue;
            }

            if (activeBall == ball)
            {
                activeBall.StopBall();
                continue;
            }

            Destroy(activeBall.gameObject);
        }

        activeBalls.Clear();

        if (ball != null)
        {
            ball.StopBall();
        }
    }

    private void OnValidate()
    {
        extraBallLaunchAngle = Mathf.Max(0f, extraBallLaunchAngle);
        extraBallSpeed = Mathf.Max(0.1f, extraBallSpeed);
    }
}
