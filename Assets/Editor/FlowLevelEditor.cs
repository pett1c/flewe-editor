using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FlowLevelEditor : EditorWindow
{
    private int gridWidth = 5;
    private int gridHeight = 5;
    private float cellSize = 40f;
    private Color[] availableColors;
    private int selectedColorIndex = 0;
    private Vector2Int? firstDot = null;
    private List<DotPair> dotPairs = new List<DotPair>();
    private Vector2 scrollPosition;
    private LevelData currentLevel;
    private bool isTestMode = false;
    private string selectedLevelPath;
    private bool isEditingExistingLevel = false;
    private Vector2 levelSelectScrollPosition;
    private bool showLevelSelector = false;
    private Dictionary<string, List<string>> levelPaths = new Dictionary<string, List<string>>();

    // Тестовые переменные
    private Vector2Int? lastGridPosition;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private Dictionary<Color, List<Vector2Int>> colorToPaths = new Dictionary<Color, List<Vector2Int>>();
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
    private Vector2Int? activeStartDot = null;

    [MenuItem("Flow Free/Level Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<FlowLevelEditor>();
        window.titleContent = new GUIContent("Flow Level Editor");
    }

    private void OnEnable()
    {
        availableColors = new Color[]
        {
           new Color(0.91f, 0.29f, 0.28f),    // #E84947 - красный
           new Color(0.22f, 0.65f, 0.87f),    // #39A7DE - голубой
           new Color(0.55f, 0.79f, 0.04f),    // #8BC90B - зеленый
           new Color(0.96f, 0.88f, 0.17f),    // #F4E02B - желтый
           new Color(0.37f, 0.27f, 0.58f),    // #5F4493 - фиолетовый
           new Color(1.00f, 0.42f, 0.71f),    // #FF6BB5 - розовый
           new Color(0.00f, 0.82f, 0.43f),    // #00D26E - изумрудный
           new Color(1.00f, 0.58f, 0.00f),    // #FF9500 - оранжевый
           new Color(0.29f, 0.18f, 0.89f),    // #4A2FE2 - синий
           new Color(0.78f, 0.61f, 1.00f)     // #C79BFF - светло-фиолетовый
        };

        currentLevel = ScriptableObject.CreateInstance<LevelData>();
        currentLevel.gridWidth = gridWidth;
        currentLevel.gridHeight = gridHeight;
    }

    private void OnGUI()
    {
        DrawSettings();
        DrawTestModeToggle();
        if (!isTestMode)
        {
            DrawColorSelector();
        }
        DrawGrid();
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginHorizontal();
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth, GUILayout.Width(200));
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create New Level"))
        {
            ResetLevel();
            isEditingExistingLevel = false;
            selectedLevelPath = null;
        }

        if (GUILayout.Button("Edit Existing Level"))
        {
            showLevelSelector = true;
            UpdateLevelPaths();
        }
        EditorGUILayout.EndHorizontal();

        if (isEditingExistingLevel)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Save Level"))
            {
                SaveExistingLevel();
            }
        }
        else
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Export Level"))
            {
                ExportLevel();
            }
        }

        if (showLevelSelector)
        {
            DrawLevelSelector();
        }
    }

    private void UpdateLevelPaths()
    {
        levelPaths.Clear();
        levelPaths["LevelData"] = new List<string>();
        levelPaths["EditorLevelData"] = new List<string>();

        // Поиск файлов в папке LevelData
        string levelDataPath = "Assets/LevelData";
        if (AssetDatabase.IsValidFolder(levelDataPath))
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { levelDataPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                levelPaths["LevelData"].Add(path);
            }
        }

        // Поиск файлов в папке Editor/Levels
        string editorLevelPath = "Assets/Editor/Levels";
        if (AssetDatabase.IsValidFolder(editorLevelPath))
        {
            string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { editorLevelPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                levelPaths["EditorLevelData"].Add(path);
            }
        }
    }

    private void DrawLevelSelector()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Level to Edit", EditorStyles.boldLabel);

        levelSelectScrollPosition = EditorGUILayout.BeginScrollView(levelSelectScrollPosition,
            GUILayout.Height(200));

        foreach (var folder in levelPaths.Keys)
        {
            EditorGUILayout.LabelField(folder, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            foreach (string path in levelPaths[folder])
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
                if (GUILayout.Button(fileName, EditorStyles.label))
                {
                    LoadLevel(path);
                    showLevelSelector = false;
                }
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Close"))
        {
            showLevelSelector = false;
        }
    }

    private void LoadLevel(string path)
    {
        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);
        if (levelData != null)
        {
            selectedLevelPath = path;
            isEditingExistingLevel = true;
            gridWidth = levelData.gridWidth;
            gridHeight = levelData.gridHeight;
            dotPairs.Clear();
            dotPairs.AddRange(levelData.dotPairs);
            ResetTestMode();
        }
    }

    private void SaveExistingLevel()
    {
        if (string.IsNullOrEmpty(selectedLevelPath))
            return;

        LevelData levelData = AssetDatabase.LoadAssetAtPath<LevelData>(selectedLevelPath);
        if (levelData != null)
        {
            Undo.RecordObject(levelData, "Update Level Data");

            levelData.gridWidth = gridWidth;
            levelData.gridHeight = gridHeight;
            levelData.dotPairs = dotPairs.ToArray();

            EditorUtility.SetDirty(levelData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Level saved successfully: {selectedLevelPath}");
        }
    }

    private void ExportLevel()
    {
        // Находим следующий доступный номер уровня
        int nextLevelNumber = 1;
        while (AssetDatabase.LoadAssetAtPath<LevelData>($"Assets/Levels/Level{nextLevelNumber}Data.asset") != null)
        {
            nextLevelNumber++;
        }

        // Создаём новый LevelData
        var levelData = ScriptableObject.CreateInstance<LevelData>();
        levelData.gridWidth = gridWidth;
        levelData.gridHeight = gridHeight;
        levelData.dotPairs = dotPairs.ToArray();

        // Убеждаемся, что директория существует
        if (!AssetDatabase.IsValidFolder("Assets/Editor/Levels"))
        {
            AssetDatabase.CreateFolder("Editor", "Levels");
        }

        // Сохраняем asset файл
        string assetPath = $"Assets/Editor/Levels/Level{nextLevelNumber}Data.asset";
        AssetDatabase.CreateAsset(levelData, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Level exported successfully: {assetPath}");
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = levelData;
    }

    private void DrawTestModeToggle()
    {
        bool newTestMode = EditorGUILayout.Toggle("Test Mode", isTestMode);
        if (newTestMode != isTestMode)
        {
            isTestMode = newTestMode;
            if (isTestMode)
            {
                ResetTestMode();
            }
        }
    }

    private void ResetTestMode()
    {
        colorToPaths.Clear();
        occupiedCells.Clear();
        activeStartDot = null;
        currentPath.Clear();
        lastGridPosition = null;

        // Добавляем точки в список занятых ячеек
        foreach (var pair in dotPairs)
        {
            occupiedCells.Add(pair.dot1Position);
            occupiedCells.Add(pair.dot2Position);
        }
    }

    private void DrawColorSelector()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Color:");
        EditorGUILayout.BeginHorizontal();

        for (int i = 0; i < availableColors.Length; i++)
        {
            bool isColorUsed = IsColorUsed(i);
            GUI.enabled = !isColorUsed || selectedColorIndex == i;
            GUI.backgroundColor = availableColors[i];

            if (GUILayout.Button(selectedColorIndex == i ? "✓" : "", GUILayout.Width(30), GUILayout.Height(30)))
            {
                selectedColorIndex = i;
                firstDot = null;
            }
        }

        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    private bool IsColorUsed(int colorIndex)
    {
        return dotPairs.Exists(pair => pair.colorIndex == colorIndex);
    }

    private void DrawGrid()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        float totalWidth = gridWidth * cellSize;
        float totalHeight = gridHeight * cellSize;
        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        // Рисуем линии сетки
        Handles.color = Color.gray;
        for (int x = 0; x <= gridWidth; x++)
        {
            float xPos = gridRect.x + x * cellSize;
            Handles.DrawLine(
                new Vector3(xPos, gridRect.y, 0),
                new Vector3(xPos, gridRect.y + totalHeight, 0)
            );
        }
        for (int y = 0; y <= gridHeight; y++)
        {
            float yPos = gridRect.y + y * cellSize;
            Handles.DrawLine(
                new Vector3(gridRect.x, yPos, 0),
                new Vector3(gridRect.x + totalWidth, yPos, 0)
            );
        }

        // Рисуем завершенные линии
        if (isTestMode)
        {
            foreach (var path in colorToPaths)
            {
                DrawTestLine(path.Key, path.Value, gridRect);
            }

            // Рисуем текущую линию
            if (currentPath.Count > 0 && activeStartDot.HasValue)
            {
                Color pathColor = availableColors[GetDotColor(activeStartDot.Value)];
                DrawTestLine(pathColor, currentPath, gridRect);
            }
        }

        // Рисуем точки и обрабатываем ввод
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int currentPos = new Vector2Int(x, y);
                Rect cellRect = new Rect(
                    gridRect.x + x * cellSize,
                    gridRect.y + y * cellSize,
                    cellSize,
                    cellSize
                );

                if (!isTestMode && firstDot.HasValue && firstDot.Value == currentPos)
                {
                    DrawDot(cellRect, availableColors[selectedColorIndex]);
                }

                DotPair? dotPair = FindDotPairAtPosition(currentPos);
                if (dotPair.HasValue)
                {
                    DrawDot(cellRect, availableColors[dotPair.Value.colorIndex]);
                }

                if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                {
                    if (isTestMode)
                    {
                        HandleTestModeClick(currentPos);
                    }
                    else
                    {
                        HandleGridClick(currentPos);
                    }
                    Event.current.Use();
                    Repaint();
                }
                else if (isTestMode && Event.current.type == EventType.MouseDrag &&
                         cellRect.Contains(Event.current.mousePosition))
                {
                    HandleTestModeDrag(currentPos);
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleTestModeClick(Vector2Int clickedPos)
    {
        DotPair? clickedDotPair = FindDotPairAtPosition(clickedPos);
        if (!clickedDotPair.HasValue) return;

        int dotColor = clickedDotPair.Value.colorIndex;
        Color lineColor = availableColors[dotColor];

        // Если нажали на стартовую точку текущей линии - отменяем рисование
        if (activeStartDot.HasValue && clickedPos == activeStartDot.Value)
        {
            CancelCurrentLine();
            return;
        }

        // Если уже есть активная точка
        if (activeStartDot.HasValue)
        {
            // Проверяем, что точка того же цвета
            if (GetDotColor(activeStartDot.Value) == dotColor)
            {
                // Если это конечная точка - завершаем линию
                if (clickedPos != activeStartDot.Value)
                {
                    CompleteCurrentLine(clickedPos, lineColor);
                }
            }
            else
            {
                // Если другой цвет - начинаем новую линию
                CancelCurrentLine();
                StartNewLine(clickedPos);
            }
        }
        else
        {
            // Если существует линия этого цвета - удаляем её
            if (colorToPaths.ContainsKey(lineColor))
            {
                RemoveLine(lineColor);
            }
            StartNewLine(clickedPos);
        }
    }

    private void HandleTestModeDrag(Vector2Int currentPos)
    {
        if (!activeStartDot.HasValue || !lastGridPosition.HasValue) return;

        // Проверяем, что движемся на одну клетку
        Vector2Int delta = currentPos - lastGridPosition.Value;
        if (Mathf.Abs(delta.x) + Mathf.Abs(delta.y) != 1) return;

        // Проверяем возможность отката на шаг назад
        if (currentPath.Count > 1 && currentPos == currentPath[currentPath.Count - 2])
        {
            currentPath.RemoveAt(currentPath.Count - 1);
            lastGridPosition = currentPos;
            return;
        }

        // Проверяем, что не пересекаем существующую линию
        if (currentPath.Contains(currentPos)) return;

        // Проверяем занятость клетки
        if (occupiedCells.Contains(currentPos))
        {
            // Проверяем, не является ли это конечной точкой нужного цвета
            DotPair? dotPair = FindDotPairAtPosition(currentPos);
            if (dotPair.HasValue && dotPair.Value.colorIndex == GetDotColor(activeStartDot.Value))
            {
                CompleteCurrentLine(currentPos, availableColors[dotPair.Value.colorIndex]);
            }
            return;
        }

        // Добавляем новую точку к пути
        currentPath.Add(currentPos);
        lastGridPosition = currentPos;
    }

    private void StartNewLine(Vector2Int startPos)
    {
        activeStartDot = startPos;
        lastGridPosition = startPos;
        currentPath.Clear();
        currentPath.Add(startPos);
    }

    private void CompleteCurrentLine(Vector2Int endPos, Color lineColor)
    {
        List<Vector2Int> completePath = new List<Vector2Int>(currentPath);
        completePath.Add(endPos);

        // Добавляем путь в словарь
        colorToPaths[lineColor] = completePath;

        // Обновляем занятые клетки
        foreach (var pos in completePath)
        {
            if (!IsPositionDot(pos))
            {
                occupiedCells.Add(pos);
            }
        }

        // Сбрасываем текущие переменные
        activeStartDot = null;
        currentPath.Clear();
        lastGridPosition = null;
    }

    private void CancelCurrentLine()
    {
        activeStartDot = null;
        currentPath.Clear();
        lastGridPosition = null;
    }

    private void RemoveLine(Color color)
    {
        if (colorToPaths.TryGetValue(color, out List<Vector2Int> path))
        {
            foreach (var pos in path)
            {
                if (!IsPositionDot(pos))
                {
                    occupiedCells.Remove(pos);
                }
            }
            colorToPaths.Remove(color);
        }
    }

    private bool IsPositionDot(Vector2Int pos)
    {
        return FindDotPairAtPosition(pos).HasValue;
    }

    private void DrawTestLine(Color color, List<Vector2Int> points, Rect gridRect)
    {
        if (points.Count < 2) return;

        Handles.color = color;
        Vector3[] linePoints = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            linePoints[i] = new Vector3(
                gridRect.x + points[i].x * cellSize + cellSize / 2,
                gridRect.y + points[i].y * cellSize + cellSize / 2,
                0
            );
        }

        Handles.DrawAAPolyLine(3f, linePoints);
    }

    private void DrawDot(Rect position, Color color)
    {
        float dotSize = cellSize * 0.8f;
        float halfSize = dotSize / 2;
        Vector2 center = new Vector2(
            position.x + position.width / 2,
            position.y + position.height / 2
        );

        Color oldColor = Handles.color;
        Handles.color = color;

        Vector3[] points = new Vector3[60];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = (i / (float)points.Length) * 2 * Mathf.PI;
            points[i] = new Vector3(
                center.x + Mathf.Cos(angle) * halfSize,
                center.y + Mathf.Sin(angle) * halfSize,
                0
            );
        }
        Handles.DrawAAConvexPolygon(points);

        Handles.color = oldColor;
    }

    private void HandleGridClick(Vector2Int clickedPosition)
    {
        if (FindDotPairAtPosition(clickedPosition).HasValue)
        {
            RemoveDotAtPosition(clickedPosition);
            firstDot = null;
            return;
        }

        if (!firstDot.HasValue)
        {
            if (!IsColorUsed(selectedColorIndex))
            {
                firstDot = clickedPosition;
            }
        }
        else if (clickedPosition != firstDot.Value)
        {
            dotPairs.Add(new DotPair
            {
                dot1Position = firstDot.Value,
                dot2Position = clickedPosition,
                colorIndex = selectedColorIndex
            });
            firstDot = null;
        }
    }

    private DotPair? FindDotPairAtPosition(Vector2Int position)
    {
        foreach (var pair in dotPairs)
        {
            if (pair.dot1Position == position || pair.dot2Position == position)
            {
                return pair;
            }
        }
        return null;
    }

    private void RemoveDotAtPosition(Vector2Int position)
    {
        dotPairs.RemoveAll(pair =>
            pair.dot1Position == position || pair.dot2Position == position);
    }

    private int GetDotColor(Vector2Int position)
    {
        var dotPair = FindDotPairAtPosition(position);
        return dotPair?.colorIndex ?? -1;
    }

    private void ResetLevel()
    {
        dotPairs.Clear();
        firstDot = null;
        colorToPaths.Clear();
        occupiedCells.Clear();
        activeStartDot = null;
        currentPath.Clear();
        lastGridPosition = null;
        currentLevel = ScriptableObject.CreateInstance<LevelData>();
        currentLevel.gridWidth = gridWidth;
        currentLevel.gridHeight = gridHeight;
    }
}