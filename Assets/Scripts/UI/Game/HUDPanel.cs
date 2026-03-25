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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void ShowNotification(string message, Color color, float duration = 1f)
    {
        if (notifyTxt == null || notifyGameObject == null) return;

        notifyTxt.text = message;
        notifyTxt.color = color;
        notifyGameObject.SetActive(true);

        // Cập nhật thời gian tự tắt (nếu bạn muốn linh hoạt hơn 1s)
        var disableScript = notifyGameObject.GetComponent<DisableOverTime>();
        if (disableScript != null)
        {
            disableScript.timeToDisable = duration;
        }
    }
    public void UpdateExpUI(int current, int required)
    {
        if (expTxt != null)   // Bạn đã đổi tên thành expTxt
        {
            expTxt.text = $"EXP: {current}/{required}";
        }

        if (expSlider != null)
        {
            expSlider.maxValue = required;
            expSlider.value = current;
        }
    }
    public void RefreshExpUI()
    {
        if (ResultManager.HasInstance)
        {
            int current = ResultManager.Instance.CurrentExp;
            int required = ResultManager.Instance.GetRequiredExp();   // cần public ở ResultManager

            UpdateExpUI(current, required);
        }
    }
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
}
