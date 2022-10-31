using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacient : Person
{

    public PacientMark pacientMark { get; }

    /*
    public Pacient(Tracker tracker) : base(tracker)
    {
    }

    public Pacient(legacy_TrackerMOSSE tracker) : base(tracker)
    {
    }
    */

    public Pacient(PacientMark personMarker, legacy_TrackerCSRT tracker) : base(tracker)
    {
        this.pacientMark = personMarker;

    }

    public Pacient() : base(null)
    {

    }

    public void UpdateEmotion(string emotion)
    {
        pacientMark.UpdateActiveEmotion(emotion);
    }


}
