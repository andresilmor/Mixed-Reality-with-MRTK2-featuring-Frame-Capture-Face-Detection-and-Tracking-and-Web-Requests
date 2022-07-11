using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraLocation
{

    public Vector4 position;
    public Vector4 upwards;
    public Vector4 forward;

    public CameraLocation(Vector4 position, Vector4 upwards, Vector4 forward)
    {
        this.position = position;
        this.upwards = upwards;
        this.forward = forward;
        
    }

}
