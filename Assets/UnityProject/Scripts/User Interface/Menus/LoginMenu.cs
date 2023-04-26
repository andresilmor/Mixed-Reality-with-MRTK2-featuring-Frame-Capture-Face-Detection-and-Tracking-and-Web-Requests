using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable _QRCodeBtn;

    [Header("Texts:")]
    [SerializeField] TextMeshPro _QRCodeText;

    bool _validatingLogin = false;


    // Start is called before the first frame update
    void Start()
    {
        _QRCodeBtn.OnClick.AddListener(() => {
            if (_validatingLogin)
                return;

            _QRCodeText.text = "Looking for QR Code...";

            System.Threading.Tasks.Task<bool> task = AccountManager.LoginQR();


        });
    }

    private void OnEnable() {
        _QRCodeText.text = "QR Code";

    }

}
