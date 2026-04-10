using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Enity Data/New Enity Data", fileName = "New_EnityData")]
public class EntityData : ScriptableObject
{
    [Header("Pixel List")]
    public List<PixelInfo> pixels = new List<PixelInfo>();

    public Vector2Int gridSize;
}
