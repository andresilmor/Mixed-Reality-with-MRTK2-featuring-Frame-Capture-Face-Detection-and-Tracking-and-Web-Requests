using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour {
    public WindowType windowType { get; set; }
    public bool wasInstantiated { get; private set; }
    public bool isNotification = false;

    public UIStacker stacker { get; set; }

    public Dictionary<string, object> components = new Dictionary<string, object>();
    public Dictionary<string, string> componentsContent = new Dictionary<string, string>();

    [Header("Events:")]
    [SerializeField] UnityEvent PrePushAction;
    [SerializeField] UnityEvent PostPushAction;
    [SerializeField] UnityEvent PrePopAction;
    [SerializeField] UnityEvent PostPopAction;

    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(isNotification ? UIManager.Instance.NotificationClip : UIManager.Instance.OpenWindowClip);

    }

    public void DefineComponents(GraphicUserInterfaceScriptableObject.data uiData) {
        foreach (GraphicUserInterfaceScriptableObject.windowComponents component in uiData.components) {
            switch (component.type) {
                case GUIComponentType.Text:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<TextMeshPro>());
                    break;
                case GUIComponentType.Button:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<Interactable>());
                    break;
                case GUIComponentType.Material:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<MeshRenderer>());
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

    public void Exit(AudioSource closeCallerAudioSource = null) {
        PrePopAction?.Invoke();

        Debug.Log(closeCallerAudioSource != null);
        if (closeCallerAudioSource != null) 
            StartCoroutine(WaitForAudioFinish(closeCallerAudioSource));
        else
            gameObject.SetActive(false);

        PostPopAction?.Invoke();

    }

    private IEnumerator WaitForAudioFinish(AudioSource closeCallerAudioSource) {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitWhile(() => closeCallerAudioSource.isPlaying);
        gameObject.SetActive(false);

    }


    public void UpdateContent(string key, string content) {
        try {
            (components[key] as TextMeshPro).text = content;
        } catch (Exception e) {
            Debug.LogException(e);
        }
    }


}

