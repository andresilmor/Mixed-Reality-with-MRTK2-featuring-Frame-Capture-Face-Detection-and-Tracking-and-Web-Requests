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
        debugger.AddText("2");
        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());
        RectCV region = new RectCV(top, bottom);
        trackerCSRT.init(frame, region);
        debugger.AddText("3");
        Person newtracker = null;
        switch (type)
        {
            case "Pacient":
                newtracker = new Pacient(trackerCSRT);
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

        for (int i = 0; i < trackers.Count; i++)
        {
            debugger.AddText("1" );
            Tracker tracker = trackers[i].trackerSetting.tracker;
            RectCV boundingBox = trackers[i].trackerSetting.boundingBox;
            debugger.AddText("2");
#if ENABLE_WINMD_SUPPORT
            tracker.update(frameMat, boundingBox);
#endif
            if (tracker is TrackerCSRT)
            {

                debugger.AddText(boundingBox.ToString());
         
            }
            
        }

        return true;
    }

}
