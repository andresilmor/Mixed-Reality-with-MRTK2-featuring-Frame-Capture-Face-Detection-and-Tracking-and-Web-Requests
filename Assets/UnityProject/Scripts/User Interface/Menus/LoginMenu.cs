using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Interactable _QRCodeBtn;
    [SerializeField] Interactable _keyboardBtn;
    [SerializeField] Interactable _loginBtn;
    [SerializeField] Interactable _closeBtn;
    [SerializeField] Interactable _unInputBtn;
    [SerializeField] Interactable _pwInputBtn;

    [Header("Texts")]
    [SerializeField] TextMeshPro _QRCodeText;
    [SerializeField] TextMeshPro _usernameText;
    [SerializeField] TextMeshPro _passwordText;
    [SerializeField] TextMeshPro _loginText;

    [Header("Mesh")]
    [SerializeField] MeshRenderer _keyboardMesh;
    [SerializeField] MeshRenderer _loginMesh;

    [Header("Panels")]
    [SerializeField] GameObject _optionPanel;
    [SerializeField] GameObject _loginFormPanel;

    private TouchScreenKeyboard _keyboard = null;

    bool _validatingLogin = false;
    bool _insertingPassword = false;

    bool _cleanUsername = true;
    bool _cleanPassword = true;

    string _password;

    // Start is called before the first frame update
    void Start() {
        SetupLoginPage();

    }

    public void SetupLoginPage() {
        SetListeners();

        _QRCodeText.text = "QR Code";
        _loginText.text = "Login";

        _validatingLogin = false;

#if ENABLE_WINMD_SUPPORT
        _usernameText.text = "Insert username...";
        _passwordText.text = "Insert password...";
#else
        _usernameText.text = "CareXR_Tdasdasester_Caregiver";
        _passwordText.text = "password";
#endif

        if (_cleanPassword || _cleanUsername)
            _loginMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.InactiveMaterial;

        AccountManager.OnLoggedStatusChange += OnSucessfullLogin;
    }

    private void SetListeners() {
        _QRCodeBtn.OnClick.AddListener(() => {
            if (_validatingLogin)
                return;

            _QRCodeText.text = "Looking for QR Code...";

            _keyboardMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.InactiveMaterial;
            _keyboardBtn.enabled = false;

            AccountManager.LoginQR();


        });

        _keyboardBtn.OnClick.AddListener(() => {
            if (_validatingLogin)
                return;

            _loginText.text = "Login";

            _optionPanel.gameObject.SetActive(false);
            _loginFormPanel.gameObject.SetActive(true);

        });

        _closeBtn.OnClick.AddListener(() => {
            if (_validatingLogin)
                return;

            if (_validatingLogin)
                return;

            _usernameText.text = "Insert username...";
            _passwordText.text = "Insert password...";

            _loginFormPanel.gameObject.SetActive(false);
            _optionPanel.gameObject.SetActive(true);

            _cleanUsername = true;
            _cleanPassword = true;

            if (_keyboard != null)
                _keyboard = null;

        });

        _unInputBtn.OnClick.AddListener(() => {
            if (_validatingLogin)
                return;

            if (_cleanUsername) {
                _usernameText.text = "";
                _cleanUsername = false;

            }

            _keyboard = TouchScreenKeyboard.Open(_usernameText.text, TouchScreenKeyboardType.EmailAddress, false, false, false, false);

            _insertingPassword = false;

        });

        _pwInputBtn.OnClick.AddListener(() => {
            if (_cleanPassword) {
                _passwordText.text = "";
                _password = "";
                _cleanPassword = false;

            }

            _keyboard = TouchScreenKeyboard.Open(_password, TouchScreenKeyboardType.EmailAddress, false, false, true, false);

            _insertingPassword = true;

        });

        _loginBtn.OnClick.AddListener(() => {
#if ENABLE_WINMD_SUPPORT
            if ((_cleanPassword || _cleanUsername) || _validatingLogin)
                    return;
#else
            if (_validatingLogin)
                return;
#endif


            _loginText.text = "Validating...";
            _validatingLogin = true;

            AccountManager.LoginWithCredentials(_usernameText.text, _passwordText.text);

        });
    }

    private static void OnSucessfullLogin(bool logged) {
        UIManager.Instance.HandMenu.ToggleHomeButton(logged);
        UIManager.Instance.HandMenu.ToggleLogoutButton(logged);

        UIManager.Instance.LoginMenu.gameObject.SetActive(false);

        UIManager.Instance.HomeMenu.gameObject.SetActive(!UIManager.Instance.HomeMenu.gameObject.activeInHierarchy);

        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

        position.y += UIManager.Instance.AxisYOffset;

        UIManager.Instance.HomeMenu.gameObject.transform.position = position;
        UIManager.Instance.HomeMenu.gameObject.transform.LookAt(Camera.main.transform.position);

        UIManager.Instance.LoginMenu.ClearLoginPage();
    }

    public void ClearLoginPage() {
        AccountManager.OnLoggedStatusChange -= OnSucessfullLogin;

        _QRCodeBtn.OnClick.RemoveAllListeners();
        _keyboardBtn.OnClick.RemoveAllListeners();
        _closeBtn.OnClick.RemoveAllListeners();
        _unInputBtn.OnClick.RemoveAllListeners();
        _pwInputBtn.OnClick.RemoveAllListeners();
        _loginBtn.OnClick.RemoveAllListeners();

    }

    public void ShowLoginErrorMessage(string message) {
        _loginText.text = message;
        _validatingLogin = false;

    }

    private void FixedUpdate() {
        if (_keyboard != null && _keyboard.status != TouchScreenKeyboard.Status.Canceled) {
            if (_insertingPassword) {
                _password = _keyboard.text;
                _passwordText.text = new string('*', _password.Length);
            } else 
                _usernameText.text = _keyboard.text;

        }
        if (!_cleanPassword && !_cleanUsername)
            _loginMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.ActiveMaterial;

    }


}
