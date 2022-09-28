using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacient : Person
{

    private PersonMarker personMarker;

    /*
    public Pacient(Tracker tracker) : base(tracker)
    {
    }

    public Pacient(legacy_TrackerMOSSE tracker) : base(tracker)
    {
    }
    */

    public Pacient(PersonMarker personMarker)
    {
       this.personMarker = personMarker;

    }


    public Pacient(PersonMarker personMarker, legacy_TrackerCSRT tracker)
    {
        this.personMarker = personMarker;

    }

    public void UpdateEmotion(string emotion)
    {
        return;
        Debugger.AddText("UpdateEmotion");
        personMarker.GetEmotionsHandler().UpdateActiveEmotion(emotion);
    }


}
