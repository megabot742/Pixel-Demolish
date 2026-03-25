using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    public void OnNextLevelClicked()
    {
        if (ResultManager.HasInstance)
            ResultManager.Instance.NextLevel();
    }
}
