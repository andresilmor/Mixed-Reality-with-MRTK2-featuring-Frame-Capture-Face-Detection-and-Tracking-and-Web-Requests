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

public class LoginView : UIView {
    protected override UIWindow _window { 
        get {
            return _window; 
        } 
        set {
            _window = value;
        } 
    }

    public override void Bind(UIWindow window) {
        if (UIManager.ValidateWindow(window.WindowType, WindowType.H_2btn_00)) {
            _window = window;
            BindActions();
            BindTexts();
            BindMaterials();

        }

    }
    protected override void BindActions() {
        AccountManager.loginWindow = _window;

        (_window.components["BotButton"] as Interactable).OnClick.AddListener(() => {
            System.Threading.Tasks.Task<bool> task = AccountManager.LoginQR();
        });

       

    }

    protected override void BindMaterials() {   }

    protected override void BindTexts() {
        (_window.components["Title"] as TextMeshPro).text = "Welcome Caregiver";
        (_window.components["Subtitle"] as TextMeshPro).text = "Select Login Method";
        (_window.components["TopButtonText"] as TextMeshPro).text = "Keyboard";
        (_window.components["BotButtonText"] as TextMeshPro).text = "QR Code";
        
    }

}
