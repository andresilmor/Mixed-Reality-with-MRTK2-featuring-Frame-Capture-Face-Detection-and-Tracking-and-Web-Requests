using Microsoft.MixedReality.Toolkit.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using RectCV = OpenCVForUnity.CoreModule.Rect;

public interface ITrackerEntity {}


public static class TrackerManager {

    private static Dictionary<string, TrackerHandler> _liveTrackers = new Dictionary<string, TrackerHandler>();  
    public static Dictionary<string, TrackerHandler> LiveTrackers {
        get { return _liveTrackers; }
        private set { _liveTrackers = value; }
    }

    public static TrackerHandler CreateTracker(BoxRect boxRect, Mat frame, Vector3 mrPosition, TrackerType trackerType) {
        if (LiveTrackers == null)
            LiveTrackers = new Dictionary<string, TrackerHandler>();

        Debugger.AddText("Create Tracker");
        Point top = new Point(boxRect.x1, boxRect.y1);
        Point bottom = new Point(boxRect.x2, boxRect.y2);

        RectCV region = new RectCV(top, bottom);

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

        legacy_TrackerCSRT trackerCSRT = legacy_TrackerCSRT.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());

        // ------------------------------------ DANGER ZONE --------------------------------------------- //
        Debugger.AddText("Pre Create");
        UIWindow newVisualTracker = UIManager.Instance.OpenWindowAt(WindowType.PacientMarker, mrPosition, Quaternion.identity);
        newVisualTracker.transform.LookAt(AppCommandCenter.cameraMain.transform);
        Debugger.AddText("Pro Create");
        Debugger.AddText("Width T: " + _region.width.ToString());

        //Debugger.AddText("Pos: " + mrPosition.ToString());
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

        layForwardTop.z = mrPosition.z;
        layForwardBottom.z = mrPosition.z;

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
                    newTracker = new TrackerHandler(trackerCSRT, TrackerType.PacientTracker);
                    PacientTracker pacientTracker = newVisualTracker.gameObject.GetComponent<PacientTracker>();
                    pacientTracker.TrackerHandler = newTracker;
                    pacientTracker.WindowContainer = newVisualTracker;
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

        trackerCSRT.init(frame, _region);
        Debugger.AddText("Over tracker create");
        return newTracker;

    }




    public static bool UpdateTrackers() {
        if (_liveTrackers.Count <= 0)
            return false;

        /* Tracker CSRT
        for (int i = 0; i < trackers.Count; i++)
        {
            
            Tracker tracker = trackers[i].trackerSetting.tracker;
            RectCV boundingBox = trackers[i].trackerSetting.boundingBox;
            

            tracker.update(frameMat, boundingBox);

           
         
            
        }
        */

        // Tracker legacy
        /*
        for (int i = 0; i < trackers.Count; i++)
        {
            if (!trackers[i].trackerSetting.isUpdating) { 
                //Debugger.AddText("1");
                legacy_Tracker tracker = trackers[i].trackerSetting.tracker;
                if (tracker == null)
                    Debugger.AddText("Tracker is NULL!!!!!");
                Rect2d boundingBox = trackers[i].trackerSetting.boundingBox;
                if (boundingBox == null)
                   
                    //Debugger.AddText("2");
#if ENABLE_WINMD_SUPPORT

                tracker.update(AppCommandCenter.CameraFrameReader.LastFrame.frameMat, boundingBox);
#endif
                //Debugger.AddText(boundingBox.ToString());

            }

        }
        */

        return true;
    }

}
