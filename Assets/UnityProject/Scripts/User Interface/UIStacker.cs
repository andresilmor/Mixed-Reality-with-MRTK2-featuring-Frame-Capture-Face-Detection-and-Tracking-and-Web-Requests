using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class UIStacker : MonoBehaviour {
    private Stack<UIWindow> WindowStack = new Stack<UIWindow>();

    public Vector3 GetActiveWindowPosition() {
        return WindowStack.Peek().gameObject.transform.position;

    }


    public void PushWindow(UIWindow window) {
        if (WindowStack.Count > 0) {
            UIWindow current = WindowStack.Peek();
            if (true) {
                current.Exit();
            }

        }

        WindowStack.Push(window);
        window.Enter();

    }


    public bool PopWindow(out UIWindow windowToPool) {
        if (WindowStack.Count == 1) {
            windowToPool = WindowStack.Pop();
            windowToPool.Exit();
            return true;

        }

        windowToPool = WindowStack.Pop();
        windowToPool.Exit();
        UIWindow next = WindowStack.Peek();
        if (true)
            next.Enter();
        return false;


    }
}
