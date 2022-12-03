using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Graphic User Interface")]
public class GraphicUserInterfaceScriptableObject : ScriptableObject
{

    [System.Serializable]
    public struct data
    {
        public string name;
        public GameObject window;
        public string[] windowComponents;
    }





    [SerializeField] data[] _windows;
    public data[] windows
    {
        get
        {
            return _windows;
        }
    }
}
