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

    [Header("Connected Menus:")]
    [SerializeField] QRCodeMenu _QRCodeMenu;
    [SerializeField] DebugMenu _debugMenu;
    [SerializeField] FaceReconMenu _faceReconMenu;


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
            _QRCodeMenu.gameObject.SetActive(!_QRCodeMenu.gameObject.activeInHierarchy);

            Vector3 position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            _QRCodeMenu.gameObject.transform.position = position;
            _QRCodeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
            //HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform);   
        });



        _debugBtn.OnClick.AddListener(() => {
            _debugMenu.gameObject.SetActive(!_debugMenu.gameObject.activeInHierarchy);

            Vector3 position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

            position.y += UIManager.Instance.AxisYOffset;

            _debugMenu.gameObject.transform.position = position;
            _debugMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
            //HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform);   
        });

    }

}
