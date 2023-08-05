using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIView {
    protected abstract UIWindow _window { get; set; }


    abstract public void Bind(UIWindow window); 
    abstract protected void SetTexts(); 
    abstract protected void BindActions(); 
    abstract protected void SetMaterials();
    abstract protected void UnbindActions();

}
