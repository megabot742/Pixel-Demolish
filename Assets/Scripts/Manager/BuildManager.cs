using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildManager : BaseManager<BuildManager>
{
    [Header("Build Settings")]
    [SerializeField] private GameObject sawPrefab;           // Saw Prefab
    [SerializeField] private int minCoin = 100;              // Base Coint Price
    [SerializeField] private int nextCoin = 50;              // Increases each subsequent build

    [Header("UI Build Prompt")]
    [SerializeField] private GameObject buildPromptPrefab;

    private int currentBuildCost;
    public int CurrentBuildCost => currentBuildCost;
    private List<PointBuild> pointBuilds = new List<PointBuild>();

    #region Singleton
    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    private void Start()
    {
        currentBuildCost = minCoin;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only processed when entering Levels
        if (scene.name == "Level 1" || scene.name == "Level 2" ||
            scene.name == "Level 3" || scene.name == "Level 4" || scene.name == "Level 5")
        {
            //Debug.Log($"[BuildManager] Scene {scene.name} loaded → Re-initialize PointBuilds");

            currentBuildCost = minCoin;           //Reset the build price per level (you can remove this line if you want the price to increase forever)
            FindAllPointBuilds();
            SpawnInitialSaws();
            SpawnBuildPromptsForEmptyPoints();
        }
    }
    #region Handle
    private void FindAllPointBuilds()
    {
        pointBuilds.Clear();
        var found = FindObjectsByType<PointBuild>(FindObjectsSortMode.None);
        pointBuilds.AddRange(found);
        //Debug.Log($"[BuildManager] Found {pointBuilds.Count} PointBuild in scene.");
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
        ui.transform.localPosition = new Vector3(0, 0, 0);   // tranfrom spawn position 
        ui.transform.localRotation = Quaternion.identity;

        var promptScript = ui.GetComponent<BuildPromptUI>();
        if (promptScript != null)
        {
            promptScript.Setup(point);
        }

        point.SetBuildUI(ui);   // Attach UI to PointBuild to hide later
        //Debug.Log($"[BuildManager] Show build button at {point.name}");
    }
    private void SpawnSawAt(PointBuild point, bool isFree = false)
    {
        if (point.HasSaw)
        {
            //Debug.LogWarning($"[BuildManager] Point {point.name} have built");
            return;
        }

        if (!isFree)
        {
            //Not enough coins
            if (!CoinManager.HasInstance || CoinManager.Instance.CurrentCoins < currentBuildCost)
            {

                string notEnoughMsg = $"Not enough coins!\nNeed {currentBuildCost}, have {CoinManager.Instance.CurrentCoins}";

                if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
                {
                    UIManager.Instance.hUDPanel.ShowNotification(notEnoughMsg, Color.magenta, 1);
                }
                else
                {
                    Debug.LogWarning(notEnoughMsg);
                }
                return;
            }

            CoinManager.Instance.SubtractCoin(currentBuildCost);

            //Build Saw successfully
            string successMsg = $"Build Saw successfully!\nNext cost: {currentBuildCost + nextCoin}";

            if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
            {
                UIManager.Instance.hUDPanel.ShowNotification(successMsg, Color.green, 1.8f);
            }
            else
            {
                Debug.Log(successMsg);
            }
            currentBuildCost += nextCoin;
        }
        else
        {
            //Debug.Log($"[BuildManager] Spawn initial free Saw tại {point.name}");
        }

        if (sawPrefab == null)
        {
            //Debug.LogError("[BuildManager] Saw Prefab has not been assigned yet!");
            return;
        }

        // Spawn saw child in PointBuild
        GameObject newSaw = Instantiate(sawPrefab, point.transform);
        newSaw.transform.localPosition = Vector3.zero;
        newSaw.transform.localRotation = Quaternion.identity;

        point.MarkAsBuilt();
        point.HideBuildUI();
        Debug.Log($"[BuildManager] Saw spawned at {point.name}");
    }

    public void BuildSaw(PointBuild point)
    {
        if (point == null) return;
        SpawnSawAt(point, false);
    }
    #endregion
    #region ContextMenu
    [ContextMenu("Test Spawn All Initial (Free)")]
    private void TestSpawnInitial() => SpawnInitialSaws();
    #endregion
}
