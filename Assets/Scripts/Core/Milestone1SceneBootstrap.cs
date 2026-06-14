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
    private const float DefaultBlockSpacing = 0.12f;
    private const float DefaultBallSpeed = 7f;
    private const float DefaultPaddleSpeed = 9f;
    private const int DefaultInitialLives = 3;
    private const int DefaultAddBallsCount = 2;
    private const float DefaultAddBallSpeed = 7f;
    private static readonly Vector2 DefaultBlockStartPosition = new Vector2(-3.24f, 3.25f);

    private enum SceneKind
    {
        Title,
        StageSelect,
        Game
    }

    [SerializeField] private SceneKind sceneKind = SceneKind.Title;
    [SerializeField] private StageData stageData;
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

    private static Sprite squareSprite;
    private static Sprite ballSprite;
    private static Sprite backgroundSprite;
    private static Font defaultFont;

    private struct StageRuntimeSettings
    {
        public string StageName;
        public Sprite BackgroundSprite;
        public int BlockRows;
        public int BlockColumns;
        public float BlockSize;
        public float BlockSpacing;
        public Vector2 BlockStartPosition;
        public float BallSpeed;
        public float PaddleSpeed;
        public int InitialLives;
        public float ItemDropChance;
        public float PaddleExpandMultiplier;
        public float PaddleExpandDuration;
        public int AddBallsCount;
        public float AddBallLaunchAngle;
        public float AddBallSpeed;
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
    }

    private StageRuntimeSettings CreateStageSettings()
    {
        StageRuntimeSettings settings = new StageRuntimeSettings
        {
            StageName = string.IsNullOrWhiteSpace(stageName) ? DefaultStageName : stageName,
            BackgroundSprite = fallbackBackgroundSprite,
            BlockRows = blockRows > 0 ? blockRows : DefaultBlockRows,
            BlockColumns = blockColumns > 0 ? blockColumns : DefaultBlockColumns,
            BlockSize = blockSize > 0f ? blockSize : DefaultBlockSize,
            BlockSpacing = Mathf.Max(0f, blockSpacing),
            BlockStartPosition = blockStartPosition,
            BallSpeed = ballSpeed > 0f ? ballSpeed : DefaultBallSpeed,
            PaddleSpeed = paddleSpeed > 0f ? paddleSpeed : DefaultPaddleSpeed,
            InitialLives = initialLives > 0 ? initialLives : DefaultInitialLives,
            ItemDropChance = Mathf.Clamp01(itemDropChance),
            PaddleExpandMultiplier = Mathf.Max(1f, paddleExpandMultiplier),
            PaddleExpandDuration = Mathf.Max(0f, paddleExpandDuration),
            AddBallsCount = addBallsCount > 0 ? addBallsCount : DefaultAddBallsCount,
            AddBallLaunchAngle = Mathf.Max(0f, addBallLaunchAngle),
            AddBallSpeed = addBallSpeed > 0f ? addBallSpeed : DefaultAddBallSpeed
        };

        if (stageData == null)
        {
            return settings;
        }

        settings.StageName = stageData.StageName;
        settings.BackgroundSprite = stageData.BackgroundSprite != null ? stageData.BackgroundSprite : settings.BackgroundSprite;
        settings.BlockRows = stageData.BlockRows;
        settings.BlockColumns = stageData.BlockColumns;
        settings.BlockSize = stageData.BlockSize;
        settings.BlockSpacing = stageData.BlockSpacing;
        settings.BlockStartPosition = stageData.BlockStartPosition;
        settings.BallSpeed = stageData.BallSpeed;
        settings.PaddleSpeed = stageData.PaddleSpeed;
        settings.InitialLives = stageData.InitialLives;
        settings.ItemDropChance = stageData.ItemDropChance;
        settings.PaddleExpandMultiplier = stageData.PaddleExpandMultiplier;
        settings.PaddleExpandDuration = stageData.PaddleExpandDuration;
        settings.AddBallsCount = stageData.AddBallsCount;
        settings.AddBallLaunchAngle = stageData.AddBallLaunchAngle;
        settings.AddBallSpeed = stageData.AddBallSpeed;

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

    private static void BuildStageSelectScene()
    {
        CreateCamera(new Color(0.05f, 0.09f, 0.15f, 1f));
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        CreateText(canvas.transform, "StageSelectTitle", "Stage Select", 64, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(760f, 100f));

        Button stageButton = CreateButton(canvas.transform, "Stage1Button", "Stage 1",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(360f, 84f));
        stageButton.onClick.AddListener(loader.LoadGame);

        Button backButton = CreateButton(canvas.transform, "BackButton", "Back",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(260f, 72f));
        backButton.onClick.AddListener(loader.LoadTitle);
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

        CreateBackground(settings.BackgroundSprite);
        CreateWall("LeftWall", new Vector2(-8.95f, 0f), new Vector2(0.3f, 10.4f), bouncyMaterial);
        CreateWall("RightWall", new Vector2(8.95f, 0f), new Vector2(0.3f, 10.4f), bouncyMaterial);
        CreateWall("TopWall", new Vector2(0f, 5.1f), new Vector2(18.2f, 0.3f), bouncyMaterial);

        GameManager gameManager = new GameObject("GameManager").AddComponent<GameManager>();
        ItemEffectManager itemEffectManager = new GameObject("ItemEffectManager").AddComponent<ItemEffectManager>();
        GameObject paddle = CreatePaddle(camera, bouncyMaterial, settings.PaddleSpeed);
        PaddleController paddleController = paddle.GetComponent<PaddleController>();
        itemEffectManager.Configure(
            paddleController,
            gameManager,
            settings.PaddleExpandMultiplier,
            settings.PaddleExpandDuration,
            settings.AddBallsCount);
        GameObject ballsParent = new GameObject("Balls");
        GameObject ball = CreateBall(paddle.transform, gameManager, bouncyMaterial, settings.BallSpeed);
        ball.transform.SetParent(ballsParent.transform);

        Canvas canvas = CreateCanvas();
        CreateEventSystem();
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();

        Text livesText = CreateText(canvas.transform, "LivesText", $"Lives: {settings.InitialLives}", 34, Color.white,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -24f), new Vector2(320f, 60f), new Vector2(0f, 1f), TextAnchor.MiddleLeft);

        Text stageNameText = CreateText(canvas.transform, "StageNameText", settings.StageName, 34, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(420f, 60f), new Vector2(0.5f, 1f), TextAnchor.MiddleCenter);

        Button backButton = CreateButton(canvas.transform, "BackToStageSelectButton", "Stage Select",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -24f), new Vector2(260f, 64f), new Vector2(1f, 1f));
        backButton.onClick.AddListener(loader.LoadStageSelect);

        Text clearText = CreateText(canvas.transform, "ClearText", "CLEAR", 86, new Color(0.98f, 0.92f, 0.30f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(620f, 120f));
        clearText.gameObject.SetActive(false);

        Text gameOverText = CreateText(canvas.transform, "GameOverText", "GAME OVER", 78, new Color(1f, 0.42f, 0.42f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(720f, 120f));
        gameOverText.gameObject.SetActive(false);

        UIManager uiManager = new GameObject("UIManager").AddComponent<UIManager>();
        uiManager.Configure(livesText, stageNameText, clearText, gameOverText);
        gameManager.Configure(ball.GetComponent<BallController>(), uiManager, settings.StageName, settings.InitialLives);
        gameManager.ConfigureBallSpawning(
            ball.GetComponent<BallController>(),
            paddle.transform,
            ballsParent.transform,
            settings.AddBallLaunchAngle,
            settings.AddBallSpeed);

        CreateBlockGrid(gameManager, bouncyMaterial, itemEffectManager, settings);
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

    private static void CreateBackground(Sprite stageBackgroundSprite)
    {
        GameObject background = new GameObject("Background");
        background.transform.position = new Vector3(0f, 0f, 1f);
        SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
        renderer.sprite = stageBackgroundSprite != null ? stageBackgroundSprite : backgroundSprite;
        renderer.sortingOrder = -20;
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

    private static GameObject CreatePaddle(Camera camera, PhysicsMaterial2D material, float speed)
    {
        GameObject paddle = new GameObject("Paddle");
        paddle.transform.position = new Vector3(0f, -4.15f, 0f);
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
        return paddle;
    }

    private static GameObject CreateBall(Transform paddle, GameManager gameManager, PhysicsMaterial2D material, float speed)
    {
        GameObject ball = new GameObject("Ball");
        ball.transform.position = new Vector3(0f, -3.7f, 0f);
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
        controller.SetMoveSpeed(speed);
        return ball;
    }

    private static void CreateBlockGrid(
        GameManager gameManager,
        PhysicsMaterial2D material,
        ItemEffectManager itemEffectManager,
        StageRuntimeSettings settings)
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
        builder.Configure(
            block,
            gameManager,
            blocksParent.transform,
            settings.BlockRows,
            settings.BlockColumns,
            settings.BlockSize,
            settings.BlockSpacing,
            settings.BlockStartPosition);
        builder.ConfigureItemDrops(itemPrefab, safeDropChance, itemEffectManager);
    }

    private static ItemController CreateItemPrefab(Transform parent)
    {
        GameObject itemPrefab = new GameObject("ItemPrefab");
        itemPrefab.transform.SetParent(parent);
        itemPrefab.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
        itemPrefab.SetActive(false);

        SpriteRenderer renderer = itemPrefab.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.38f, 0.95f, 0.70f, 1f);
        renderer.sortingOrder = 15;

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
