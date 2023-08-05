using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRCodeMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable _scanBtn;
    [SerializeField] Interactable _passiveModeOnBtn;
    [SerializeField] Interactable _passiveModeOffBtn;


    void Start()
    {
        _scanBtn.OnClick.RemoveAllListeners();
        _passiveModeOnBtn.OnClick.RemoveAllListeners();
        _passiveModeOffBtn.OnClick.RemoveAllListeners();

        _scanBtn.OnClick.AddListener(() => {
            /*
            QRCodeReaderManager.DetectQRCodes((List<QRCodeReaderManager.QRCodeDetected> detectedCode) => {
                foreach (QRCodeReaderManager.QRCodeDetected detected in detectedCode) { 
                    Debug.Log(detected.Info);
                }

            }, 0);
            */

        });

        _passiveModeOnBtn.OnClick.AddListener(() => {
            /*
            QRCodeReaderManager.DetectQRCodes((List<QRCodeReaderManager.QRCodeDetected> detectedCode) => {
                foreach (QRCodeReaderManager.QRCodeDetected detected in detectedCode) {
                    Debug.Log(detected.Info);
                }

            });
            */
        });

        _passiveModeOffBtn.OnClick.AddListener(() => {
        });

    }

    private void OnEnable() {
        UIManager.Instance.HomeMenu.gameObject.SetActive(false);

    }

}
