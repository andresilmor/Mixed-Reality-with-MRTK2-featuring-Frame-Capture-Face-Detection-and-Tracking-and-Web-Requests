using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = MRDebug;

public class HandMenu : MonoBehaviour
{
    [Header("Buttons Config:")]
    [SerializeField] Interactable HomeBtn;
    [SerializeField] Interactable HelpBtn;
    [SerializeField] Interactable LogoutBtn;
    [SerializeField] Interactable ShutdownBtn;

    [Header("Windows:")]
    [SerializeField] HomeMenu HomeMenu;

    // Start is called before the first frame update
    void Start() {
        if (HomeMenu == null)
            Debug.Log("HomeMenu not defined");
        else {

            HomeBtn.OnClick.RemoveAllListeners();

            // Home Button
            HomeMenu.gameObject.SetActive(false);

            HomeBtn.OnClick.AddListener(() => {
                HomeMenu.gameObject.SetActive(!HomeMenu.gameObject.activeInHierarchy);

                Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

                position.y += UIManager.Instance.AxisYOffset;

                HomeMenu.gameObject.transform.position = position;
                HomeMenu.gameObject.transform.LookAt(Camera.main.transform.position); 

            });

            ShutdownBtn.OnClick.AddListener(() => {
                AppCommandCenter.StopApplication();

            });

        }


    }
}
