using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FrameCapture
{

    public string bytes;
    public CameraLocation cameraLocation;

    public FrameCapture (string bytes, CameraLocation cameraLocation)
    {
        this.bytes = bytes;
        this.cameraLocation = cameraLocation;
    }
}
