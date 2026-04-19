using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    public void OnNextLevelClicked() //Button Action
    {
        if (ResultManager.HasInstance)
            ResultManager.Instance.NextLevel();
    }
}
