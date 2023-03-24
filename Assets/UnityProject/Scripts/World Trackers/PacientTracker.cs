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
        if ((Window.components["EmotionDisplay"] as MeshRenderer).isVisible)
            gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform);
        

    }

    public UIWindow Window;
    

    public string id = "";
    
    public PacientTracker() { }

    public bool UpdateActiveEmotion(string emotionName) {
        //Debugger.AddText("Emotion: " + emotionName);
        foreach (EmotionsListScriptableObject.data data in emotionsList.categorical) {
            if (data.name.Equals(emotionName)) {
                //Debugger.AddText("Found it");
                (Window.components["EmotionDisplay"] as MeshRenderer).material = data.material;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localPosition = data.localPosition;
                (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.transform.localScale = data.localScale;
                //Debugger.AddText("Changed it?");

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
        Debugger.AddText("------------------------------------");
        if (AppCommandCenter.cameraMain.transform.rotation.x <= -0.025f) { // -0.036 -0.020f
            Debugger.AddText("Here");
            Debugger.AddText("Old Y: " + newPosition.y.ToString("0.##########"));
            Debugger.AddText("Old Pos: " + newPosition.ToString("0.##########"));
            newPosition.y = -(newPosition.y + 0.0985f); //0.0991  0.0979 0.0985f 0.0983f
            Debugger.AddText("New Y: " + newPosition.y.ToString("0.##########"));
            newPosition.x = Window.gameObject.transform.position.x;
            newPosition.z = Window.gameObject.transform.position.z;
            Debugger.AddText("New Pos: " + newPosition.ToString("0.##########"));


        }

        if (Vector3.Distance(Window.gameObject.transform.position, newPosition) >= 0.045f)
            gameObject.transform.position = newPosition;

        /*
        if (!(Window.components["EmotionDisplay"] as MeshRenderer).gameObject.activeInHierarchy) {
            Debugger.AddText("Setting active");
            (Window.components["EmotionDisplay"] as MeshRenderer).gameObject.SetActive(true);
            

        }*/


        /*
        if (Vector3.Distance(Window.gameObject.transform.position, newPosition) >= 0.045f) {
            //Debugger.AddText("------------------------------------");
            //Debugger.AddText("Old Y: " + gameObject.transform.position.y.ToString("0.##########"));
            //Debugger.AddText("New Y: " + newPosition.y.ToString("0.##########"));
            //Debugger.AddText("Diff Y: " + (newPosition.y - gameObject.transform.position.y).ToString("0.##########"));
            //Debugger.AddText("Rotation X: " + AppCommandCenter.cameraMain.transform.rotation.x.ToString("0.##########"));
            //Debugger.AddText("Rotation Y: " + AppCommandCenter.cameraMain.transform.rotation.y.ToString("0.##########"));
            //Debugger.AddText("Rotation Z: " + AppCommandCenter.cameraMain.transform.rotation.z.ToString("0.##########"));

            if (AppCommandCenter.cameraMain.transform.rotation.x <= -0.048f) { 
                newPosition.y = -(newPosition.y + 0.0998f);
                newPosition.x = Window.gameObject.transform.position.x;
                newPosition.z = Window.gameObject.transform.position.z;
                if (Vector3.Distance(Window.gameObject.transform.position, newPosition) < 0.045f) 
                    return;

            }

            Debugger.AddText("------------------------------------");
            //Debugger.AddText("Pos: " + newPosition.ToString("0.##########") + "| Dist: " + Vector3.Distance(Window.gameObject.transform.position, newPosition).ToString("0.##########"));
            gameObject.transform.position = newPosition;

        }*/


    }
    
}
