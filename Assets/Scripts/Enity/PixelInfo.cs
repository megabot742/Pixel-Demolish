using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PixelInfo
{
    public Vector3 localPosition;
    public Color color;

    //Contructor
    public PixelInfo(Vector3 pos, Color col)
    {
        localPosition = pos;
        color = col;
    }
}
