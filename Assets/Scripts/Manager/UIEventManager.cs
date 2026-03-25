using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIEventManager : BaseManager<UIEventManager>
{
    protected override void Awake()
    {
        base.Awake();
    }
    public void PlayGame()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.homePanel.gameObject.SetActive(false);
            UIManager.Instance.hUDPanel.gameObject.SetActive(true);
            UIManager.Instance.SwitchToScene("Level 1");
        }
    }
    public void SettingGame()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.homePanel.gameObject.SetActive(false);
            UIManager.Instance.settingPanel.gameObject.SetActive(true);
        }
    }
    public void RestartLevel()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.RestartCurrentLevel();
            Time.timeScale = 1f;
        }
    }
    public void BackMenu()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.SwitchToScene("Menu");
            Time.timeScale = 1f;
        }
    }
    public void QuitGame()
    {
        Debug.Log("Exit game");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
