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

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif


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

        Debug.Log("Create Tracker");
        Point top = new Point(boxRect.x1, boxRect.y1);
        Point bottom = new Point(boxRect.x2, boxRect.y2);

        //RectCV region = new RectCV(top, bottom);

        #region Backup 01
        /* Tracker CSRT
        Debug.Log("2");
        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());
        trackerCSRT.init(frame, region);
        */

        /* Tracker legacy_MOSSE
        legacy_TrackerMOSSE trackerMOSSE = legacy_TrackerMOSSE.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());
        Debug.Log(_region.ToString());
        trackerMOSSE.init(frame, _region);
        */

        // Tracker legacy_CSRT

        #endregion 

        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());

        MatOfPoint selectedPointMat = new MatOfPoint(top, bottom);
        OpenCVForUnity.CoreModule.Rect region = Imgproc.boundingRect(selectedPointMat);
        Rect2d _region = new Rect2d(region.tl(), region.size());

        // ------------------------------------ DANGER ZONE --------------------------------------------- //
        Debug.Log("Pre Create");
        UIWindow newVisualTracker = UIManager.Instance.OpenWindowAt(WindowType.Sp_ML_E_1btn_Pacient, worldPosition, Quaternion.identity);
        newVisualTracker.transform.LookAt(AppCommandCenter.cameraMain.transform);
        Debug.Log("Pro Create");
        Debug.Log("Height Frame: " + frame.height().ToString());

        //Debug.Log("Pos: " + worldPosition.ToString());
        //Debug.Log("Width: " + (boxRect.x2 - boxRect.x1));
        //Debug.Log("Height: " + (boxRect.y2 - boxRect.y1));



        #region Backup 00

        /*
        
        int width = boxRect.x2 - boxRect.x1;
        int height = boxRect.y2 - boxRect.y1;
        Debug.Log("Width: " + width + " | Height: " + height);

        //int x1 = boxRect.x1 > 0.0f ? (int)boxRect.x1 : 3;
        //int y1 = boxRect.y1> 0.0f ? (int)boxRect.y1 : 3;
        //int x2 = (width + x1) > 1504 ? (int)(1504) - 3 : (int)(width + x1);
        //int y2 = (height + y1) > 846 ? (int)(846) - 3 : (int)(height + y1);

        //Debug.Log("x1: " + x1 + " | x2: " + x2 + " | y1: " + y1 + " | y2: " + y2);
      
        //Vector2 topLeft = new Vector2(x1, y1);
        //Vector2 bottomRight = new Vector2(x2, y2);

        Vector3 layForwardTop = Vector3.zero;
        Vector3 layForwardBottom = Vector3.one;
#if ENABLE_WINMD_SUPPORT
        Windows.Foundation.Point topLeft = top.ToWindowsPoint();
        Windows.Foundation.Point bottomRight = bottom.ToWindowsPoint();
        Debug.Log("Points");

        layForwardTop = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(boxRect.y1), topLeft, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
        layForwardBottom = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(boxRect.y2), bottomRight, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
         Debug.Log("LayForward");
#endif

        layForwardTop.z = worldPosition.z;
        layForwardBottom.z = worldPosition.z;

        Debug.Log("PositionZ");
        AppCommandCenter.Strech(newVisualTracker, layForwardBottom, layForwardTop, true);
        Debug.Log("Strech");
        // Apply and set main material texture;
        //texture.Apply();
        //material.mainTexture = texture;
        Debug.Log("Apply");  */

        //newVisualTracker.GetComponent<PacientTracker>().SetMarkerVisibility(true);

        #endregion
        // mice
        // ---------------------------------------------------------------------------------------------- //


        TrackerHandler newTracker = newVisualTracker.gameObject.AddComponent<TrackerHandler>();
        Debug.Log("1");
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
            Debug.Log(e.Message);
        }
        Debug.Log("------");

        Debug.Log("Frame: " + (frame is null).ToString());
        Debug.Log("Region: " + (_region is null).ToString());
        Debug.Log("Region Test: " + _region.width);
        Debug.Log("Tracker: " + (trackerCSRT is null).ToString());

        trackerCSRT.init(frame, region);
        Debug.Log("Over tracker create");
        return newTracker;

    }

#if ENABLE_WINMD_SUPPORT
    public static void UpdateTrackers(MediaFrameReader sender, MediaFrameArrivedEventArgs args) {
        Debug.Log("I'm Here tyo xd");
    }
#endif

    public static IEnumerator UpdateTrackers() {
        Debug.Log("Yo");
         
        while (true) {
            if (_liveTrackers.Count > 0) { 
                Debug.Log("Ma Men");
            foreach (KeyValuePair<string, TrackerHandler> tracker in _liveTrackers) {
                Debug.Log("I need to?");
                if (ToUpdate) {
                    Debug.Log("May I?");
                    lock (tracker.Value) {
                        Debug.Log("You May");
                        Debug.Log("---- BEFORE UPDATE ----");
                        Debug.Log("Updating: " + tracker.Value.TrackerIdentifier);
                        Debug.Log("Updater Width: " + tracker.Value.TrackerSettings.boundingBox.width);
                        Debug.Log("Updater X: " + tracker.Value.TrackerSettings.boundingBox.x);
                        Debug.Log("Updater Height: " + tracker.Value.TrackerSettings.boundingBox.height);
                        Debug.Log("Updater Y: " + tracker.Value.TrackerSettings.boundingBox.y);
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
                            Debug.Log("bY");
                    }

                    Vector3 worldPositon;
                    Debug.Log("---- AFTER UPDATE ----");
                    Debug.Log("Updating: " + tracker.Value.TrackerIdentifier);
                    Debug.Log("Updater Width: " + tracker.Value.TrackerSettings.boundingBox.width);
                    Debug.Log("Updater X: " + tracker.Value.TrackerSettings.boundingBox.x);
                    Debug.Log("Updater Height: " + tracker.Value.TrackerSettings.boundingBox.height);
                    Debug.Log("Modified Y: " + tracker.Value.TrackerSettings.boundingBox.y);
                    //MRWorld.GetFaceWorldPosition(out worldPositon, tracker.Value.TrackerSettings.boundingBox);
                    //Debug.Log("Position: " + worldPositon.ToString("0.############"));

                    try { 
                        //tracker.Value.TrackerEntity.GetBindedWindow().SetPosition(worldPositon, true);

                    } catch(Exception ex) {
                        Debug.Log("Binded Window Error: " + ex.Message);

                    }

                }

            }
            }

            if (_liveTrackers.Count > 0)
                AppCommandCenter.Instance.timeToStop += 1;

            yield return new WaitForEndOfFrame();
            if (_liveTrackers.Count > 0)
                Debug.Log("Round: " + AppCommandCenter.Instance.timeToStop);
         

        }

    }


}
