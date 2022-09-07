using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Emotions
{
    public string[] continuous;
    public string[] categorical;

    public Emotions(string[] continuous, string[] categorical)
    {
        this.continuous = continuous;
        this.categorical = categorical;
    }
}
