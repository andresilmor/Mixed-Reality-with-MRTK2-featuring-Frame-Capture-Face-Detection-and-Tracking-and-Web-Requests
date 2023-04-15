using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleView : UIView {
    protected override UIWindow _window {
        get { 
            return _window;
        }
        set { 
            _window = value; 
        }
    }

    public override void Bind(UIWindow window) {
        if (UIManager.ValidateWindow(window.WindowType, WindowType.Txt_Pag_Cl_00)) {
            _window = window;
            BindActions();
            SetTexts();
            SetMaterials();

        }

    }

    protected override void BindActions() {
        throw new System.NotImplementedException();
    }

    protected override void SetMaterials() {
    }

    protected override void SetTexts() {
        throw new System.NotImplementedException();
    }

    protected override void UnbindActions() {
        throw new System.NotImplementedException();
    }
}
