using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonMarker : MonoBehaviour
{
    [SerializeField] EmotionsHandler emotionDisplay;
    [SerializeField] GameObject markerRect;
    
    public PersonMarker()
    {
    }

    public EmotionsHandler GetEmotionsHandler()
    {
        return emotionDisplay;
    
    }

    void Update()
    {
        this.gameObject.transform.LookAt(Camera.main.transform.position);
        
    }

    public void SetMarkerVisibility(bool to)
    {
        markerRect.GetComponent<SpriteRenderer>().enabled = to;
    }
}
