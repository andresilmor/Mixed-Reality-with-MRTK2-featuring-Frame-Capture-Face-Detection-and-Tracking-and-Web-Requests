using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = XRDebug;

public interface ITrackerEntity {
    public void UpdatePosition(Vector3 newPosition, CameraFrame cameraFrame = null);

}
