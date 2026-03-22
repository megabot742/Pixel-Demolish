using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField] private Color cubeColor = Color.white;

    private void Awake()
    {
        ApplyColor(cubeColor);
    }
    private void OnValidate()
    {
        ApplyColor(cubeColor);
    }
    private void ApplyColor(Color color)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_BaseColor", color); //URP option
        renderer.SetPropertyBlock(propertyBlock);
        
    }
    public void ApplyDetouchColor()
    {
        ApplyColor(cubeColor * 0.8f); //fade out color
    }
}
