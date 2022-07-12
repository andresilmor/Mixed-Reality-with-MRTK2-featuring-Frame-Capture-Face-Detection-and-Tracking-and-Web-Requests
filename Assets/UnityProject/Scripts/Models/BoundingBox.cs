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

    public BoundingBox(int y1, int x2, int y2, int x1)
    {
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
        this.x1 = x1;
    }
}
