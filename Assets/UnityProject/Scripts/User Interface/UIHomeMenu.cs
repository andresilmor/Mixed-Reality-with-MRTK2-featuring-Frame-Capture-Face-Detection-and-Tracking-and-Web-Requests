using Microsoft.MixedReality.Toolkit.Diagnostics;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHomeMenu : MonoBehaviour
{

    [Header("Buttons Config:")]
    [SerializeField] Interactable CloseButton;
    [SerializeField] Interactable SettingsButtons;
    [SerializeField] Interactable FaceReconButton;
    [SerializeField] Interactable QRCodeButton;

    [Header("Others:")]
    [SerializeField] Image MemoryUsageBar;
    [SerializeField] Image MemoryPeakBar;
    [SerializeField] TextMeshPro FPSCount;

    [Header("Windows:")]
    [SerializeField] MixedRealityToolkitVisualProfiler MRTKVisualProfiler;


    void Start()
    {
        CloseButton.OnClick.AddListener(() => {
            // save any game data here
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        });

        MRTKVisualProfiler.OnNewDiagnostic += (object sender, DiagnosticData e) => {
            

            FPSCount.text = e.cpuFrameRate.ToString();

            //  e.memoryLimit - 1
            //
            
            MemoryUsageBar.fillAmount = e.memoryUsage / e.memoryLimit;
            MemoryPeakBar.fillAmount = e.memoryPeak / e.memoryLimit;

        };


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
