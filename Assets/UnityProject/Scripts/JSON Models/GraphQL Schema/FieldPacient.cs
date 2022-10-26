using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldPacient : GraphQLField
{
    public string name;

    public FieldPacient(string name) : base()
    {
        this.name = name;

    }


}
