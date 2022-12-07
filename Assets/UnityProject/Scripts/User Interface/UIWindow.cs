using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour {
    public string designation { get; set; }
    public bool wasInstantiated { get; private set; }
    public UIStacker stacker { get; set; }

    public Dictionary<string, object> components = new Dictionary<string, object>();
    public Dictionary<string, string> componentsContent = new Dictionary<string, string>();


    [Header("Events:")]
    [SerializeField] UnityEvent PrePushAction;
    [SerializeField] UnityEvent PostPushAction;
    [SerializeField] UnityEvent PrePopAction;
    [SerializeField] UnityEvent PostPopAction;

    public void DefineComponents(GraphicUserInterfaceScriptableObject.data uiData) {


        foreach (GraphicUserInterfaceScriptableObject.windowComponents component in uiData.components) {
            switch (component.type) {
                case GraphicUserInterfaceScriptableObject.componentType.Text:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<TextMeshPro>());
                    break;
                case GraphicUserInterfaceScriptableObject.componentType.Button:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<Interactable>());
                    break;

            }

        }

    }


    public void Enter(bool wasInstantiated = false) {
        PrePushAction?.Invoke();


        this.wasInstantiated = wasInstantiated;

        gameObject.SetActive(true);

        PostPushAction?.Invoke();
    }

    public void Exit() {
        PrePopAction?.Invoke();

        gameObject.SetActive(false);

        PostPopAction?.Invoke();

    }

    public void UpdateContent(string key, string content) {
        try {
            (components[key] as TextMeshPro).text = content;
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }


}

