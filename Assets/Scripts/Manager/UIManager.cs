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
        //Clear DotWeen
        DOTween.KillAll();
        // Load the new scene
        SceneManager.LoadScene(sceneName);
        currentSceneName = sceneName;

        // Update UI based on the loaded scene
        UpdateUIForScene(sceneName);

       
    }

    public void RestartCurrentLevel()
    {
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            DOTween.KillAll();           
            SceneManager.LoadScene(currentSceneName);
            UpdateUIForScene(currentSceneName);
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
    private void UpdateUIForScene(string sceneName)
    {
        // Disable all panels first
        ChangeUIGameObject(homePanel.gameObject);
        ChangeUIGameObject(settingPanel.gameObject);

        ChangeUIGameObject(hUDPanel.gameObject);
        ChangeUIGameObject(resultPanel.gameObject);

        // Enable the default panel based on the scene
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
