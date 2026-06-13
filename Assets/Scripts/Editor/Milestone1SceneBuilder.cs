using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class Milestone1SceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string PrefabsFolder = "Assets/prefabs";
    private const string BackgroundsFolder = "Assets/Sprites/Backgrounds";
    private const string PhysicsMaterialsFolder = "Assets/PhysicsMaterials";

    private const string TitleScenePath = ScenesFolder + "/TitleScene.unity";
    private const string StageSelectScenePath = ScenesFolder + "/StageSelectScene.unity";
    private const string GameScenePath = ScenesFolder + "/GameScene.unity";

    private const string PaddlePrefabPath = PrefabsFolder + "/Paddle.prefab";
    private const string BallPrefabPath = PrefabsFolder + "/Ball.prefab";
    private const string BlockPrefabPath = PrefabsFolder + "/Block.prefab";

    private const string SquareSpritePath = BackgroundsFolder + "/M1_WhiteSquare.png";
    private const string BallSpritePath = BackgroundsFolder + "/M1_Ball.png";
    private const string BackgroundSpritePath = BackgroundsFolder + "/M1_Background.png";
    private const string BouncyMaterialPath = PhysicsMaterialsFolder + "/M1_Bouncy.physicsMaterial2D";

    static Milestone1SceneBuilder()
    {
        EditorApplication.delayCall += BuildIfMissing;
    }

    [MenuItem("Tools/Arkanoid/Milestone 1/Rebuild Scenes and Prefabs")]
    public static void BuildAll()
    {
        EnsureFolders();

        Sprite squareSprite = CreateSolidSprite(SquareSpritePath, new Color(1f, 1f, 1f, 1f), 64, 64, 64f, FilterMode.Point);
        Sprite ballSprite = CreateCircleSprite(BallSpritePath, new Color(1f, 0.96f, 0.70f, 1f), 64, 64f);
        Sprite backgroundSprite = CreateBackgroundSprite(BackgroundSpritePath);
        PhysicsMaterial2D bouncyMaterial = CreateBouncyMaterial();

        CreatePaddlePrefab(squareSprite, bouncyMaterial);
        CreateBallPrefab(ballSprite, bouncyMaterial);
        CreateBlockPrefab(squareSprite, bouncyMaterial);

        CreateTitleScene();
        CreateStageSelectScene();
        CreateGameScene(squareSprite, backgroundSprite, bouncyMaterial);
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Arkanoid Milestone 1 scenes, prefabs, and build settings were generated.");
    }

    private static void BuildIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        if (AllMilestoneAssetsExist())
        {
            return;
        }

        BuildAll();
    }

    private static bool AllMilestoneAssetsExist()
    {
        string[] requiredPaths =
        {
            TitleScenePath,
            StageSelectScenePath,
            GameScenePath,
            PaddlePrefabPath,
            BallPrefabPath,
            BlockPrefabPath
        };

        return requiredPaths.All(File.Exists);
    }

    private static void EnsureFolders()
    {
        EnsureFolder(ScenesFolder);
        EnsureFolder(PrefabsFolder);
        EnsureFolder(BackgroundsFolder);
        EnsureFolder(PhysicsMaterialsFolder);
    }

    private static void EnsureFolder(string folderPath)
    {
        folderPath = folderPath.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parent))
        {
            return;
        }

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static Sprite CreateSolidSprite(string assetPath, Color color, int width, int height, float pixelsPerUnit, FilterMode filterMode)
    {
        if (!File.Exists(assetPath))
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
            texture.SetPixels(pixels);
            texture.Apply();
            WritePng(assetPath, texture);
            Object.DestroyImmediate(texture);
        }

        ImportAsSprite(assetPath, pixelsPerUnit, filterMode);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite CreateCircleSprite(string assetPath, Color color, int size, float pixelsPerUnit)
    {
        if (!File.Exists(assetPath))
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
            WritePng(assetPath, texture);
            Object.DestroyImmediate(texture);
        }

        ImportAsSprite(assetPath, pixelsPerUnit, FilterMode.Bilinear);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static Sprite CreateBackgroundSprite(string assetPath)
    {
        if (!File.Exists(assetPath))
        {
            const int width = 160;
            const int height = 90;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color top = new Color(0.08f, 0.17f, 0.28f, 1f);
            Color bottom = new Color(0.02f, 0.33f, 0.38f, 1f);

            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                Color rowColor = Color.Lerp(bottom, top, t);

                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, rowColor);
                }
            }

            texture.Apply();
            WritePng(assetPath, texture);
            Object.DestroyImmediate(texture);
        }

        ImportAsSprite(assetPath, 10f, FilterMode.Bilinear);
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static void WritePng(string assetPath, Texture2D texture)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void ImportAsSprite(string assetPath, float pixelsPerUnit, FilterMode filterMode)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        }

        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = filterMode;
        importer.SaveAndReimport();
    }

    private static PhysicsMaterial2D CreateBouncyMaterial()
    {
        PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(BouncyMaterialPath);
        if (material == null)
        {
            material = new PhysicsMaterial2D("M1_Bouncy");
            AssetDatabase.CreateAsset(material, BouncyMaterialPath);
        }

        material.friction = 0f;
        material.bounciness = 1f;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreatePaddlePrefab(Sprite squareSprite, PhysicsMaterial2D bouncyMaterial)
    {
        GameObject paddle = new GameObject("Paddle");
        paddle.transform.localScale = new Vector3(2.2f, 0.32f, 1f);

        SpriteRenderer renderer = paddle.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.30f, 0.82f, 0.95f, 1f);
        renderer.sortingOrder = 10;

        BoxCollider2D collider = paddle.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = bouncyMaterial;

        Rigidbody2D rigidbody = paddle.AddComponent<Rigidbody2D>();
        rigidbody.bodyType = RigidbodyType2D.Kinematic;
        rigidbody.gravityScale = 0f;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        PaddleController controller = paddle.AddComponent<PaddleController>();
        SetFloat(controller, "moveSpeed", 9f);
        SetFloat(controller, "screenPadding", 0.25f);

        SavePrefab(paddle, PaddlePrefabPath);
    }

    private static void CreateBallPrefab(Sprite ballSprite, PhysicsMaterial2D bouncyMaterial)
    {
        GameObject ball = new GameObject("Ball");
        ball.transform.localScale = new Vector3(0.34f, 0.34f, 1f);

        SpriteRenderer renderer = ball.AddComponent<SpriteRenderer>();
        renderer.sprite = ballSprite;
        renderer.sortingOrder = 20;

        CircleCollider2D collider = ball.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.sharedMaterial = bouncyMaterial;

        Rigidbody2D rigidbody = ball.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        BallController controller = ball.AddComponent<BallController>();
        SetFloat(controller, "moveSpeed", 7f);
        SetFloat(controller, "lostY", -5.6f);
        SetFloat(controller, "minimumVerticalSpeed", 1.5f);

        SavePrefab(ball, BallPrefabPath);
    }

    private static void CreateBlockPrefab(Sprite squareSprite, PhysicsMaterial2D bouncyMaterial)
    {
        GameObject block = new GameObject("Block");

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.95f, 0.28f, 0.34f, 1f);
        renderer.sortingOrder = 5;

        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = bouncyMaterial;

        block.AddComponent<Block>();
        SavePrefab(block, BlockPrefabPath);
    }

    private static void SavePrefab(GameObject prefabRoot, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        Object.DestroyImmediate(prefabRoot);
    }

    private static void CreateTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateMainCamera(new Color(0.05f, 0.09f, 0.15f, 1f));

        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        CreateText(canvas.transform, "TitleText", "ARKANOID", 76, new Color(0.95f, 0.98f, 1f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 170f), new Vector2(760f, 120f));

        Button startButton = CreateButton(canvas.transform, "StartButton", "Start",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(320f, 90f));
        UnityEventTools.AddPersistentListener(startButton.onClick, loader.LoadStageSelect);

        EditorSceneManager.SaveScene(scene, TitleScenePath);
    }

    private static void CreateStageSelectScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateMainCamera(new Color(0.05f, 0.09f, 0.15f, 1f));

        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        CreateText(canvas.transform, "StageSelectTitle", "Stage Select", 64, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(760f, 100f));

        Button stageButton = CreateButton(canvas.transform, "Stage1Button", "Stage 1",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(360f, 84f));
        UnityEventTools.AddPersistentListener(stageButton.onClick, loader.LoadGame);

        Button backButton = CreateButton(canvas.transform, "BackButton", "Back",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -85f), new Vector2(260f, 72f));
        UnityEventTools.AddPersistentListener(backButton.onClick, loader.LoadTitle);

        EditorSceneManager.SaveScene(scene, StageSelectScenePath);
    }

    private static void CreateGameScene(Sprite squareSprite, Sprite backgroundSprite, PhysicsMaterial2D bouncyMaterial)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = CreateMainCamera(new Color(0.04f, 0.06f, 0.08f, 1f));

        SceneLoader loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();

        GameObject background = new GameObject("Background");
        background.transform.position = new Vector3(0f, 0f, 1f);
        SpriteRenderer backgroundRenderer = background.AddComponent<SpriteRenderer>();
        backgroundRenderer.sprite = backgroundSprite;
        backgroundRenderer.sortingOrder = -20;

        CreateWall("LeftWall", new Vector2(-8.95f, 0f), new Vector2(0.3f, 10.4f), squareSprite, bouncyMaterial);
        CreateWall("RightWall", new Vector2(8.95f, 0f), new Vector2(0.3f, 10.4f), squareSprite, bouncyMaterial);
        CreateWall("TopWall", new Vector2(0f, 5.1f), new Vector2(18.2f, 0.3f), squareSprite, bouncyMaterial);

        GameObject gameManagerObject = new GameObject("GameManager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        GameObject paddle = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PaddlePrefabPath)) as GameObject;
        paddle.transform.position = new Vector3(0f, -4.15f, 0f);
        PaddleController paddleController = paddle.GetComponent<PaddleController>();
        SetObjectReference(paddleController, "targetCamera", camera);

        GameObject ball = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(BallPrefabPath)) as GameObject;
        ball.transform.position = new Vector3(0f, -3.7f, 0f);
        BallController ballController = ball.GetComponent<BallController>();
        SetObjectReference(ballController, "paddle", paddle.transform);
        SetObjectReference(ballController, "gameManager", gameManager);

        Canvas canvas = CreateCanvas();
        CreateEventSystem();

        Text livesText = CreateText(canvas.transform, "LivesText", "Lives: 3", 34, Color.white,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -24f), new Vector2(320f, 60f), new Vector2(0f, 1f), TextAnchor.MiddleLeft);

        Text stageNameText = CreateText(canvas.transform, "StageNameText", "Stage 1", 34, Color.white,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(420f, 60f), new Vector2(0.5f, 1f), TextAnchor.MiddleCenter);

        Button backButton = CreateButton(canvas.transform, "BackToStageSelectButton", "Stage Select",
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -24f), new Vector2(260f, 64f), new Vector2(1f, 1f));
        UnityEventTools.AddPersistentListener(backButton.onClick, loader.LoadStageSelect);

        Text clearText = CreateText(canvas.transform, "ClearText", "CLEAR", 86, new Color(0.98f, 0.92f, 0.30f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(620f, 120f));
        clearText.gameObject.SetActive(false);

        Text gameOverText = CreateText(canvas.transform, "GameOverText", "GAME OVER", 78, new Color(1f, 0.42f, 0.42f, 1f),
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 35f), new Vector2(720f, 120f));
        gameOverText.gameObject.SetActive(false);

        GameObject uiManagerObject = new GameObject("UIManager");
        UIManager uiManager = uiManagerObject.AddComponent<UIManager>();
        SetObjectReference(uiManager, "livesText", livesText);
        SetObjectReference(uiManager, "stageNameText", stageNameText);
        SetObjectReference(uiManager, "clearText", clearText);
        SetObjectReference(uiManager, "gameOverText", gameOverText);

        SetInt(gameManager, "initialLives", 3);
        SetString(gameManager, "stageName", "Stage 1");
        SetObjectReference(gameManager, "ball", ballController);
        SetObjectReference(gameManager, "uiManager", uiManager);

        GameObject blocksParent = new GameObject("Blocks");
        GameObject builderObject = new GameObject("BlockGridBuilder");
        BlockGridBuilder builder = builderObject.AddComponent<BlockGridBuilder>();
        GameObject blockPrefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(BlockPrefabPath);
        SetObjectReference(builder, "blockPrefab", blockPrefabObject.GetComponent<Block>());
        SetObjectReference(builder, "gameManager", gameManager);
        SetObjectReference(builder, "blocksParent", blocksParent.transform);
        SetInt(builder, "rows", 5);
        SetInt(builder, "columns", 10);
        SetFloat(builder, "blockSize", 0.6f);
        SetFloat(builder, "spacing", 0.12f);
        SetVector2(builder, "startPosition", new Vector2(-3.24f, 3.25f));

        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static Camera CreateMainCamera(Color backgroundColor)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 1000f;

        cameraObject.AddComponent<AudioListener>();
        return camera;
    }

    private static void CreateWall(string name, Vector2 position, Vector2 size, Sprite squareSprite, PhysicsMaterial2D bouncyMaterial)
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
        collider.sharedMaterial = bouncyMaterial;
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
        Font defaultFont = GetDefaultFontSafe();
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

    private static Font GetDefaultFontSafe()
    {
        try
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"Built-in UI font could not be loaded. Generated Text objects will keep Unity's default font. {exception.Message}");
            return null;
        }
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

    private static void UpdateBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(TitleScenePath, true),
            new EditorBuildSettingsScene(StageSelectScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };
    }

    private static void SetObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        property.intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        property.stringValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector2(Object target, string propertyName, Vector2 value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        property.vector2Value = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}
