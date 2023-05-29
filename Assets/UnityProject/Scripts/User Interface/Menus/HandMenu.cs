using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = MRDebug;

public class HandMenu : MonoBehaviour
{
    [Header("Buttons:")]
    [SerializeField] Interactable HomeBtn;
    [SerializeField] Interactable HelpBtn;
    [SerializeField] Interactable LogoutBtn;
    [SerializeField] Interactable ShutdownBtn;

    [Header("Mesh:")]
    [SerializeField] MeshRenderer HomeMesh;
    [SerializeField] MeshRenderer LogoutMesh;


    private static HandMenu _instance = null;
    public static HandMenu Instance {
        get { return _instance; }
        set {
            if (_instance == null) {
                _instance = value;
            } else {
                Destroy(value);
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        if (UIManager.Instance.HomeMenu == null)
            Debug.Log("UIManager.Instance.HomeMenu not defined");
        else {

            // Home Button
            UIManager.Instance.HomeMenu.gameObject.SetActive(false);

            if (AccountManager.IsLogged) {
                ToggleHomeButton(true);
                ToggleLogoutButton(true);

            } else {
                ToggleHomeButton(false);
                ToggleLogoutButton(false);

            }


            ShutdownBtn.OnClick.AddListener(() => {
                AppCommandCenter.StopApplication();

            });

        }


    }

    public void ToggleHomeButton(bool isActive) {
        try {

            if (isActive) {
                HomeMesh.material = UIManager.Instance.GetCircleButtonMaterial("Home").Value.ActiveMaterial;

                HomeBtn.OnClick.AddListener(() => {
                    UIManager.Instance.HomeMenu.gameObject.SetActive(!UIManager.Instance.HomeMenu.gameObject.activeInHierarchy);

                    Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

                    position.y += UIManager.Instance.AxisYOffset;

                    UIManager.Instance.HomeMenu.gameObject.transform.position = position;
                    UIManager.Instance.HomeMenu.gameObject.transform.LookAt(Camera.main.transform.position);

                });



                return;

            }

            HomeMesh.material = UIManager.Instance.GetCircleButtonMaterial("Home").Value.InactiveMaterial;

            HomeBtn.OnClick.RemoveAllListeners();

        } catch (Exception ex) {
            Debug.Log(ex.Message, LogType.Exception);
        }

    }

    public void ToggleLogoutButton(bool isActive) {
        try {
            Debug.Log("Toggle");

            if (isActive) {
                LogoutMesh.material = UIManager.Instance.GetCircleButtonMaterial("Logout").Value.ActiveMaterial;

                LogoutBtn.OnClick.AddListener(() => {
                    UIManager.Instance.HomeMenu.gameObject.SetActive(false);
                    UIManager.Instance.DebugMenu.gameObject.SetActive(false);

                    UIManager.Instance.CloseAllWindows();

                    Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;
                    position.y += UIManager.Instance.AxisYOffset;
                    UIManager.Instance.LoginMenu.gameObject.SetActive(true);
                    UIManager.Instance.LoginMenu.gameObject.transform.position = position;
                    UIManager.Instance.LoginMenu.gameObject.transform.LookAt(Camera.main.transform.position);

                    LogoutMesh.material = UIManager.Instance.GetCircleButtonMaterial("Logout").Value.InactiveMaterial;

                    ToggleHomeButton(false);
                    ToggleLogoutButton(false);

                    AccountManager.Logout();

                    UIManager.Instance.LoginMenu.SetupLoginPage();


                });

                return;

            }

            LogoutMesh.material = UIManager.Instance.GetCircleButtonMaterial("Logout").Value.InactiveMaterial;

            LogoutBtn.OnClick.RemoveAllListeners();

        } catch (Exception ex) {
            Debug.Log(ex.Message, LogType.Exception);
        }

    }

}
