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

    [SerializeField] GraphicUserInterfaceScriptableObject graphicUserInterface;
    [SerializeField] GameObject uiPool;

    private List<UIStacker> UIStackers = new List<UIStacker>();
    private Dictionary<string, List<UIWindow>> WindowPool = new Dictionary<string, List<UIWindow>>();

    void Awake() {
        Instance = this;

    }

    void Start() {
        graphicUserInterface.SetupComponentsDictionary();
        //AppCommandCenter.StartApplication();

    }

    public UIWindow OpenWindow(string toOpen, UIStacker stacker = null, string stackerName = "") {
        UIWindow window = WindowPool.ContainsKey(toOpen) ? WindowPool[toOpen].First() : null;

        Vector3 position;
        if (stacker is null) {   
            position = AppCommandCenter.cameraMain.transform.position;
            position.z += 0.50f;
            position.y += -0.105f;

            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();
            UIStackers.Add(stacker);

        } else {
            position = stacker.GetActiveWindowPosition();

        }

        if (!window) {
            foreach (var data in graphicUserInterface.windows) {
                Debug.Log(data.name);
                Debug.Log(toOpen);
                if (data.name.Equals(toOpen)) {
                    window = Instantiate(data.window, position, Quaternion.identity, stacker.gameObject.transform).GetComponent<UIWindow>();
                    Debug.Log(data is null);
                    Debug.Log(data.name);
                    Debug.Log(data.components.Count);
                    window.DefineComponents(data);
                    break;

                }

            }

        } else {
            WindowPool[toOpen].Remove(window);
            window.gameObject.transform.SetParent(stacker.gameObject.transform, true);

        }

        window.stacker = stacker;
        window.designation = toOpen;
        stacker.PushWindow(window);

        return window;

    }

    public void CloseWindow(UIStacker stacker) {

        bool destroyStacker = stacker.PopWindow(out UIWindow windowToPool);

        if (windowToPool != null) {
            windowToPool.gameObject.transform.SetParent(uiPool.transform);

            if (!WindowPool.ContainsKey(windowToPool.designation))
                WindowPool.Add(windowToPool.designation, new List<UIWindow>());

            WindowPool[windowToPool.designation].Add(windowToPool);

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
