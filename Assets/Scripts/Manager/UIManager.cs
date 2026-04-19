using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class UIManager : BaseManager<UIManager>
{
    [Header("Menu")]
    public HomePanel homePanel;
    public SettingPanel settingPanel;
    [Header("Game")]
    public HUDPanel hUDPanel;
    public ResultPanel resultPanel;
    [Header("Loading")]
    public LoadingPanel loadingPanel;

    [Header("Scene")]
    public string currentSceneName;

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        // Load Menu scene when game starts
        SwitchToScene("Menu");
    }
    public void SwitchToScene(string sceneName)
    {
        //Dọn các Animation Dotween
        DOTween.KillAll();
        if (resultPanel != null) resultPanel.gameObject.SetActive(false);
        //Dọn Enity
        Enity[] entities = FindObjectsByType<Enity>(FindObjectsSortMode.None);
        foreach (var entity in entities)
        {
            if (entity != null)
            {
                entity.EntityCleanupAndDestroy();
            }
        }
        //Dọn phần tử về Pool
        if (PoolManager.HasInstance)
        {
            PoolManager.Instance.ResetAllPool();
        }
        //Loading
        if (loadingPanel != null)
        {
            loadingPanel.ShowLoading(sceneName);   // ← dùng loading thay vì LoadScene trực tiếp
        }
        else // Fallback
        {
            SceneManager.LoadScene(sceneName);
            currentSceneName = sceneName;
            UpdateUIForScene(sceneName);
        }


    }

    public void RestartCurrentLevel()
    {
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            DOTween.KillAll(); //Dọn các Animation Dotween
            if (resultPanel != null) resultPanel.gameObject.SetActive(false);
            //Dọn Enity
            Enity[] entities = FindObjectsByType<Enity>(FindObjectsSortMode.None);
            foreach (var entity in entities)
            {
                if (entity != null)
                {
                    entity.EntityCleanupAndDestroy();
                }
            }
            //Dọn phần tử về Pool
            if (PoolManager.HasInstance)
            {
                PoolManager.Instance.ResetAllPool();
            }
            //Loading
            if (loadingPanel != null)
            {
                loadingPanel.ShowLoading(currentSceneName);
            }
            else
            {
                SceneManager.LoadScene(currentSceneName);
                UpdateUIForScene(currentSceneName);
            }
        }
    }

    public void ChangeUIGameObject(GameObject currentObejct = null, GameObject activeObject = null)
    {
        if (currentObejct != null)
        {
            currentObejct.SetActive(false);
        }
        if (activeObject != null)
        {
            activeObject.SetActive(true);
        }
    }
    public void UpdateUIForScene(string sceneName)
    {
        // Tắt hết các panel trước
        ChangeUIGameObject(homePanel.gameObject);
        ChangeUIGameObject(settingPanel.gameObject);

        ChangeUIGameObject(hUDPanel.gameObject);
        ChangeUIGameObject(resultPanel.gameObject);

        ChangeUIGameObject(loadingPanel.gameObject);

        // Hiện các panel phù hợp với scene
        switch (sceneName)
        {
            case "Menu":
                ChangeUIGameObject(null, homePanel.gameObject);
                break;
            case "Level 1":
            case "Level 2":
            case "Level 3":
            case "Level 4":
            case "Level 5":
                ChangeUIGameObject(null, hUDPanel.gameObject);
                break;
        }
    }
}
