using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Debug = XRDebug;

[CreateAssetMenu(menuName = "Scriptable Object/Button Visual Matertial List")]
public class ButtonVisualMaterialScriptableObject : ScriptableObject {

    [System.Serializable]
    public struct ButtonStatus {
        public Material ActiveMaterial;
        public Material InactiveMaterial;
        public Material SelectedMaterial;
        public Material UnselectedMaterial;
        public Material HoverMaterial;

    }

    [System.Serializable]
    public struct Data {
        public string Name;
        public ButtonStatus ButtonStatus;

    }

    [SerializeField] Data[] _buttonMaterial;
    public Data[] ButtonMaterial {
        get {
            return _buttonMaterial;
        }
    }

}
