using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DetectionsList
{
    public string type;
    public CameraLocation cameraLocation;
    public List<Detection> list;

    public DetectionsList(string type, CameraLocation cameraLocation, List<Detection> list)
    {
        this.type = type;
        this.cameraLocation = cameraLocation;
        this.list = list;
    }
}
