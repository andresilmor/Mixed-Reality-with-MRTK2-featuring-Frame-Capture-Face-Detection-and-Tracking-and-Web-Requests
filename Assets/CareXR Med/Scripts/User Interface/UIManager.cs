using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Debug = XRDebug;

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour {
    private static UIManager _instance;
    public static UIManager Instance {
        get { return _instance; }
        set {
            if (_instance == null) {
                _instance = value;
            } else {
                Destroy(value);
            }
        }
    }

    [Header("Config:")]
    [SerializeField] public float ViewScale = 1f;
    [SerializeField] public float WindowDistance = 0.55f;
    [SerializeField] public float AxisZOffset = 0.40f;
    [SerializeField] public float AxisYOffset = 0.30f;

    [SerializeField] GameObject _uiPool;
    [SerializeField] GameObject _uiContainer;

    [Header("Menus")]
    [SerializeField] public HandMenu HandMenu;
    [SerializeField] public HomeMenu HomeMenu;
    [SerializeField] public LoginMenu LoginMenu;
    [SerializeField] public DebugMenu DebugMenu;
    [SerializeField] public QRCodeMenu QRCodeMenu;
    [SerializeField] public FaceReconMenu FaceReconMenu;

    [Header("Scriptable Objects:")]
    [SerializeField] GraphicUserInterfaceScriptableObject _graphicUserInterface;
    [SerializeField] ButtonVisualMaterialScriptableObject _circleButtonsMaterial;
    [SerializeField] ButtonVisualMaterialScriptableObject _rectangleButtonsMaterial;

    [Header("Audio Clips:")]
    [SerializeField] public AudioClip OpenWindowClip;
    [SerializeField] public AudioClip CloseWindowClip;
    [SerializeField] public AudioClip NotificationClip;

    private List<UIStacker> _uiStackers = new List<UIStacker>();
    private Dictionary<WindowType, List<UIWindow>> _windowPool = new Dictionary<WindowType, List<UIWindow>>();


    


    void Awake() {
        Instance = this;

    }

    void Start() {
        _graphicUserInterface.SetupComponentsDictionary();


        //Controller.StartApplication();

    }

    public ButtonVisualMaterialScriptableObject.ButtonStatus? GetCircleButtonMaterial(string id) {
        foreach (ButtonVisualMaterialScriptableObject.Data data in _circleButtonsMaterial.ButtonMaterial) {
            if (data.Name.Equals(id))
                return data.ButtonStatus;

        }

        return null;

    }

    public ButtonVisualMaterialScriptableObject.ButtonStatus? GetRectangleButtonMaterial(string id) {
        foreach (ButtonVisualMaterialScriptableObject.Data data in _rectangleButtonsMaterial.ButtonMaterial) {
            if (data.Name.Equals(id))
                return data.ButtonStatus;

        }

        return null;

    }

    public UIWindow OpenWindow(WindowType toOpen, UIView uiView, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {

        Vector3 position;
        if (stacker is null) {
            //position = Controller.CameraMain.transform.position;
            //position.z += AxisZOffset;
            //position.y += AxisYOffset;

            position = Controller.CameraMain.transform.position + Controller.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = _uiContainer.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            _uiStackers.Add(stacker);

        } else {
            position = stacker.GetActiveWindowPosition();

        }

        return InstantiateWindow(toOpen, uiView, stacker, position, Quaternion.identity, isNotification);

    }

    public UIWindow OpenWindowAt(WindowType toOpen, Vector3? position, Quaternion rotation, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {
        if (stacker is null) {
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = _uiContainer.gameObject.transform  ;
            stacker = newGameObject.AddComponent<UIStacker>();
            _uiStackers.Add(stacker);

        } 

        return InstantiateWindow(toOpen, null, stacker, position, rotation, isNotification);

    }

    private UIWindow InstantiateWindow(WindowType toOpen, UIView uiView, UIStacker stacker, Vector3? position, Quaternion? rotation, bool isNotification = false) {
        UIWindow window = _windowPool.ContainsKey(toOpen) ? _windowPool[toOpen].First() : null;
        if (!window) {
            foreach (var data in _graphicUserInterface.windows) {
                if (data.windowType.Equals(toOpen)) {
                    window = Instantiate(data.window, position is null ? Vector3.zero : (Vector3)position, (Quaternion)rotation, stacker.gameObject.transform).GetComponent<UIWindow>();

                    if (rotation is null)
                        window.gameObject.transform.LookAt(Controller.CameraMain.transform.position);

                    window.gameObject.transform.localScale *= ViewScale;

                    window.WindowType = toOpen;
                    window.BindedView = uiView;
                    window.DefineComponents(data);
                    window.BindedView.Bind(window);

                    break;

                }

            }

        } else {
            _windowPool[toOpen].Remove(window);
            window.gameObject.transform.SetParent(stacker.gameObject.transform, true);

        }

        window.stacker = stacker;
        window.WindowType = toOpen;
        window.isNotification = isNotification;
        stacker.PushWindow(window);

        return window;
    }

    public void CloseWindow(UIStacker stacker, AudioSource closeCallerAudioSource = null) {

        bool destroyStacker = stacker.PopWindow(out UIWindow windowToPool, closeCallerAudioSource);

        if (windowToPool != null) {
            windowToPool.gameObject.transform.SetParent(_uiPool.transform);

            if (!_windowPool.ContainsKey(windowToPool.WindowType))
                _windowPool.Add(windowToPool.WindowType, new List<UIWindow>());

            _windowPool[windowToPool.WindowType].Add(windowToPool);

        }

        if (destroyStacker) { 
            Destroy(stacker.gameObject);
            _uiStackers.Remove(stacker);

        }

    }
    
    public void CloseAllWindows() {
        foreach (UIStacker stacker in _uiStackers)
            CloseWindow(stacker);

    }

    public static bool ValidateWindow(WindowType windowType, WindowType typeRequired) {
        if (!windowType.Equals(typeRequired)) { // Select here the interface "needed"
            Debug.Log("Not validated?");
            return false;

        }

        return true;

    }

    public static Vector3 GetPositionInFront() {
        Vector3 position = Controller.CameraMain.transform.position + Controller.CameraMain.transform.forward * UIManager.Instance.WindowDistance;
        position.y += UIManager.Instance.AxisYOffset;
        return position;
    }


    /*
    public void PopAllWindows() {
        foreach (UIWindow window in WindowStack)
            PopWindow();

        WindowStack.Clear();

    }

    public bool IsWindowInStack(UIWindow window) { 
        return WindowStack.Contains(window);

    }

    public bool IsWindowOnTop(UIWindow window) { 
        return WindowStack.Count > 0 && WindowStack.Peek() == window;
    }
    }
     */




}
