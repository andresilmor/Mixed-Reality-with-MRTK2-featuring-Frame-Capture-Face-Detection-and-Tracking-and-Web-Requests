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
    ///<summary>Title | Description block | 1 Basic btn | Close btn</summary>
    TD_1btn_Cl_00,
    ///<summary>Header (Title/Subtitle) | 2 basic button</summary>
    H_2btn_00,
    ///<summary>Special UI | ML Use | Emotion Display | 1 Profile btn</summary>
    Sp_ML_E_1btn_Pacient,
    ///<summary>Large Text Block | Pagination | Close btn</summary>
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

[SerializeField]
public enum DetectionMode {
    OneShot,
    Passive,
    Timing

}


