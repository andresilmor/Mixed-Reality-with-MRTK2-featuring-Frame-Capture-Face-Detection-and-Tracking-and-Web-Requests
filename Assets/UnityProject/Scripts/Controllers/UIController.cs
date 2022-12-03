using Microsoft.MixedReality.Toolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MixedRealitySceneContent))]
[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    private static UIController _instance = null;
    public static UIController Instance
    {
        get { return _instance; }
        set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else
            {
                Destroy(value);
            }
        }
    }

    [SerializeField] GraphicUserInterfaceScriptableObject graphicUserInterface;
    [SerializeField] GameObject uiPool;

    private List<UIStacker> UIStackers = new List<UIStacker>();
    private Dictionary<string, List<UIWindow>> WindowPool = new Dictionary<string, List<UIWindow>>();

    public UIStacker OpenWindow(string toOpen, UIStacker stacker = null, string stackerName = "")
    {
        UIWindow window = WindowPool.ContainsKey(toOpen) ? WindowPool[toOpen].First() : null;

        Vector3 position = AppCommandCenter.cameraMain.transform.position;
        position.z += 0.50f;

        if (stacker is null) {
            GameObject newGameObject = new GameObject(stackerName);
            newGameObject.transform.position = position;
            newGameObject.transform.rotation = Quaternion.identity;
            newGameObject.transform.parent = this.gameObject.transform;
            stacker = newGameObject.AddComponent<UIStacker>();

        }

        if (!window) {
            foreach (var data in graphicUserInterface.windows) {
                if (data.name.Equals(toOpen))
                    window = Instantiate(data.window, position, Quaternion.identity, stacker.gameObject.transform).GetComponent<UIWindow>();

            }

        } else {
            WindowPool[toOpen].Remove(window);
            window.gameObject.transform.SetParent(stacker.gameObject.transform, true);

        }

        window.stacker = stacker;
        window.designation = toOpen;
        stacker.PushWindow(window);

        return stacker;

    }

    private void CloseWindow(UIStacker stacker)
    {


        bool destroyStacker = stacker.PopWindow(out UIWindow windowToPool);

        if (windowToPool != null) { 
            windowToPool.gameObject.transform.SetParent(uiPool.transform);

            if (!WindowPool.ContainsKey(windowToPool.designation))
                WindowPool.Add(windowToPool.designation, new List<UIWindow>());

            WindowPool[windowToPool.designation].Add(windowToPool);

        }

        if (destroyStacker)
            Destroy(stacker.gameObject);

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

    public async void LoginQR() {
        await AccountController.LoginQR();

    }



    
}
