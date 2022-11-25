using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIController : MonoBehaviour
{
    public static GameObject sceneContent;

    public async void Login() {
        await AccountController.Login();
    }


}
