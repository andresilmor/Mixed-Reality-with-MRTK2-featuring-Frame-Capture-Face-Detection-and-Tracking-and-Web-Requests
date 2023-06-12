using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

using Debug = MRDebug;
using System;
using System.Linq;

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

    private static bool _faceReconActive = false;
    public bool FaceReconActive {
        get { return _faceReconActive; }
    }

    void Start() {
        _scanBtn.OnClick.RemoveAllListeners();
        _passiveModeOnBtn.OnClick.RemoveAllListeners();
        _passiveModeOffBtn.OnClick.RemoveAllListeners();

        _scanBtn.OnClick.AddListener(async () => {
            if (_faceReconActive)
                return;

            _faceReconActive = true;

            Debug.Log("- Test 5 -");
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
                    getLatestFrameTask = MediaCaptureManager.GetLatestFrame(async (object sender, FrameArrivedEventArgs e) => OneShotFaceRecon(sender, e));
#endif
                    getLatestFrameTask.Wait();

                } while (getLatestFrameTask == null || getLatestFrameTask.Result == null);
                Debug.Log("4");
                Debug.Log("Tentatives: " + tentatives);
               
                Debug.Log("5");

            });

            Debug.Log("- Test End -");

        });

        _passiveModeOnBtn.OnClick.AddListener(async () => {
            if (_faceReconActive)
                return;

            _faceReconActive = true;

#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.InitializeMediaFrameReaderAsync();

#endif
            MediaCaptureManager.PassiveServicesUsing += 1;

        });

        _passiveModeOffBtn.OnClick.AddListener(async () => {
            if (!_faceReconActive)
                return;

            _faceReconActive = false;

#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.StopMediaFrameReaderAsync();

#endif

            MediaCaptureManager.PassiveServicesUsing -= 1;

        });

    }

    async private static void OneShotFaceRecon(object sender, FrameArrivedEventArgs args) {
        Debug.Log("OneShotFaceRecon");
        Task thread = Task.Run(async () =>
        {
            try {
                Task<DetectedFaces> evaluateVideoFrameTask = null;
                Debug.Log("OneShotFaceRecon A");
                DetectedFaces result = new DetectedFaces();
                Debug.Log("OneShotFaceRecon B");
#if ENABLE_WINMD_SUPPORT
                Debug.Log("OneShotFaceRecon C");
                evaluateVideoFrameTask = FaceDetectionManager.EvaluateVideoFrameAsync(args.Frame.Bitmap);
                
                Debug.Log("OneShotFaceRecon D");
                evaluateVideoFrameTask.Wait();
#endif

                Debug.Log("OneShotFaceRecon E");
                result = evaluateVideoFrameTask.Result;

                if (result.Faces.Length > 0) {
                    Debug.Log("OneShotFaceRecon F");
                    UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                    {
                        Debug.Log("Number of faces: " + result.Faces.Count());
#if ENABLE_WINMD_SUPPORT
                        FaceDetectionManager.RGBDetectionToWorldspace(result, args.Frame);
#endif
                    }, true);
                }

            } catch (Exception ex) {
                Debug.Log("Exception:" + ex.Message);
            }

        });

        thread.Wait();

        if (MediaCaptureManager.PassiveServicesUsing == 0) {
#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.StopMediaFrameReaderAsync();

#endif

        }

        _faceReconActive = false;


    }

    private void OnEnable() {
        UIManager.Instance.HomeMenu.gameObject.SetActive(false);

    }

}
