using Microsoft.MixedReality.Toolkit.Examples.Demos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Debug = MRDebug;

public class TimedEventHandler {
    public DateTime timerStart { get; private set; }
    public DateTime timerEnd { get; private set; }

    public double secondsToFinish { get; private set; }

    private Action finishedAction = null;

    public bool TimeRunOut { get; private set; }

    public Coroutine timerCoroutine { get; private set; }
    public Coroutine timerDisplayCoroutine { get; private set; }


    public TimedEventHandler(DateTime timerEnd, Action action, DateTime? timerStart = null) {
        this.timerStart = timerStart is null ? DateTime.Now : (DateTime)timerStart;
        this.timerEnd = timerEnd;

        finishedAction = action;
        TimeRunOut = false;

        timerCoroutine = AppCommandCenter.Instance.StartCoroutine(Timer());


    }

    private IEnumerator Timer() {
        DateTime start = DateTime.Now;
        this.secondsToFinish = (this.timerEnd - start).TotalSeconds;
        Debug.Log("Timer Start");
        yield return new WaitForSeconds(Convert.ToSingle(this.secondsToFinish));
        Debug.Log("Timer Over");
        TimeRunOut = true;
        finishedAction.Invoke();

    }

    public string GetTimeLeft() {
        TimeSpan timeLeft = this.timerEnd - DateTime.Now;
        string text = "";

        if (timeLeft.TotalSeconds > 1) {
            if (timeLeft.Days != 0)
                text += timeLeft.Days + "d ";

            if (timeLeft.Hours != 0)
                text += timeLeft.Hours + "h ";

            if (timeLeft.Minutes != 0) {
                TimeSpan ts = TimeSpan.FromSeconds(timeLeft.TotalSeconds);
                text += ts.Minutes + "m ";

            }

            if (timeLeft.Seconds != 0) {
                TimeSpan ts = TimeSpan.FromSeconds(timeLeft.TotalSeconds);
                text += ts.Seconds + "s";

            }

            return text;

        } else {
            Debug.Log("Time Out");
            return text;

        }

    }

    public int GetSeconds() {
        return (this.timerEnd - DateTime.Now).Seconds;

    }

    public IEnumerator DisplayTime() {
        TimeSpan timeLeft = this.timerEnd - DateTime.Now;

        double totalSecondsLeft = timeLeft.TotalSeconds;
        double totalSeconds = (this.timerEnd - this.timerStart).TotalSeconds;

        string text = "";

        // If using slide: slideObjec.value = 1 - Convert.ToSingle((this.timerEnd - DateTime.utcNow).TotalSeconds/

        while (true) //Change condition to when windows is opend or alike
        {
            if (totalSecondsLeft > 1) {
                if (timeLeft.Days != 0) {
                    text += timeLeft.Days + "d ";
                    text += timeLeft.Hours + "h";
                    yield return new WaitForSeconds(timeLeft.Minutes * 60);

                } else if (timeLeft.Hours != 0) {
                    text += timeLeft.Hours + "h ";
                    text += timeLeft.Minutes + "m";
                    yield return new WaitForSeconds(timeLeft.Seconds * 60);

                } else if (timeLeft.Minutes != 0) {
                    TimeSpan ts = TimeSpan.FromSeconds(totalSecondsLeft);
                    text += ts.Minutes + "m ";
                    text += ts.Seconds + "s";

                } else {
                    text += Mathf.FloorToInt((float)totalSecondsLeft) + "s";

                }

                Debug.Log(text);

                totalSecondsLeft -= Time.deltaTime;

                yield return null;

            } else {
                break;
                //time is out
            }



        }

        yield return null;


    }






}
