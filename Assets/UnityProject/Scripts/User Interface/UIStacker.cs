using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

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
                current.Close();
            }

        }

        WindowStack.Push(window);
        window.Open();

    }


    public bool PopWindow(out UIWindow windowToPool, AudioSource closeCallerAudioSource) {
        if (WindowStack.Count == 1) {
            windowToPool = WindowStack.Pop();
            windowToPool.Close(closeCallerAudioSource);
            return true;

        }

        windowToPool = WindowStack.Pop();
        windowToPool.Close(closeCallerAudioSource);
        UIWindow next = WindowStack.Peek();
        if (true)
            next.Open();
        return false;


    }
}
