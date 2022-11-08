using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacient
{
    public PacientMark pacientMark { get; }
    public TrackerHandler trackerHandler { get; }

    private string _id = "";
    public string id
    {
        get { return _id; }
        set
        {
            if (id.Length <= 0)
                _id = value;
        }
    }

    /*
    public Pacient(Tracker tracker) : base(tracker)
    {
    }

    public Pacient(legacy_TrackerMOSSE tracker) : base(tracker)
    {
    }
    */

    public Pacient(PacientMark personMarker, legacy_TrackerCSRT tracker) 
    {
        this.pacientMark = personMarker;
        this.trackerHandler = new TrackerHandler(tracker);
    }


    public void UpdateEmotion(string emotion)
    {
        pacientMark.UpdateActiveEmotion(emotion);
    }


}
