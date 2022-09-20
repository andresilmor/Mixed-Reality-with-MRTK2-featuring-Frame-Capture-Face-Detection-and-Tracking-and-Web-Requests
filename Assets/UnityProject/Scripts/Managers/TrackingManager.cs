using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using RectCV = OpenCVForUnity.CoreModule.Rect;

public static class TrackingManager
{
    public static List<Person> trackers;


    public static void CreateTracker(FaceRect faceRect, Mat frame, string type = "Pacient")
    {
        if (trackers == null)
            TrackingManager.trackers = new List<Person>();

        Debugger debugger = GameObject.FindObjectOfType<Debugger>();

        debugger.AddText("1");
        
        Point top = new Point(faceRect.x1, faceRect.y1);
        Point bottom = new Point(faceRect.x2, faceRect.y2);

        RectCV region = new RectCV(top, bottom);
        debugger.AddText(region.ToString());
        /* Tracker CSRT
        debugger.AddText("2");
        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());
        trackerCSRT.init(frame, region);
        */

        /* Tracker legacy_MOSSE
        legacy_TrackerMOSSE trackerMOSSE = legacy_TrackerMOSSE.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());
        debugger.AddText(_region.ToString());
        trackerMOSSE.init(frame, _region);
        */

        // Tracker legacy_CSRT
        legacy_TrackerCSRT trackerCSRT = legacy_TrackerCSRT.create();
        Rect2d _region = new Rect2d(region.tl(), region.size());
        debugger.AddText(_region.ToString());
        trackerCSRT.init(frame, _region);
   

        debugger.AddText("3");
        Person newtracker = null;
        switch (type)
        {
            case "Pacient":
                newtracker = new Pacient(trackerCSRT);
                //newtracker = new Pacient(trackerMOSSE);
                break;
            
        }

        if (newtracker == null)
            return;
        debugger.AddText("4");
        TrackingManager.trackers.Add(newtracker);
     
        debugger.AddText("5");
    }


    public static bool UpdateTrackers(Mat frameMat)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();

        if (trackers.Count == 0)
            return false;
        else
            debugger.AddText("trackers count: " + trackers.Count);


        /* Tracker CSRT
        for (int i = 0; i < trackers.Count; i++)
        {
            debugger.AddText("1" );
            Tracker tracker = trackers[i].trackerSetting.tracker;
            RectCV boundingBox = trackers[i].trackerSetting.boundingBox;
            debugger.AddText("2");

            tracker.update(frameMat, boundingBox);

            debugger.AddText(boundingBox.ToString());
         
            
        }
        */

        // Tracker legacy
        for (int i = 0; i < trackers.Count; i++)
        {
            debugger.AddText("1");
            legacy_Tracker tracker = trackers[i].trackerSetting.tracker;
            if (tracker == null)
                debugger.AddText("Tracker is NULL!!!!!");
            Rect2d boundingBox = trackers[i].trackerSetting.boundingBox;
            if (boundingBox == null)
                debugger.AddText("boundingBox is NULL!!!!!");
            debugger.AddText("2");

            tracker.update(frameMat, boundingBox);

            debugger.AddText(boundingBox.ToString());

          

        }
        

        return true;
    }

}
