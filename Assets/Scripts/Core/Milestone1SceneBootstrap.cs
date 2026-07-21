using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Milestone1SceneBootstrap : MonoBehaviour
{
    private const string DefaultStageName = "Stage 1";
    private const int DefaultBlockRows = 5;
    private const int DefaultBlockColumns = 10;
    private const float DefaultBlockSize = 0.6f;
    private const float DefaultBlockSpacing = 0.01f;
    private const float DefaultBallSpeed = 7f;
    private const float DefaultPaddleSpeed = 9f;
    private const int DefaultInitialLives = 3;
    private const int DefaultAddBallsCount = 2;
    private const float DefaultAddBallSpeed = 7f;
    private const string DefaultIcon1Label = "C";
    private const string DefaultIcon2Label = "L";
    private const string DefaultIcon3Label = "S";
    private const int DefaultIconScoreTarget = 3000;
    private const int BackgroundSortingOrder = -20;
    private static readonly Vector2 DefaultBlockStartPosition = new Vector2(-3.24f, 3.25f);
    private static readonly Vector2 DefaultPlayAreaCenter = Vector2.zero;
    private static readonly Vector2 DefaultPlayAreaSize = new Vector2(10f, 9.6f);

    private enum SceneKind
    {
        Title,
        StageSelect,
        Game
    }

    [SerializeField] private SceneKind sceneKind = SceneKind.Title;
    [SerializeField] private StageData stageData;
    [SerializeField] private StageDatabase stageDatabase;
    [SerializeField] private string stageName = DefaultStageName;
    [SerializeField] private Sprite fallbackBackgroundSprite;
    [SerializeField] private int blockRows = DefaultBlockRows;
    [SerializeField] private int blockColumns = DefaultBlockColumns;
    [SerializeField] private float blockSize = DefaultBlockSize;
    [SerializeField] private float blockSpacing = DefaultBlockSpacing;
    [SerializeField] private Vector2 blockStartPosition = DefaultBlockStartPosition;
    [SerializeField] private float ballSpeed = DefaultBallSpeed;
    [SerializeField] private float paddleSpeed = DefaultPaddleSpeed;
    [SerializeField] private int initialLives = DefaultInitialLives;
    [SerializeField, Range(0f, 1f)] private float itemDropChance = 0.5f;
    [SerializeField] private float paddleExpandMultiplier = 1.5f;
    [SerializeField] private float paddleExpandDuration = 8f;
    [SerializeField] private int addBallsCount = DefaultAddBallsCount;
    [SerializeField] private float addBallLaunchAngle = 25f;
    [SerializeField] private float addBallSpeed = DefaultAddBallSpeed;
    [SerializeField] private Vector2 playAreaCenter = DefaultPlayAreaCenter;
    [SerializeField] private Vector2 playAreaSize = DefaultPlayAreaSize;
    [SerializeField] private float playAreaWallThickness = 0.3f;
    [SerializeField] private float ballLostPadding = 0.55f;
    [SerializeField] private bool showPlayAreaDebugFrame = true;
    [SerializeField] private Color playAreaDebugFrameColor = new Color(0.78f, 0.88f, 1f, 0.34f);
    [SerializeField] private float playAreaDebugFrameThickness = 0.035f;

    private static Sprite squareSprite;
    private static Sprite ballSprite;
    private static Sprite backgroundSprite;
    private static Font defaultFont;

    private struct StageRuntimeSettings
    {
        public int StageId;
        public bool HasNextStage;
        public string StageName;
        public Sprite BackgroundSprite;
        public BackgroundFitMode BackgroundFitMode;
        public Vector2 BackgroundOffset;
        public Vector2 BackgroundScaleMultiplier;
        public int BlockRows;
        public int BlockColumns;
        public float BlockSize;
        public float BlockSpacing;
        public Vector2 BlockStartPosition;
        public bool UseSingleBlockColor;
        public Color SingleBlockColor;
        public bool UseManualBlockLayout;
        public string[] BlockLayout;
        public float BallSpeed;
        public float PaddleSpeed;
        public int InitialLives;
        public string Icon1Label;
        public string Icon2Label;
        public string Icon3Label;
        public int IconScoreTarget;
        public float ItemDropChance;
        public float PaddleExpandMultiplier;
        public float PaddleExpandDuration;
        public int AddBallsCount;
        public float AddBallLaunchAngle;
        public float AddBallSpeed;
        public Vector2 PlayAreaCenter;
        public Vector2 PlayAreaSize;
        public float PlayAreaWallThickness;
        public float BallLostPadding;
        public bool ShowPlayAreaDebugFrame;
        public Color PlayAreaDebugFrameColor;
        public float PlayAreaDebugFrameThickness;
    }

    private void Awake()
    {
        EnsureSharedAssets();

        switch (sceneKind)
        {
            case SceneKind.Title:
                BuildTitleScene();
                break;
            case SceneKind.StageSelect:
                BuildStageSelectScene();
                break;
            case SceneKind.Game:
                BuildGameScene(CreateStageSettings());
                break;
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(stageName))
        {
            stageName = DefaultStageName;
        }

        blockRows = blockRows > 0 ? blockRows : DefaultBlockRows;
        blockColumns = blockColumns > 0 ? blockColumns : DefaultBlockColumns;
        blockSize = blockSize > 0f ? blockSize : DefaultBlockSize;
        blockSpacing = Mathf.Max(0f, blockSpacing);
        ballSpeed = ballSpeed > 0f ? ballSpeed : DefaultBallSpeed;
        paddleSpeed = paddleSpeed > 0f ? paddleSpeed : DefaultPaddleSpeed;
        initialLives = initialLives > 0 ? initialLives : DefaultInitialLives;
        itemDropChance = Mathf.Clamp01(itemDropChance);
        paddleExpandMultiplier = Mathf.Max(1f, paddleExpandMultiplier);
        paddleExpandDuration = Mathf.Max(0f, paddleExpandDuration);
        addBallsCount = addBallsCount > 0 ? addBallsCount : DefaultAddBallsCount;
        addBallLaunchAngle = Mathf.Max(0f, addBallLaunchAngle);
        addBallSpeed = addBallSpeed > 0f ? addBallSpeed : DefaultAddBallSpeed;
        playAreaSize = new Vector2(
            Mathf.Max(1f, playAreaSize.x),
            Mathf.Max(1f, playAreaSize.y));
        playAreaWallThickness = Mathf.Max(0.05f, playAreaWallThickness);
        ballLostPadding = Mathf.Max(0f, ballLostPadding);
        playAreaDebugFrameThickness = Mathf.Max(0.005f, playAreaDebugFrameThickness);
    }

    private StageRuntimeSettings CreateStageSettings()
    {
        StageRuntimeSettings settings = new StageRuntimeSettings
        {
            StageId = 1,
            HasNextStage = true,
            StageName = string.IsNullOrWhiteSpace(stageName) ? DefaultStageName : stageName,
            BackgroundSprite = fallbackBackgroundSprite,
            BackgroundFitMode = BackgroundFitMode.Stretch,
            BackgroundOffset = Vector2.zero,
            BackgroundScaleMultiplier = Vector2.one,
            BlockRows = blockRows > 0 ? blockRows : DefaultBlockRows,
            BlockColumns = blockColumns > 0 ? blockColumns : DefaultBlockColumns,
            BlockSize = blockSize > 0f ? blockSize : DefaultBlockSize,
            BlockSpacing = Mathf.Max(0f, blockSpacing),
            BlockStartPosition = blockStartPosition,
            UseSingleBlockColor = true,
            SingleBlockColor = new Color(0.75f, 0.75f, 0.75f, 1f),
            UseManualBlockLayout = false,
            BlockLayout = null,
            BallSpeed = ballSpeed > 0f ? ballSpeed : DefaultBallSpeed,
            PaddleSpeed = paddleSpeed > 0f ? paddleSpeed : DefaultPaddleSpeed,
            InitialLives = initialLives > 0 ? initialLives : DefaultInitialLives,
            Icon1Label = DefaultIcon1Label,
            Icon2Label = DefaultIcon2Label,
            Icon3Label = DefaultIcon3Label,
            IconScoreTarget = DefaultIconScoreTarget,
            ItemDropChance = Mathf.Clamp01(itemDropChance),
            PaddleExpandMultiplier = Mathf.Max(1f, paddleExpandMultiplier),
            PaddleExpandDuration = Mathf.Max(0f, paddleExpandDuration),
            AddBallsCount = addBallsCount > 0 ? addBallsCount : DefaultAddBallsCount,
            AddBallLaunchAngle = Mathf.Max(0f, addBallLaunchAngle),
            AddBallSpeed = addBallSpeed > 0f ? addBallSpeed : DefaultAddBallSpeed,
            PlayAreaCenter = playAreaCenter,
            PlayAreaSize = GetSafePlayAreaSize(playAreaSize),
            PlayAreaWallThickness = Mathf.Max(0.05f, playAreaWallThickness),
            BallLostPadding = Mathf.Max(0f, ballLostPadding),
            ShowPlayAreaDebugFrame = showPlayAreaDebugFrame,
            PlayAreaDebugFrameColor = playAreaDebugFrameColor,
            PlayAreaDebugFrameThickness = Mathf.Max(0.005f, playAreaDebugFrameThickness)
        };

        StageData effectiveStageData = StageSelectionContext.SelectedStageData != null
            ? StageSelectionContext.SelectedStageData
            : stageData;

        if (effectiveStageData == null)
        {
            return settings;
        }

        settings.StageId = effectiveStageData.StageId;
        settings.StageName = effectiveStageData.StageName;
        settings.BackgroundSprite = effectiveStageData.BackgroundSprite != null ? effectiveStageData.BackgroundSprite : settings.BackgroundSprite;
        settings.BackgroundFitMode = effectiveStageData.BackgroundFitMode;
        settings.BackgroundOffset = effectiveStageData.BackgroundOffset;
        settings.BackgroundScaleMultiplier = effectiveStageData.BackgroundScaleMultiplier;
        settings.BlockRows = effectiveStageData.BlockRows;
        settings.BlockColumns = effectiveStageData.BlockColumns;
        settings.BlockSize = effectiveStageData.BlockSize;
        settings.BlockSpacing = effectiveStageData.BlockSpacing;
        settings.BlockStartPosition = effectiveStageData.BlockStartPosition;
        settings.UseSingleBlockColor = effectiveStageData.UseSingleBlockColor;
        settings.SingleBlockColor = effectiveStageData.SingleBlockColor;
        settings.UseManualBlockLayout = effectiveStageData.UseManualBlockLayout;
        settings.BlockLayout = effectiveStageData.BlockLayout;
        settings.BallSpeed = effectiveStageData.BallSpeed;
        settings.PaddleSpeed = effectiveStageData.PaddleSpeed;
        settings.InitialLives = effectiveStageData.InitialLives;
        settings.Icon1Label = effectiveStageData.Icon1Label;
        settings.Icon2Label = effectiveStageData.Icon2Label;
        settings.Icon3Label = effectiveStageData.Icon3Label;
        settings.IconScoreTarget = effectiveStageData.IconScoreTarget;
        settings.ItemDropChance = effectiveStageData.ItemDropChance;
        settings.PaddleExpandMultiplier = effectiveStageData.PaddleExpandMultiplier;
        settings.PaddleExpandDuration = effectiveStageData.PaddleExpandDuration;
        settings.AddBallsCount = effectiveStageData.AddBallsCount;
        settings.AddBallLaunchAngle = effectiveStageData.AddBallLaunchAngle;
        settings.AddBallSpeed = effectiveStageData.AddBallSpeed;

        if (stageDatabase != null && stageDatabase.Stages.Count > 0)
        {
            settings.HasNextStage = stageDatabase.HasStageAfter(settings.StageId);
        }

        return settings;
    }

    private static void EnsureSharedAssets()
    {
        if (defaultFont == null)
        {
            defaultFont = GetDefaultFontSafe();
        }

        if (squareSprite == null)
        {
            squareSprite = Sprite.Create(CreateSolidTexture(64, 64, Color.white), new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
        }

        if (ballSprite == null)
        {
            ballSprite = Sprite.Create(CreateCircleTexture(64, new Color(1f, 0.96f, 0.70f, 1f)), new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
        }

        if (backgroundSprite == null)
        {
            backgroundSprite = Sprite.Create(CreateBackgroundTexture(160, 90), new Rect(0, 0, 160, 90), new Vector2(0.5f, 0.5f), 10f);
        }
    }

    private static Font GetDefaultFontSafe()
    {
        try
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Built-in UI font could not be loaded. UI will keep Unity's default Text font. {exception.Message}");
            return null;
        }
    }

    private static Texture2D CreateSolidTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius + 1.2f - distance);
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateBackgroundTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color top = new Color(0.08f, 0.17f, 0.28f, 1f);
        Color bottom = new Color(0.02f, 0.33f, 0.38f, 1f);

        for (int y = 0; y < height; y++)
        {
            Color rowColor = Color.Lerp(bottom, top, y / (float)(height - 1));

            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, rowColor);
            }
        }

        texture.Apply();
        return texture;
    }

    private static void BuildTitleScene()
    {
        CreateCamera(new Color(0.05f, 0.09f, 0.15f, 1f));
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        CreateText(canvas.transform, "TitleText", "ARKANOID", 76, new Color(0.95f, 0.98f, 1f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(760f, 120f));

        Button startButton = CreateButton(canvas.transform, "StartButton", "Start",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(320f, 90f));
        startButton.onClick.AddListener(loader.LoadStageSelect);
    }

    private void BuildStageSelectScene()
    {
        CreateCamera(new Color(0.05f, 0.09f, 0.15f, 1f));
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        CreateText(canvas.transform, "StageSelectTitle", "Stage Select", 64, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(760f, 100f));

        int buttonCount = CreateStageButtonsFromDatabase(canvas.transform);
        float backButtonY = buttonCount > 0 ? 70f - buttonCount * 95f - 15f : -85f;

        Button backButton = CreateButton(canvas.transform, "BackButton", "Back",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, backButtonY), new Vector2(260f, 72f));
        backButton.onClick.AddListener(loader.LoadTitle);
    }

    private int CreateStageButtonsFromDatabase(Transform parent)
    {
        if (stageDatabase == null)
        {
            Debug.LogWarning("StageSelectScene needs a StageDatabase.");
            return 0;
        }

        IReadOnlyList<StageData> stages = stageDatabase.Stages;
        if (stages == null || stages.Count <= 0)
        {
            Debug.LogWarning("StageDatabase has no stages.");
            return 0;
        }

        int buttonCount = 0;
        for (int i = 0; i < stages.Count; i++)
        {
            StageData currentStageData = stages[i];
            if (currentStageData == null)
            {
                Debug.LogWarning($"StageDatabase contains a null StageData at index {i}. It will be skipped.");
                continue;
            }

            bool isUnlocked = StageUnlockManager.IsStageUnlocked(currentStageData.StageId);

            CreateStageSelectButton(
                parent,
                $"StageButton_{i + 1}",
                GetStageSelectLabel(currentStageData, isUnlocked),
                currentStageData,
                isUnlocked,
                new Vector2(0f, 70f - buttonCount * 95f));
            buttonCount++;
        }

        if (buttonCount <= 0)
        {
            Debug.LogWarning("StageDatabase has no valid StageData entries.");
            return 0;
        }

        return buttonCount;
    }

    private static string GetStageSelectLabel(StageData stageData, bool isUnlocked)
    {
        if (stageData == null)
        {
            return "Stage";
        }

        return isUnlocked ? stageData.StageName : $"{stageData.StageName} (Locked)";
    }

    private static void CreateStageSelectButton(
        Transform parent,
        string name,
        string label,
        StageData data,
        bool isUnlocked,
        Vector2 anchoredPosition)
    {
        Button stageButton = CreateButton(parent, name, label,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            anchoredPosition,
            new Vector2(360f, 84f));
        StageSelectButton stageSelectButton = stageButton.gameObject.AddComponent<StageSelectButton>();
        stageSelectButton.Configure(data, isUnlocked);
        stageButton.onClick.AddListener(stageSelectButton.SelectStageAndLoadGame);
    }

    private static void BuildGameScene(StageRuntimeSettings settings)
    {
        Physics2D.gravity = Vector2.zero;
        Camera camera = CreateCamera(new Color(0.04f, 0.06f, 0.08f, 1f));
        PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D("M1_Bouncy_Runtime")
        {
            friction = 0f,
            bounciness = 1f
        };

        Rect playAreaBounds = GetPlayAreaBounds(settings);

        CreateBackground(settings, playAreaBounds);
        CreatePlayAreaWalls(playAreaBounds, settings.PlayAreaWallThickness, bouncyMaterial);
        CreatePlayAreaDebugFrame(playAreaBounds, settings);

        GameManager gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        ItemEffectManager itemEffectManager = new GameObject("ItemEffectManager").AddComponent<ItemEffectManager>();
        GameObject paddle = CreatePaddle(camera, bouncyMaterial, settings.PaddleSpeed, playAreaBounds);
        PaddleController paddleController = paddle.GetComponent<PaddleController>();
        itemEffectManager.Configure(
            paddleController,
            gameManager,
            settings.PaddleExpandMultiplier,
            settings.PaddleExpandDuration,
            settings.AddBallsCount);
        GameObject ballsParent = new GameObject("Balls");
        GameObject ball = CreateBall(paddle.transform, gameManager, bouncyMaterial, settings.BallSpeed, playAreaBounds, settings.BallLostPadding);
        ball.transform.SetParent(ballsParent.transform);

        Canvas canvas = CreateCanvas();
        CreateEventSystem();
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();

        CreateGameHudLayout(canvas.transform, out RectTransform leftPanel, out _, out RectTransform rightPanel);

        CreateText(leftPanel, "StatusTitleText", "STATUS", 34, new Color(0.94f, 0.97f, 1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(340f, 58f), new Vector2(0.5f, 1f));

        Text stageNameText = CreateText(leftPanel, "StageNameText", $"STAGE: {settings.StageName}", 28, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -120f), new Vector2(340f, 70f), new Vector2(0.5f, 1f));

        Text scoreText = CreateText(leftPanel, "ScoreText", "SCORE: 0", 28, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), new Vector2(340f, 58f), new Vector2(0.5f, 1f));

        Text livesText = CreateText(leftPanel, "LivesText", "LIFE\n-", 28, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -292f), new Vector2(340f, 92f), new Vector2(0.5f, 1f));

        Button backButton = CreateButton(leftPanel, "BackToStageSelectButton", "Stage Select",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(260f, 64f), new Vector2(0.5f, 0f));
        backButton.onClick.AddListener(loader.LoadStageSelect);

        CreateIconPanelContents(rightPanel, out Text icon1Text, out Text icon2Text, out Text icon3Text);

        Text clearText = CreateText(canvas.transform, "ClearText", UIManager.ClearMessage, 36, new Color(0.98f, 0.92f, 0.30f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1080f, 420f));
        clearText.gameObject.SetActive(false);

        Text gameOverText = CreateText(canvas.transform, "GameOverText", UIManager.GameOverMessage, 36, new Color(1f, 0.42f, 0.42f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1080f, 420f));
        gameOverText.gameObject.SetActive(false);

        UIManager uiManager = new GameObject("UIManager").AddComponent<UIManager>();
        uiManager.Configure(livesText, stageNameText, scoreText, clearText, gameOverText, icon1Text, icon2Text, icon3Text);
        gameManager.Configure(ball.GetComponent<BallController>(), uiManager, settings.StageName, settings.InitialLives, settings.StageId, settings.HasNextStage);
        gameManager.ConfigureStageIcons(settings.Icon1Label, settings.Icon2Label, settings.Icon3Label, settings.IconScoreTarget);
        gameManager.ConfigureBallSpawning(
            ball.GetComponent<BallController>(),
            paddle.transform,
            ballsParent.transform,
            settings.AddBallLaunchAngle,
            settings.AddBallSpeed);

        CreateBlockGrid(gameManager, bouncyMaterial, itemEffectManager, settings, playAreaBounds);
    }

    private static Camera CreateCamera(Color backgroundColor)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.orthographic = true;
        camera.orthographicSize = 5f;

        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateBackground(StageRuntimeSettings settings, Rect playAreaBounds)
    {
        GameObject background = new GameObject("Background");
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        bool hasStageBackground = settings.BackgroundSprite != null;
        renderer.sprite = hasStageBackground ? settings.BackgroundSprite : backgroundSprite;
        renderer.sortingOrder = BackgroundSortingOrder;

        FitBackgroundToPlayArea(background.transform, renderer, settings, playAreaBounds, hasStageBackground);
    }

    private static void FitBackgroundToPlayArea(
        Transform backgroundTransform,
        SpriteRenderer renderer,
        StageRuntimeSettings settings,
        Rect playAreaBounds,
        bool logAppliedBackground)
    {
        if (backgroundTransform == null)
        {
            return;
        }

        Vector2 targetSize = playAreaBounds.size;
        Vector2 backgroundPosition = playAreaBounds.center + settings.BackgroundOffset;
        backgroundTransform.position = new Vector3(backgroundPosition.x, backgroundPosition.y, 1f);
        backgroundTransform.localScale = Vector3.one;

        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        Vector2 scale = CalculateBackgroundScale(targetSize, spriteSize, settings.BackgroundFitMode);
        Vector2 scaleMultiplier = GetSafeBackgroundScaleMultiplier(settings.BackgroundScaleMultiplier);
        Vector3 finalScale = new Vector3(
            Mathf.Max(0.01f, scale.x * scaleMultiplier.x),
            Mathf.Max(0.01f, scale.y * scaleMultiplier.y),
            1f);
        backgroundTransform.localScale = finalScale;

        if (logAppliedBackground)
        {
            Debug.Log($"Background applied: sprite={renderer.sprite.name}, mode={settings.BackgroundFitMode}, playAreaSize={targetSize}, spriteSize={spriteSize}, scale={finalScale}, offset={settings.BackgroundOffset}");
        }
    }

    private static Vector2 CalculateBackgroundScale(Vector2 gridSize, Vector2 spriteSize, BackgroundFitMode fitMode)
    {
        float scaleX = gridSize.x / spriteSize.x;
        float scaleY = gridSize.y / spriteSize.y;

        switch (fitMode)
        {
            case BackgroundFitMode.Fit:
            {
                float fitScale = Mathf.Min(scaleX, scaleY);
                return new Vector2(fitScale, fitScale);
            }
            case BackgroundFitMode.Cover:
            {
                float coverScale = Mathf.Max(scaleX, scaleY);
                return new Vector2(coverScale, coverScale);
            }
            case BackgroundFitMode.Stretch:
                return new Vector2(scaleX, scaleY);
            default:
                return new Vector2(scaleX, scaleY);
        }
    }

    private static Vector2 GetSafeBackgroundScaleMultiplier(Vector2 value)
    {
        return new Vector2(Mathf.Max(0.01f, value.x), Mathf.Max(0.01f, value.y));
    }

    private static Vector2 GetSafePlayAreaSize(Vector2 size)
    {
        return new Vector2(Mathf.Max(1f, size.x), Mathf.Max(1f, size.y));
    }

    private static Rect GetPlayAreaBounds(StageRuntimeSettings settings)
    {
        Vector2 safeSize = GetSafePlayAreaSize(settings.PlayAreaSize);
        return new Rect(settings.PlayAreaCenter - safeSize * 0.5f, safeSize);
    }

    private static Vector2 GetBlockGridSize(StageRuntimeSettings settings)
    {
        float safeBlockSize = Mathf.Max(0.1f, settings.BlockSize);
        float safeSpacing = Mathf.Max(0f, settings.BlockSpacing);
        GetEffectiveBlockGridDimensions(settings, out int safeRows, out int safeColumns);

        float width = safeColumns * safeBlockSize + (safeColumns - 1) * safeSpacing;
        float height = safeRows * safeBlockSize + (safeRows - 1) * safeSpacing;
        return new Vector2(width, height);
    }

    private static Vector2 GetBlockGridCenter(StageRuntimeSettings settings)
    {
        float step = Mathf.Max(0.1f, settings.BlockSize) + Mathf.Max(0f, settings.BlockSpacing);
        GetEffectiveBlockGridDimensions(settings, out int safeRows, out int safeColumns);

        return settings.BlockStartPosition + new Vector2(
            (safeColumns - 1) * step * 0.5f,
            -(safeRows - 1) * step * 0.5f);
    }

    private static Vector2 GetBlockStartPositionWithinPlayArea(StageRuntimeSettings settings, Rect playAreaBounds)
    {
        Vector2 gridSize = GetBlockGridSize(settings);
        Vector2 gridCenter = GetBlockGridCenter(settings);
        Vector2 clampedCenter = gridCenter;

        float minCenterX = playAreaBounds.xMin + gridSize.x * 0.5f;
        float maxCenterX = playAreaBounds.xMax - gridSize.x * 0.5f;
        clampedCenter.x = minCenterX <= maxCenterX
            ? Mathf.Clamp(gridCenter.x, minCenterX, maxCenterX)
            : playAreaBounds.center.x;

        float minCenterY = playAreaBounds.yMin + gridSize.y * 0.5f;
        float maxCenterY = playAreaBounds.yMax - gridSize.y * 0.5f;
        clampedCenter.y = minCenterY <= maxCenterY
            ? Mathf.Clamp(gridCenter.y, minCenterY, maxCenterY)
            : playAreaBounds.center.y;

        float step = Mathf.Max(0.1f, settings.BlockSize) + Mathf.Max(0f, settings.BlockSpacing);
        GetEffectiveBlockGridDimensions(settings, out int safeRows, out int safeColumns);

        return clampedCenter - new Vector2(
            (safeColumns - 1) * step * 0.5f,
            -(safeRows - 1) * step * 0.5f);
    }

    private static void GetEffectiveBlockGridDimensions(StageRuntimeSettings settings, out int rows, out int columns)
    {
        if (TryGetManualBlockLayoutDimensions(settings, out rows, out columns))
        {
            return;
        }

        rows = Mathf.Max(1, settings.BlockRows);
        columns = Mathf.Max(1, settings.BlockColumns);
    }

    private static bool TryGetManualBlockLayoutDimensions(StageRuntimeSettings settings, out int rows, out int columns)
    {
        rows = 0;
        columns = 0;

        if (!settings.UseManualBlockLayout || settings.BlockLayout == null || settings.BlockLayout.Length <= 0)
        {
            return false;
        }

        bool hasBlock = false;
        rows = settings.BlockLayout.Length;

        for (int row = 0; row < settings.BlockLayout.Length; row++)
        {
            string rowText = settings.BlockLayout[row];
            if (rowText == null)
            {
                continue;
            }

            columns = Mathf.Max(columns, rowText.Length);
            for (int column = 0; column < rowText.Length; column++)
            {
                if (rowText[column] == '1')
                {
                    hasBlock = true;
                }
            }
        }

        return rows > 0 && columns > 0 && hasBlock;
    }

    private static void CreatePlayAreaWalls(Rect playAreaBounds, float wallThickness, PhysicsMaterial2D material)
    {
        float safeThickness = Mathf.Max(0.05f, wallThickness);
        float halfThickness = safeThickness * 0.5f;

        CreateWall("LeftWall",
            new Vector2(playAreaBounds.xMin - halfThickness, playAreaBounds.center.y),
            new Vector2(safeThickness, playAreaBounds.height + safeThickness * 2f),
            material);
        CreateWall("RightWall",
            new Vector2(playAreaBounds.xMax + halfThickness, playAreaBounds.center.y),
            new Vector2(safeThickness, playAreaBounds.height + safeThickness * 2f),
            material);
        CreateWall("TopWall",
            new Vector2(playAreaBounds.center.x, playAreaBounds.yMax + halfThickness),
            new Vector2(playAreaBounds.width + safeThickness * 2f, safeThickness),
            material);
    }

    private static void CreatePlayAreaDebugFrame(Rect playAreaBounds, StageRuntimeSettings settings)
    {
        if (!settings.ShowPlayAreaDebugFrame)
        {
            return;
        }

        float safeThickness = Mathf.Max(0.005f, settings.PlayAreaDebugFrameThickness);
        GameObject frameParent = new GameObject("PlayAreaDebugFrame");

        CreateDebugFrameLine(frameParent.transform, "Top",
            new Vector2(playAreaBounds.center.x, playAreaBounds.yMax),
            new Vector2(playAreaBounds.width, safeThickness),
            settings.PlayAreaDebugFrameColor);
        CreateDebugFrameLine(frameParent.transform, "Bottom",
            new Vector2(playAreaBounds.center.x, playAreaBounds.yMin),
            new Vector2(playAreaBounds.width, safeThickness),
            settings.PlayAreaDebugFrameColor);
        CreateDebugFrameLine(frameParent.transform, "Left",
            new Vector2(playAreaBounds.xMin, playAreaBounds.center.y),
            new Vector2(safeThickness, playAreaBounds.height),
            settings.PlayAreaDebugFrameColor);
        CreateDebugFrameLine(frameParent.transform, "Right",
            new Vector2(playAreaBounds.xMax, playAreaBounds.center.y),
            new Vector2(safeThickness, playAreaBounds.height),
            settings.PlayAreaDebugFrameColor);
    }

    private static void CreateDebugFrameLine(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        line.transform.position = position;
        line.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = line.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = color;
        renderer.sortingOrder = 25;
    }

    private static void CreateWall(string name, Vector2 position, Vector2 size, PhysicsMaterial2D material)
    {
        GameObject wall = new GameObject(name);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.74f, 0.84f, 0.92f, 0.28f);
        renderer.sortingOrder = -5;

        BoxCollider2D collider = wall.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = material;
    }

    private static GameObject CreatePaddle(Camera camera, PhysicsMaterial2D material, float speed, Rect playAreaBounds)
    {
        GameObject paddle = new GameObject("Paddle");
        paddle.transform.position = new Vector3(playAreaBounds.center.x, playAreaBounds.yMin + 0.85f, 0f);
        paddle.transform.localScale = new Vector3(2.2f, 0.32f, 1f);

        SpriteRenderer renderer = paddle.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.30f, 0.82f, 0.95f, 1f);
        renderer.sortingOrder = 10;

        BoxCollider2D collider = paddle.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = material;

        Rigidbody2D rigidbody = paddle.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        PaddleController controller = paddle.AddComponent<PaddleController>();
        controller.Configure(camera, speed);
        controller.ConfigurePlayArea(playAreaBounds.xMin, playAreaBounds.xMax);
        return paddle;
    }

    private static GameObject CreateBall(Transform paddle, GameManager gameManager, PhysicsMaterial2D material, float speed, Rect playAreaBounds, float lostPadding)
    {
        GameObject ball = new GameObject("Ball");
        ball.transform.position = new Vector3(playAreaBounds.center.x, playAreaBounds.yMin + 1.3f, 0f);
        ball.transform.localScale = new Vector3(0.34f, 0.34f, 1f);

        SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
        renderer.sprite = ballSprite;
        renderer.sortingOrder = 20;

        CircleCollider2D collider = ball.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.sharedMaterial = material;

        Rigidbody2D rigidbody = ball.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        BallController controller = ball.AddComponent<BallController>();
        controller.Configure(paddle, gameManager);
        controller.ConfigurePlayArea(playAreaBounds, lostPadding);
        controller.SetMoveSpeed(speed);
        return ball;
    }

    private static void CreateBlockGrid(
        GameManager gameManager,
        PhysicsMaterial2D material,
        ItemEffectManager itemEffectManager,
        StageRuntimeSettings settings,
        Rect playAreaBounds)
    {
        float safeDropChance = Mathf.Clamp01(settings.ItemDropChance);
        GameObject runtimePrefabs = new GameObject("RuntimePrefabs");
        ItemController itemPrefab = CreateItemPrefab(runtimePrefabs.transform);

        GameObject blockPrefab = new GameObject("BlockPrefab");
        blockPrefab.transform.SetParent(runtimePrefabs.transform);
        blockPrefab.SetActive(false);

        SpriteRenderer renderer = blockPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.95f, 0.28f, 0.34f, 1f);
        renderer.sortingOrder = 5;

        BoxCollider2D collider = blockPrefab.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = material;

        Block block = blockPrefab.AddComponent<Block>();
        block.ConfigureItemDrop(itemPrefab, safeDropChance, itemEffectManager);
        GameObject blocksParent = new GameObject("Blocks");

        BlockGridBuilder builder = new GameObject("BlockGridBuilder").AddComponent<BlockGridBuilder>();
        Vector2 adjustedBlockStartPosition = GetBlockStartPositionWithinPlayArea(settings, playAreaBounds);

        builder.Configure(
            block,
            gameManager,
            blocksParent.transform,
            settings.BlockRows,
            settings.BlockColumns,
            settings.BlockSize,
            settings.BlockSpacing,
            adjustedBlockStartPosition);
        builder.ConfigureItemDrops(itemPrefab, safeDropChance, itemEffectManager);
        builder.ConfigureBlockColor(settings.UseSingleBlockColor, settings.SingleBlockColor);
        builder.ConfigureManualBlockLayout(settings.UseManualBlockLayout, settings.BlockLayout);
    }

    private static ItemController CreateItemPrefab(Transform parent)
    {
        GameObject itemPrefab = new GameObject("ItemPrefab");
        itemPrefab.transform.SetParent(parent);
        itemPrefab.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
        itemPrefab.SetActive(false);

        SpriteRenderer renderer = itemPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.30f, 0.68f, 1f, 1f);
        renderer.sortingOrder = 15;
        CreateItemLabel(itemPrefab.transform, "P", renderer.sortingOrder + 1);

        CircleCollider2D collider = itemPrefab.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        Rigidbody2D rigidbody = itemPrefab.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        return itemPrefab.AddComponent<ItemController>();
    }

    private static TextMesh CreateItemLabel(Transform parent, string label, int sortingOrder)
    {
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = Vector3.zero;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one;

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = label;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontSize = 64;
        textMesh.characterSize = 0.12f;
        textMesh.color = Color.white;

        if (defaultFont != null)
        {
            textMesh.font = defaultFont;
        }

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = labelObject.AddComponent<MeshRenderer>();
        }

        if (renderer != null)
        {
            renderer.sortingOrder = sortingOrder;
            if (textMesh.font != null)
            {
                renderer.sharedMaterial = textMesh.font.material;
            }
        }

        return textMesh;
    }

    private static void CreateGameHudLayout(
        Transform canvasTransform,
        out RectTransform leftPanel,
        out RectTransform centerPanel,
        out RectTransform rightPanel)
    {
        leftPanel = CreatePanel(canvasTransform, "LeftStatusPanel",
            new Vector2(0f, 0f), new Vector2(0.22f, 1f), new Color(0.02f, 0.04f, 0.07f, 0.72f));
        centerPanel = CreatePanel(canvasTransform, "CenterGamePanel",
            new Vector2(0.22f, 0f), new Vector2(0.78f, 1f), new Color(1f, 1f, 1f, 0.018f));
        rightPanel = CreatePanel(canvasTransform, "RightIconPanel",
            new Vector2(0.78f, 0f), new Vector2(1f, 1f), new Color(0.02f, 0.04f, 0.07f, 0.62f));

        Color borderColor = new Color(0.78f, 0.88f, 1f, 0.26f);
        CreatePanelBorder(leftPanel, "LeftStatusPanelBorder", borderColor, 2f);
        CreatePanelBorder(centerPanel, "CenterGamePanelBorder", new Color(0.78f, 0.88f, 1f, 0.20f), 2f);
        CreatePanelBorder(rightPanel, "RightIconPanelBorder", borderColor, 2f);
    }

    private static void CreateIconPanelContents(Transform rightPanel, out Text icon1Text, out Text icon2Text, out Text icon3Text)
    {
        CreateText(rightPanel, "IconTitleText", "ICON", 34, new Color(0.94f, 0.97f, 1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(300f, 58f), new Vector2(0.5f, 1f));

        icon1Text = CreateIconSlot(rightPanel, "IconSlot_1", -130f);
        icon2Text = CreateIconSlot(rightPanel, "IconSlot_2", -250f);
        icon3Text = CreateIconSlot(rightPanel, "IconSlot_3", -370f);
    }

    private static Text CreateIconSlot(Transform parent, string name, float topOffset)
    {
        RectTransform slot = CreatePanel(parent, name,
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            new Color(1f, 1f, 1f, 0.035f),
            new Vector2(0f, topOffset),
            new Vector2(92f, 92f),
            new Vector2(0.5f, 1f));
        CreatePanelBorder(slot, $"{name}_Border", new Color(0.86f, 0.93f, 1f, 0.45f), 2f);
        return CreateText(slot, $"{name}_Text", "[-]", 38, new Color(0.86f, 0.93f, 1f, 0.58f),
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
    }

    private static RectTransform CreatePanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Color color,
        Vector2? anchoredPosition = null,
        Vector2? sizeDelta = null,
        Vector2? pivot = null)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition ?? Vector2.zero;
        rectTransform.sizeDelta = sizeDelta ?? Vector2.zero;

        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        return rectTransform;
    }

    private static void CreatePanelBorder(RectTransform parent, string name, Color color, float thickness)
    {
        float safeThickness = Mathf.Max(1f, thickness);

        CreatePanel(parent, $"{name}_Top", new Vector2(0f, 1f), new Vector2(1f, 1f), color,
            Vector2.zero, new Vector2(0f, safeThickness), new Vector2(0.5f, 1f));
        CreatePanel(parent, $"{name}_Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), color,
            Vector2.zero, new Vector2(0f, safeThickness), new Vector2(0.5f, 0f));
        CreatePanel(parent, $"{name}_Left", new Vector2(0f, 0f), new Vector2(0f, 1f), color,
            Vector2.zero, new Vector2(safeThickness, 0f), new Vector2(0f, 0.5f));
        CreatePanel(parent, $"{name}_Right", new Vector2(1f, 0f), new Vector2(1f, 1f), color,
            Vector2.zero, new Vector2(safeThickness, 0f), new Vector2(1f, 0.5f));
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static Text CreateText(
        Transform parent,
        string name,
        string text,
        int fontSize,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2? pivot = null,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Text uiText = textObject.GetComponent<Text>();
        uiText.text = text;
        if (defaultFont != null)
        {
            uiText.font = defaultFont;
        }

        uiText.fontSize = fontSize;
        uiText.alignment = alignment;
        uiText.color = color;
        uiText.raycastTarget = false;

        return uiText;
    }

    private static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2? pivot = null)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot ?? new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.16f, 0.54f, 0.76f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        Text buttonText = CreateText(buttonObject.transform, "Text", label, 32, Color.white,
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        buttonText.fontStyle = FontStyle.Bold;

        return button;
    }
}
