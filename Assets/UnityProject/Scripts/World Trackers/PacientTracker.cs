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

    public void UpdatePosition(Vector3 newPosition, CameraFrame cameraFrame = null) {
        //newPosition.z += (Window.gameObject.transform.position.z > 0 ? 1.12f : -1.12f);

        bool wasEdited = false;
        bool firstIf = false;

        float exisX = cameraFrame is null ? AppCommandCenter.cameraMain.transform.rotation.x : cameraFrame.Extrinsic.GetRotation().x;
        float exisXcop =  AppCommandCenter.cameraMain.transform.rotation.x;

        if (exisX <= -0.0001508334f) {
            wasEdited = true;
            newPosition.y = -(newPosition.y + 0.098f); //0.0991  0.0979 0.0985f 0.0983f 0.0988f
            newPosition.x = Window.gameObject.transform.position.x;
            newPosition.z = Window.gameObject.transform.position.z;

        }



        if (Vector3.Distance(Window.gameObject.transform.position, newPosition) >= 0.045f) {
            Debugger.AddText("------------------------------------");
            if (exisX <= -0.009f)
                Debugger.AddText("Yo 2");
            if (firstIf)
                Debugger.AddText("First if");
            if (wasEdited)
                Debugger.AddText("It was changed");

            Debugger.AddText("Rotation Comparation | Camera: " + exisXcop.ToString("0.##########") + " | Extrinsic : " + cameraFrame.Extrinsic.GetRotation().x.ToString("0.##########"));

            Debugger.AddText("Rotation : " + exisX.ToString("0.##########") + " | Distance: " + Vector3.Distance(Window.gameObject.transform.position, newPosition).ToString("0.##########"));
            Debugger.AddText("Changed");
            gameObject.transform.position = newPosition;
        }

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
