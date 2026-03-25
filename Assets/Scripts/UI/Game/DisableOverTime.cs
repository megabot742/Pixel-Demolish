using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOverTime : MonoBehaviour
{
    [SerializeField] public float timeToDisable;
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
