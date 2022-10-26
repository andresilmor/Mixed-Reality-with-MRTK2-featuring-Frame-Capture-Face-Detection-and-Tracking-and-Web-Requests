using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphQLData
{
    public List<GraphQLField> medicationToTake;

    public GraphQLField medication;

    public GraphQLData(List<FieldMedicationToTake> medicationToTake)
    {
        this.medicationToTake = medicationToTake.ConvertAll(x => (GraphQLField)x);

    }

}
