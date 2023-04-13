using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

[CreateAssetMenu(menuName = "Scriptable Object/Button Visual Matertial List")]
public class ButtonVisualMaterialScriptableObject : ScriptableObject {

    [System.Serializable]
    public struct data {
        public string name;
        public Material material;

    }

    [SerializeField] data[] _buttonMaterial;
    public data[] ButtonMaterial {
        get {
            return _buttonMaterial;
        }
    }

}
