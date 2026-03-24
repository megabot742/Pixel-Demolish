using System.Collections;
using System.Collections.Generic;
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
        Debug.Log($"[PointBuild] Have Reset {name}");
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
