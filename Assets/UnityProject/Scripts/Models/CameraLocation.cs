using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraLocation
{

    public Vector3 position;
    public Vector4 upwards;
    public Vector4 forward;

    public CameraLocation(Vector3 position, Vector4 upwards, Vector4 forward)
    {
        this.position = position;
        this.upwards = upwards;
        this.forward = forward;
        
    }
}
