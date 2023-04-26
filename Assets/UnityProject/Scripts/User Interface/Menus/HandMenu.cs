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

    [Header("Windows:")]
    [SerializeField] HomeMenu HomeMenu;

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
        if (HomeMenu == null)
            Debug.Log("HomeMenu not defined");
        else {

            // Home Button
            HomeMenu.gameObject.SetActive(false);

            if (AccountManager.IsLogged) {
                ToggleHomeButton(true);


            } else {
                ToggleHomeButton(false);

            }

            AccountManager.OnLoggedStatusChange += ToggleHomeButton;

            ShutdownBtn.OnClick.AddListener(() => {
                AppCommandCenter.StopApplication();

            });

        }


    }

    public void ToggleHomeButton(bool isActive) {
        try {
            Debug.Log("Toggle");

            if (isActive) {
                HomeMesh.material = UIManager.Instance.GetCircleButtonMaterial("Home").Value.ActiveMaterial;

                HomeBtn.OnClick.AddListener(() => {
                    HomeMenu.gameObject.SetActive(!HomeMenu.gameObject.activeInHierarchy);

                    Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

                    position.y += UIManager.Instance.AxisYOffset;

                    HomeMenu.gameObject.transform.position = position;
                    HomeMenu.gameObject.transform.LookAt(Camera.main.transform.position);

                });

                return;

            }

            HomeMesh.material = UIManager.Instance.GetCircleButtonMaterial("Home").Value.InactiveMaterial;

            HomeBtn.OnClick.RemoveAllListeners();

        } catch (Exception ex) {
            Debug.Log(ex.Message, LogType.Exception);
        }

    }

}
