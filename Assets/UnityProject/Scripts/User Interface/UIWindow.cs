using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour
{
    public string designation { get; set; }  
    public bool wasInstantiated { get; private set; }
    public UIStacker stacker { get; set; }


    [Header("Events:")]
    [SerializeField] UnityEvent PrePushAction;
    [SerializeField] UnityEvent PostPushAction;
    [SerializeField] UnityEvent PrePopAction;
    [SerializeField] UnityEvent PostPopAction;



    public void Enter(bool wasInstantiated = false)
    {
        PrePushAction?.Invoke();


        this.wasInstantiated = wasInstantiated;

        gameObject.SetActive(true);

        PostPushAction?.Invoke();
    }

    public void Exit()
    {
        PrePopAction?.Invoke(); 

        gameObject.SetActive(false);

        PostPopAction?.Invoke();

    }
}

