using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIController : MonoBehaviour
{
    public static GameObject sceneContent;

    async public void Login() {
        await AccountController.Login();
    }


}
