using OpenCVForUnity.TrackingModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Realms.ChangeSet;
using UnityEngine.UIElements;

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
        gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform);
    }

    public UIWindow Window;

    public string id = "";
    
    public PacientTracker() { }

    public bool UpdateActiveEmotion(string emotionName) {
        foreach (EmotionsListScriptableObject.data data in emotionsList.categorical) {
            if (data.name == emotionName) {
                Debugger.AddText("Found it");
                (Window.components["EmotionDisplay"] as MeshRenderer).material = data.material;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localPosition = data.localPosition;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localScale = data.localScale;
                Debugger.AddText("Changed it?");

                return true;

            }

        }
        return false;

    }

    public UIWindow GetBindedWindow() {
        return Window;

    }

    public void UpdatePosition(Vector3 newPosition) {
        //newPosition.z += (Window.gameObject.transform.position.z > 0 ? 1.12f : -1.12f);
        if (Vector3.Distance(Window.gameObject.transform.position, newPosition) > 0.045f) {
            //Debugger.AddText("Pos: " + newPosition.ToString("0.##########") + "| Dist: " + Vector3.Distance(Window.gameObject.transform.position, newPosition).ToString("0.##########"));
            gameObject.transform.position = newPosition;

        }


    }
    
}
