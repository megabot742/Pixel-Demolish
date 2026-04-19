using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildManager : BaseManager<BuildManager>
{
    [Header("Build Settings")]
    [SerializeField] private GameObject sawPrefab; //Prefab Saw
    [SerializeField] private int minBuildCost = 100;    //Giá tiền cơ bản
    [SerializeField] private int costIncreasePerBuild = 50;    //Giá tiền tăng sau mỗi lần build
    [Header("UI Build Prompt")]
    [SerializeField] private GameObject buildPromptPrefab;

    private int currentBuildCost;
    private List<PointBuild> pointBuilds = new List<PointBuild>();

    public int GetCurrentBuildCost => currentBuildCost;
    public event Action OnBuildCostChanged;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        ResetBuildCost();
    }
    private void ResetBuildCost()
    {
        currentBuildCost = minBuildCost;
        OnBuildCostChanged?.Invoke();
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check Scene
        if (IsLevelScene(scene.name))
        {
            ResetBuildCost(); //Reset giá build mỗi màn chơi
            FindAllPointBuilds();
            SpawnInitialSaws();
            SpawnBuildPromptsForEmptyPoints();
            //Debug.Log($"[BuildManager] Scene {scene.name} loaded → Re-initialize PointBuilds");
        }
    }
    private bool IsLevelScene(string sceneName) => sceneName.StartsWith("Level ");
    private void FindAllPointBuilds() //Tìm các điểm point build
    {
        pointBuilds.Clear();
        var found = FindObjectsByType<PointBuild>(FindObjectsSortMode.None);
        pointBuilds.AddRange(found);
        //Debug.Log($"[BuildManager] Found {pointBuilds.Count} PointBuild in scene.");
    }
    public void BuildSaw(PointBuild point) //Xử lí build saw
    {
        if (point == null || point.GetHasSaw) return;
        SpawnSawAt(point, isFree: false);
    }
    private void SpawnInitialSaws() //Xử lí build Saw nếu có check khởi đầu
    {
        foreach (var point in pointBuilds)
        {
            if (point.GetHasStartBuild && !point.GetHasSaw)
            {
                SpawnSawAt(point, isFree: true);
            }
        }
    }
    private void SpawnBuildPromptsForEmptyPoints()  //Xử lí kiểm tra Point Build trống
    {
        foreach (var point in pointBuilds)
        {
            if (!point.GetHasSaw)
            {
                SpawnBuildPromptAt(point);
            }
        }
    }

    private void SpawnBuildPromptAt(PointBuild point) //Xử lí sinh ra UI buildPromptPrefab ở vị trí Point Build
    {
        if (buildPromptPrefab == null) return;

        GameObject ui = Instantiate(buildPromptPrefab, point.transform);
        ui.transform.localPosition = new Vector3(0, 0, -1);
        ui.transform.localRotation = Quaternion.identity;

        var promptScript = ui.GetComponent<BuildPromptUI>();
        if (promptScript != null)
        {
            promptScript.Setup(point);
        }

        point.SetBuildUI(ui);   // Đính kèm UI vào PointBuild để ẩn sau
        //Debug.Log($"[BuildManager] Show build button at {point.name}");
    }
    #region SpawnSawAt
    private void SpawnSawAt(PointBuild point, bool isFree = false)
    {
        //Trường hợp 1: nếu đã spawn Saw rồi
        if (point.GetHasSaw)
        {
            //Debug.LogWarning($"[BuildManager] Point {point.name} have built");
            return;
        }

        //Trường hợp 2: Nếu chỗ Point Build ko miễn phí, nếu free sẽ nhảy thẳng xuống dưới, ko thông qua trừ tiền
        if (!isFree)
        {
            if (CoinManager.HasInstance)
            {
                //Khi không đủ Coin
                if (CoinManager.Instance.CurrentCoins < currentBuildCost)
                {
                    //Thông báo
                    string notEnoughMsg = $"Not enough coins!\nNeed {currentBuildCost}, have {CoinManager.Instance.CurrentCoins}";

                    if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
                    {
                        UIManager.Instance.hUDPanel.ShowNotification(notEnoughMsg, Color.white, 0.7f); 
                    }
                    return;
                }
                else //Khi đủ coin build
                {
                    // Trừ coin
                    if (CoinManager.HasInstance)
                        CoinManager.Instance.SubtractCoin(currentBuildCost);

                    // Tăng giá và thông báo Event
                    currentBuildCost += costIncreasePerBuild;
                    OnBuildCostChanged?.Invoke();   

                    //Thông báo
                    string successMsg = $"Build Saw successfully!\nNext cost: {currentBuildCost}";

                    if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
                    {
                        UIManager.Instance.hUDPanel.ShowNotification(successMsg, Color.cyan, 0.7f);
                    }
                }
            }
        }
        
        
        //Quá trình build saw
        if (sawPrefab == null) //Kiểm tra Prefab
        {
            //Debug.LogError("[BuildManager] Saw Prefab has not been assigned yet!");
            return;
        }

        //Saw làm gameObject con ở dưới PointBuild
        point.HideModelRootWithEffect(() =>
        {
            //Saw sẽ được build sau khi ModelRoot đã fade xong
            GameObject newSaw = Instantiate(sawPrefab, point.transform);
            newSaw.transform.localPosition = Vector3.zero;
            newSaw.transform.localRotation = Quaternion.identity;

            //Hiệu ứng Saw xuất hiện (pop-in)
            newSaw.transform.localScale = Vector3.zero;
            newSaw.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack);

            point.SetHasSaw();
            point.HideBuildUI();

            //Debug.Log($"[BuildManager] Saw spawned at {point.name}");
        });
    }
    #endregion

    #region ContextMenu
    [ContextMenu("Test Spawn All Initial (Free)")]
    private void TestSpawnInitial() => SpawnInitialSaws();
    #endregion
}
