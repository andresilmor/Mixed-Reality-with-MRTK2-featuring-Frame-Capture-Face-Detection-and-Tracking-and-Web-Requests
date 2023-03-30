using UnityEngine;

[SerializeField]
public enum GUIComponentType {
    Text,
    Button,
    Material,
    MeshRenderer,
    Generic

}

public enum WindowType {
    HeaderOneButtonAndClose,
    HeaderTwoButtons00,
    PacientMarker

}

public enum TrackerType {
    PacientTracker

}

public enum DetectionType {
    Person

}


public enum ColorFormat {
    Grayscale,
    RGB,
    Unknown

}


public enum LogType {
    Info,
    Warning,
    Error,
    Fatal,
    Exception

}


