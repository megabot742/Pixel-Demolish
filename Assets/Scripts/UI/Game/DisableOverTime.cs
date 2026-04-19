using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOverTime : MonoBehaviour
{
    private float timeToDisable;
    public void SetTimeToDisable(float value)
    {
        timeToDisable = value;
    }
    private float counterTime;
    void OnEnable()
    {
        counterTime = timeToDisable;
    }
    void Update()
    {
        counterTime -= Time.deltaTime;
        if (counterTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
