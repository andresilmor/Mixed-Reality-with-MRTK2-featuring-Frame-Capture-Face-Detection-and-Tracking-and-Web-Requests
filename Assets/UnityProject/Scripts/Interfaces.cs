using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

public interface ITrackerEntity {
    public void UpdatePosition(Vector3 newPosition, CameraFrame cameraFrame = null);

}


public interface IUIView {
    public void Bind(UIWindow window);
    public void BindTexts(UIWindow window, bool alreadyValidated);
    public void BindActions(UIWindow window, bool alreadyValidated);

}
