using System.IO;
using System.Linq;
using UnityEditor;
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
    private const string StagesFolder = "Assets/ScriptableObjects/Stages";

    private const string TitleScenePath = ScenesFolder + "/TitleScene.unity";
    private const string StageSelectScenePath = ScenesFolder + "/StageSelectScene.unity";
    private const string GameScenePath = ScenesFolder + "/GameScene.unity";
    private const string Stage1DataPath = StagesFolder + "/Stage1.asset";

    private const string PaddlePrefabPath = PrefabsFolder + "/Paddle.prefab";
    private const string BallPrefabPath = PrefabsFolder + "/Ball.prefab";
    private const string BlockPrefabPath = PrefabsFolder + "/Block.prefab";
    private const string ItemPrefabPath = PrefabsFolder + "/Item.prefab";

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
        StageData stage1Data = EnsureStage1Data(backgroundSprite);

        CreatePaddlePrefab(squareSprite, bouncyMaterial);
        CreateBallPrefab(ballSprite, bouncyMaterial);
        CreateItemPrefab(squareSprite);
        GameObject itemPrefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(ItemPrefabPath);
        ItemController itemPrefab = itemPrefabObject != null ? itemPrefabObject.GetComponent<ItemController>() : null;
        CreateBlockPrefab(squareSprite, bouncyMaterial, itemPrefab);

        CreateTitleScene();
        CreateStageSelectScene();
        CreateGameScene(stage1Data);
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Arkanoid Milestone 1 scenes, prefabs, stage data, and build settings were generated.");
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
            Stage1DataPath,
            PaddlePrefabPath,
            BallPrefabPath,
            BlockPrefabPath,
            ItemPrefabPath
        };

        return requiredPaths.All(File.Exists);
    }

    private static void EnsureFolders()
    {
        EnsureFolder(ScenesFolder);
        EnsureFolder(PrefabsFolder);
        EnsureFolder(BackgroundsFolder);
        EnsureFolder(PhysicsMaterialsFolder);
        EnsureFolder(StagesFolder);
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

    private static StageData EnsureStage1Data(Sprite backgroundSprite)
    {
        StageData stageData = AssetDatabase.LoadAssetAtPath<StageData>(Stage1DataPath);
        if (stageData != null)
        {
            FillMissingStage1References(stageData, backgroundSprite);
            return stageData;
        }

        stageData = ScriptableObject.CreateInstance<StageData>();
        stageData.name = "Stage1";
        AssetDatabase.CreateAsset(stageData, Stage1DataPath);

        SetInt(stageData, "stageId", 1);
        SetString(stageData, "stageName", "Stage 1");
        SetObjectReference(stageData, "backgroundSprite", backgroundSprite);
        SetInt(stageData, "blockRows", 5);
        SetInt(stageData, "blockColumns", 10);
        SetFloat(stageData, "blockSize", 0.6f);
        SetFloat(stageData, "blockSpacing", 0.12f);
        SetVector2(stageData, "blockStartPosition", new Vector2(-3.24f, 3.25f));
        SetFloat(stageData, "ballSpeed", 7f);
        SetFloat(stageData, "paddleSpeed", 9f);
        SetInt(stageData, "initialLives", 3);
        SetFloat(stageData, "itemDropChance", 0.5f);
        SetFloat(stageData, "paddleExpandMultiplier", 1.5f);
        SetFloat(stageData, "paddleExpandDuration", 8f);
        SetInt(stageData, "addBallsCount", 2);
        SetFloat(stageData, "addBallLaunchAngle", 25f);
        SetFloat(stageData, "addBallSpeed", 7f);

        EditorUtility.SetDirty(stageData);
        return stageData;
    }

    private static void FillMissingStage1References(StageData stageData, Sprite backgroundSprite)
    {
        if (stageData == null || backgroundSprite == null)
        {
            return;
        }

        SerializedObject serializedObject = new SerializedObject(stageData);
        SerializedProperty backgroundProperty = serializedObject.FindProperty("backgroundSprite");
        if (backgroundProperty == null || backgroundProperty.objectReferenceValue != null)
        {
            return;
        }

        backgroundProperty.objectReferenceValue = backgroundSprite;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(stageData);
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

    private static void CreateBlockPrefab(Sprite squareSprite, PhysicsMaterial2D bouncyMaterial, ItemController itemPrefab)
    {
        GameObject block = new GameObject("Block");

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.95f, 0.28f, 0.34f, 1f);
        renderer.sortingOrder = 5;

        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
        collider.sharedMaterial = bouncyMaterial;

        Block blockController = block.AddComponent<Block>();
        SetObjectReference(blockController, "itemPrefab", itemPrefab);
        SetFloat(blockController, "itemDropChance", 0.5f);
        SavePrefab(block, BlockPrefabPath);
    }

    private static void CreateItemPrefab(Sprite squareSprite)
    {
        GameObject item = new GameObject("Item");
        item.transform.localScale = new Vector3(0.38f, 0.38f, 1f);

        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.color = new Color(0.38f, 0.95f, 0.70f, 1f);
        renderer.sortingOrder = 15;

        CircleCollider2D collider = item.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        Rigidbody2D rigidbody = item.AddComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;

        ItemController controller = item.AddComponent<ItemController>();
        SetFloat(controller, "fallSpeed", 2.5f);
        SetFloat(controller, "destroyY", -5.8f);
        SetObjectReference(controller, "spriteRenderer", renderer);

        SavePrefab(item, ItemPrefabPath);
    }

    private static void SavePrefab(GameObject prefabRoot, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        Object.DestroyImmediate(prefabRoot);
    }

    private static void CreateTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateBootstrap(0, null);
        EditorSceneManager.SaveScene(scene, TitleScenePath);
    }

    private static void CreateStageSelectScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateBootstrap(1, null);
        EditorSceneManager.SaveScene(scene, StageSelectScenePath);
    }

    private static void CreateGameScene(StageData stageData)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateBootstrap(2, stageData);
        EditorSceneManager.SaveScene(scene, GameScenePath);
    }

    private static Milestone1SceneBootstrap CreateBootstrap(int sceneKind, StageData stageData)
    {
        GameObject bootstrapObject = new GameObject("Milestone1Bootstrap");
        Milestone1SceneBootstrap bootstrap = bootstrapObject.AddComponent<Milestone1SceneBootstrap>();
        SetEnumIndex(bootstrap, "sceneKind", sceneKind);

        if (stageData != null)
        {
            SetObjectReference(bootstrap, "stageData", stageData);
            SetString(bootstrap, "stageName", stageData.StageName);
            SetObjectReference(bootstrap, "fallbackBackgroundSprite", stageData.BackgroundSprite);
            SetInt(bootstrap, "blockRows", stageData.BlockRows);
            SetInt(bootstrap, "blockColumns", stageData.BlockColumns);
            SetFloat(bootstrap, "blockSize", stageData.BlockSize);
            SetFloat(bootstrap, "blockSpacing", stageData.BlockSpacing);
            SetVector2(bootstrap, "blockStartPosition", stageData.BlockStartPosition);
            SetFloat(bootstrap, "ballSpeed", stageData.BallSpeed);
            SetFloat(bootstrap, "paddleSpeed", stageData.PaddleSpeed);
            SetInt(bootstrap, "initialLives", stageData.InitialLives);
            SetFloat(bootstrap, "itemDropChance", stageData.ItemDropChance);
            SetFloat(bootstrap, "paddleExpandMultiplier", stageData.PaddleExpandMultiplier);
            SetFloat(bootstrap, "paddleExpandDuration", stageData.PaddleExpandDuration);
            SetInt(bootstrap, "addBallsCount", stageData.AddBallsCount);
            SetFloat(bootstrap, "addBallLaunchAngle", stageData.AddBallLaunchAngle);
            SetFloat(bootstrap, "addBallSpeed", stageData.AddBallSpeed);
        }

        return bootstrap;
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

    private static void SetEnumIndex(Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{propertyName}' was not found on {target.name}.");
            return;
        }

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            property.enumValueIndex = value;
        }
        else
        {
            property.intValue = value;
        }

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
