using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedEventHandler : MonoBehaviour
{
    private bool inProgress;
    private DateTime timeStart;
    private DateTime timeEnd;

    public int Years { get; private set; }
    public int Months { get; private set; }
    public int Days { get; private set; }
    public int Hours { get; private set; }
    public int Minutes { get; private set; }
    public int Seconds { get; private set; }

    public TimedEventHandler(DateTime timeEnd, DateTime? timeStart = null)
    {
        this.timeStart = timeStart is null ? DateTime.UtcNow : (DateTime)timeStart;
        this.timeEnd = timeEnd;

        TimeSpan span = new TimeSpan(0, 0, 0, 0);

        inProgress = true;

    }




}
