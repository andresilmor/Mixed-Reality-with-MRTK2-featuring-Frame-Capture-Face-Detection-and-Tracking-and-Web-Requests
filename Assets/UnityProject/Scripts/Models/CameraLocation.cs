using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraLocation
{

    public Vector4 position;
    public Quaternion rotation;

    public CameraLocation(Vector4 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

}
