using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRCodeMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable ScanBtn;
    [SerializeField] Interactable PassiveModeOnBtn;
    [SerializeField] Interactable PassiveModeOffBtn;


    void Start()
    {
        ScanBtn.OnClick.RemoveAllListeners();
        PassiveModeOnBtn.OnClick.RemoveAllListeners();
        PassiveModeOffBtn.OnClick.RemoveAllListeners();

        ScanBtn.OnClick.AddListener(() => {
            QRCodeCVManager.DetectQRCodes((List<QRCodeCVManager.QRCodeDetected> detectedCode) => {
                foreach (QRCodeCVManager.QRCodeDetected detected in detectedCode) { 
                    Debug.Log(detected.Info);
                }

            }, 0);

        });

        PassiveModeOnBtn.OnClick.AddListener(() => {
            QRCodeCVManager.DetectQRCodes((List<QRCodeCVManager.QRCodeDetected> detectedCode) => {
                foreach (QRCodeCVManager.QRCodeDetected detected in detectedCode) {
                    Debug.Log(detected.Info);
                }

            });
        });

        PassiveModeOffBtn.OnClick.AddListener(() => {
            QRCodeCVManager.DeactivatePassiveMode();
        });

    }

    private void OnEnable() {
        UIManager.Instance.HomeMenu.gameObject.SetActive(false);

    }

}
