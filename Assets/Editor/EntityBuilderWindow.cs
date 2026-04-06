using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EntityBuilderWindow : EditorWindow
{
    [MenuItem("Tools/Entity Builder (Pixel Art)")]
    public static void ShowWindow() => GetWindow<EntityBuilderWindow>("Entity Builder");

    private GameObject pixelPrefab;
    private int gridWidth = 20;
    private int gridHeight = 20;

    private struct PixelData
    {
        public bool placed;
        public Color color;
    }
    private PixelData[,] grid;

    private Color currentColor = Color.white;
    private GUIStyle pixelStyle;

    private void OnEnable()
    {
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        grid = new PixelData[gridWidth, gridHeight];
        // Mặc định để một vài pixel ví dụ (có thể xóa)
        // Ví dụ: grid[10, 10].placed = true; grid[10, 10].color = Color.red;
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();

        // ====================== LEFT PANEL (Settings) ======================
        GUILayout.BeginVertical(GUILayout.Width(280));
        GUILayout.Label("ENTITY BUILDER TOOL", EditorStyles.boldLabel);

        pixelPrefab = (GameObject)EditorGUILayout.ObjectField("Pixel Prefab", pixelPrefab, typeof(GameObject), false);

        if (pixelPrefab == null)
            EditorGUILayout.HelpBox("Chưa gán PixelCube Prefab!", MessageType.Warning);

        currentColor = EditorGUILayout.ColorField("Màu vẽ hiện tại", currentColor);

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh Grid (20x20)", GUILayout.Height(30)))
            RefreshGrid();

        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All?", "Delete all pixels ?", "Yes", "Cancel"))
                RefreshGrid();
        }

        GUILayout.Space(20);
        GUILayout.Label("Hướng dẫn:", EditorStyles.boldLabel);
        GUILayout.Label("• Click ô = vẽ màu hiện tại\n• Click lại ô cùng màu = xóa", EditorStyles.wordWrappedLabel);

        GUILayout.EndVertical();

        // ====================== RIGHT PANEL (Pixel Grid) ======================
        GUILayout.BeginVertical();
        GUILayout.Label($"GRID {gridWidth} × {gridHeight}  (Click để vẽ)", EditorStyles.boldLabel);

        if (grid == null)
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            return;
        }

        // Vẽ grid từ trên xuống (y = height-1 → 0)
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridWidth; x++)
            {
                PixelData p = grid[x, y];

                // === SỬA Ở ĐÂY ===
                Color displayColor = p.placed
                    ? p.color               // Tăng sáng mạnh
                    : new Color(0.25f, 0.25f, 0.25f, 1f);   // Nền tối hơn một chút cho dễ nhìn

                GUI.backgroundColor = displayColor;

                InitPixelStyle();

                if (GUILayout.Button("", pixelStyle, GUILayout.Width(24), GUILayout.Height(24)))
                {
                    if (p.placed &&
                        Mathf.Approximately(p.color.r, currentColor.r) &&
                        Mathf.Approximately(p.color.g, currentColor.g) &&
                        Mathf.Approximately(p.color.b, currentColor.b))
                    {
                        grid[x, y].placed = false;   // Xóa
                    }
                    else
                    {
                        grid[x, y].placed = true;
                        grid[x, y].color = currentColor;
                    }
                }

                GUI.backgroundColor = Color.white;   // Reset
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // ====================== CREATE BUTTON ======================
        GUILayout.Space(10);
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("CREATE ENTITY", GUILayout.Height(50)))
        {
            if (pixelPrefab == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Chưa gán Pixel Prefab!", "OK");
                return;
            }
            CreateEntity();
        }
        GUI.backgroundColor = Color.white;
    }
    private void InitPixelStyle()
    {
        if (pixelStyle == null)
        {
            pixelStyle = new GUIStyle(GUI.skin.button);
            pixelStyle.normal.background = Texture2D.whiteTexture;   // Nền trắng tinh
            pixelStyle.hover.background = Texture2D.whiteTexture;
            pixelStyle.active.background = Texture2D.whiteTexture;
            pixelStyle.focused.background = Texture2D.whiteTexture;

            // Loại bỏ border để ô vuông đẹp hơn
            pixelStyle.border = new RectOffset(0, 0, 0, 0);

            // Tùy chọn: bo góc nhẹ nếu muốn
            // pixelStyle.border = new RectOffset(2, 2, 2, 2);
        }
    }
    private void CreateEntity()
    {
        GameObject entityGO = new GameObject("New_Entity");
        Enity enity = entityGO.AddComponent<Enity>();

        int placedCount = 0;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].placed) continue;

                // Tạo cube
                GameObject cube = (GameObject)PrefabUtility.InstantiatePrefab(pixelPrefab);
                cube.transform.SetParent(entityGO.transform, false);

                // Vị trí (để Enity tự tính min sau)
                cube.transform.localPosition = new Vector3(x, y, Random.Range(-0.15f, 0.15f));

                // Set màu
                ColorCube colorComp = cube.GetComponent<ColorCube>();
                if (colorComp != null)
                    colorComp.CubeColor = grid[x, y].color;   // Dùng setter vừa thêm

                placedCount++;
            }
        }

        if (placedCount == 0)
        {
            DestroyImmediate(entityGO);
            EditorUtility.DisplayDialog("Rỗng", "Bạn chưa vẽ pixel nào!", "OK");
            return;
        }

        // Khởi tạo Enity ngay trong Editor
        enity.CollectCubes();           // Quan trọng!
        // enity.RecalculateCubes();    // Không cần vì mới tạo là 1 khối liền

        // Select và ping trong Hierarchy
        Selection.activeGameObject = entityGO;
        EditorGUIUtility.PingObject(entityGO);

        Debug.Log($"✅ Tạo thành công Entity với {placedCount} pixel!");
    }
}

