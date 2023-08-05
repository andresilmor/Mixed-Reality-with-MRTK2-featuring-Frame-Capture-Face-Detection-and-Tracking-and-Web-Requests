using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Debug = XRDebug;

public static class TimedEventManager {
    private static Dictionary<string, TimedEventHandler> _timers = new Dictionary<string, TimedEventHandler>();


    public static void AddUpdateTimedEvent(string key, TimedEventHandler timedEvent) {
        if (_timers.ContainsKey(key)) {
            Controller.Instance.StopCoroutine(_timers[key].timerCoroutine);
            _timers[key] = timedEvent;
            return;

        }

        _timers.Add(key, timedEvent);   

    }

    public static bool StopTimedEvent(string key) {
        if (_timers.ContainsKey(key)) {
            Controller.Instance.StopCoroutine(_timers[key].timerCoroutine);
            _timers.Remove(key);
            return true;

        }
        
        return false;
    
    }

    public static string GetTimedEventTimeLeft(string key) {
        if (_timers.ContainsKey(key))
            return _timers[key].GetTimeLeft();

        return null;

    }

    public static TimedEventHandler GetTimedEvent(string key) {
        if (_timers.ContainsKey(key))
            return _timers[key];

        return null;

    }

    public static Dictionary<string, TimedEventHandler> GetTimers(int limit) { 
        Dictionary<string, TimedEventHandler> timers = _timers;

        return timers.OrderBy(c => c.Value.GetSeconds()).Take(limit).ToDictionary(c => c.Key, c => c.Value);

    }

}
