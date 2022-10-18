using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Object/GraphQL Schema")]
public class GraphQLShemaScriptableObject : ScriptableObject
{
    [System.Serializable]
    public struct data
    {
        public string name;
        public field[] field;
    }

    public struct field
    {
        public string name;
    }


    [Header("-- Query ---")]
    [SerializeField] data[] _queryList;
    public data[] query
    {
        get
        {
            return _queryList;
        }
    }



    [Header("-- Mutation ---")]
    [SerializeField] data[] _mutationList;
    public data[] mutation
    {
        get
        {
            return _mutationList;
        }
    }
}
