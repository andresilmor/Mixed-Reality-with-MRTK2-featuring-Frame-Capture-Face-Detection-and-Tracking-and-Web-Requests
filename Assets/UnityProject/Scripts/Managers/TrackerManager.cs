using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using RectCV = OpenCVForUnity.CoreModule.Rect;

#if ENABLE_WINMD_SUPPORT
using Windows.Media.Capture.Frames;
#endif

public interface ITrackerEntity {
    public UIWindow GetBindedWindow();

}


public static class TrackerManager {

    private static Dictionary<string, TrackerHandler> _liveTrackers = new Dictionary<string, TrackerHandler>();  
    public static Dictionary<string, TrackerHandler> LiveTrackers {
        get { return _liveTrackers; }
        private set { _liveTrackers = value; }
    }

    public static Coroutine TrackersUpdater { get; set; }
    public static bool ToUpdate = true;


    public static TrackerHandler CreateTracker(BoxRect boxRect, Mat frame, Vector3 worldPosition, TrackerType trackerType) {
        if (LiveTrackers == null)
            LiveTrackers = new Dictionary<string, TrackerHandler>();

        Debugger.AddText("Create Tracker");
        Point top = new Point(boxRect.x1, boxRect.y1);
        Point bottom = new Point(boxRect.x2, boxRect.y2);

        //RectCV region = new RectCV(top, bottom);

        #region Backup 01
        /* Tracker CSRT
        Debugger.AddText("2");
        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());
        trackerCSRT.init(frame, region);
        */

        /* Tracker legacy_MOSSE
        legacy_TrackerMOSSE trackerMOSSE = legacy_TrackerMOSSE.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());
        Debugger.AddText(_region.ToString());
        trackerMOSSE.init(frame, _region);
        */

        // Tracker legacy_CSRT

        #endregion 

        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());

        MatOfPoint selectedPointMat = new MatOfPoint(top, bottom);
        OpenCVForUnity.CoreModule.Rect region = Imgproc.boundingRect(selectedPointMat);
        Rect2d _region = new Rect2d(region.tl(), region.size());

        // ------------------------------------ DANGER ZONE --------------------------------------------- //
        Debugger.AddText("Pre Create");
        UIWindow newVisualTracker = UIManager.Instance.OpenWindowAt(WindowType.PacientMarker, worldPosition, Quaternion.identity);
        newVisualTracker.transform.LookAt(AppCommandCenter.cameraMain.transform);
        Debugger.AddText("Pro Create");
        Debugger.AddText("Height Frame: " + frame.height().ToString());

        //Debugger.AddText("Pos: " + worldPosition.ToString());
        //Debugger.AddText("Width: " + (boxRect.x2 - boxRect.x1));
        //Debugger.AddText("Height: " + (boxRect.y2 - boxRect.y1));



        #region Backup 00

        /*
        
        int width = boxRect.x2 - boxRect.x1;
        int height = boxRect.y2 - boxRect.y1;
        Debugger.AddText("Width: " + width + " | Height: " + height);

        //int x1 = boxRect.x1 > 0.0f ? (int)boxRect.x1 : 3;
        //int y1 = boxRect.y1> 0.0f ? (int)boxRect.y1 : 3;
        //int x2 = (width + x1) > 1504 ? (int)(1504) - 3 : (int)(width + x1);
        //int y2 = (height + y1) > 846 ? (int)(846) - 3 : (int)(height + y1);

        //Debugger.AddText("x1: " + x1 + " | x2: " + x2 + " | y1: " + y1 + " | y2: " + y2);
      
        //Vector2 topLeft = new Vector2(x1, y1);
        //Vector2 bottomRight = new Vector2(x2, y2);

        Vector3 layForwardTop = Vector3.zero;
        Vector3 layForwardBottom = Vector3.one;
#if ENABLE_WINMD_SUPPORT
        Windows.Foundation.Point topLeft = top.ToWindowsPoint();
        Windows.Foundation.Point bottomRight = bottom.ToWindowsPoint();
        Debugger.AddText("Points");

        layForwardTop = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(boxRect.y1), topLeft, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
        layForwardBottom = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(boxRect.y2), bottomRight, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
         Debugger.AddText("LayForward");
#endif

        layForwardTop.z = worldPosition.z;
        layForwardBottom.z = worldPosition.z;

        Debugger.AddText("PositionZ");
        AppCommandCenter.Strech(newVisualTracker, layForwardBottom, layForwardTop, true);
        Debugger.AddText("Strech");
        // Apply and set main material texture;
        //texture.Apply();
        //material.mainTexture = texture;
        Debugger.AddText("Apply");  */

        //newVisualTracker.GetComponent<PacientTracker>().SetMarkerVisibility(true);

        #endregion
        // mice
        // ---------------------------------------------------------------------------------------------- //


        TrackerHandler newTracker = newVisualTracker.gameObject.AddComponent<TrackerHandler>();
        Debugger.AddText("1");
        // NOT NICE
        try {
            switch (trackerType) {
                case TrackerType.PacientTracker:
                    newTracker = new TrackerHandler(new TrackerHandler.TrackerSetting(
                        trackerCSRT, null, region, frame.height(), false), TrackerType.PacientTracker);
                    PacientTracker pacientTracker = newVisualTracker.gameObject.GetComponent<PacientTracker>();
                    pacientTracker.TrackerHandler = newTracker;
                    pacientTracker.Window = newVisualTracker;
                    newTracker.TrackerEntity = pacientTracker;
                    break;

                default:
                    return null;

            }
        } catch (Exception e) {
            Debugger.AddText(e.Message);
        }
        Debugger.AddText("------");

        Debugger.AddText("Frame: " + (frame is null).ToString());
        Debugger.AddText("Region: " + (_region is null).ToString());
        Debugger.AddText("Region Test: " + _region.width);
        Debugger.AddText("Tracker: " + (trackerCSRT is null).ToString());

        trackerCSRT.init(frame, region);
        Debugger.AddText("Over tracker create");
        return newTracker;

    }

#if ENABLE_WINMD_SUPPORT
    public static void UpdateTrackers(MediaFrameReader sender, MediaFrameArrivedEventArgs args) {
        Debugger.AddText("I'm Here tyo xd");
    }
#endif

    public static IEnumerator UpdateTrackers() {
        Debugger.AddText("Yo");
         
        while (true) {
            if (_liveTrackers.Count > 0) { 
                Debugger.AddText("Ma Men");
            foreach (KeyValuePair<string, TrackerHandler> tracker in _liveTrackers) {
                Debugger.AddText("I need to?");
                if (ToUpdate) {
                    Debugger.AddText("May I?");
                    lock (tracker.Value) {
                        Debugger.AddText("You May");
                        Debugger.AddText("---- BEFORE UPDATE ----");
                        Debugger.AddText("Updating: " + tracker.Value.TrackerIdentifier);
                        Debugger.AddText("Updater Width: " + tracker.Value.TrackerSettings.boundingBox.width);
                        Debugger.AddText("Updater X: " + tracker.Value.TrackerSettings.boundingBox.x);
                        Debugger.AddText("Updater Height: " + tracker.Value.TrackerSettings.boundingBox.height);
                        Debugger.AddText("Updater Y: " + tracker.Value.TrackerSettings.boundingBox.y);
                            OpenCVForUnity.CoreModule.Rect rect = null;
#if ENABLE_WINMD_SUPPORT
                    rect = tracker.Value.UpdateTracker(tracker.Value.TrackerSettings.boundingBox, CameraFrameReader.GenerateCVMat(AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference));
#endif
                            if (rect == null) {
                                tracker.Value.Updated = false;
                                continue;
                            }
                            tracker.Value.Updated = true;
                            tracker.Value.TrackerSettings = new TrackerHandler.TrackerSetting(tracker.Value.TrackerSettings.tracker, null, rect, tracker.Value.TrackerSettings.FrameHeight);
                            Debugger.AddText("bY");
                    }

                    Vector3 worldPositon;
                    Debugger.AddText("---- AFTER UPDATE ----");
                    Debugger.AddText("Updating: " + tracker.Value.TrackerIdentifier);
                    Debugger.AddText("Updater Width: " + tracker.Value.TrackerSettings.boundingBox.width);
                    Debugger.AddText("Updater X: " + tracker.Value.TrackerSettings.boundingBox.x);
                    Debugger.AddText("Updater Height: " + tracker.Value.TrackerSettings.boundingBox.height);
                    Debugger.AddText("Modified Y: " + tracker.Value.TrackerSettings.boundingBox.y);
                    //MRWorld.GetWorldPosition(out worldPositon, tracker.Value.TrackerSettings.boundingBox);
                    //Debugger.AddText("Position: " + worldPositon.ToString("0.############"));

                    try { 
                        //tracker.Value.TrackerEntity.GetBindedWindow().SetPosition(worldPositon, true);

                    } catch(Exception ex) {
                        Debugger.AddText("Binded Window Error: " + ex.Message);

                    }

                }

            }
            }

            if (_liveTrackers.Count > 0)
                AppCommandCenter.Instance.timeToStop += 1;

            yield return new WaitForEndOfFrame();
            if (_liveTrackers.Count > 0)
                Debugger.AddText("Round: " + AppCommandCenter.Instance.timeToStop);
         

        }

    }


}
