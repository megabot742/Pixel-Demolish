using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{
    public TMP_Text coinTxt;
    public TMP_Text notifyTxt;
    public GameObject notifyGameObject;

    [Header("Level")]
    public TMP_Text expTxt;
    public Slider expSlider;
    
    #region Handler
    public void ShowNotification(string message, Color color, float duration = 1f)
    {
        if (notifyTxt == null || notifyGameObject == null) return;

        notifyTxt.text = message;
        notifyTxt.color = color;
        notifyGameObject.SetActive(true);

        // Update thời gian tự tắt thông báo
        var disableScript = notifyGameObject.GetComponent<DisableOverTime>();
        if (disableScript != null)
        {
            disableScript.SetTimeToDisable(duration);
        }
    }
    public void UpdateExpUI(int current, int required) //Updae thông tin EXP
    {
        if (expTxt != null)
        {
            expTxt.text = $"EXP: {current}/{required}";
        }

        if (expSlider != null)
        {
            expSlider.maxValue = required;
            expSlider.value = current;
        }
    }
    public void RefreshExpUI() //Reset thông tin EXP
    {
        if (ResultManager.HasInstance)
        {
            int current = ResultManager.Instance.CurrentExp;
            int required = ResultManager.Instance.GetRequiredExp();

            UpdateExpUI(current, required);
        }
    }
    #endregion
    #region Button action
    public void OnClickRestart()
    {
        if (UIManager.HasInstance)
        {
            UIManager.Instance.RestartCurrentLevel();
            Time.timeScale = 1f;
            Debug.Log($"[UIEventManager] Restarting current level: {UIManager.Instance.currentSceneName}");
        }
    }
    public void OnClickMenu()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.BackMenu(); //Return scene Menu
        }
    }
    #endregion
}
