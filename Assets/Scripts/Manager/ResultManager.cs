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
            // Hide ResultPanel when entering a new level
            if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
            {
                UIManager.Instance.hUDPanel.RefreshExpUI();
            }

            //Debug.Log($"[ResultManager] Level {currentLevel} loaded. Need {GetRequiredExp()} EXP to complete.");
        }
    }
    #region Handle
    // Calculate EXP
    public void AddExp(int amount = 1)
    {
        if (levelCompleted) return;

        currentExp += amount;

        // Update UI in HUD
        if (UIManager.HasInstance && UIManager.Instance.hUDPanel != null)
        {
            UIManager.Instance.hUDPanel.UpdateExpUI(currentExp, GetRequiredExp());
        }

        // Check if you have enough EXP
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

        // Stop SpawnManager
        if (SpawnManager.HasInstance)
            SpawnManager.Instance.CompleteLevel();

        Enity[] allEntities = FindObjectsByType<Enity>(FindObjectsSortMode.None);
        foreach (var entity in allEntities)
        {
            if (entity != null)
                entity.ForceDisassembleAllCubes();
        }
        if (UIManager.HasInstance)
        {

            UIManager.Instance.resultPanel.gameObject.SetActive(true);
            // Animation
            UIManager.Instance.resultPanel.transform.localScale = Vector3.zero;
            UIManager.Instance.resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);

        }
        // Show ResultPanel

        //Debug.Log($"[ResultManager] Level {currentLevel} COMPLETE! EXP: {currentExp}/{GetRequiredExp()}");
    }
    public void NextLevel()
    {
        int nextLevel = currentLevel + 1;
        if (nextLevel > 5) nextLevel = 1; //Loop level

        string nextScene = $"Level {nextLevel}";

        // Switch scene
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
    #endregion
}
