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
    private const string DefaultIcon2Label = "N";
    private const string DefaultIcon3Label = "S";
    private const string DefaultIcon1Description = "Clear";
    private const string DefaultIcon2Description = "No Miss";
    private const string DefaultIcon3Description = "Score";
    private const int DefaultIconScoreTarget = 3000;
    private const float StageSelectButtonSpacing = 148f;
    private const float StageSelectButtonWidth = 440f;
    private const float StageSelectButtonHeight = 132f;
    private const float StageSelectScrollViewWidth = 640f;
    private const float StageSelectScrollViewHeight = 560f;
    private const float StageSelectContentPadding = 18f;
    private const string TitleSceneTitle = "NEON BREAK";
    private const string TitleSceneSubtitle = "Break the blocks, reveal the secret.";
    private const string TitleSceneDescription = "Clear stages and collect all icons.";
    private const string TitleSceneStartLabel = "START GAME";
    private const int BackgroundSortingOrder = -20;
    private static readonly Color ThemeBackground = new Color32(0x12, 0x09, 0x14, 0xFF);
    private static readonly Color ThemeDarkPurple = new Color32(0x24, 0x10, 0x2F, 0xFF);
    private static readonly Color ThemePanel = new Color32(0x1A, 0x0D, 0x24, 0xFF);
    private static readonly Color ThemePink = new Color32(0xFF, 0x4F, 0xD8, 0xFF);
    private static readonly Color ThemeText = new Color32(0xFF, 0xD6, 0xF5, 0xFF);
    private static readonly Color ThemeWhite = Color.white;
    private static readonly Color ThemeCyan = new Color32(0x64, 0xF5, 0xFF, 0xFF);
    private static readonly Color ThemePerfect = new Color32(0xFF, 0xD8, 0x66, 0xFF);
    private static readonly Color ThemeLocked = new Color32(0x55, 0x50, 0x5A, 0xFF);
    private static readonly Color ThemeDanger = new Color32(0xFF, 0x5D, 0xA8, 0xFF);
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
        public string Icon1Description;
        public string Icon2Description;
        public string Icon3Description;
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

    private struct IconSlotViews
    {
        public Text LabelText;
        public Image FillImage;
        public Image BorderImage;
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
            Icon1Description = DefaultIcon1Description,
            Icon2Description = DefaultIcon2Description,
            Icon3Description = DefaultIcon3Description,
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
        settings.Icon1Description = effectiveStageData.Icon1Description;
        settings.Icon2Description = effectiveStageData.Icon2Description;
        settings.Icon3Description = effectiveStageData.Icon3Description;
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

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
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
        CreateCamera(ThemeBackground);
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        RectTransform frame = CreatePanel(canvas.transform, "TitleNeonFrame",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WithAlpha(ThemePanel, 0.74f),
            new Vector2(0f, 42f), new Vector2(1060f, 560f));
        CreatePanelBorder(frame, "TitleNeonFrameBorder", WithAlpha(ThemePink, 0.72f), 3f);

        CreatePanel(frame, "TitleAccentLineTop", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), WithAlpha(ThemeCyan, 0.78f),
            new Vector2(0f, -62f), new Vector2(520f, 4f));
        CreatePanel(frame, "TitleAccentLineBottom", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), WithAlpha(ThemePink, 0.72f),
            new Vector2(0f, 70f), new Vector2(640f, 4f));
        CreatePanel(frame, "TitleAccentLeft", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), WithAlpha(ThemePink, 0.42f),
            new Vector2(46f, 0f), new Vector2(4f, 360f));
        CreatePanel(frame, "TitleAccentRight", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), WithAlpha(ThemeCyan, 0.38f),
            new Vector2(-46f, 0f), new Vector2(4f, 360f));

        Text titleGlowText = CreateText(frame, "TitleGlowText", TitleSceneTitle, 104, WithAlpha(ThemePink, 0.20f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(6f, 118f), new Vector2(980f, 150f));
        titleGlowText.fontStyle = FontStyle.Bold;

        Text titleShadowText = CreateText(frame, "TitleShadowText", TitleSceneTitle, 88, WithAlpha(ThemeCyan, 0.38f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3f, 112f), new Vector2(980f, 142f));
        titleShadowText.fontStyle = FontStyle.Bold;

        Text titleText = CreateText(frame, "TitleText", TitleSceneTitle, 88, ThemePink,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 116f), new Vector2(980f, 142f));
        titleText.fontStyle = FontStyle.Bold;

        Text subtitleText = CreateText(frame, "SubtitleText", TitleSceneSubtitle, 30, ThemeText,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 32f), new Vector2(860f, 64f));
        subtitleText.fontStyle = FontStyle.Bold;

        Button startButton = CreateButton(frame, "StartButton", TitleSceneStartLabel,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -92f), new Vector2(360f, 88f));
        Text startButtonText = startButton.transform.Find("Text")?.GetComponent<Text>();
        if (startButtonText != null)
        {
            startButtonText.fontSize = 34;
            startButtonText.color = ThemePink;
        }

        CreateText(frame, "TitleDescriptionText", TitleSceneDescription, 24, WithAlpha(ThemeCyan, 0.92f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -178f), new Vector2(860f, 54f));

        startButton.onClick.AddListener(loader.LoadStageSelect);
    }

    private void BuildStageSelectScene()
    {
        CreateCamera(ThemeBackground);
        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        Text titleGlowText = CreateText(canvas.transform, "StageSelectTitleGlow", "STAGE SELECT", 70, WithAlpha(ThemePink, 0.20f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(4f, 404f), new Vector2(820f, 108f));
        titleGlowText.fontStyle = FontStyle.Bold;

        Text titleShadowText = CreateText(canvas.transform, "StageSelectTitleShadow", "STAGE SELECT", 60, WithAlpha(ThemeCyan, 0.34f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(3f, 398f), new Vector2(820f, 102f));
        titleShadowText.fontStyle = FontStyle.Bold;

        Text titleText = CreateText(canvas.transform, "StageSelectTitle", "STAGE SELECT", 60, ThemePink,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 400f), new Vector2(820f, 102f));
        titleText.fontStyle = FontStyle.Bold;

        RectTransform stageListContent = CreateStageSelectScrollView(canvas.transform);
        int buttonCount = CreateStageButtonsFromDatabase(stageListContent);
        SetStageSelectContentHeight(stageListContent, buttonCount);

        Button backButton = CreateButton(canvas.transform, "BackButton", "Back",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -420f), new Vector2(260f, 72f));
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
                GetStageSelectButtonPosition(buttonCount));
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

        return stageData.StageName;
    }

    private static RectTransform CreateStageSelectScrollView(Transform parent)
    {
        RectTransform scrollView = CreatePanel(parent, "StageSelectScrollView",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WithAlpha(ThemePanel, 0.88f),
            new Vector2(0f, 20f), new Vector2(StageSelectScrollViewWidth, StageSelectScrollViewHeight));
        Image scrollViewImage = scrollView.GetComponent<Image>();
        if (scrollViewImage != null)
        {
            scrollViewImage.raycastTarget = true;
        }

        ScrollRect scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 36f;
        scrollRect.inertia = true;

        CreatePanelBorder(scrollView, "StageSelectScrollViewBorder", WithAlpha(ThemePink, 0.62f), 2f);

        RectTransform viewport = CreatePanel(scrollView, "Viewport",
            new Vector2(0f, 0f), new Vector2(1f, 1f), WithAlpha(ThemeWhite, 0.01f),
            Vector2.zero, new Vector2(-30f, 0f));
        Image viewportImage = viewport.GetComponent<Image>();
        if (viewportImage != null)
        {
            viewportImage.raycastTarget = true;
        }

        Mask viewportMask = viewport.gameObject.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform));
        contentObject.transform.SetParent(viewport, false);

        RectTransform content = contentObject.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, StageSelectScrollViewHeight);

        Scrollbar verticalScrollbar = CreateVerticalScrollbar(scrollView);
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.verticalScrollbar = verticalScrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
        scrollRect.verticalNormalizedPosition = 1f;

        return content;
    }

    private static Scrollbar CreateVerticalScrollbar(Transform parent)
    {
        GameObject scrollbarObject = new GameObject("VerticalScrollbar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Scrollbar));
        scrollbarObject.transform.SetParent(parent, false);

        RectTransform scrollbarTransform = scrollbarObject.GetComponent<RectTransform>();
        scrollbarTransform.anchorMin = new Vector2(1f, 0f);
        scrollbarTransform.anchorMax = new Vector2(1f, 1f);
        scrollbarTransform.pivot = new Vector2(1f, 0.5f);
        scrollbarTransform.anchoredPosition = Vector2.zero;
        scrollbarTransform.sizeDelta = new Vector2(22f, 0f);

        Image scrollbarBackground = scrollbarObject.GetComponent<Image>();
        scrollbarBackground.sprite = squareSprite;
        scrollbarBackground.color = WithAlpha(ThemeDarkPurple, 0.86f);

        GameObject slidingAreaObject = new GameObject("Sliding Area", typeof(RectTransform));
        slidingAreaObject.transform.SetParent(scrollbarTransform, false);

        RectTransform slidingArea = slidingAreaObject.GetComponent<RectTransform>();
        slidingArea.anchorMin = Vector2.zero;
        slidingArea.anchorMax = Vector2.one;
        slidingArea.offsetMin = new Vector2(4f, 4f);
        slidingArea.offsetMax = new Vector2(-4f, -4f);

        GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handleObject.transform.SetParent(slidingArea, false);

        RectTransform handle = handleObject.GetComponent<RectTransform>();
        handle.anchorMin = Vector2.zero;
        handle.anchorMax = Vector2.one;
        handle.offsetMin = Vector2.zero;
        handle.offsetMax = Vector2.zero;

        Image handleImage = handleObject.GetComponent<Image>();
        handleImage.sprite = squareSprite;
        handleImage.color = WithAlpha(ThemePink, 0.92f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handle;

        return scrollbar;
    }

    private static void SetStageSelectContentHeight(RectTransform content, int buttonCount)
    {
        if (content == null)
        {
            return;
        }

        float usedHeight = buttonCount > 0
            ? StageSelectContentPadding * 2f + StageSelectButtonHeight + Mathf.Max(0, buttonCount - 1) * StageSelectButtonSpacing
            : StageSelectScrollViewHeight;
        float contentHeight = Mathf.Max(StageSelectScrollViewHeight, usedHeight);
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
        content.anchoredPosition = Vector2.zero;
    }

    private static Vector2 GetStageSelectButtonPosition(int index)
    {
        float y = -StageSelectContentPadding - StageSelectButtonHeight * 0.5f - index * StageSelectButtonSpacing;
        return new Vector2(0f, y);
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
            new Vector2(0.5f, 1f),
            new Vector2(0.5f, 1f),
            anchoredPosition,
            new Vector2(StageSelectButtonWidth, StageSelectButtonHeight));

        Text titleText = stageButton.transform.Find("Text")?.GetComponent<Text>();
        if (titleText != null)
        {
            RectTransform titleTransform = titleText.GetComponent<RectTransform>();
            titleTransform.anchorMin = new Vector2(0f, 0.60f);
            titleTransform.anchorMax = new Vector2(1f, 1f);
            titleTransform.anchoredPosition = Vector2.zero;
            titleTransform.sizeDelta = Vector2.zero;
            titleTransform.offsetMin = new Vector2(54f, titleTransform.offsetMin.y);
            titleTransform.offsetMax = new Vector2(-26f, titleTransform.offsetMax.y);
            titleText.fontSize = 30;
            titleText.alignment = TextAnchor.MiddleLeft;
        }

        Image cardBorderImage = CreatePanel(stageButton.transform, "CardBorder",
            new Vector2(0f, 0f), new Vector2(0f, 1f), WithAlpha(ThemePink, 0.90f),
            Vector2.zero, new Vector2(7f, 0f), new Vector2(0f, 0.5f)).GetComponent<Image>();

        Text lockedText = CreateText(stageButton.transform, "LockedText", string.Empty,
            22, WithAlpha(ThemeLocked, 0.96f),
            new Vector2(0f, 0.42f), new Vector2(1f, 0.60f), Vector2.zero, Vector2.zero,
            null, TextAnchor.MiddleLeft);
        RectTransform lockedTransform = lockedText.GetComponent<RectTransform>();
        lockedTransform.offsetMin = new Vector2(54f, lockedTransform.offsetMin.y);
        lockedTransform.offsetMax = new Vector2(-26f, lockedTransform.offsetMax.y);
        lockedText.fontStyle = FontStyle.Bold;

        Text iconStatusText = CreateText(stageButton.transform, "IconStatusText", "[-] [-] [-]",
            21, WithAlpha(ThemeText, 0.96f),
            new Vector2(0f, 0.18f), new Vector2(1f, 0.44f), Vector2.zero, Vector2.zero,
            null, TextAnchor.MiddleLeft);
        RectTransform iconStatusTransform = iconStatusText.GetComponent<RectTransform>();
        iconStatusTransform.offsetMin = new Vector2(54f, iconStatusTransform.offsetMin.y);
        iconStatusTransform.offsetMax = new Vector2(-26f, iconStatusTransform.offsetMax.y);
        iconStatusText.fontStyle = FontStyle.Bold;

        Text perfectText = CreateText(stageButton.transform, "PerfectText", string.Empty,
            22, ThemePerfect,
            new Vector2(0f, 0f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero,
            null, TextAnchor.MiddleLeft);
        RectTransform perfectTransform = perfectText.GetComponent<RectTransform>();
        perfectTransform.offsetMin = new Vector2(54f, perfectTransform.offsetMin.y);
        perfectTransform.offsetMax = new Vector2(-26f, perfectTransform.offsetMax.y);
        perfectText.fontStyle = FontStyle.Bold;

        StageSelectButton stageSelectButton = stageButton.gameObject.AddComponent<StageSelectButton>();
        stageSelectButton.Configure(
            data,
            isUnlocked,
            stageButton.GetComponent<Image>(),
            cardBorderImage,
            titleText,
            lockedText,
            iconStatusText,
            perfectText);
        stageButton.onClick.AddListener(stageSelectButton.SelectStageAndLoadGame);
    }

    private static void BuildGameScene(StageRuntimeSettings settings)
    {
        Physics2D.gravity = Vector2.zero;
        Camera camera = CreateCamera(ThemeBackground);
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

        Text statusTitleText = CreateText(leftPanel, "StatusTitleText", "STATUS", 34, ThemePink,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -34f), new Vector2(340f, 58f), new Vector2(0.5f, 1f));
        statusTitleText.fontStyle = FontStyle.Bold;

        CreateHudStatusCard(leftPanel, "StageStatusCard", "STAGE", settings.StageName, new Vector2(0f, -132f), new Vector2(334f, 104f), 26, ThemeText, out Text stageNameText);
        CreateHudStatusCard(leftPanel, "ScoreStatusCard", "SCORE", "0", new Vector2(0f, -266f), new Vector2(334f, 118f), 38, ThemeWhite, out Text scoreText);
        CreateHudStatusCard(leftPanel, "LifeStatusCard", "LIFE", "-", new Vector2(0f, -404f), new Vector2(334f, 118f), 36, ThemePink, out Text livesText);
        livesText.resizeTextForBestFit = true;
        livesText.resizeTextMinSize = 22;
        livesText.resizeTextMaxSize = 36;

        Button backButton = CreateButton(leftPanel, "BackToStageSelectButton", "STAGE SELECT",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 44f), new Vector2(280f, 64f), new Vector2(0.5f, 0f));
        Text backButtonText = backButton.GetComponentInChildren<Text>();
        if (backButtonText != null)
        {
            backButtonText.fontSize = 22;
            backButtonText.color = ThemePink;
        }
        backButton.onClick.AddListener(loader.LoadStageSelect);

        CreateIconPanelContents(
            rightPanel,
            out IconSlotViews icon1View,
            out IconSlotViews icon2View,
            out IconSlotViews icon3View,
            out Text iconConditionText);

        Text clearText = CreateResultText(canvas.transform, "ClearResultPanel", "ClearText", UIManager.ClearMessage, 36, ThemeCyan, out Image clearResultBackground);
        Text gameOverText = CreateResultText(canvas.transform, "GameOverResultPanel", "GameOverText", UIManager.GameOverMessage, 36, ThemeDanger, out Image gameOverResultBackground);
        clearResultBackground.gameObject.SetActive(false);
        gameOverResultBackground.gameObject.SetActive(false);

        UIManager uiManager = new GameObject("UIManager").AddComponent<UIManager>();
        uiManager.Configure(
            livesText,
            stageNameText,
            scoreText,
            clearText,
            gameOverText,
            icon1View.LabelText,
            icon2View.LabelText,
            icon3View.LabelText);
        uiManager.ConfigureLifeHearts("\u2665", "  ", "-");
        uiManager.ConfigureHudTextFormats("{0}", "{0}", "{0}");
        uiManager.ConfigureStageIconImages(
            icon1View.FillImage,
            icon2View.FillImage,
            icon3View.FillImage,
            icon1View.BorderImage,
            icon2View.BorderImage,
            icon3View.BorderImage,
            iconConditionText);
        uiManager.ConfigureResultBackgrounds(clearResultBackground, gameOverResultBackground);
        gameManager.Configure(ball.GetComponent<BallController>(), uiManager, settings.StageName, settings.InitialLives, settings.StageId, settings.HasNextStage);
        gameManager.ConfigureStageIcons(
            settings.Icon1Label,
            settings.Icon1Description,
            settings.Icon2Label,
            settings.Icon2Description,
            settings.Icon3Label,
            settings.Icon3Description,
            settings.IconScoreTarget);
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
            new Vector2(0f, 0f), new Vector2(0.22f, 1f), WithAlpha(ThemePanel, 0.86f));
        centerPanel = CreatePanel(canvasTransform, "CenterGamePanel",
            new Vector2(0.22f, 0f), new Vector2(0.78f, 1f), WithAlpha(ThemeDarkPurple, 0.10f));
        rightPanel = CreatePanel(canvasTransform, "RightIconPanel",
            new Vector2(0.78f, 0f), new Vector2(1f, 1f), WithAlpha(ThemePanel, 0.82f));

        Color borderColor = WithAlpha(ThemePink, 0.34f);
        CreatePanelBorder(leftPanel, "LeftStatusPanelBorder", borderColor, 2f);
        Image centerImage = centerPanel.GetComponent<Image>();
        if (centerImage != null)
        {
            centerImage.color = WithAlpha(ThemeDarkPurple, 0.045f);
        }

        CreatePanelBorder(centerPanel, "CenterGamePanelBorder", WithAlpha(ThemeCyan, 0.32f), 2f);
        CreatePanelBorder(rightPanel, "RightIconPanelBorder", borderColor, 2f);
    }

    private static RectTransform CreateHudStatusCard(
        Transform parent,
        string name,
        string label,
        string value,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        int valueFontSize,
        Color valueColor,
        out Text valueText)
    {
        RectTransform card = CreatePanel(parent, name,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), WithAlpha(ThemeDarkPurple, 0.68f),
            anchoredPosition, sizeDelta, new Vector2(0.5f, 0.5f));
        CreatePanelBorder(card, $"{name}Border", WithAlpha(ThemePink, 0.36f), 2f);
        CreatePanel(card, $"{name}NeonAccent",
            new Vector2(0f, 0f), new Vector2(0f, 1f), WithAlpha(ThemePink, 0.86f),
            Vector2.zero, new Vector2(5f, 0f), new Vector2(0f, 0.5f));

        Text labelText = CreateText(card, $"{name}Label", label, 20, ThemePink,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -18f), new Vector2(-36f, 30f),
            new Vector2(0.5f, 1f), TextAnchor.MiddleLeft);
        labelText.fontStyle = FontStyle.Bold;

        valueText = CreateText(card, $"{name}ValueText", value, valueFontSize, valueColor,
            new Vector2(0f, 0f), new Vector2(1f, 0.72f), new Vector2(18f, 4f), new Vector2(-44f, -6f),
            new Vector2(0.5f, 0.5f), TextAnchor.MiddleLeft);
        valueText.fontStyle = FontStyle.Bold;

        return card;
    }

    private static void CreateIconPanelContents(
        Transform rightPanel,
        out IconSlotViews icon1View,
        out IconSlotViews icon2View,
        out IconSlotViews icon3View,
        out Text iconConditionText)
    {
        Text iconTitleText = CreateText(rightPanel, "IconTitleText", "TARGET ICON", 30, ThemePink,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(300f, 58f), new Vector2(0.5f, 1f));
        iconTitleText.fontStyle = FontStyle.Bold;
        iconTitleText.resizeTextForBestFit = true;
        iconTitleText.resizeTextMinSize = 24;
        iconTitleText.resizeTextMaxSize = 30;

        RectTransform missionCard = CreatePanel(rightPanel, "TargetIconCard",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), WithAlpha(ThemeDarkPurple, 0.58f),
            new Vector2(0f, -102f), new Vector2(358f, 470f), new Vector2(0.5f, 1f));
        CreatePanelBorder(missionCard, "TargetIconCardBorder", WithAlpha(ThemePink, 0.34f), 2f);
        CreatePanel(missionCard, "TargetIconCardTopGlow",
            new Vector2(0f, 1f), new Vector2(1f, 1f), WithAlpha(ThemeCyan, 0.58f),
            Vector2.zero, new Vector2(0f, 3f), new Vector2(0.5f, 1f));

        icon1View = CreateIconSlot(missionCard, "IconSlot_1", new Vector2(0f, -112f));
        icon2View = CreateIconSlot(missionCard, "IconSlot_2", new Vector2(-88f, -278f));
        icon3View = CreateIconSlot(missionCard, "IconSlot_3", new Vector2(88f, -278f));

        RectTransform conditionCard = CreatePanel(missionCard, "IconConditionCard",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), WithAlpha(ThemePanel, 0.74f),
            new Vector2(0f, -400f), new Vector2(324f, 106f), new Vector2(0.5f, 0.5f));
        CreatePanelBorder(conditionCard, "IconConditionCardBorder", WithAlpha(ThemeCyan, 0.28f), 2f);

        iconConditionText = CreateText(conditionCard, "IconConditionText", "C: Clear\nN: No Miss\nS: Score",
            22, WithAlpha(ThemeText, 0.94f),
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-32f, -14f),
            null, TextAnchor.MiddleLeft);
    }

    private static IconSlotViews CreateIconSlot(Transform parent, string name, Vector2 anchoredPosition)
    {
        GameObject slotObject = new GameObject(name, typeof(RectTransform));
        slotObject.transform.SetParent(parent, false);

        RectTransform slot = slotObject.GetComponent<RectTransform>();
        slot.anchorMin = new Vector2(0.5f, 1f);
        slot.anchorMax = new Vector2(0.5f, 1f);
        slot.pivot = new Vector2(0.5f, 0.5f);
        slot.anchoredPosition = anchoredPosition;
        slot.sizeDelta = new Vector2(142f, 142f);

        Image borderImage = CreateCircleImage(slot, $"{name}_BorderCircle", new Vector2(142f, 142f), WithAlpha(ThemeLocked, 0.76f));
        Image fillImage = CreateCircleImage(slot, $"{name}_FillCircle", new Vector2(124f, 124f), WithAlpha(ThemeLocked, 0.50f));
        Text labelText = CreateText(slot, $"{name}_Text", "-", 60, WithAlpha(ThemeText, 0.88f),
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        labelText.fontStyle = FontStyle.Bold;

        return new IconSlotViews
        {
            LabelText = labelText,
            FillImage = fillImage,
            BorderImage = borderImage
        };
    }

    private static Text CreateResultText(
        Transform parent,
        string panelName,
        string textName,
        string text,
        int fontSize,
        Color textColor,
        out Image backgroundImage)
    {
        RectTransform panel = CreatePanel(parent, panelName,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WithAlpha(ThemePanel, 0.90f),
            new Vector2(0f, 20f), new Vector2(1120f, 460f));
        backgroundImage = panel.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.raycastTarget = false;
        }

        CreatePanelBorder(panel, $"{panelName}Border", WithAlpha(ThemePink, 0.78f), 3f);

        Text resultText = CreateText(panel, textName, text, fontSize, textColor,
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-80f, -64f));
        resultText.fontStyle = FontStyle.Bold;
        return resultText;
    }

    private static Image CreateCircleImage(Transform parent, string name, Vector2 sizeDelta, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = sizeDelta;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = ballSprite;
        image.preserveAspect = true;
        image.color = color;
        image.raycastTarget = false;

        return image;
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
        image.color = ThemeDarkPurple;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        ColorBlock buttonColors = button.colors;
        buttonColors.normalColor = ThemeDarkPurple;
        buttonColors.highlightedColor = WithAlpha(ThemePink, 0.82f);
        buttonColors.pressedColor = ThemePink;
        buttonColors.selectedColor = WithAlpha(ThemeDarkPurple, 0.95f);
        buttonColors.disabledColor = WithAlpha(ThemeLocked, 0.68f);
        buttonColors.colorMultiplier = 1f;
        button.colors = buttonColors;

        Text buttonText = CreateText(buttonObject.transform, "Text", label, 32, ThemeText,
            new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
        buttonText.fontStyle = FontStyle.Bold;
        CreatePanelBorder(rectTransform, $"{name}NeonBorder", WithAlpha(ThemePink, 0.54f), 2f);

        return button;
    }
}
