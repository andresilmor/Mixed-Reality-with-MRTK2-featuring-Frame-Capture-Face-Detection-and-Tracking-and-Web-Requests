using OpenCVForUnity.TrackingModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Realms.ChangeSet;
using UnityEngine.UIElements;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

[RequireComponent(typeof(UIWindow))]
public class PacientTracker : MonoBehaviour, ITrackerEntity  {
    [SerializeField] EmotionsListScriptableObject emotionsList;

    private int activeEmotionIndex = -1;

    private TrackerHandler _trackerHandler;
    public TrackerHandler TrackerHandler {
        get { return _trackerHandler; }
        set {
            _trackerHandler = value;
            if (value != null) 
                _trackerHandler.TrackerEntity = this;
        
        }
    }

    private void FixedUpdate() {
        if ((Window.components["EmotionDisplay"] as MeshRenderer).isVisible)
            gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform);
        

    }

    public UIWindow Window;
    

    public string id = "";
    
    public PacientTracker() { }

    public bool UpdateActiveEmotion(string emotionName) {
        //Debug.Log("Emotion: " + emotionName);
        foreach (EmotionsListScriptableObject.data data in emotionsList.categorical) {
            if (data.name.Equals(emotionName)) {
                (Window.components["EmotionDisplay"] as MeshRenderer).material = data.material;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localPosition = data.localPosition;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localScale = data.localScale;

                return true;

            }

        }

        return false;

    }

    public void UpdatePosition(Vector3 newPosition, CameraFrame cameraFrame = null) {
        float exisX = cameraFrame is null ? AppCommandCenter.cameraMain.transform.rotation.x : cameraFrame.Extrinsic.GetRotation().x;

        if (exisX <= -0.0001508334f) {
            newPosition.y = -(newPosition.y + 0.098f); //0.0991  0.0979 0.0985f 0.0983f 0.0988f
            newPosition.x = Window.gameObject.transform.position.x;
            newPosition.z = Window.gameObject.transform.position.z;

        }

        if (Vector3.Distance(Window.gameObject.transform.position, newPosition) >= 0.045f) 
            gameObject.transform.position = newPosition;
        
    }
    
}
