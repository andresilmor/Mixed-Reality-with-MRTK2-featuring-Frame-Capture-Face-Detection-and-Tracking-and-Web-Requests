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
            BindTexts();
            BindMaterials();

        }

    }

    protected override void BindActions() {
        throw new System.NotImplementedException();
    }

    protected override void BindMaterials() {
    }

    protected override void BindTexts() {
        throw new System.NotImplementedException();
    }
}
