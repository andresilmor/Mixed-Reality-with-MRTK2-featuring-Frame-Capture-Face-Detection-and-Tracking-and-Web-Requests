using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RectCV = OpenCVForUnity.CoreModule.Rect;

public class TrackerHandler : MonoBehaviour 
{
    TrackerSetting _trackerSettings;
    public TrackerSetting TrackerSettings
    {
        get
        {
            return _trackerSettings;
        }
    }

    public string TrackerIdentifier { get; private set; }
    public TrackerType TrackerType { get; private set; }    
    public ITrackerEntity TrackerEntity { get; set; }   
     
    /*
    public TrackerHandler(Tracker tracker) // FOR CSRT
    {

        //_trackerSettings = new TrackerSetting(tracker);

        Debugger.AddText("TrackerHandler created");
    }


    public TrackerHandler(legacy_TrackerMOSSE tracker) // FOR MOSSE
    {
        //_trackerSettings = new TrackerSetting(tracker);

        Debugger.AddText("TrackerHandler MOSSE created");
    }


    public TrackerHandler(int id, legacy_TrackerCSRT tracker = null) // FOR Legacy_CSRT
    {
        _trackerSettings = new TrackerSetting(tracker);

        this.id = id;

        Debugger.AddText("TrackerHandler legacy CSRT created");
    }
    */

    public TrackerHandler(legacy_TrackerCSRT tracker, TrackerType type, string uuid = "") // FOR Legacy_CSRT
    {
        _trackerSettings = new TrackerSetting(tracker);
        TrackerIdentifier = uuid;
        TrackerType = type;
   
    }

    public void RestartTracker(BoxRect boxRect, Mat newMat) {
        RectCV region = new RectCV(new Point(boxRect.x1, boxRect.y1), new Point(boxRect.x2, boxRect.y2));
        Rect2d _region = new Rect2d(region.tl(), region.size());

        TrackerSettings.tracker.init(newMat, _region);

    }

    public void UpdateTracker(Rect2d boxRect, Mat newMat) {
        Debugger.AddText("IM HERE!!!!!!!!");
        Debugger.AddText("Mat Widht" + newMat.width());
        Debugger.AddText("Mat Height" + newMat.height());
        TrackerSettings.tracker.update(newMat, boxRect);
        Debugger.AddText("STILL HERE!!!!!!!!");

    }


    public void SetIdentifier(string uuid) {
        TrackerIdentifier = uuid.Trim();
    }


    /*
    public TrackerHandler(legacy_TrackerCSRT tracker) // FOR Legacy_CSRT
    {
        _trackerSettings = new TrackerSetting(tracker);
        id = -1;

        Debugger.AddText("TrackerHandler legacy CSRT created");
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
        public legacy_TrackerCSRT tracker;
        public Scalar lineColor;
        public Rect2d boundingBox;
        public bool isUpdating;

        public TrackerSetting(legacy_TrackerCSRT tracker, Scalar lineColor = null)
        {
            this.tracker = tracker;
            this.lineColor = lineColor == null ? new Scalar(0, 255, 0) : lineColor;
            this.boundingBox = new Rect2d();
            this.isUpdating = false;
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
