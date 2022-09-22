using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public class EmotionsHandler : MonoBehaviour
{
    [SerializeField] GameObject[] emotionSprites;
    private int activeEmotionIndex = -1;

    //Create something alike more common emotion XD

    // Start is called before the first frame update
    void Start()
    {
        if (activeEmotionIndex > 0)
        {
            transform.GetChild(activeEmotionIndex).gameObject.SetActive(true);
            activeEmotionIndex = -1;
        }

        if (emotionSprites.Length != 26)
        {
            Debug.Log("Emotions Listed < 26");
        }
        
    }

    public bool UpdateActiveEmotion(string emotionName)
    {
        Debug.Log(emotionName);
        
        Debug.Log("try");
        for (byte index = 0; index < emotionSprites.Length; index++)
        {
            if (emotionSprites[index].name == emotionName) { 
                transform.GetChild(index).gameObject.SetActive(true);

                if (activeEmotionIndex > 0)
                    transform.GetChild(activeEmotionIndex).gameObject.SetActive(false);

                activeEmotionIndex = index;
            }
        }

        
        return true;

        


    }

}
