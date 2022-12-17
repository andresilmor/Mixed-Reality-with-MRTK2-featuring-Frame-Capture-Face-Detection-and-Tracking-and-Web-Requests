using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RectCV = OpenCVForUnity.CoreModule.Rect;

public class TrackerHandler : MonoBehaviour 
{
    TrackerSetting _trackerSetting;
    public TrackerSetting trackerSetting
    {
        get
        {
            return _trackerSetting;
        }
    }

    public string TrackerIdentifier { get; private set; }
    public TrackerType TrackerType { get; private set; }    
     
    /*
    public TrackerHandler(Tracker tracker) // FOR CSRT
    {

        //_trackerSetting = new TrackerSetting(tracker);

        Debugger.AddText("TrackerHandler created");
    }


    public TrackerHandler(legacy_TrackerMOSSE tracker) // FOR MOSSE
    {
        //_trackerSetting = new TrackerSetting(tracker);

        Debugger.AddText("TrackerHandler MOSSE created");
    }


    public TrackerHandler(int id, legacy_TrackerCSRT tracker = null) // FOR Legacy_CSRT
    {
        _trackerSetting = new TrackerSetting(tracker);

        this.id = id;

        Debugger.AddText("TrackerHandler legacy CSRT created");
    }
    */

    public TrackerHandler(legacy_TrackerCSRT tracker, TrackerType type, string uuid = "") // FOR Legacy_CSRT
    {
        _trackerSetting = new TrackerSetting(tracker);
        TrackerIdentifier = uuid;
        TrackerType = type;
   
    }

    public void SetIdentifier(string uuid) {
        TrackerIdentifier = uuid.Trim();
    }


    /*
    public TrackerHandler(legacy_TrackerCSRT tracker) // FOR Legacy_CSRT
    {
        _trackerSetting = new TrackerSetting(tracker);
        id = -1;

        Debugger.AddText("TrackerHandler legacy CSRT created");
    }

    public void UpdateOneTracker(FaceRect faceRect, Mat frame)
    {
        this._trackerSetting.isUpdating = true;

        Point top = new Point(faceRect.x1, faceRect.y1);
        Point bottom = new Point(faceRect.x2, faceRect.y2);

        RectCV region = new RectCV(top, bottom);
        Rect2d _region = new Rect2d(region.tl(), region.size());
        this._trackerSetting.tracker.update(frame, _region);
        this._trackerSetting.isUpdating = false
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
