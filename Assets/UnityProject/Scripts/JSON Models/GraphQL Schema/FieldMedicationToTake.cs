using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldMedicationToTake : GraphQLField
{
    public string timeMeasure;

    public FieldMedicationToTake(string timeMeasure) : base()
    {
        this.timeMeasure = timeMeasure;

    }

    
}
