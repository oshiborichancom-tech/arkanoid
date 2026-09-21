using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageBlockEditorWindow : EditorWindow
{
    private const string WindowTitle = "Stage Block Editor";
    private const int MinCellSize = 16;
    private const int MaxCellSize = 40;
    private const int GridControlHint = 0x534245;

    private static readonly string[] ToolLabels =
    {
        "Normal [1]",
        "Durable [2]",
        "Indestructible [3]",
        "Erase [0]"
    };

    private static readonly Color WindowAccent = new Color32(255, 79, 216, 255);
    private static readonly Color EmptyColor = new Color32(26, 13, 36, 255);
    private static readonly Color EmptyBorderColor = new Color32(85, 80, 90, 255);
    private static readonly Color NormalColor = new Color32(255, 214, 245, 255);
    private static readonly Color DurableColor = new Color32(155, 72, 205, 255);
    private static readonly Color IndestructibleColor = new Color32(48, 40, 56, 255);
    private static readonly Color IndestructibleBorderColor = new Color32(100, 245, 255, 255);

    [SerializeField] private StageData selectedStage;
    [SerializeField] private int selectedTool;
    [SerializeField] private float cellSize = 24f;

    private char[,] workingGrid;
    private char[,] loadedGrid;
    private int rowCount;
    private int columnCount;
    private Vector2 gridScrollPosition;
    private bool hasUnsavedChanges;
    private bool sourceManualLayoutEnabled;
    private bool loadedFallbackGrid;
    private bool normalizedUnsupportedCharacters;
    private readonly HashSet<int> paintedCells = new HashSet<int>();
    private GUIStyle titleStyle;
    private GUIStyle statusStyle;
    private GUIStyle cellLabelStyle;

    [MenuItem("Tools/Arkanoid/Stage Block Editor")]
    public static void OpenWindow()
    {
        StageBlockEditorWindow window = GetWindow<StageBlockEditorWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(560f, 420f);
        window.Show();
    }

    private void OnEnable()
    {
        titleContent = new GUIContent(WindowTitle);
        minSize = new Vector2(560f, 420f);
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
        EditorGUI.DrawRect(headerRect, new Color32(26, 13, 36, 255));
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
                "Manual layout is OFF. The preview uses blockRows x blockColumns filled with normal blocks. " +
                "Saving will enable useManualBlockLayout.",
                MessageType.Info);
        }
        else if (loadedFallbackGrid)
        {
            EditorGUILayout.HelpBox(
                "The saved manual layout was empty. The preview uses the full blockRows x blockColumns fallback grid.",
                MessageType.Warning);
        }

        if (normalizedUnsupportedCharacters)
        {
            EditorGUILayout.HelpBox(
                "Unsupported layout characters were displayed as empty cells. Saving will normalize them to 0.",
                MessageType.Warning);
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

        cellSize = EditorGUILayout.Slider("Cell Size", cellSize, MinCellSize, MaxCellSize);
        cellSize = Mathf.Round(cellSize);
    }

    private void DrawGridSummary()
    {
        CountCells(out int normal, out int durable, out int indestructible, out int empty);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(
            $"Rows: {rowCount}   Columns: {columnCount}",
            GUILayout.Width(190f));
        EditorGUILayout.LabelField(
            $"1: {normal}   2: {durable}   3: {indestructible}   Empty: {empty}");
        GUILayout.FlexibleSpace();
        GUILayout.Label(
            hasUnsavedChanges ? "Unsaved Changes" : "Saved",
            statusStyle,
            GUILayout.Width(120f));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(
            "Orientation: row 1 is the top of the GameScene; columns run left to right.",
            EditorStyles.miniLabel);
    }

    private void DrawGrid()
    {
        float gridWidth = Mathf.Max(1, columnCount) * cellSize;
        float gridHeight = Mathf.Max(1, rowCount) * cellSize;

        gridScrollPosition = EditorGUILayout.BeginScrollView(
            gridScrollPosition,
            true,
            true,
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true));

        Rect gridRect = GUILayoutUtility.GetRect(
            gridWidth,
            gridHeight,
            GUILayout.ExpandWidth(false),
            GUILayout.ExpandHeight(false));

        DrawGridCells(gridRect);
        HandleGridInput(gridRect);
        EditorGUILayout.EndScrollView();
    }

    private void DrawGridCells(Rect gridRect)
    {
        int labelFontSize = Mathf.Clamp(Mathf.RoundToInt(cellSize * 0.5f), 9, 16);
        cellLabelStyle.fontSize = labelFontSize;

        for (int row = 0; row < rowCount; row++)
        {
            for (int column = 0; column < columnCount; column++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + column * cellSize,
                    gridRect.y + row * cellSize,
                    cellSize,
                    cellSize);
                char value = workingGrid[row, column];
                DrawCell(cellRect, value);
            }
        }
    }

    private void DrawCell(Rect cellRect, char value)
    {
        Color fillColor;
        Color borderColor;
        Color textColor;

        switch (value)
        {
            case '1':
                fillColor = NormalColor;
                borderColor = WindowAccent;
                textColor = new Color32(40, 20, 45, 255);
                break;
            case '2':
                fillColor = DurableColor;
                borderColor = WindowAccent;
                textColor = Color.white;
                break;
            case '3':
                fillColor = IndestructibleColor;
                borderColor = IndestructibleBorderColor;
                textColor = IndestructibleBorderColor;
                break;
            default:
                fillColor = EmptyColor;
                borderColor = EmptyBorderColor;
                textColor = new Color32(145, 135, 150, 255);
                break;
        }

        EditorGUI.DrawRect(cellRect, borderColor);
        Rect innerRect = new Rect(
            cellRect.x + 1f,
            cellRect.y + 1f,
            Mathf.Max(0f, cellRect.width - 2f),
            Mathf.Max(0f, cellRect.height - 2f));
        EditorGUI.DrawRect(innerRect, fillColor);

        cellLabelStyle.normal.textColor = textColor;
        GUI.Label(cellRect, value == '0' ? "0" : value.ToString(), cellLabelStyle);
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
        hasUnsavedChanges = !AreGridsEqual(workingGrid, loadedGrid);
        Repaint();
    }

    private void DrawActions()
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = selectedStage != null && workingGrid != null;
        if (GUILayout.Button("Save", GUILayout.Height(28f)))
        {
            SaveLayout();
        }

        GUI.enabled = selectedStage != null && workingGrid != null && hasUnsavedChanges;
        if (GUILayout.Button("Revert", GUILayout.Height(28f)))
        {
            RevertLayout();
        }

        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    private void TryChangeStage(StageData candidate)
    {
        if (hasUnsavedChanges)
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
            ClearWorkingState();
            EditorUtility.DisplayDialog(
                WindowTitle,
                "The selected StageData does not contain the expected block layout fields.",
                "OK");
            return;
        }

        sourceManualLayoutEnabled = manualProperty.boolValue;
        normalizedUnsupportedCharacters = false;
        loadedFallbackGrid = false;

        if (sourceManualLayoutEnabled && TryLoadManualGrid(layoutProperty))
        {
            loadedFallbackGrid = false;
        }
        else
        {
            rowCount = Mathf.Max(1, rowsProperty.intValue);
            columnCount = Mathf.Max(1, columnsProperty.intValue);
            workingGrid = CreateFilledGrid(rowCount, columnCount, '1');
            loadedFallbackGrid = sourceManualLayoutEnabled;
        }

        loadedGrid = CloneGrid(workingGrid);
        hasUnsavedChanges = false;
        gridScrollPosition = Vector2.zero;
        paintedCells.Clear();
    }

    private bool TryLoadManualGrid(SerializedProperty layoutProperty)
    {
        int rows = layoutProperty.arraySize;
        if (rows <= 0)
        {
            return false;
        }

        int columns = 0;
        for (int row = 0; row < rows; row++)
        {
            string rowText = layoutProperty.GetArrayElementAtIndex(row).stringValue ?? string.Empty;
            columns = Mathf.Max(columns, rowText.Length);
        }

        if (columns <= 0)
        {
            return false;
        }

        rowCount = rows;
        columnCount = columns;
        workingGrid = CreateFilledGrid(rowCount, columnCount, '0');

        for (int row = 0; row < rowCount; row++)
        {
            string rowText = layoutProperty.GetArrayElementAtIndex(row).stringValue ?? string.Empty;
            for (int column = 0; column < rowText.Length; column++)
            {
                char value = rowText[column];
                if (IsSupportedLayoutValue(value))
                {
                    workingGrid[row, column] = NormalizeLayoutValue(value);
                }
                else
                {
                    workingGrid[row, column] = '0';
                    normalizedUnsupportedCharacters = true;
                }
            }
        }

        return true;
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

        Undo.RecordObject(selectedStage, "Save Stage Block Layout");
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

        layoutProperty.arraySize = rowCount;
        for (int row = 0; row < rowCount; row++)
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
        loadedGrid = CloneGrid(workingGrid);
        hasUnsavedChanges = false;
        ShowNotification(new GUIContent("Block layout saved."));
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
        if (rowCount < 1)
        {
            errorMessage = "The layout must contain at least one row.";
            return false;
        }

        if (columnCount < 1)
        {
            errorMessage = "The layout must contain at least one column.";
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

    private void HandleUndoRedo()
    {
        if (selectedStage != null && !hasUnsavedChanges)
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
        hasUnsavedChanges = false;
        sourceManualLayoutEnabled = false;
        loadedFallbackGrid = false;
        normalizedUnsupportedCharacters = false;
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
        char[] values = new char[columnCount];
        for (int column = 0; column < columnCount; column++)
        {
            values[column] = workingGrid[row, column];
        }

        return new string(values);
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

        statusStyle.normal.textColor = hasUnsavedChanges
            ? new Color32(255, 216, 102, 255)
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
