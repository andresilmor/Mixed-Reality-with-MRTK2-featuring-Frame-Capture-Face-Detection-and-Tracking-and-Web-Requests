using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = MRDebug;

public interface ITrackerEntity {
    public void UpdatePosition(Vector3 newPosition, CameraFrame cameraFrame = null);

}


