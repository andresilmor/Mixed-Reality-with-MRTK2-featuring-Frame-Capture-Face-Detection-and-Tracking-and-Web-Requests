using Microsoft.MixedReality.Toolkit.Diagnostics;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable CloseBtn;
    [SerializeField] Interactable SettingsBtn;
    [SerializeField] Interactable FaceReconBtn;
    [SerializeField] Interactable QRCodeBtn;
    [SerializeField] Interactable DebugBtn;

    [Header("Graphics:")]
    [SerializeField] Image MemoryUsageBar;
    [SerializeField] Image MemoryPeakBar;
    [SerializeField] TextMeshPro FPSCount;

    [Header("Utility:")]
    [SerializeField] HardwareDiagnostic HardwareDiagnostic;

    [Header("Connected Menus:")]
    [SerializeField] QRCodeMenu QRCodeMenu;
    [SerializeField] DebugMenu DebugMenu;


    void Start()
    {
        if (HardwareDiagnostic != null) {
            HardwareDiagnostic.OnNewDiagnostic += (object sender, DiagnosticData e) => {
                if (!gameObject.activeInHierarchy)
                    return; 

                FPSCount.text = e.cpuFrameRate.ToString();
            
                MemoryUsageBar.fillAmount = e.memoryUsage / e.memoryLimit;
                MemoryPeakBar.fillAmount = e.memoryPeak / e.memoryLimit;

            };

        }


        QRCodeBtn.OnClick.AddListener(() => {
            QRCodeMenu.gameObject.SetActive(!QRCodeMenu.gameObject.activeInHierarchy);

            Vector3 position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            QRCodeMenu.gameObject.transform.position = position;
            QRCodeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
            //HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform);   
        });



        DebugBtn.OnClick.AddListener(() => {
            DebugMenu.gameObject.SetActive(!DebugMenu.gameObject.activeInHierarchy);

            Vector3 position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            DebugMenu.gameObject.transform.position = position;
            DebugMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
            //HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform);   
        });

    }

}
