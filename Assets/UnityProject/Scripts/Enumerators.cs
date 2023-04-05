using UnityEngine;

[SerializeField]
public enum GUIComponentType {
    Text,
    Button,
    Material,
    MeshRenderer,
    Generic

}

[SerializeField]
public enum WindowType {
    TD_1btn_Cl_00,
    H_2btn_00,
    Sp_ML_E_1btn_Pacient,
    Txt_Pag_Cl_00

}

[SerializeField]
public enum TrackerType {
    PacientTracker

}

[SerializeField]
public enum DetectionType {
    Person

}

[SerializeField]
public enum ColorFormat {
    Grayscale,
    RGB,
    Unknown

}

[SerializeField]
public enum LogType {
    Info,
    Warning,
    Error,
    Fatal,
    Exception

}


