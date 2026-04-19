using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultManager : BaseManager<ResultManager>
{
    [Header("Result Panel")]
    //[SerializeField] private ResultPanel resultPanel;

    [Header("EXP Settings")]
    [SerializeField] private int[] levelExpRequirements = { 500, 700, 900, 1200, 1500 }; // Level 1 -> 5

    private int currentExp = 0;
    private int currentLevel = 1;

    public int CurrentExp => currentExp;
    public int CurrentLevel => currentLevel;

    private bool isLevelCompleted = false;
    public bool GetLevelCompleted() => isLevelCompleted;
    public void SetLevelComplete(bool setLevelCompleted) //Hàm set được gọi khi level hoàn thành
    {
        isLevelCompleted = setLevelCompleted;
    }

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsLevelScene(scene.name))
        {
            //Game luôn khởi đầu ở Level 1, Exp = 0, trạng thái màn chưa hoàn thành
            currentLevel = GetLevelNumber(scene.name);
            currentExp = 0;
            isLevelCompleted = false;
            //Ẩn ResultPanel khi qua level mới
            if (UIManager.HasInstance)
            {
                UIManager.Instance.resultPanel.gameObject.SetActive(false);
            }
            //Reset lại HUDPanel khi qua level mới
            if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
            {
                UIManager.Instance.hUDPanel.RefreshExpUI();
            }
            if (UIManager.HasInstance && UIManager.Instance.resultPanel != null)
            {
                UIManager.Instance.resultPanel.gameObject.SetActive(false);
            }
            //Debug.Log($"[ResultManager] Level {currentLevel} loaded. Need {GetRequiredExp()} EXP to complete.");
        }
    }
    #region Handle EXP
    //Thêm EXP
    public void AddExp(int amount = 1)
    {
        if (isLevelCompleted) return;

        currentExp += amount;

        //Update lên UI
        if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
        {
            UIManager.Instance.hUDPanel.UpdateExpUI(currentExp, GetRequiredExp());
        }

        //Kiểm tra lượng EXP (khi đủ -> hoàn thành màn chơi)
        if (currentExp >= GetRequiredExp())
        {
            CompleteLevel();
        }
    }
    //Xử lí lượng Exp mỗi Level
    public int GetRequiredExp()
    {
        int index = currentLevel - 1;
        if (index < 0 || index >= levelExpRequirements.Length)
            return 1500; // fallback 

        return levelExpRequirements[index];
    }
    //Xử lí hoàn thành Level
    private void CompleteLevel()
    {
        isLevelCompleted = true;

        //Dừng spawn
        if (SpawnManager.HasInstance)
        {
            SpawnManager.Instance.StopSpawning();
        }

        //Duyệt qua hết Entity, gọi tự phá hủy để thả các PixelCube ra
        Enity[] allEntities = FindObjectsByType<Enity>(FindObjectsSortMode.None);
        foreach (var entity in allEntities)
        {
            if (entity != null)
            {
                entity.EntityCleanupAndDestroy();
            }
        }

        // Reset SpawnZone
        var spawnZone = FindObjectOfType<SpawnZone>();
        if (spawnZone != null)
            spawnZone.ForceReset();

        //Chạy Coroutine chờ dọn dẹp và hiện UI
        StartCoroutine(ShowResultPanelAfterDelay(3.0f));
    }
    private IEnumerator ShowResultPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Hiện Result Panel
        if (UIManager.HasInstance)
        {
            UIManager.Instance.resultPanel.gameObject.SetActive(true);
            UIManager.Instance.resultPanel.transform.localScale = Vector3.zero;
            UIManager.Instance.resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
        }
        //Debug.Log($"[ResultManager] Level {currentLevel} COMPLETE! EXP: {currentExp}/{GetRequiredExp()}");
    }
    #endregion
    #region Handle Level
    // Xử lí level kế tiếp
    public void NextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel > 5) nextLevel = 1; //Loop level

        string nextScene = $"Level {nextLevel}";

        //Chuyển Scene
        if (UIManager.HasInstance)
            UIManager.Instance.SwitchToScene(nextScene);
    }
    // Kiểm tra level hiện tại
    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level ");
    }
    //Lấy số level hiện tại (khi đạt max sẽ quay lại level 1)
    private int GetLevelNumber(string sceneName)
    {
        if (int.TryParse(sceneName.Replace("Level ", ""), out int level))
            return level;
        return 1;
    }
    #endregion
}
