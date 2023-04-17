using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Events;

using Debug = MRDebug;


[RequireComponent(typeof(MixedRealitySceneContent))]
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

    [SerializeField] GameObject uiPool;

    [Header("Menus")]
    [SerializeField] public HandMenu HandMenu;
    [SerializeField] public HomeMenu HomeMenu;

    [Header("Scriptable Objects:")]
    [SerializeField] GraphicUserInterfaceScriptableObject graphicUserInterface;
    [SerializeField] ButtonVisualMaterialScriptableObject circleButtonsMaterial;
    [SerializeField] ButtonVisualMaterialScriptableObject rectangleButtonsMaterial;

    [Header("Audio Clips:")]
    [SerializeField] public AudioClip OpenWindowClip;
    [SerializeField] public AudioClip CloseWindowClip;
    [SerializeField] public AudioClip NotificationClip;

    private List<UIStacker> UIStackers = new List<UIStacker>();
    private Dictionary<WindowType, List<UIWindow>> WindowPool = new Dictionary<WindowType, List<UIWindow>>();


    


    void Awake() {
        Instance = this;

    }

    void Start() {
        graphicUserInterface.SetupComponentsDictionary();


        //AppCommandCenter.StartApplication();

    }

    public ButtonVisualMaterialScriptableObject.ButtonStatus? GetCircleButtonMaterial(string id) {
        foreach (ButtonVisualMaterialScriptableObject.Data data in circleButtonsMaterial.ButtonMaterial) {
            if (data.Name.Equals(id))
                return data.ButtonStatus;

        }

        return null;

    }

    public UIWindow OpenWindow(WindowType toOpen, UIView uiView, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {

        Vector3 position;
        if (stacker is null) {
            //position = AppCommandCenter.CameraMain.transform.position;
            //position.z += AxisZOffset;
            //position.y += AxisYOffset;

            position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            UIStackers.Add(stacker);

        } else {
            position = stacker.GetActiveWindowPosition();

        }

        return InstantiateWindow(toOpen, uiView, stacker, position, Quaternion.identity, isNotification);

    }

    public UIWindow OpenWindowAt(WindowType toOpen, Vector3? position, Quaternion rotation, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {
        if (stacker is null) {
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            UIStackers.Add(stacker);

        } 

        return InstantiateWindow(toOpen, null, stacker, position, rotation, isNotification);

    }

    private UIWindow InstantiateWindow(WindowType toOpen, UIView uiView, UIStacker stacker, Vector3? position, Quaternion? rotation, bool isNotification = false) {
        UIWindow window = WindowPool.ContainsKey(toOpen) ? WindowPool[toOpen].First() : null;
        if (!window) {
            foreach (var data in graphicUserInterface.windows) {
                if (data.windowType.Equals(toOpen)) {
                    window = Instantiate(data.window, position is null ? Vector3.zero : (Vector3)position, (Quaternion)rotation, stacker.gameObject.transform).GetComponent<UIWindow>();

                    if (rotation is null)
                        window.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);

                    window.gameObject.transform.localScale *= ViewScale;

                    window.WindowType = toOpen;
                    window.BindedView = uiView;
                    window.DefineComponents(data);
                    window.BindedView.Bind(window);

                    break;

                }

            }

        } else {
            WindowPool[toOpen].Remove(window);
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
            windowToPool.gameObject.transform.SetParent(uiPool.transform);

            if (!WindowPool.ContainsKey(windowToPool.WindowType))
                WindowPool.Add(windowToPool.WindowType, new List<UIWindow>());

            WindowPool[windowToPool.WindowType].Add(windowToPool);

        }

        if (destroyStacker) { 
            Destroy(stacker.gameObject);
            UIStackers.Remove(stacker);

        }

    }

    public static bool ValidateWindow(WindowType windowType, WindowType typeRequired) {
        if (!windowType.Equals(typeRequired)) { // Select here the interface "needed"
            Debug.Log("Not validated?");
            return false;

        }

        return true;

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
