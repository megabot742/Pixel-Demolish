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
        // Automatically assign Event Camera if not available (very important for World Space)
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }
    public void Setup(PointBuild targetPoint)
    {
        this.targetPoint = targetPoint;
        buildButton.onClick.AddListener(OnBuildClicked);
    }

    private void OnBuildClicked()
    {
        if (targetPoint == null || !BuildManager.HasInstance) return;
        BuildManager.Instance.BuildSaw(targetPoint);
    }

    private void LateUpdate()
    {
        //Update coin cost
        if (costText != null && BuildManager.HasInstance)
        {
            costText.text = $"Coin: {BuildManager.Instance.CurrentBuildCost}";
        }
    }
    public void Hide()
    {
        Destroy(gameObject);
    }
}
