using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePanel : MonoBehaviour
{
    public void OnClickPlayGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.PlayGame();
        }
    }
    public void OnClickSettingGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.SettingGame();
        }
    }
    public void OnClickExitGame()
    {
        if (UIEventManager.HasInstance)
        {
            UIEventManager.Instance.QuitGame();
        }
    }
}
