using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GraphQLResponse
{
    public GraphQLData data;

    public GraphQLResponse(GraphQLData data)
    {
        this.data = data;

    }
}