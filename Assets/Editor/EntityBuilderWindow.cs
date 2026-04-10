using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;

public class EntityBuilderWindow : EditorWindow
{
    [MenuItem("Tools/Entity Builder (Pixel Art)")]
    public static void ShowWindow() => GetWindow<EntityBuilderWindow>("Entity Builder");

    private GameObject pixelPrefab;
    private int gridWidth = 20; //chiều rộng
    private int gridHeight = 20; //chiều cao
    //Lí do: 20x20, vì game ban đầu thiết kế với 1 Box 1x1x1, khoảng cách giữa 2 thành màn hình khoảng 24 box cố định ở mọi level, nên phải có giới hạn về kích thước Enity

    private struct PixelData
    {
        public bool placed; //Check vị trí
        public Color color; //Check màu
        public float zOffset; //Check trục Z
    }
    private PixelData[,] grid;
    private Color currentColor = Color.white;
    private GUIStyle pixelStyle;
    private EntityData loadedEntityData;
    private EntityDatabaseSO targetDatabase;

    private void OnEnable()
    {
        RefreshGrid();
        // Tự load prefab lần cuối cùng
        string prefabPath = EditorPrefs.GetString("EntityBuilder_PixelPrefabPath", "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            pixelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // Tự load Database lần cuối
        string dbPath = EditorPrefs.GetString("EntityBuilder_DatabasePath", "");
        if (!string.IsNullOrEmpty(dbPath))
        {
            targetDatabase = AssetDatabase.LoadAssetAtPath<EntityDatabaseSO>(dbPath);
        }
    }
    private void OnDisable()
    {
        // Lưu lại khi đóng tool
        if (pixelPrefab != null)
            EditorPrefs.SetString("EntityBuilder_PixelPrefabPath", AssetDatabase.GetAssetPath(pixelPrefab));
        if (targetDatabase != null)
            EditorPrefs.SetString("EntityBuilder_DatabasePath", AssetDatabase.GetAssetPath(targetDatabase));
    }

    private void RefreshGrid()
    {
        grid = new PixelData[gridWidth, gridHeight];
        // Mặc định zOffset = 0 (sẽ set lại khi vẽ mới hoặc load)
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        #region LeftPanel
        // ====================== LEFT PANEL (Settings) ======================
        GUILayout.BeginVertical(GUILayout.Width(280));
        GUILayout.Label("ENTITY BUILDER TOOL", EditorStyles.boldLabel);

        pixelPrefab = (GameObject)EditorGUILayout.ObjectField("Pixel Prefab", pixelPrefab, typeof(GameObject), false);

        if (pixelPrefab == null)
            EditorGUILayout.HelpBox("PixelCube Prefab not found!", MessageType.Warning);

        currentColor = EditorGUILayout.ColorField("Current paint color", currentColor);

        GUILayout.Space(10);

        // ====================== LOAD & EDIT ======================
        EditorGUILayout.LabelField("Edit Existing Asset", EditorStyles.boldLabel);
        loadedEntityData = (EntityData)EditorGUILayout.ObjectField("EntityData", loadedEntityData, typeof(EntityData), false);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load EnityData to Grid", GUILayout.Height(25)))
        {
            if (loadedEntityData != null)
                LoadEntityData(loadedEntityData);
            else
                EditorUtility.DisplayDialog("Error", "No EntityData selected yet!", "OK");
        }
        if (GUILayout.Button("Clear Grid", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Clear All?", "Delete all pixels in the grid?", "Yes", "Cancel"))
                RefreshGrid();
        }
        GUILayout.EndHorizontal();

        // === DATABASE ===
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Target Database", EditorStyles.boldLabel);
        targetDatabase = (EntityDatabaseSO)EditorGUILayout.ObjectField("EntityDatabase", targetDatabase, typeof(EntityDatabaseSO), false);
        if (targetDatabase == null)
            EditorGUILayout.HelpBox("EnityDatabase not found!", MessageType.Warning);

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh Grid (20x20)", GUILayout.Height(30)))
            RefreshGrid();

        GUILayout.Space(20);
        GUILayout.Label("Tutorial:", EditorStyles.boldLabel);
        GUILayout.Label("• Click cell = draw\n• Same color = delete\n• New pixel = random Z\n• Old pixel = keep Z", EditorStyles.wordWrappedLabel);

        GUILayout.EndVertical();
        #endregion

        #region RightPanel
        // ====================== RIGHT PANEL (Pixel Grid) ======================
        GUILayout.BeginVertical();
        GUILayout.Label($"Grid matrix interface {gridWidth} × {gridHeight}  (Click to draw)", EditorStyles.boldLabel);

        if (grid == null)
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            return;
        }

        // Hiển thị grid (y = height-1 → 0)
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < gridWidth; x++)
            {
                PixelData p = grid[x, y];

                Color displayColor = p.placed
                    ? p.color
                    : new Color(0.25f, 0.25f, 0.25f, 1f);

                GUI.backgroundColor = displayColor;
                InitPixelStyle();

                if (GUILayout.Button("", pixelStyle, GUILayout.Width(24), GUILayout.Height(24)))
                {
                    HandlePixelClick(x, y, p);
                }

                GUI.backgroundColor = Color.white;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        #endregion
        // ====================== Save Button ======================
        GUILayout.Space(10);
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button(loadedEntityData != null ? "SAVE ENTITY" : "CREATE NEW ENTITY", GUILayout.Height(50)))
        {
            if (pixelPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Prefab Not Found!", "OK");
                return;
            }

            if (loadedEntityData != null)
                SaveToExistingEntity(loadedEntityData);
            else
                CreateNewEntity();
        }
        GUI.backgroundColor = Color.white;
    }
    #region HandlePixelClick
    private void HandlePixelClick(int x, int y, PixelData p)
    {
        bool sameColor = p.placed &&
            Mathf.Approximately(p.color.r, currentColor.r) &&
            Mathf.Approximately(p.color.g, currentColor.g) &&
            Mathf.Approximately(p.color.b, currentColor.b);

        if (p.placed && sameColor)
        {
            // Xóa pixel
            grid[x, y].placed = false;
        }
        else
        {
            // Vẽ / đổi màu
            grid[x, y].placed = true;
            grid[x, y].color = currentColor;

            // Chỉ random Z khi là pixel MỚI
            if (!p.placed)
                grid[x, y].zOffset = Random.Range(-0.15f, 0.15f);
            // Nếu đã có pixel cũ → giữ nguyên zOffset
        }
    }
    #endregion
    #region LoadData
    private void LoadEntityData(EntityData data)
    {
        RefreshGrid();

        foreach (var pixel in data.pixels)
        {
            int gx = Mathf.RoundToInt(pixel.localPosition.x);
            int gy = Mathf.RoundToInt(pixel.localPosition.y);

            if (gx >= 0 && gx < gridWidth && gy >= 0 && gy < gridHeight)
            {
                grid[gx, gy].placed = true;
                grid[gx, gy].color = pixel.color;
                grid[gx, gy].zOffset = pixel.localPosition.z;   // Giữ nguyên Z cũ
            }
        }

        Debug.Log($"Loading EntityData: {data.name} ({data.pixels.Count} pixels)");
    }
    #endregion
    #region SaveData
    private void SaveToExistingEntity(EntityData data)
    {
        data.pixels.Clear();

        int placedCount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].placed) continue;

                Vector3 pos = new Vector3(x, y, grid[x, y].zOffset);
                data.pixels.Add(new PixelInfo(pos, grid[x, y].color));
                placedCount++;
            }
        }

        data.gridSize = new Vector2Int(gridWidth, gridHeight);

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        TryAddToDatabase(data);

        Debug.Log($"EntityData Update: {data.name} ({placedCount} pixels)");
        EditorGUIUtility.PingObject(data);
    }
    #endregion
    #region CreateData
    private void CreateNewEntity()
    {
        EntityData entityData = ScriptableObject.CreateInstance<EntityData>();
        entityData.gridSize = new Vector2Int(gridWidth, gridHeight);

        int placedCount = 0;
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].placed) continue;

                Vector3 pos = new Vector3(x, y, grid[x, y].zOffset);
                entityData.pixels.Add(new PixelInfo(pos, grid[x, y].color));
                placedCount++;
            }
        }

        if (placedCount == 0)
        {
            EditorUtility.DisplayDialog("Null pixel", "You haven't drawn any pixels yet!", "OK");
            return;
        }

        // Cho phép người dùng đặt tên asset (tên này sẽ là entity name sau này)
        string defaultName = "New_Entity_" + System.DateTime.Now.ToString("yyyyMMdd_HHmm");
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Entity Data",
            defaultName,
            "asset",
            "Save Entity");

        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(entityData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TryAddToDatabase(entityData);

            Selection.activeObject = entityData;
            EditorGUIUtility.PingObject(entityData);

            Debug.Log($"✅ Tạo mới EntityData thành công ({placedCount} pixels)");
        }
    }
    #endregion
    private void TryAddToDatabase(EntityData data)
    {
        if (targetDatabase == null) return;

        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));

        foreach (var r in targetDatabase.entityReferences)
            if (r != null && r.AssetGUID == guid) return;

        var newRef = new AssetReferenceEntityData(guid);
        targetDatabase.entityReferences.Add(newRef);

        EditorUtility.SetDirty(targetDatabase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Added to Database: {data.name}");
    }
    private void InitPixelStyle()
    {
        if (pixelStyle == null)
        {
            pixelStyle = new GUIStyle(GUI.skin.button);
            pixelStyle.normal.background = Texture2D.whiteTexture;
            pixelStyle.hover.background = Texture2D.whiteTexture;
            pixelStyle.active.background = Texture2D.whiteTexture;
            pixelStyle.focused.background = Texture2D.whiteTexture;
            pixelStyle.border = new RectOffset(0, 0, 0, 0);
        }
    }
}