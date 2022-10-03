using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.VideoModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacient : Person
{

    private PersonProfile personProfile;

    /*
    public Pacient(Tracker tracker) : base(tracker)
    {
    }

    public Pacient(legacy_TrackerMOSSE tracker) : base(tracker)
    {
    }
    */

  


    public Pacient(PersonProfile personMarker, legacy_TrackerCSRT tracker) : base(tracker)
    {
        this.personProfile = personMarker;

    }

    public void UpdateEmotion(string emotion)
    {
        personProfile.UpdateActiveEmotion(emotion);
    }


}
