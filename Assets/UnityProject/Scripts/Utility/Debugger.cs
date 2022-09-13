using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public TextMeshPro debugText;

    public void AddText(string text)
    {
        debugText.text = debugText.text + "\n" + text;
    }

}
