using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PointBuild : MonoBehaviour
{
    [Header("Point Settings")]
    [SerializeField] private bool hasStartBuild = false;
    public bool HasStartBuild => hasStartBuild;

    private bool hasSaw = false;
    public bool HasSaw => hasSaw;
    private GameObject buildUIInstance;

    [Header("Model Root")]
    [SerializeField] private Transform modelRoot;
    private MeshRenderer modelRenderer;
    private Collider modelCollider;

    private void Awake()
    {
        FindModelRoot();
    }
    private void FindModelRoot()
    {
        // Tìm theo Tag để linh hoạt hơn
        GameObject rootObj = GameObject.FindWithTag("ModelRoot");
        
        if (rootObj != null && rootObj.transform.IsChildOf(transform))
        {
            modelRoot = rootObj.transform;
            modelRenderer = modelRoot.GetComponent<MeshRenderer>();
            modelCollider = modelRoot.GetComponent<Collider>();   // CapsuleCollider hoặc BoxCollider gì cũng được

            if (modelRenderer == null)
                Debug.LogWarning($"[PointBuild] ModelRoot at {name} không có MeshRenderer!");
            if (modelCollider == null)
                Debug.LogWarning($"[PointBuild] ModelRoot at {name} không có Collider!");
        }
        else
        {
            Debug.LogWarning($"[PointBuild] Không tìm thấy ModelRoot với Tag 'ModelRoot' trong {name}");
        }
    }
    public void SetBuildUI(GameObject uiInstance)
    {
        buildUIInstance = uiInstance;
    }

    public void HideBuildUI()
    {
        if (buildUIInstance != null)
        {
            Destroy(buildUIInstance);
            buildUIInstance = null;
        }
    }
    public void HideModelRootWithEffect(Action onComplete = null)
    {
        if (modelRoot == null || modelRenderer == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Fade Out Material
        if (modelRenderer.material.HasProperty("_Color"))
        {
            Color color = modelRenderer.material.color;
            modelRenderer.material.DOFade(0f, 0.4f)           // Fade trong 0.4 giây
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // Sau khi fade xong thì disable renderer + collider
                    if (modelRenderer != null) modelRenderer.enabled = false;
                    if (modelCollider != null) modelCollider.enabled = false;

                    onComplete?.Invoke();
                });
        }
        else
        {
            // Fallback nếu material không hỗ trợ _Color
            if (modelRenderer != null) modelRenderer.enabled = false;
            if (modelCollider != null) modelCollider.enabled = false;
            onComplete?.Invoke();
        }
    }
    public void ShowModelRoot()
    {
        if (modelRoot == null) return;

        // Bật lại Renderer + Collider
        if (modelRenderer != null)
        {
            modelRenderer.enabled = true;
            // Fade In lại (nếu muốn)
            Color color = modelRenderer.material.color;
            color.a = 0f;
            modelRenderer.material.color = color;
            modelRenderer.material.DOFade(1f, 0.3f);
        }

        if (modelCollider != null)
            modelCollider.enabled = true;

        hasSaw = false;   // Reset trạng thái
    }
    public void MarkAsBuilt() //Check mark
    {
        hasSaw = true;
    }

    public void ResetBuildPoint() //Reset Point Build, use only for testing
    {
        hasSaw = false;
        //Detele child
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        //Debug.Log($"[PointBuild] Have Reset {name}");
    }

    #region ContextMenu Testing
    [ContextMenu("Test Build Saw Here (Costs Coins)")]
    private void TestBuildSaw()
    {
        if (BuildManager.HasInstance)
        {
            BuildManager.Instance.BuildSaw(this);
        }
        else
        {
            Debug.LogError("BuildManager not found");
        }
    }

    [ContextMenu("Test reset This PointBuild")]
    private void TestResetPointBuild()
    {
        ResetBuildPoint();
    }
    #endregion
}
