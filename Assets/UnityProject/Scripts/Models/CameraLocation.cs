using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraLocation
{

    public CameraExtrinsic extrinsic;
    public CameraIntrinsic intrinsic;

    public CameraLocation(CameraExtrinsic extrinsic, CameraIntrinsic intrinsic)
    {
        this.extrinsic = extrinsic;
        this.intrinsic = intrinsic;   

    }
}
