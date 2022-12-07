using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Microsoft.MixedReality.Toolkit.Input.KeyBinding;

[CreateAssetMenu(menuName = "Scriptable Object/Graphic User Interface")]
public class GraphicUserInterfaceScriptableObject : ScriptableObject {

    [System.Serializable]
    public class data {
        public string name;
        public GameObject window;
        public List<windowComponents> components = new List<windowComponents>();
        public Dictionary<string, string> componentsDict = new Dictionary<string, string>();
    }

    [System.Serializable]
    public struct windowComponents {
        public string name;
        public componentType type;
        public string path;
    }



    [SerializeField] data[] _windows;
    public data[] windows {
        get {
            return _windows;
        }
    }

    [SerializeField]
    public enum componentType {
        Text,
        Button
    }


    public void SetupComponentsDictionary() {
        foreach (var i in windows) {
            foreach (var n in i.components) {
                i.componentsDict.Add(n.name, n.path);

            }

        }

    }
}
