using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Detection
{
    public string name;
    public BoundingBox box;

    public Detection(string name, BoundingBox box)
    {
        this.name = name;
        this.box = box;
    }
}
