using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable _closeBtn;
    [SerializeField] Interactable _settingsBtn;
    [SerializeField] Interactable _faceReconBtn;
    [SerializeField] Interactable _QRCodeBtn;
    [SerializeField] Interactable _debugBtn;

    [Header("Graphics:")]
    [SerializeField] Image _memoryUsageBar;
    [SerializeField] Image _memoryPeakBar;
    [SerializeField] TextMeshPro _FPSCount;

    [Header("Utility:")]
    [SerializeField] HardwareDiagnostic _hardwareDiagnostic;


    void Start()
    {
        if (_hardwareDiagnostic != null) {
            _hardwareDiagnostic.OnNewDiagnostic += (object sender, DiagnosticData e) => {
                if (!gameObject.activeInHierarchy)
                    return; 

                _FPSCount.text = e.cpuFrameRate.ToString();
            
                _memoryUsageBar.fillAmount = e.memoryUsage / e.memoryLimit;
                _memoryPeakBar.fillAmount = e.memoryPeak / e.memoryLimit;

            };

        }


        _QRCodeBtn.OnClick.AddListener(() => {
            UIManager.Instance.QRCodeMenu.gameObject.SetActive(!UIManager.Instance.QRCodeMenu.gameObject.activeInHierarchy);
            UIManager.Instance.QRCodeMenu.gameObject.transform.position = UIManager.GetPositionInFront();
            UIManager.Instance.QRCodeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
       
        });

        _faceReconBtn.OnClick.AddListener(() => {
            UIManager.Instance.FaceReconMenu.gameObject.SetActive(!UIManager.Instance.FaceReconMenu.gameObject.activeInHierarchy);
            UIManager.Instance.FaceReconMenu.gameObject.transform.position = UIManager.GetPositionInFront();
            UIManager.Instance.FaceReconMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);

        });

        _debugBtn.OnClick.AddListener(() => {
            UIManager.Instance.DebugMenu.gameObject.SetActive(!UIManager.Instance.DebugMenu.gameObject.activeInHierarchy);
            UIManager.Instance.DebugMenu.gameObject.transform.position = UIManager.GetPositionInFront();
            UIManager.Instance.DebugMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);

        });

    }
}
