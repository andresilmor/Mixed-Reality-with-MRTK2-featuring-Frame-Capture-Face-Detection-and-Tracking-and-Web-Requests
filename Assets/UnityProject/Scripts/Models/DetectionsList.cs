using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DetectionsList
{
    public string type;
    public List<Detection> list;

    public DetectionsList(string type, List<Detection> list)
    {
        this.type = type;
        this.list = list;
    }
}
