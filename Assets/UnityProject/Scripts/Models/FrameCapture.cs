using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FrameCapture
{

    public string bytes;

    public FrameCapture(string bytes)
    {
        this.bytes = bytes;
    }
}
