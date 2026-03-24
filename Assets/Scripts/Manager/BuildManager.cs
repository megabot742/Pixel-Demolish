using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildManager : BaseManager<BuildManager>
{
    [Header("Build Settings")]
    [SerializeField] private GameObject sawPrefab;           // Kéo prefab Saw vào đây
    [SerializeField] private int minCoin = 100;              // Giá tiền cơ bản (Saw đầu tiên)
    [SerializeField] private int nextCoin = 50;              // Tăng thêm mỗi lần build tiếp theo

    [Header("UI Build Prompt")]
    [SerializeField] private GameObject buildPromptPrefab;
    
    private int currentBuildCost;
    public int CurrentBuildCost => currentBuildCost;
    private List<PointBuild> pointBuilds = new List<PointBuild>();

    #region Singleton
    protected override void Awake()
    {
        base.Awake();
    }
    #endregion

    private void Start()
    {
        FindAllPointBuilds();
        currentBuildCost = minCoin;
        SpawnInitialSaws();
        SpawnBuildPromptsForEmptyPoints();
    }

    private void FindAllPointBuilds()
    {
        pointBuilds.Clear();
        var found = FindObjectsByType<PointBuild>(FindObjectsSortMode.None);
        pointBuilds.AddRange(found);
        Debug.Log($"[BuildManager] Found {pointBuilds.Count} PointBuild in scene.");
    }

    private void SpawnInitialSaws()
    {
        foreach (var point in pointBuilds)
        {
            if (point.HasStartBuild && !point.HasSaw)
            {
                SpawnSawAt(point, isFree: true);
            }
        }
    }
    private void SpawnBuildPromptsForEmptyPoints()
    {
        foreach (var point in pointBuilds)
        {
            if (!point.HasSaw)
            {
                SpawnBuildPromptAt(point);
            }
        }
    }

    private void SpawnBuildPromptAt(PointBuild point)
    {
        if (buildPromptPrefab == null) return;

        GameObject ui = Instantiate(buildPromptPrefab, point.transform);
        ui.transform.localPosition = new Vector3(0, 0, 0);   // ← Chỉnh chiều cao nút nổi lên, or 1.8f
        ui.transform.localRotation = Quaternion.identity;

        var promptScript = ui.GetComponent<BuildPromptUI>();
        if (promptScript != null)
        {
            promptScript.Setup(point);
        }

        point.SetBuildUI(ui);   // Gắn UI vào PointBuild để sau này ẩn
        Debug.Log($"[BuildManager] Show build button at {point.name}");
    }
    private void SpawnSawAt(PointBuild point, bool isFree = false)
    {
        if (point.HasSaw)
        {
            Debug.LogWarning($"[BuildManager] Point {point.name} have built");
            return;
        }

        if (!isFree)
        {
            if (!CoinManager.HasInstance || CoinManager.Instance.CurrentCoins < currentBuildCost)
            {
                Debug.LogWarning($"[BuildManager] Not enough coins! Need {currentBuildCost}, currently available {CoinManager.Instance.CurrentCoins}");
                return;
            }

            CoinManager.Instance.SubtractCoin(currentBuildCost);
            Debug.Log($"[BuildManager] Build Saw successfully at {point.name}. Cost: {currentBuildCost} | Next cost: {currentBuildCost + nextCoin}");
            currentBuildCost += nextCoin;
        }
        else
        {
            Debug.Log($"[BuildManager] Spawn initial free Saw tại {point.name}");
        }

        if (sawPrefab == null)
        {
            Debug.LogError("[BuildManager] Saw Prefab has not been assigned yet!");
            return;
        }

        // Spawn làm con của PointBuild
        GameObject newSaw = Instantiate(sawPrefab, point.transform);
        newSaw.transform.localPosition = Vector3.zero;
        newSaw.transform.localRotation = Quaternion.identity;

        point.MarkAsBuilt();
        point.HideBuildUI();
        Debug.Log($"[BuildManager] Đã spawn Saw tại {point.name}");
    }

    /// <summary>
    /// Gọi từ UI / script khác để build Saw (có trừ coin)
    /// </summary>
    public void BuildSaw(PointBuild point)
    {
        if (point == null) return;
        SpawnSawAt(point, false);
    }

    #region ContextMenu
    [ContextMenu("Test Spawn All Initial (Free)")]
    private void TestSpawnInitial() => SpawnInitialSaws();
    #endregion
}
