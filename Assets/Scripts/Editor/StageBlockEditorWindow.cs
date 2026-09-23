using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageBlockEditorWindow : EditorWindow
{
    private const string WindowTitle = "Stage Block Editor";
    private const int FixedRows = 25;
    private const int FixedColumns = 26;
    private const int MinCellSize = 16;
    private const int MaxCellSize = 40;
    private const int GridControlHint = 0x534245;
    private const float CoordinateTolerance = 0.0001f;

    private static readonly string[] ToolLabels =
    {
        "Normal [1]",
        "Durable [2]",
        "Indestructible [3]",
        "Erase [0]"
    };

    private static readonly Color WindowAccent = new Color32(255, 79, 216, 255);
    private static readonly Color PreviewBackground = new Color32(26, 13, 36, 255);
    private static readonly Color GridLineColor = new Color32(255, 214, 245, 255);
    private static readonly Color NormalColor = new Color32(255, 214, 245, 255);
    private static readonly Color DurableColor = new Color32(155, 72, 205, 255);
    private static readonly Color IndestructibleColor = new Color32(48, 40, 56, 255);
    private static readonly Color IndestructibleBorderColor = new Color32(100, 245, 255, 255);

    [SerializeField] private StageData selectedStage;
    [SerializeField] private int selectedTool;
    [SerializeField] private float cellSize = 24f;
    [SerializeField] private bool showBackground = true;
    [SerializeField, Range(0f, 1f)] private float backgroundPreviewOpacity = 0.85f;
    [SerializeField, Range(0f, 1f)] private float blockPreviewOpacity = 0.58f;

    private char[,] workingGrid;
    private char[,] loadedGrid;
    private int rowCount;
    private int columnCount;
    private float blockWorldSize;
    private float blockStep;
    private Rect playAreaWorldBounds;
    private Vector2 fixedGridStartPosition;
    private Vector2 gridScrollPosition;
    private bool hasPendingChanges;
    private bool sourceManualLayoutEnabled;
    private bool loadedFallbackGrid;
    private bool normalizedUnsupportedCharacters;
    private bool requiresFixedGridConversion;
    private bool sourceCellsSnapped;
    private float maximumSnapDistance;
    private int sourceOutOfBoundsCount;
    private int sourceCollisionCount;
    private readonly HashSet<int> paintedCells = new HashSet<int>();
    private GUIStyle titleStyle;
    private GUIStyle statusStyle;
    private GUIStyle cellLabelStyle;

    [MenuItem("Tools/Arkanoid/Stage Block Editor")]
    public static void OpenWindow()
    {
        StageBlockEditorWindow window = GetWindow<StageBlockEditorWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(600f, 460f);
        window.Show();
    }

    private void OnEnable()
    {
        titleContent = new GUIContent(WindowTitle);
        minSize = new Vector2(600f, 460f);
        Undo.undoRedoPerformed += HandleUndoRedo;

        if (selectedStage != null)
        {
            LoadStage(selectedStage);
        }
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= HandleUndoRedo;
    }

    private void OnGUI()
    {
        EnsureStyles();
        DrawHeader();
        DrawStageSelector();

        if (selectedStage == null)
        {
            EditorGUILayout.HelpBox(
                "Select a StageData asset to edit its block layout.",
                MessageType.Info);
            return;
        }

        if (workingGrid == null)
        {
            LoadStage(selectedStage);
        }

        DrawSourceStatus();
        DrawTools();
        DrawGridSummary();
        DrawGrid();
        DrawActions();
    }

    private void DrawHeader()
    {
        Rect headerRect = EditorGUILayout.GetControlRect(false, 42f);
        EditorGUI.DrawRect(headerRect, PreviewBackground);
        EditorGUI.DrawRect(
            new Rect(headerRect.x, headerRect.yMax - 2f, headerRect.width, 2f),
            WindowAccent);
        GUI.Label(headerRect, WindowTitle, titleStyle);
    }

    private void DrawStageSelector()
    {
        EditorGUILayout.Space(6f);
        StageData candidate = (StageData)EditorGUILayout.ObjectField(
            "StageData",
            selectedStage,
            typeof(StageData),
            false);

        if (candidate != selectedStage)
        {
            TryChangeStage(candidate);
        }
    }

    private void DrawSourceStatus()
    {
        if (!sourceManualLayoutEnabled)
        {
            EditorGUILayout.HelpBox(
                "Manual layout is OFF. The current blockRows x blockColumns grid was mapped into the fixed field. " +
                "Saving will enable useManualBlockLayout.",
                MessageType.Info);
        }
        else if (loadedFallbackGrid)
        {
            EditorGUILayout.HelpBox(
                "The saved manual layout was empty. The current full-grid fallback was mapped into the fixed field.",
                MessageType.Warning);
        }

        if (requiresFixedGridConversion)
        {
            EditorGUILayout.HelpBox(
                $"The source layout is shown inside the fixed {FixedColumns} x {FixedRows} field. " +
                "The StageData asset is not converted until Save is pressed.",
                MessageType.Info);
        }

        if (sourceCellsSnapped)
        {
            EditorGUILayout.HelpBox(
                $"Existing blocks were mapped to the nearest fixed-grid cells. " +
                $"Maximum position adjustment: {maximumSnapDistance:F4} world units.",
                MessageType.Warning);
        }

        if (normalizedUnsupportedCharacters)
        {
            EditorGUILayout.HelpBox(
                "Unsupported layout characters were displayed as empty cells. Saving will normalize every cell to 0-3.",
                MessageType.Warning);
        }

        if (sourceOutOfBoundsCount > 0 || sourceCollisionCount > 0)
        {
            EditorGUILayout.HelpBox(
                $"The source layout cannot be converted safely. Outside cells: {sourceOutOfBoundsCount}, " +
                $"overlapping mapped cells: {sourceCollisionCount}. Saving is disabled.",
                MessageType.Error);
        }
    }

    private void DrawTools()
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Paint Tool", EditorStyles.boldLabel);
        int nextTool = GUILayout.Toolbar(selectedTool, ToolLabels, GUILayout.Height(26f));
        if (nextTool != selectedTool)
        {
            selectedTool = nextTool;
            paintedCells.Clear();
        }

        cellSize = Mathf.Round(EditorGUILayout.Slider("Cell Size", cellSize, MinCellSize, MaxCellSize));
        showBackground = EditorGUILayout.Toggle("Show Background", showBackground);

        EditorGUI.BeginDisabledGroup(!showBackground);
        backgroundPreviewOpacity = EditorGUILayout.Slider(
            "Background Opacity",
            backgroundPreviewOpacity,
            0f,
            1f);
        EditorGUI.EndDisabledGroup();

        blockPreviewOpacity = EditorGUILayout.Slider(
            "Block Opacity",
            blockPreviewOpacity,
            0f,
            1f);
    }

    private void DrawGridSummary()
    {
        CountCells(out int normal, out int durable, out int indestructible, out int empty);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            $"Fixed Field: {columnCount} x {rowCount}",
            GUILayout.Width(180f));
        EditorGUILayout.LabelField(
            $"1: {normal}   2: {durable}   3: {indestructible}   Empty: {empty}");
        GUILayout.FlexibleSpace();
        GUILayout.Label(GetStatusText(), statusStyle, GUILayout.Width(145f));
        EditorGUILayout.EndHorizontal();

        Vector2 gridMin = GetGridWorldMin();
        Vector2 gridMax = GetGridWorldMax();
        EditorGUILayout.LabelField(
            $"PlayArea: X {playAreaWorldBounds.xMin:F3}..{playAreaWorldBounds.xMax:F3}, " +
            $"Y {playAreaWorldBounds.yMin:F3}..{playAreaWorldBounds.yMax:F3}   |   " +
            $"Grid coverage: X {gridMin.x:F3}..{gridMax.x:F3}, Y {gridMin.y:F3}..{gridMax.y:F3}",
            EditorStyles.miniLabel);
        EditorGUILayout.LabelField(
            "Orientation: row 1 is the top of the GameScene; columns run left to right.",
            EditorStyles.miniLabel);
    }

    private void DrawGrid()
    {
        if (blockStep <= 0f)
        {
            EditorGUILayout.HelpBox("The selected StageData has an invalid block step.", MessageType.Error);
            return;
        }

        float pixelsPerWorldUnit = cellSize / blockStep;
        float previewWidth = playAreaWorldBounds.width * pixelsPerWorldUnit;
        float previewHeight = playAreaWorldBounds.height * pixelsPerWorldUnit;

        gridScrollPosition = EditorGUILayout.BeginScrollView(
            gridScrollPosition,
            true,
            true,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        Rect contentRect = GUILayoutUtility.GetRect(
            previewWidth + 8f,
            previewHeight + 8f,
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false));
        Rect previewRect = new Rect(
            contentRect.x + 4f,
            contentRect.y + 4f,
            previewWidth,
            previewHeight);

        DrawBackgroundPreview(previewRect);

        Vector2 firstCellCenter = WorldToPreview(fixedGridStartPosition, previewRect);
        Rect gridRect = new Rect(
            firstCellCenter.x - cellSize * 0.5f,
            firstCellCenter.y - cellSize * 0.5f,
            columnCount * cellSize,
            rowCount * cellSize);

        DrawGridCells(gridRect, pixelsPerWorldUnit);
        DrawPlayAreaFrame(previewRect);
        HandleGridInput(gridRect);
        EditorGUILayout.EndScrollView();
    }

    private void DrawBackgroundPreview(Rect previewRect)
    {
        EditorGUI.DrawRect(previewRect, PreviewBackground);
        if (!showBackground || selectedStage == null || selectedStage.BackgroundSprite == null)
        {
            return;
        }

        Sprite sprite = selectedStage.BackgroundSprite;
        Texture2D texture = sprite.texture;
        Vector2 spriteSize = sprite.bounds.size;
        if (texture == null || spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        Vector2 fitScale = Milestone1SceneBootstrap.CalculateEditorBackgroundScale(
            playAreaWorldBounds.size,
            spriteSize,
            selectedStage.BackgroundFitMode);
        Vector2 multiplier = selectedStage.BackgroundScaleMultiplier;
        Vector2 finalScale = new Vector2(
            Mathf.Max(0.01f, fitScale.x * multiplier.x),
            Mathf.Max(0.01f, fitScale.y * multiplier.y));

        Vector2 worldPosition = playAreaWorldBounds.center + selectedStage.BackgroundOffset;
        Bounds spriteBounds = sprite.bounds;
        Vector2 worldMin = worldPosition + Vector2.Scale(
            new Vector2(spriteBounds.min.x, spriteBounds.min.y),
            finalScale);
        Vector2 worldMax = worldPosition + Vector2.Scale(
            new Vector2(spriteBounds.max.x, spriteBounds.max.y),
            finalScale);

        Vector2 guiTopLeft = WorldToPreview(new Vector2(worldMin.x, worldMax.y), previewRect);
        Vector2 guiBottomRight = WorldToPreview(new Vector2(worldMax.x, worldMin.y), previewRect);
        Rect drawRect = Rect.MinMaxRect(
            guiTopLeft.x,
            guiTopLeft.y,
            guiBottomRight.x,
            guiBottomRight.y);
        Rect uvRect = GetSpriteUvRect(sprite, texture);

        GUI.BeginClip(previewRect);
        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(backgroundPreviewOpacity));
        Rect clippedDrawRect = new Rect(
            drawRect.x - previewRect.x,
            drawRect.y - previewRect.y,
            drawRect.width,
            drawRect.height);
        GUI.DrawTextureWithTexCoords(clippedDrawRect, texture, uvRect, true);
        GUI.color = previousColor;
        GUI.EndClip();
    }

    private void DrawGridCells(Rect gridRect, float pixelsPerWorldUnit)
    {
        int labelFontSize = Mathf.Clamp(Mathf.RoundToInt(cellSize * 0.45f), 8, 16);
        cellLabelStyle.fontSize = labelFontSize;
        float blockPixelSize = Mathf.Clamp(
            blockWorldSize * pixelsPerWorldUnit,
            1f,
            cellSize);

        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + column * cellSize,
                    gridRect.y + row * cellSize,
                    cellSize,
                    cellSize);
                DrawCell(cellRect, workingGrid[row, column], blockPixelSize);
            }
        }
    }

    private void DrawCell(Rect cellRect, char value, float blockPixelSize)
    {
        if (value != '0')
        {
            Rect blockRect = new Rect(
                cellRect.center.x - blockPixelSize * 0.5f,
                cellRect.center.y - blockPixelSize * 0.5f,
                blockPixelSize,
                blockPixelSize);
            Color fillColor = GetBlockPreviewColor(value);
            EditorGUI.DrawRect(blockRect, WithAlpha(fillColor, blockPreviewOpacity));

            Color outlineColor = value == '3' ? IndestructibleBorderColor : WindowAccent;
            DrawRectOutline(blockRect, WithAlpha(outlineColor, Mathf.Max(0.55f, blockPreviewOpacity)), 1f);
        }

        DrawRectOutline(cellRect, WithAlpha(GridLineColor, 0.38f), 1f);
        cellLabelStyle.normal.textColor = value == '3'
            ? IndestructibleBorderColor
            : value == '0'
                ? new Color32(190, 180, 195, 205)
                : Color.white;
        GUI.Label(cellRect, value.ToString(), cellLabelStyle);
    }

    private void DrawPlayAreaFrame(Rect previewRect)
    {
        DrawRectOutline(previewRect, WithAlpha(IndestructibleBorderColor, 0.85f), 2f);
    }

    private void HandleGridInput(Rect gridRect)
    {
        Event currentEvent = Event.current;
        int controlId = GUIUtility.GetControlID(GridControlHint, FocusType.Passive, gridRect);

        if (currentEvent.type == EventType.MouseDown &&
            currentEvent.button == 0 &&
            gridRect.Contains(currentEvent.mousePosition))
        {
            GUIUtility.hotControl = controlId;
            paintedCells.Clear();
            PaintCellAt(currentEvent.mousePosition, gridRect);
            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseDrag && GUIUtility.hotControl == controlId)
        {
            PaintCellAt(currentEvent.mousePosition, gridRect);
            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseUp && GUIUtility.hotControl == controlId)
        {
            GUIUtility.hotControl = 0;
            paintedCells.Clear();
            currentEvent.Use();
        }
    }

    private void PaintCellAt(Vector2 mousePosition, Rect gridRect)
    {
        if (!gridRect.Contains(mousePosition) || workingGrid == null)
        {
            return;
        }

        int column = Mathf.FloorToInt((mousePosition.x - gridRect.x) / cellSize);
        int row = Mathf.FloorToInt((mousePosition.y - gridRect.y) / cellSize);
        if (row < 0 || row >= rowCount || column < 0 || column >= columnCount)
        {
            return;
        }

        int cellKey = row * columnCount + column;
        if (!paintedCells.Add(cellKey))
        {
            return;
        }

        char paintValue = GetSelectedPaintValue();
        if (workingGrid[row, column] == paintValue)
        {
            return;
        }

        workingGrid[row, column] = paintValue;
        hasPendingChanges = !AreGridsEqual(workingGrid, loadedGrid);
        Repaint();
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = selectedStage != null &&
                      workingGrid != null &&
                      sourceOutOfBoundsCount == 0 &&
                      sourceCollisionCount == 0;
        if (GUILayout.Button("Save Fixed Layout", GUILayout.Height(28f)))
        {
            SaveLayout();
        }

        GUI.enabled = selectedStage != null && workingGrid != null && hasPendingChanges;
        if (GUILayout.Button("Revert", GUILayout.Height(28f)))
        {
            RevertLayout();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void TryChangeStage(StageData candidate)
    {
        if (hasPendingChanges)
        {
            bool discard = EditorUtility.DisplayDialog(
                WindowTitle,
                "The current layout has unsaved changes. Discard them and switch StageData?",
                "Discard and Switch",
                "Cancel");

            if (!discard)
            {
                return;
            }
        }

        selectedStage = candidate;
        if (selectedStage != null)
        {
            LoadStage(selectedStage);
        }
        else
        {
            ClearWorkingState();
        }

        Repaint();
    }

    private void LoadStage(StageData stage)
    {
        if (stage == null)
        {
            ClearWorkingState();
            return;
        }

        SerializedObject serializedStage = new SerializedObject(stage);
        serializedStage.UpdateIfRequiredOrScript();
        SerializedProperty manualProperty = serializedStage.FindProperty("useManualBlockLayout");
        SerializedProperty layoutProperty = serializedStage.FindProperty("blockLayout");
        SerializedProperty rowsProperty = serializedStage.FindProperty("blockRows");
        SerializedProperty columnsProperty = serializedStage.FindProperty("blockColumns");

        if (manualProperty == null || layoutProperty == null || rowsProperty == null || columnsProperty == null)
        {
            selectedStage = null;
            ClearWorkingState();
            EditorUtility.DisplayDialog(
                WindowTitle,
                "The selected StageData does not contain the expected block layout fields.",
                "OK");
            return;
        }

        rowCount = FixedRows;
        columnCount = FixedColumns;
        blockWorldSize = Mathf.Max(0.1f, stage.BlockSize);
        blockStep = blockWorldSize + Mathf.Max(0f, stage.BlockSpacing);
        Vector2 playAreaCenter = Milestone1SceneBootstrap.EditorPlayAreaCenter;
        Vector2 playAreaSize = Milestone1SceneBootstrap.EditorPlayAreaSize;
        playAreaWorldBounds = new Rect(playAreaCenter - playAreaSize * 0.5f, playAreaSize);
        fixedGridStartPosition = Milestone1SceneBootstrap.CalculateEditorBlockStart(
            stage,
            rowCount,
            columnCount);
        workingGrid = CreateFilledGrid(rowCount, columnCount, '0');

        sourceManualLayoutEnabled = manualProperty.boolValue;
        normalizedUnsupportedCharacters = false;
        loadedFallbackGrid = false;
        sourceCellsSnapped = false;
        maximumSnapDistance = 0f;
        sourceOutOfBoundsCount = 0;
        sourceCollisionCount = 0;

        int sourceRows = 0;
        int sourceColumns = 0;
        bool loadedManual = sourceManualLayoutEnabled &&
                            TryGetLayoutDimensions(layoutProperty, out sourceRows, out sourceColumns);
        if (loadedManual)
        {
            MapManualLayoutToFixedGrid(layoutProperty, sourceRows, sourceColumns);
            requiresFixedGridConversion = !IsFixedLayout(layoutProperty);
        }
        else
        {
            sourceRows = Mathf.Max(1, rowsProperty.intValue);
            sourceColumns = Mathf.Max(1, columnsProperty.intValue);
            MapFullGridToFixedGrid(stage, sourceRows, sourceColumns);
            loadedFallbackGrid = sourceManualLayoutEnabled;
            requiresFixedGridConversion = true;
        }

        loadedGrid = CloneGrid(workingGrid);
        hasPendingChanges = false;
        gridScrollPosition = Vector2.zero;
        paintedCells.Clear();
    }

    private void MapManualLayoutToFixedGrid(
        SerializedProperty layoutProperty,
        int sourceRows,
        int sourceColumns)
    {
        Vector2 sourceStart = Milestone1SceneBootstrap.CalculateEditorBlockStart(
            selectedStage,
            sourceRows,
            sourceColumns);
        for (int row = 0; row < sourceRows; row++)
        {
            string rowText = layoutProperty.GetArrayElementAtIndex(row).stringValue ?? string.Empty;
            for (int column = 0; column < rowText.Length; column++)
            {
                char value = rowText[column];
                if (!IsSupportedLayoutValue(value))
                {
                    normalizedUnsupportedCharacters = true;
                    continue;
                }

                value = NormalizeLayoutValue(value);
                if (value == '0')
                {
                    continue;
                }

                Vector2 worldPosition = sourceStart + new Vector2(
                    column * blockStep,
                    -row * blockStep);
                MapBlockToFixedGrid(worldPosition, value);
            }
        }
    }

    private void MapFullGridToFixedGrid(StageData stage, int sourceRows, int sourceColumns)
    {
        Vector2 sourceStart = Milestone1SceneBootstrap.CalculateEditorBlockStart(
            stage,
            sourceRows,
            sourceColumns);
        for (int row = 0; row < sourceRows; row++)
        {
            for (int column = 0; column < sourceColumns; column++)
            {
                Vector2 worldPosition = sourceStart + new Vector2(
                    column * blockStep,
                    -row * blockStep);
                MapBlockToFixedGrid(worldPosition, '1');
            }
        }
    }

    private void MapBlockToFixedGrid(Vector2 worldPosition, char value)
    {
        float columnPosition = (worldPosition.x - fixedGridStartPosition.x) / blockStep;
        float rowPosition = (fixedGridStartPosition.y - worldPosition.y) / blockStep;
        int column = Mathf.FloorToInt(columnPosition + 0.5f);
        int row = Mathf.FloorToInt(rowPosition + 0.5f);

        if (row < 0 || row >= rowCount || column < 0 || column >= columnCount)
        {
            sourceOutOfBoundsCount++;
            return;
        }

        Vector2 mappedWorldPosition = fixedGridStartPosition + new Vector2(
            column * blockStep,
            -row * blockStep);
        float snapDistance = Vector2.Distance(worldPosition, mappedWorldPosition);
        if (snapDistance > CoordinateTolerance)
        {
            sourceCellsSnapped = true;
            maximumSnapDistance = Mathf.Max(maximumSnapDistance, snapDistance);
        }

        if (workingGrid[row, column] != '0')
        {
            sourceCollisionCount++;
            return;
        }

        workingGrid[row, column] = value;
    }

    private void SaveLayout()
    {
        if (selectedStage == null || workingGrid == null)
        {
            return;
        }

        if (!AssetDatabase.Contains(selectedStage))
        {
            EditorUtility.DisplayDialog(
                WindowTitle,
                "Select a saved StageData asset before saving the layout.",
                "OK");
            return;
        }

        if (!ValidateGrid(out string validationError))
        {
            EditorUtility.DisplayDialog(WindowTitle, validationError, "OK");
            return;
        }

        SerializedObject serializedStage = new SerializedObject(selectedStage);
        SerializedProperty layoutProperty = serializedStage.FindProperty("blockLayout");
        SerializedProperty manualProperty = serializedStage.FindProperty("useManualBlockLayout");
        if (layoutProperty == null || manualProperty == null)
        {
            EditorUtility.DisplayDialog(
                WindowTitle,
                "The selected StageData does not contain the expected block layout fields.",
                "OK");
            return;
        }

        Undo.RecordObject(selectedStage, "Save Fixed Stage Block Layout");
        layoutProperty.arraySize = FixedRows;
        for (int row = 0; row < FixedRows; row++)
        {
            layoutProperty.GetArrayElementAtIndex(row).stringValue = BuildRowString(row);
        }

        manualProperty.boolValue = true;
        serializedStage.ApplyModifiedProperties();
        EditorUtility.SetDirty(selectedStage);
        AssetDatabase.SaveAssets();

        sourceManualLayoutEnabled = true;
        loadedFallbackGrid = false;
        normalizedUnsupportedCharacters = false;
        requiresFixedGridConversion = false;
        sourceCellsSnapped = false;
        maximumSnapDistance = 0f;
        sourceOutOfBoundsCount = 0;
        sourceCollisionCount = 0;
        loadedGrid = CloneGrid(workingGrid);
        hasPendingChanges = false;
        ShowNotification(new GUIContent("Fixed block layout saved."));
        Repaint();
    }

    private void RevertLayout()
    {
        if (selectedStage == null)
        {
            return;
        }

        LoadStage(selectedStage);
        ShowNotification(new GUIContent("Unsaved changes reverted."));
        Repaint();
    }

    private bool ValidateGrid(out string errorMessage)
    {
        if (sourceOutOfBoundsCount > 0 || sourceCollisionCount > 0)
        {
            errorMessage =
                "The existing layout does not fit the fixed grid without losing blocks. " +
                "No StageData changes were made.";
            return false;
        }

        if (rowCount != FixedRows || columnCount != FixedColumns)
        {
            errorMessage = $"The layout must remain {FixedColumns} columns x {FixedRows} rows.";
            return false;
        }

        int clearTargets = 0;
        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                char value = workingGrid[row, column];
                if (value != '0' && value != '1' && value != '2' && value != '3')
                {
                    errorMessage =
                        $"Unsupported value '{value}' at row {row + 1}, column {column + 1}.";
                    return false;
                }

                if (value == '1' || value == '2')
                {
                    clearTargets++;
                }
            }
        }

        if (clearTargets <= 0)
        {
            errorMessage =
                "The layout needs at least one normal [1] or durable [2] block. " +
                "An empty or indestructible-only stage cannot be saved.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    private static bool TryGetLayoutDimensions(
        SerializedProperty layoutProperty,
        out int rows,
        out int columns)
    {
        rows = layoutProperty.arraySize;
        columns = 0;
        for (int row = 0; row < rows; row++)
        {
            string rowText = layoutProperty.GetArrayElementAtIndex(row).stringValue ?? string.Empty;
            columns = Mathf.Max(columns, rowText.Length);
        }

        return rows > 0 && columns > 0;
    }

    private static bool IsFixedLayout(SerializedProperty layoutProperty)
    {
        if (layoutProperty.arraySize != FixedRows)
        {
            return false;
        }

        for (int row = 0; row < FixedRows; row++)
        {
            string rowText = layoutProperty.GetArrayElementAtIndex(row).stringValue ?? string.Empty;
            if (rowText.Length != FixedColumns)
            {
                return false;
            }
        }

        return true;
    }

    private void HandleUndoRedo()
    {
        if (selectedStage != null && !hasPendingChanges)
        {
            LoadStage(selectedStage);
        }

        Repaint();
    }

    private void ClearWorkingState()
    {
        workingGrid = null;
        loadedGrid = null;
        rowCount = 0;
        columnCount = 0;
        blockWorldSize = 0f;
        blockStep = 0f;
        playAreaWorldBounds = default;
        fixedGridStartPosition = Vector2.zero;
        hasPendingChanges = false;
        sourceManualLayoutEnabled = false;
        loadedFallbackGrid = false;
        normalizedUnsupportedCharacters = false;
        requiresFixedGridConversion = false;
        sourceCellsSnapped = false;
        maximumSnapDistance = 0f;
        sourceOutOfBoundsCount = 0;
        sourceCollisionCount = 0;
        paintedCells.Clear();
    }

    private void CountCells(out int normal, out int durable, out int indestructible, out int empty)
    {
        normal = 0;
        durable = 0;
        indestructible = 0;
        empty = 0;

        if (workingGrid == null)
        {
            return;
        }

        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                switch (workingGrid[row, column])
                {
                    case '1':
                        normal++;
                        break;
                    case '2':
                        durable++;
                        break;
                    case '3':
                        indestructible++;
                        break;
                    default:
                        empty++;
                        break;
                }
            }
        }
    }

    private string BuildRowString(int row)
    {
        char[] values = new char[FixedColumns];
        for (int column = 0; column < FixedColumns; column++)
        {
            values[column] = workingGrid[row, column];
        }

        return new string(values);
    }

    private string GetStatusText()
    {
        if (hasPendingChanges)
        {
            return "Unsaved Changes";
        }

        return requiresFixedGridConversion ? "Preview Only" : "Saved Fixed Layout";
    }

    private char GetSelectedPaintValue()
    {
        switch (selectedTool)
        {
            case 0:
                return '1';
            case 1:
                return '2';
            case 2:
                return '3';
            default:
                return '0';
        }
    }

    private Vector2 WorldToPreview(Vector2 worldPosition, Rect previewRect)
    {
        float normalizedX = Mathf.InverseLerp(
            playAreaWorldBounds.xMin,
            playAreaWorldBounds.xMax,
            worldPosition.x);
        float normalizedY = Mathf.InverseLerp(
            playAreaWorldBounds.yMin,
            playAreaWorldBounds.yMax,
            worldPosition.y);
        return new Vector2(
            previewRect.x + normalizedX * previewRect.width,
            previewRect.y + (1f - normalizedY) * previewRect.height);
    }

    private Vector2 GetGridWorldMin()
    {
        return new Vector2(
            fixedGridStartPosition.x - blockWorldSize * 0.5f,
            fixedGridStartPosition.y - (rowCount - 1) * blockStep - blockWorldSize * 0.5f);
    }

    private Vector2 GetGridWorldMax()
    {
        return new Vector2(
            fixedGridStartPosition.x + (columnCount - 1) * blockStep + blockWorldSize * 0.5f,
            fixedGridStartPosition.y + blockWorldSize * 0.5f);
    }

    private static Rect GetSpriteUvRect(Sprite sprite, Texture2D texture)
    {
        try
        {
            Rect textureRect = sprite.textureRect;
            return new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
        }
        catch (UnityException)
        {
            return new Rect(0f, 0f, 1f, 1f);
        }
    }

    private static Color GetBlockPreviewColor(char value)
    {
        switch (value)
        {
            case '1':
                return NormalColor;
            case '2':
                return DurableColor;
            case '3':
                return IndestructibleColor;
            default:
                return Color.clear;
        }
    }

    private static void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        float safeThickness = Mathf.Max(1f, thickness);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, safeThickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - safeThickness, rect.width, safeThickness), color);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, safeThickness, rect.height), color);
        EditorGUI.DrawRect(new Rect(rect.xMax - safeThickness, rect.y, safeThickness, rect.height), color);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static bool IsSupportedLayoutValue(char value)
    {
        return value == '0' || value == '1' || value == '2' || value == '3' ||
               value == '.' || value == ' ';
    }

    private static char NormalizeLayoutValue(char value)
    {
        return value == '.' || value == ' ' ? '0' : value;
    }

    private static char[,] CreateFilledGrid(int rows, int columns, char value)
    {
        char[,] grid = new char[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                grid[row, column] = value;
            }
        }

        return grid;
    }

    private static char[,] CloneGrid(char[,] source)
    {
        if (source == null)
        {
            return null;
        }

        int rows = source.GetLength(0);
        int columns = source.GetLength(1);
        char[,] clone = new char[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                clone[row, column] = source[row, column];
            }
        }

        return clone;
    }

    private static bool AreGridsEqual(char[,] first, char[,] second)
    {
        if (ReferenceEquals(first, second))
        {
            return true;
        }

        if (first == null || second == null ||
            first.GetLength(0) != second.GetLength(0) ||
            first.GetLength(1) != second.GetLength(1))
        {
            return false;
        }

        for (int row = 0; row < first.GetLength(0); row++)
        {
            for (int column = 0; column < first.GetLength(1); column++)
            {
                if (first[row, column] != second[row, column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void EnsureStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            titleStyle.normal.textColor = WindowAccent;
        }

        if (statusStyle == null)
        {
            statusStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleRight
            };
        }

        statusStyle.normal.textColor = hasPendingChanges
            ? new Color32(255, 216, 102, 255)
            : requiresFixedGridConversion
                ? WindowAccent
                : new Color32(100, 245, 255, 255);

        if (cellLabelStyle == null)
        {
            cellLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip
            };
        }
    }
}
