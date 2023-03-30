using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

public class ButtonColorHandler : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Color32 defaultColor;
    [SerializeField] Color32 touchBeginColor;
    [SerializeField] Color32 pressedColor;


    // Start is called before the first frame update
    void Start()
    {
        if (image is null)
            image = GetComponent<Image>();
        SetColor32(defaultColor);

    }

    public void SetColor(string state)
    {
        switch (state)
        {
            case "touchBegin":
                SetColor32(touchBeginColor);
                break;

            case "pressed":
                SetColor32(pressedColor);
                break;

            default:
                SetColor32(defaultColor);
                break;


        }

    }

    private void SetColor32(Color32 color)
    {
        image.GetComponent<Image>().color = new Color32(color.r, color.g, color.b, 255);
    }

}
