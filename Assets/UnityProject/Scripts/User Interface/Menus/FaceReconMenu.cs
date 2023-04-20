using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FaceReconMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable _closeBtn;
    [SerializeField] Interactable _settingsBtn;
    [SerializeField] Interactable _scanBtn;
    [SerializeField] Interactable _listBtn;
    [SerializeField] Interactable _passiveModeOnBtn;
    [SerializeField] Interactable _passiveModeOffBtn;
    [SerializeField] Interactable _emotionIconsOnBtn;
    [SerializeField] Interactable _emotionIconsOffBtn;
    [SerializeField] Interactable _profileBtnOnBtn;
    [SerializeField] Interactable _profileBtnOffBtn;

    [Header("Mesh:")]
    [SerializeField] MeshRenderer _settingsMesh;

    [Header("Dettached Windows:")]
    [SerializeField] GameObject _settingsWindow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
