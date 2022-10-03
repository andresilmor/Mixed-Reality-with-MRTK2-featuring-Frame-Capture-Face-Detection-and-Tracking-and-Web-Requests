using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Emotions List")]
public class EmotionsListScriptableObject : ScriptableObject
{

    [System.Serializable]
    public struct data
    {
        public string name; 
        public string description;  
    }

    [SerializeField] data[] _categorical;
    public data[] categorical { 
        get 
        {
            return _categorical;
        } 
    }

    [SerializeField] data[] _continuous;
    public data[] continuous
    {
        get
        {
            return _continuous;
        }
    }
}
