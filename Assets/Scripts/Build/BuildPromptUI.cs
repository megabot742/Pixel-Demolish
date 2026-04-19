using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPromptUI : MonoBehaviour
{
    [SerializeField] private Button buildButton;
    [SerializeField] private TMP_Text costText;

    private PointBuild targetPoint;
    private void Awake()
    {
        // Tự động gán Camera sự kiện nếu không có
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
        buildButton.onClick.AddListener(OnBuildClicked);
    }
    private void OnEnable()
    {
        if (BuildManager.HasInstance)
        {
            BuildManager.Instance.OnBuildCostChanged += UpdateCostText;
        }
    }

    private void OnDisable()
    {
        if (BuildManager.HasInstance)
        {
            BuildManager.Instance.OnBuildCostChanged -= UpdateCostText;
        }
    }
    public void Setup(PointBuild targetPoint) //Thiết lập vị trí
    {
        this.targetPoint = targetPoint;
    }

    private void OnBuildClicked() //Button Action
    {
        if (targetPoint == null || !BuildManager.HasInstance) return;

        buildButton.interactable = false; // tránh player nhấn nhiều lần
        BuildManager.Instance.BuildSaw(targetPoint);
        StartCoroutine(DeplayButton());
    }

    private IEnumerator DeplayButton()
    {
        yield return new WaitForSeconds(0.5f); // chờ 1 chút cho đồng bộ cùng animation thông báo
        buildButton.interactable = true;
    }

    private void UpdateCostText() //Update lại UI costText
    {
        if (costText != null && BuildManager.HasInstance)
        {
            costText.text = $"Coin: {BuildManager.Instance.GetCurrentBuildCost}";
        }
    }

    public void Hide()
    {
        Destroy(gameObject);
    }
}
