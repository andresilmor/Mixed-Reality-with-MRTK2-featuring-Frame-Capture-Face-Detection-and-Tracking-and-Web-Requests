using Microsoft.MixedReality.Toolkit.UI;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using RectCV = OpenCVForUnity.CoreModule.Rect;

public static class TrackingManager
{
    public static List<Pacient> trackers;


    public static void CreateTracker(FaceRect faceRect, Mat frame, GameObject visualMarker, Vector3 mrPosition, out Pacient newPerson, string trackerWhat)
    {
        if (trackers == null)
            TrackingManager.trackers = new List<Pacient>();


        Debugger.AddText("1");
        
        Point top = new Point(faceRect.x1, faceRect.y1);
        Point bottom = new Point(faceRect.x2, faceRect.y2);

        RectCV region = new RectCV(top, bottom);
        Debugger.AddText(region.ToString());

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
        legacy_TrackerCSRT trackerCSRT = legacy_TrackerCSRT.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());
        Debugger.AddText(_region.ToString());

        // ------------------------------------ DANGER ZONE --------------------------------------------- //
        if (visualMarker == null)
            Debugger.AddText("visual tracker is null");

   
        Debugger.AddText("Here we are");
        newPerson = null;

        GameObject newVisualTracker = UnityEngine.Object.Instantiate(visualMarker, mrPosition, Quaternion.LookRotation(Camera.main.transform.position, Vector3.up));

        Vector3 tempPos = mrPosition;




        Debugger.AddText("Here we are now");

        /*
        
        int width = faceRect.x2 - faceRect.x1;
        int height = faceRect.y2 - faceRect.y1;
        Debugger.AddText("Width: " + width + " | Height: " + height);

        //int x1 = faceRect.x1 > 0.0f ? (int)faceRect.x1 : 3;
        //int y1 = faceRect.y1> 0.0f ? (int)faceRect.y1 : 3;
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

        layForwardTop = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(faceRect.y1), topLeft, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
        layForwardBottom = MRWorld.GetLayForward(MRWorld.GetUnprojectionOffset(faceRect.y2), bottomRight, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
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

        //newVisualTracker.GetComponent<PersonMarker>().SetMarkerVisibility(true);

        if (newVisualTracker == null)
            Debugger.AddText("visual tracker is null");

        // mice
        // ---------------------------------------------------------------------------------------------- //

        Debugger.AddText("3 " + trackerWhat);
        Debugger.AddText(trackerWhat.Equals("Pacient").ToString());
        Debugger.AddText("---- TILL HERE ----");
        // NOT NICE
        try
        {
            switch (trackerWhat)
            {
                case "Pacient":

                    trackerCSRT.init(frame, _region);

                    if (newVisualTracker.GetComponent<PersonMarker>() != null)
                        Debugger.AddText("Yup eu tenho isso");

                    Debugger.AddText(trackerCSRT.GetType().ToString());
                    Debugger.AddText(newVisualTracker.GetComponent<PersonMarker>().GetType().ToString());


                    //newPerson = new Pacient(newVisualTracker.GetComponent<PersonMarker>(), trackerCSRT);
                    Pacient p = new Pacient();
                    Debugger.AddText("I WAS HERE!!!!!");

                    

                    //newPerson = new Pacient(trackerMOSSE);
                    Debugger.AddText("I WAS NOT!!!!!");
                    break;
                default:
                    newPerson = null;
                    return;

            }
        }
        catch (Exception e) {
            Debugger.AddText(e.Message);
        }


        Debugger.AddText("4");
        //TrackingManager.trackers.Add(newPerson);
     
        Debugger.AddText("5");
    }

    


    public static bool UpdateTrackers()
    {

        if (trackers.Count == 0)
            return false;
        else
            Debugger.AddText("trackers count: " + trackers.Count);


        /* Tracker CSRT
        for (int i = 0; i < trackers.Count; i++)
        {
            Debugger.AddText("1" );
            Tracker tracker = trackers[i].trackerSetting.tracker;
            RectCV boundingBox = trackers[i].trackerSetting.boundingBox;
            Debugger.AddText("2");

            tracker.update(frameMat, boundingBox);

            Debugger.AddText(boundingBox.ToString());
         
            
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
                    Debugger.AddText("boundingBox is NULL!!!!!");
                    //Debugger.AddText("2");
#if ENABLE_WINMD_SUPPORT

                tracker.update(AppCommandCenter.frameHandler.LastFrame.frameMat, boundingBox);
#endif
                //Debugger.AddText(boundingBox.ToString());

            }

        }
        */

        return true;
    }

}
