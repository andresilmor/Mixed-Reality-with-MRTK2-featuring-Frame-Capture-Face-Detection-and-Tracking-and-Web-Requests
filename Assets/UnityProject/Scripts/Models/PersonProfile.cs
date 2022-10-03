using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonProfile : MonoBehaviour
{
    [SerializeField] EmotionsListScriptableObject emotionsList;
    [SerializeField] GameObject emotionsSprites = null;
    private int activeEmotionIndex = -1;

    [SerializeField] GameObject markerRect;
    
    public PersonProfile()
    {
    }

    private void Start()
    {
        /*
        if (activeEmotionIndex > 0)
        {
            emotionsSprites.transform.GetChild(activeEmotionIndex).gameObject.SetActive(false);
            activeEmotionIndex = -1;
        }
        */
        if (emotionsList.categorical.Length != 26)
        {
            Debug.Log("Emotions Listed < 26");
        }
    }

    private void SetupEmotionsSprites()
    {
        Debugger.AddText("Person Profile Child: " + transform.childCount);
        emotionsSprites = gameObject.transform.GetChild(1).gameObject;

        if (emotionsSprites != null)
            Debugger.AddText("oK");
        else
            Debugger.AddText("Ya, not ok");
    }

    void FixedUpdate()
    {
        this.gameObject.transform.LookAt(Camera.main.transform.position);
        
    }

    public void SetMarkerVisibility(bool to)
    {
        markerRect.GetComponent<SpriteRenderer>().enabled = to;
    }

    public bool UpdateActiveEmotion(string emotionName)
    {
        Debugger.AddText("1 Update to: " + emotionName);
        if (emotionsSprites != null)
            Debugger.AddText("oK");
        else
            Debugger.AddText("Ya, not ok");
        Debugger.AddText(emotionsSprites.gameObject.transform.childCount.ToString());
        for (byte index = 0; index < 26; index++)
        {
            Debugger.AddText("On: " + index + " | " + emotionsList.categorical[index].name);
            if (emotionsList.categorical[index].name.Equals(emotionName))
            {
                if (activeEmotionIndex > 0)
                    emotionsSprites.transform.GetChild(activeEmotionIndex).gameObject.SetActive(false);


                emotionsSprites.transform.GetChild(index).gameObject.SetActive(true);

                activeEmotionIndex = index;

                return true;
            }
        }


        return false;




    }
}
