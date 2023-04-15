using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = MRDebug;

public class UIHandMenu : MonoBehaviour
{
    [Header("Buttons Config:")]
    [SerializeField] Interactable HomeButton;
    [SerializeField] Interactable HelpButton;
    [SerializeField] Interactable LogoutButton;

    [Header("Windows:")]
    [SerializeField] UIHomeMenu HomeMenu;

    // Start is called before the first frame update
    void Start() {
        if (HomeMenu == null)
            Debug.Log("HomeMenu not defined");
        else {

            // Home Button
            HomeMenu.gameObject.SetActive(false);
            HomeButton.OnClick.AddListener(() => {
                HomeMenu.gameObject.SetActive(!HomeMenu.gameObject.activeInHierarchy);

                Vector3 position = AppCommandCenter.CameraMain.transform.position + AppCommandCenter.CameraMain.transform.forward * UIManager.Instance.WindowDistance;

                position.y += UIManager.Instance.AxisYOffset;

                HomeMenu.gameObject.transform.position = position;
                HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform.position);
                //HomeMenu.gameObject.transform.LookAt(AppCommandCenter.CameraMain.transform);   

            });

        }


    }
}
