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
    [SerializeField] private int[] levelExpRequirements = { 500, 700, 900, 1200, 1500 }; // Level 1 đến 5

    private int currentExp = 0;
    private int currentLevel = 1;

    public int CurrentExp => currentExp;
    public int CurrentLevel => currentLevel;

    private bool levelCompleted = false;

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
            currentLevel = GetLevelNumber(scene.name);
            currentExp = 0;
            levelCompleted = false;
            if (UIManager.HasInstance)
            {
                UIManager.Instance.resultPanel.gameObject.SetActive(false);
            }
            // Ẩn ResultPanel khi vào level mới
            if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
            {
                UIManager.Instance.hUDPanel.RefreshExpUI();
            }

            Debug.Log($"[ResultManager] Level {currentLevel} loaded. Need {GetRequiredExp()} EXP to complete.");
        }
    }

    // Gọi mỗi khi destroy 1 PixelCube
    public void AddExp(int amount = 1)
    {
        if (levelCompleted) return;

        currentExp += amount;

        // Cập nhật UI trên HUD (nếu có)
        if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
        {
            UIManager.Instance.hUDPanel.UpdateExpUI(currentExp, GetRequiredExp());
        }

        // Kiểm tra đủ EXP chưa
        if (currentExp >= GetRequiredExp())
        {
            CompleteLevel();
        }
    }

    public int GetRequiredExp()
    {
        int index = currentLevel - 1;
        if (index < 0 || index >= levelExpRequirements.Length)
            return 1500; // fallback

        return levelExpRequirements[index];
    }

    private void CompleteLevel()
    {
        levelCompleted = true;

        // Dừng SpawnManager
        if (SpawnManager.HasInstance)
            SpawnManager.Instance.StopSpawning();

        if (UIManager.HasInstance)
        {

            UIManager.Instance.resultPanel.gameObject.SetActive(true);
            // Có thể thêm animation scale lên nếu muốn
            UIManager.Instance.resultPanel.transform.localScale = Vector3.zero;
            UIManager.Instance.resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        }
        // Hiện ResultPanel

        Debug.Log($"[ResultManager] Level {currentLevel} COMPLETE! EXP: {currentExp}/{GetRequiredExp()}");
    }

    // Gọi từ nút "NEXT LEVEL"
    public void NextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel > 5) nextLevel = 1; // vòng lặp

        string nextScene = $"Level {nextLevel}";

        // Chuyển scene (UIManager sẽ xử lý DOTween.KillAll())
        if (UIManager.HasInstance)
            UIManager.Instance.SwitchToScene(nextScene);
    }

    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level ");
    }

    private int GetLevelNumber(string sceneName)
    {
        if (int.TryParse(sceneName.Replace("Level ", ""), out int level))
            return level;
        return 1;
    }
}
