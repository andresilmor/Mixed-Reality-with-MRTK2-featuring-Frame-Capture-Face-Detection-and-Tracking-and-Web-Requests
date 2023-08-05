using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Debug = XRDebug;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get { 
            if (_instance is null)
            {
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                obj.hideFlags = HideFlags.HideAndDontSave; 
                _instance = obj.AddComponent<T>();

            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance is null)
        {
            _instance = this as T;
            DontDestroyOnLoad(_instance);
        }
        else
            Destroy(this);
    }
}
