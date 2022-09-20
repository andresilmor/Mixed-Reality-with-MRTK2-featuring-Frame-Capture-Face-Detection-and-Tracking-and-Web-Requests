using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacient : Person
{
    public Pacient(Tracker tracker) : base(tracker)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        debugger.AddText("Pacient CSRT created");
    }

    public Pacient(legacy_TrackerMOSSE tracker) : base(tracker)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        debugger.AddText("Pacient MOSSE created");
    }

    public Pacient(legacy_TrackerCSRT tracker) : base(tracker)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        debugger.AddText("Pacient legacy CSRT created");
    }

}
