using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class UIWindow : MonoBehaviour {
    public WindowType windowType { get; set; }
    public bool wasInstantiated { get; private set; }
    public bool isNotification = false;

    private Vector3? moveTo = null;

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
        if (audioSource != null) 
            audioSource.PlayOneShot(isNotification ? UIManager.Instance.NotificationClip : UIManager.Instance.OpenWindowClip);

    }

    public void SetPosition(Vector3 position, bool lookToCamera = true, bool instantMove = true) {
        //Debugger.AddText("Setting position to: " + position.ToString());
        if (instantMove) {
            moveTo = null;
            gameObject.transform.position = position; 
            //Debugger.AddText("instant move");
            if (lookToCamera)
                LookToCamera();

            return;

        }
        moveTo = position;

    }

    public void LookToCamera() {
        gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform.position);

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
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<Material>());
                    break;
                case GUIComponentType.MeshRenderer:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject.GetComponent<MeshRenderer>());
                    break;
                case GUIComponentType.Generic:
                    components.Add(component.name, gameObject.transform.Find(component.path).gameObject);
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

    void Update() {
        if (moveTo != null && !gameObject.transform.position.Equals(moveTo))
            gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, (Vector3)moveTo, 2.0f * Time.deltaTime);
        else
            moveTo = null;


    }


}

