using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour
{
    public string designation { get; set; }  
    public bool wasInstantiated { get; private set; }
    public UIStacker stacker { get; set; }

    public void Enter(bool wasInstantiated = false)
    {
        this.wasInstantiated = wasInstantiated;

        gameObject.SetActive(true);
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}

