using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonProfile : MonoBehaviour
{
    [SerializeField] EmotionsListScriptableObject emotionsList;
    [SerializeField] SpriteRenderer emotionSpriteRenderer = null;
    private int activeEmotionIndex = -1;

    [SerializeField] GameObject markerRect;
    
    public PersonProfile()
    {
    }

    private void Start()
    {
        if (emotionsList.categorical.Length != 26)
        {
            Debug.Log("Emotions Listed < 26");
        }
    }
    /*
    private void SetupEmotionsSprites()
    {
        Debugger.AddText("Person Profile Child: " + transform.childCount);
        emotionSpriteRenderer = gameObject.transform.GetChild(1).gameObject;

    }
    */

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
        for (byte index = 0; index < 26; index++)
        {
            if (emotionsList.categorical[index].name.Equals(emotionName))
            {
                emotionSpriteRenderer.sprite = emotionsList.categorical[index].sprite;

                activeEmotionIndex = index;

                return true;
            }
        }


        return false;




    }
}
