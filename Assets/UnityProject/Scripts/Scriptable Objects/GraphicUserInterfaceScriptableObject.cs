using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Microsoft.MixedReality.Toolkit.Input.KeyBinding;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

[CreateAssetMenu(menuName = "Scriptable Object/Graphic User Interface")]
public class GraphicUserInterfaceScriptableObject : ScriptableObject {

    [System.Serializable]
    public class data {
        public WindowType windowType;
        public GameObject window;
        public List<windowComponents> components = new List<windowComponents>();
        public Dictionary<string, string> componentsDict = new Dictionary<string, string>();
    }

    [System.Serializable]
    public struct windowComponents {
        public string name;
        public GUIComponentType type;
        public string path;
    }



    [SerializeField] data[] _windows;
    public data[] windows {
        get {
            return _windows;
        }
    }


    public void SetupComponentsDictionary() {
        foreach (var i in windows) {
            foreach (var n in i.components) {
                i.componentsDict.Add(n.name, n.path);

            }

        }

    }
}
