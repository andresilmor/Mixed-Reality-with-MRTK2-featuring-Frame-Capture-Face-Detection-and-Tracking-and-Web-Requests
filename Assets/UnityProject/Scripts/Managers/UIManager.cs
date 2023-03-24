using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] GraphicUserInterfaceScriptableObject graphicUserInterface;
    [SerializeField] GameObject uiPool;

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

    public UIWindow OpenWindow(WindowType toOpen, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {

        Vector3 position;
        if (stacker is null) {
            position = AppCommandCenter.cameraMain.transform.position;
            position.z += 0.40f;
            position.y += -0.105f;

            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            UIStackers.Add(stacker);

        } else {
            position = stacker.GetActiveWindowPosition();

        }

        return InstantiateWindow(toOpen, stacker, position, Quaternion.identity, isNotification);

    }

    public UIWindow OpenWindowAt(WindowType toOpen, Vector3? position, Quaternion rotation, UIStacker stacker = null, string stackerName = "", bool isNotification = false) {
        if (stacker is null) {
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            UIStackers.Add(stacker);

        } 

        return InstantiateWindow(toOpen, stacker, position, rotation, isNotification);

    }

    private UIWindow InstantiateWindow(WindowType toOpen, UIStacker stacker, Vector3? position, Quaternion rotation, bool isNotification = false) {
        UIWindow window = WindowPool.ContainsKey(toOpen) ? WindowPool[toOpen].First() : null;
        if (!window) {
            foreach (var data in graphicUserInterface.windows) {
                if (data.windowType.Equals(toOpen)) {
                    window = Instantiate(data.window, position is null ? Vector3.zero : (Vector3)position, rotation, stacker.gameObject.transform).GetComponent<UIWindow>();

                    window.DefineComponents(data);
                    break;

                }

            }

        } else {
            WindowPool[toOpen].Remove(window);
            window.gameObject.transform.SetParent(stacker.gameObject.transform, true);

        }

        window.stacker = stacker;
        window.windowType = toOpen;
        window.isNotification = isNotification;
        stacker.PushWindow(window);

        return window;
    }

    public void CloseWindow(UIStacker stacker, AudioSource closeCallerAudioSource = null) {

        bool destroyStacker = stacker.PopWindow(out UIWindow windowToPool, closeCallerAudioSource);

        if (windowToPool != null) {
            windowToPool.gameObject.transform.SetParent(uiPool.transform);

            if (!WindowPool.ContainsKey(windowToPool.windowType))
                WindowPool.Add(windowToPool.windowType, new List<UIWindow>());

            WindowPool[windowToPool.windowType].Add(windowToPool);

        }

        if (destroyStacker) { 
            Destroy(stacker.gameObject);
            UIStackers.Remove(stacker);

        }

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
