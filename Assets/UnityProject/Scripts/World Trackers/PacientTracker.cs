using OpenCVForUnity.TrackingModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public UIWindow WindowContainer;

    public string id = "";
    
    public PacientTracker(TrackerHandler trackerHandler = null) {
        this.TrackerHandler = trackerHandler;
    }


    void FixedUpdate() {
        this.gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform.position);

    }

    public bool UpdateActiveEmotion(string emotionName) {
        foreach (EmotionsListScriptableObject.data data in emotionsList.categorical) {
            if (data.name == emotionName) {
                Debugger.AddText("Found it");
                (WindowContainer.components["EmotionDisplay"] as MeshRenderer).materials[0] = data.material;

                Debugger.AddText("Changed it?");
                return true;

            }

        }
        return false;

    }

}
