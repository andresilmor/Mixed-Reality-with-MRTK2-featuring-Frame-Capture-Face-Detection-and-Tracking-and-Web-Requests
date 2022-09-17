using OpenCVForUnity.CoreModule;
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

    public Person(Tracker tracker)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();

        _trackerSetting = new TrackerSetting(tracker);

        debugger.AddText("Person created");
    }


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
}
