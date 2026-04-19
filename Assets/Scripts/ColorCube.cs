using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorCube : MonoBehaviour
{
    [SerializeField] private Color cubeColor = Color.white;
    public Color CubeColor
    {
        get => cubeColor;
        set
        {
            cubeColor = value;
            ApplyColor(cubeColor);
        }
    }

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
        propertyBlock.SetColor("_BaseColor", color); //Thiết lập URP 
        renderer.SetPropertyBlock(propertyBlock);

    }
    public void ApplyDetouchColor()
    {
        ApplyColor(cubeColor * 0.8f); //fade out màu (màu sau khi bị detouch sẽ đậm hơn)
    }
}
