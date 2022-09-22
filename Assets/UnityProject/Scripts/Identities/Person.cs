using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RectCV = OpenCVForUnity.CoreModule.Rect;

abstract public class Person : BinaryTree.NodeType
{
    TrackerSetting _trackerSetting;
    public TrackerSetting trackerSetting
    {
        get
        {
            return _trackerSetting;
        }
    }


    public Person(Tracker tracker) // FOR CSRT
    {

        //_trackerSetting = new TrackerSetting(tracker);

        Debugger.AddText("Person created");
    }
    public Person(legacy_TrackerMOSSE tracker) // FOR MOSSE
    {
        //_trackerSetting = new TrackerSetting(tracker);

        Debugger.AddText("Person MOSSE created");
    }
    public Person(legacy_TrackerCSRT tracker = null) // FOR Legacy_CSRT
    {
        _trackerSetting = new TrackerSetting(tracker);

        Debugger.AddText("Person legacy CSRT created");
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

    // Tracker MOSSE
    public struct TrackerSetting
    {
        public legacy_Tracker tracker;
        public Scalar lineColor;
        public Rect2d boundingBox;

        public TrackerSetting(legacy_Tracker tracker, Scalar lineColor = null)
        {
            this.tracker = tracker;
            this.lineColor = lineColor == null ? new Scalar(0, 255, 0) : lineColor;
            this.boundingBox = new Rect2d();
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

    public struct BoundingBoxXYWH
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public string label;
        public Color color;
    }

}
