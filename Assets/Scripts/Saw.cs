using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saw : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PixelCube pixelCube))
        {
            pixelCube.DetouchCube();
        }
    }
}
