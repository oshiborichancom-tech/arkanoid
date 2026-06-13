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
    [SerializeField] private UIManager uiManager;
    [SerializeField] private bool autoFindReferences = true;

    private int lives;
    private int remainingBlocks;

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

    public void RegisterBlocks(int blockCount)
    {
        int safeCount = Mathf.Max(0, blockCount);
        remainingBlocks += safeCount;
        Debug.Log($"Registered {safeCount} blocks. Remaining blocks: {remainingBlocks}");
    }

    public void NotifyBallLaunched()
    {
        if (!CanLaunchBall)
        {
            return;
        }

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
        if (CurrentState == GameState.Clear || CurrentState == GameState.GameOver)
        {
            return;
        }

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

        if (ball != null)
        {
            ball.StopBall();
        }

        if (uiManager != null)
        {
            uiManager.ShowClear();
        }
    }

    private void SetGameOver()
    {
        CurrentState = GameState.GameOver;
        Debug.Log("Game over.");

        if (ball != null)
        {
            ball.StopBall();
        }

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

        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }
}
