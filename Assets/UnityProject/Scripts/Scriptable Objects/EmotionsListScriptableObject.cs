using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

[CreateAssetMenu(menuName = "Scriptable Object/EmotionsDetected List")]
public class EmotionsListScriptableObject : ScriptableObject {

    [System.Serializable]
    public struct data {
        public string name;
        public string description;
        public Material material;
        public Vector3 localPosition;
        public Vector3 localScale;

    }

    [SerializeField] data[] _categorical;
    public data[] categorical {
        get {
            return _categorical;
        }
    }

}
