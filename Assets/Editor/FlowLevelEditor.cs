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
            new Color(1f, 0f, 0f),      // Red
            new Color(0f, 0f, 1f),      // Blue
            new Color(0f, 1f, 0f),      // Green
            new Color(1f, 1f, 0f),      // Yellow
            new Color(1f, 0f, 1f),      // Purple
        };

        currentLevel = ScriptableObject.CreateInstance<LevelData>();
        currentLevel.gridWidth = gridWidth;
        currentLevel.gridHeight = gridHeight;
    }

    private void OnGUI()
    {
        DrawSettings();
        DrawColorSelector();
        DrawGrid();
    }

    private void DrawSettings()
    {
        EditorGUILayout.BeginHorizontal();
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth, GUILayout.Width(200));
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight, GUILayout.Width(200));
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create New Level"))
        {
            ResetLevel();
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

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Rect cellRect = new Rect(
                    gridRect.x + x * cellSize,
                    gridRect.y + y * cellSize,
                    cellSize,
                    cellSize
                );

                Vector2Int currentPos = new Vector2Int(x, y);

                // Рисуем первую точку
                if (firstDot.HasValue && firstDot.Value == currentPos)
                {
                    DrawDot(cellRect, availableColors[selectedColorIndex]);
                }

                // Рисуем существующие пары точек
                DotPair? dotPair = FindDotPairAtPosition(currentPos);
                if (dotPair.HasValue)
                {
                    DrawDot(cellRect, availableColors[dotPair.Value.colorIndex]);
                }

                if (Event.current.type == EventType.MouseDown &&
                    cellRect.Contains(Event.current.mousePosition))
                {
                    HandleGridClick(currentPos);
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        EditorGUILayout.EndScrollView();
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

        // Рисуем заполненный круг
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

    private void ResetLevel()
    {
        dotPairs.Clear();
        firstDot = null;
        currentLevel = ScriptableObject.CreateInstance<LevelData>();
        currentLevel.gridWidth = gridWidth;
        currentLevel.gridHeight = gridHeight;
    }
}