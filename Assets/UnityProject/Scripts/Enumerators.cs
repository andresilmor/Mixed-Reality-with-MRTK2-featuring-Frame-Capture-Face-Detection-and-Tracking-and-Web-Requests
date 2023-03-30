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
    HeaderOneButtonAndClose,
    HeaderTwoButtons00,
    PacientMarker,
    DebugConsole

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


