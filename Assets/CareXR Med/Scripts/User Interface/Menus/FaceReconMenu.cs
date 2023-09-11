using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

using Debug = XRDebug;
using System;
using System.Linq;
using BestHTTP.WebSocket;

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

    // ------------------------------------------------

    public static bool FaceReconActive = false;
   

    void Start() {
        _scanBtn.OnClick.RemoveAllListeners();
        _passiveModeOnBtn.OnClick.RemoveAllListeners();
        _passiveModeOffBtn.OnClick.RemoveAllListeners();

        _scanBtn.OnClick.AddListener((UnityEngine.Events.UnityAction)(async () => {
            if (FaceReconActive)
                return;

            FaceReconActive = true;

            Debug.Log("- Test 6 -");
            await Task.Run(() => {
                Task initializeMediaFrameReaderTask = null;

                Debug.Log("1");
#if ENABLE_WINMD_SUPPORT
                initializeMediaFrameReaderTask = MediaCaptureManager.InitializeMediaFrameReaderAsync();
#endif
                Debug.Log("2");
                initializeMediaFrameReaderTask.Wait();


                Task<CameraFrame> getLatestFrameTask = null;
                
                int tentatives = 0;
                Debug.Log("3");
                do {
                    tentatives += 1;
#if ENABLE_WINMD_SUPPORT
                    getLatestFrameTask = MediaCaptureManager.GetLatestFrame(async (object sender, FrameArrivedEventArgs e) => FaceDetectionManager.OneShotFaceRecognition(sender, e));
#endif
                    getLatestFrameTask.Wait();

                } while (getLatestFrameTask == null || getLatestFrameTask.Result == null);
                Debug.Log("4");
                Debug.Log("Tentatives: " + tentatives);

                Debug.Log("5");

            });

            Debug.Log("- Test End -");

        }));

        _passiveModeOnBtn.OnClick.AddListener((UnityEngine.Events.UnityAction)(async () => {
            if (FaceReconActive)
                return;

            FaceReconActive = true;

#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.InitializeMediaFrameReaderAsync();

#endif
            MediaCaptureManager.PassiveServicesUsing += 1;

        }));

        _passiveModeOffBtn.OnClick.AddListener((UnityEngine.Events.UnityAction)(async () => {
            if (!FaceReconActive)
                return;

            FaceReconActive = false;

#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.StopMediaFrameReaderAsync();

#endif

            MediaCaptureManager.PassiveServicesUsing -= 1;

        }));

    }

    private void OnEnable() {
        UIManager.Instance.HomeMenu.gameObject.SetActive(false);

    }

}
