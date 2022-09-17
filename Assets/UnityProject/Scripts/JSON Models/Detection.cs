using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Detection
{
    public string id;
    public BodyCenter bodyCenter;
    public FaceRect faceRect;

    public Detection(string id, BodyCenter bodyCenter, FaceRect faceRect)
    {
        this.id = id;
        this.bodyCenter = bodyCenter;
        this.faceRect = faceRect;
    }
}
