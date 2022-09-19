using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EmotionsHandler : MonoBehaviour
{
    [Serializable]
    public struct EmotionSprite
    {
        public string name;
        public GameObject sprite;
    }
    public EmotionSprite[] emotionSprite;
    private EmotionSprite? activeEmotion = null;

    //Create something alike more common emotion XD

    // Start is called before the first frame update
    void Start()
    {
        if (activeEmotion != null)
        {
            activeEmotion.Value.sprite.SetActive(false);
            activeEmotion = null;

        }

        if (emotionSprite.Length != 26)
        {
            Debug.Log("Emotions Listed < 26");
        }

    }

    public bool UpdateActiveEmotion(string emotionName)
    {
        try {
            EmotionSprite sprite = (EmotionSprite)FindEmotionSprite(emotionName);
            if (sprite.Equals(default(EmotionSprite))) // Because is a struct :/
            {
                if (activeEmotion != null)
                    activeEmotion.Value.sprite.SetActive(false);

                sprite.sprite.SetActive(true);
                activeEmotion = sprite;
                return true;

            }

        } catch (Exception e) {
            Debug.Log(e.ToString());
            return false;

        }

        return false;


    }

    private EmotionSprite? FindEmotionSprite(string emotionName)
    {
        foreach (EmotionSprite sprite in emotionSprite)
        {
            if (sprite.name == emotionName)
                return sprite;

        }
        return new EmotionSprite();
    }
}
