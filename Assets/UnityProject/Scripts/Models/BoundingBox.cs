using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoundingBox
{
    public int y1;
    public int x2;
    public int y2;
    public int x1;
    public float centerX;
    public float centerY;

    public BoundingBox(int y1, int x2, int y2, int x1, float centerX, float centerY)
    {
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.x1 = x1;
        this.centerX = centerX;
        this.centerY = centerY;
    }
}
