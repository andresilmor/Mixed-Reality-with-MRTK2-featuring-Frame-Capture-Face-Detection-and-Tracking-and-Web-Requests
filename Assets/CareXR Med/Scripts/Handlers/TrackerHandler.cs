using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using RectCV = OpenCVForUnity.CoreModule.Rect;

using Debug = XRDebug;

public class TrackerHandler : MonoBehaviour 
{
    TrackerSetting _trackerSettings;
    public TrackerSetting TrackerSettings
    {
        get
        {
            return _trackerSettings;
        }

        set { _trackerSettings = value; }
    }

    public bool Updated = true;

    public string TrackerIdentifier { get; private set; }
    public TrackerType TrackerType { get; private set; }    
    public ITrackerEntity TrackerEntity { get; set; }   
     
    /*
    public TrackerHandler(Tracker tracker) // FOR CSRT
    {

        //_trackerSettings = new TrackerSetting(tracker);

        Debug.Log("TrackerHandler created");
    }


    public TrackerHandler(legacy_TrackerMOSSE tracker) // FOR MOSSE
    {
        //_trackerSettings = new TrackerSetting(tracker);

        Debug.Log("TrackerHandler MOSSE created");
    }


    public TrackerHandler(int id, legacy_TrackerCSRT tracker = null) // FOR Legacy_CSRT
    {
        _trackerSettings = new TrackerSetting(tracker);

        this.id = id;

        Debug.Log("TrackerHandler legacy CSRT created");
    }
    */

    public TrackerHandler(TrackerSetting trackerSetting, TrackerType type, string uuid = "") // FOR Legacy_CSRT
    {
        _trackerSettings = trackerSetting;
        TrackerIdentifier = uuid;
        TrackerType = type;
   
    }

    public void RestartTracker(BoxRect boxRect, Mat newMat) {
        //RectCV region = new RectCV(new Point(boxRect.x1, boxRect.y1), new Point(boxRect.x2, boxRect.y2));
        //Rect2d _region = new Rect2d(region.tl(), region.size());

        Point top = new Point(boxRect.x1, boxRect.y1);
        Point bottom = new Point(boxRect.x2, boxRect.y2);

        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());

        MatOfPoint selectedPointMat = new MatOfPoint(top, bottom);
        OpenCVForUnity.CoreModule.Rect region = Imgproc.boundingRect(selectedPointMat);

        TrackerSettings.tracker.init(newMat, region);

    }

    public OpenCVForUnity.CoreModule.Rect UpdateTracker( OpenCVForUnity.CoreModule.Rect boxRect, Mat newMat) {
        Debug.Log("IM HERE!!!!!!!!");
        Debug.Log("Mat Widht" + newMat.width());
        Debug.Log("Mat Height" + newMat.height());
        OpenCVForUnity.CoreModule.Rect rect = new OpenCVForUnity.CoreModule.Rect();
        bool wasUpdated = TrackerSettings.tracker.update(newMat, rect);
        Debug.Log("STILL HERE!!!!!!!!");
        return wasUpdated ? rect : null;

    }


    public void SetIdentifier(string uuid) {
        TrackerIdentifier = uuid.Trim();
    }


    /*
    public TrackerHandler(legacy_TrackerCSRT tracker) // FOR Legacy_CSRT
    {
        _trackerSettings = new TrackerSetting(tracker);
        id = -1;

        Debug.Log("TrackerHandler legacy CSRT created");
    }

    public void UpdateOneTracker(FaceRectOld faceRect, Mat frame)
    {
        this._trackerSettings.isUpdating = true;

        Point top = new Point(faceRect.x1, faceRect.y1);
        Point bottom = new Point(faceRect.x2, faceRect.y2);

        RectCV region = new RectCV(top, bottom);
        Rect2d _region = new Rect2d(region.tl(), region.size());
        this._trackerSettings.tracker.update(frame, _region);
        this._trackerSettings.isUpdating = false
            ;
    }


    /* Tracker CSRT
    public struct TrackerSetting
    {
        public Tracker tracker;
        public Scalar lineColor;
        public RectCV boundingBox;

        public TrackerSetting(Tracker tracker, Scalar lineColor = null)
        {
            this.tracker = tracker;
            this.lineColor = lineColor == null ? new Scalar(0, 255, 0) : lineColor;
            this.boundingBox = new RectCV();
        }

        public void Dispose()
        {
            if (tracker != null)
            {
                tracker.Dispose();
                tracker = null;
            }
        }
    }
    */

    // Tracker CSRT
    public struct TrackerSetting
    {
        public TrackerCSRT tracker;
        public Scalar lineColor;
        //public Rect2d boundingBox;
        public OpenCVForUnity.CoreModule.Rect boundingBox;
        public int FrameHeight;
        public bool isUpdating;



        public TrackerSetting(TrackerCSRT tracker, Scalar lineColor, OpenCVForUnity.CoreModule.Rect boundingBox, int frameHeight, bool isUpdating = false) {
            this.tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
            this.lineColor = lineColor == null ? new Scalar(0, 255, 0) : lineColor; this.boundingBox = boundingBox ?? throw new ArgumentNullException(nameof(boundingBox));
            FrameHeight = frameHeight;
            this.isUpdating = isUpdating;
        }

        public void Dispose()
        {
            if (tracker != null)
            {
                tracker.Dispose();
                tracker = null;
            }
        }
    }


}
