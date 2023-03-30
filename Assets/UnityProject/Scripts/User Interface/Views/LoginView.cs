using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

public class LoginView : IUIView
{
    private const WindowType _windowTypeRequired = WindowType.HeaderTwoButtons00;

    private bool ValidWindow(UIWindow window) {
        if (!window.WindowType.Equals(_windowTypeRequired)) {
            Debug.Log("Not validated?");
            return false;

        }

        return true;
    }

    public void Bind(UIWindow window) {
        if (ValidWindow(window)) {
            BindActions(window, true);
            BindTexts(window, true);

        }

    }
 
    public void BindActions(UIWindow window, bool alreadyValidated = false) {
        if (alreadyValidated || ValidWindow(window)) {
            (window.components["BotButton"] as Interactable).OnClick.AddListener(() => {
                System.Threading.Tasks.Task<bool> task = AccountManager.LoginQR();
            });

        }

    }

    public void BindTexts(UIWindow window, bool alreadyValidated = false) {
        if (alreadyValidated || ValidWindow(window)) {
            (window.components["Title"] as TextMeshPro).text = "Welcome Caregiver";
            (window.components["Subtitle"] as TextMeshPro).text = "Select Login Method";
            (window.components["TopButtonText"] as TextMeshPro).text = "Keyboard";
            (window.components["BotButtonText"] as TextMeshPro).text = "QR Code";

        }

    }

}
