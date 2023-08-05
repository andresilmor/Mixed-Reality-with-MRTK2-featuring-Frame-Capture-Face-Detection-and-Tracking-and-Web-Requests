using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Debug = XRDebug;


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
    [SerializeField] TextMeshPro _emailText;
    [SerializeField] TextMeshPro _passwordText;
    [SerializeField] TextMeshPro _loginText;

    [Header("Mesh")]
    [SerializeField] MeshRenderer _keyboardMesh;
    [SerializeField] MeshRenderer _loginMesh;

    [Header("Panels")]
    [SerializeField] GameObject _optionPanel;
    [SerializeField] GameObject _loginFormPanel;

    private TouchScreenKeyboard _keyboard = null;

    public TextMeshPro QRCodeText {
        get {
            return _QRCodeText;
        }
        set {
            _QRCodeText = value;
        }

    }

    public bool ValidatingLogin = false;
    bool _insertingPassword = false;

    bool _cleanEmail = true;
    bool _cleanPassword = true;

    string _password;

    // Start is called before the first frame update
    void Start() {
        SetupLoginPage();
        UIManager.Instance.DebugMenu.gameObject.SetActive(true);

    }

    public void SetupLoginPage() {
        SetListeners();

        _QRCodeText.text = "QR Code";
        _loginText.text = "Login";

        ValidatingLogin = false;


        //_emailText.text = "Insert username...";
        //_passwordText.text = "Insert password...";

        _emailText.text = "caregiver@carexr.com";
        _passwordText.text = "password";


        if (_cleanPassword || _cleanEmail)
            _loginMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.InactiveMaterial;

        AccountManager.OnLoggedStatusChange += AccountManager.OnSucessfullLogin;

    }


    private void SetListeners() {
        _QRCodeBtn.OnClick.AddListener(() => {
            Debug.Log("QR Code BTN CALLED");
            if (ValidatingLogin)
                return;

            _QRCodeText.text = "Looking for QRCode...";

            _keyboardMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.InactiveMaterial;
            _keyboardBtn.enabled = false;

            UIManager.Instance.DebugMenu.gameObject.SetActive(true);
            Debug.Log("Starting");
            AccountManager.LoginQR();


        });

        _keyboardBtn.OnClick.AddListener(() => {
            if (ValidatingLogin)
                return;

            _loginText.text = "Login";

            _optionPanel.gameObject.SetActive(false);
            _loginFormPanel.gameObject.SetActive(true);

        });

        _closeBtn.OnClick.AddListener(() => {
            if (ValidatingLogin)
                return;

            if (ValidatingLogin)
                return;

            _emailText.text = "Insert username...";
            _passwordText.text = "Insert password...";

            _loginFormPanel.gameObject.SetActive(false);
            _optionPanel.gameObject.SetActive(true);

            _cleanEmail = true;
            _cleanPassword = true;

            if (_keyboard != null)
                _keyboard = null;

        });

        _unInputBtn.OnClick.AddListener(() => {
            if (ValidatingLogin)
                return;

            if (_cleanEmail) {
                _emailText.text = "";
                _cleanEmail = false;

            }

            _keyboard = TouchScreenKeyboard.Open(_emailText.text, TouchScreenKeyboardType.EmailAddress, false, false, false, false);

            _insertingPassword = false;

        });

        _pwInputBtn.OnClick.AddListener(() => {
            if (_cleanPassword) {
                _passwordText.text = "";
                _password = "";
                _cleanPassword = false;

            }

            _keyboard = TouchScreenKeyboard.Open(_password, TouchScreenKeyboardType.Default, false, false, false, false);

            _insertingPassword = true;

        });

        _loginBtn.OnClick.AddListener(() => {

            if (/*(_cleanPassword || _cleanEmail) || */ ValidatingLogin)
                    return;



            _loginText.text = "Validating...";
            ValidatingLogin = true;

            AccountManager.LoginWithCredentials(_emailText.text, _passwordText.text);

        });
    }

    

    public void ClearLoginPage() {
        AccountManager.OnLoggedStatusChange -= AccountManager.OnSucessfullLogin;

        _QRCodeBtn.OnClick.RemoveAllListeners();
        _keyboardBtn.OnClick.RemoveAllListeners();
        _closeBtn.OnClick.RemoveAllListeners();
        _unInputBtn.OnClick.RemoveAllListeners();
        _pwInputBtn.OnClick.RemoveAllListeners();
        _loginBtn.OnClick.RemoveAllListeners();


    }

    public void ShowLoginErrorMessage(string message) {
        _loginText.text = message;
        ValidatingLogin = false;

    }

    private void FixedUpdate() {
        if (_keyboard != null && _keyboard.status != TouchScreenKeyboard.Status.Canceled) {
            if (_insertingPassword) {
                _password = _keyboard.text;
                _passwordText.text = new string('*', _password.Length);
            } else 
                _emailText.text = _keyboard.text;

        }
        if (!_cleanPassword && !_cleanEmail)
            _loginMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.ActiveMaterial;

    }


    private void OnEnable() {
        ResetButtons();
        UIManager.Instance.LoginMenu.gameObject.transform.LookAt(Camera.main.transform.position);

    }

    public void ResetButtons() {
        _loginMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.ActiveMaterial;
        _keyboardMesh.material = UIManager.Instance.GetRectangleButtonMaterial("Normal").Value.ActiveMaterial;

        QRCodeText.text = "QR Code";

    }
}
