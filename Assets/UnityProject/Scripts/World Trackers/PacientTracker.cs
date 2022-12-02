using OpenCVForUnity.TrackingModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PacientTracker : MonoBehaviour
{
    [SerializeField] EmotionsListScriptableObject emotionsList;
    [SerializeField] SpriteRenderer emotionSpriteRenderer = null;
    private int activeEmotionIndex = -1;

    public TrackerHandler trackerHandler;

    [SerializeField] GameObject markerRect;

    private string _id = "";
    public string id
    {
        get { return _id; }
        set
        {
            if (id.Length <= 0)
                _id = value;
        }
    }

    public PacientTracker(TrackerHandler trackerHandler = null)
    {
        this.trackerHandler = trackerHandler;
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
        this.gameObject.transform.LookAt(AppCommandCenter.cameraMain.transform.position);
        
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
