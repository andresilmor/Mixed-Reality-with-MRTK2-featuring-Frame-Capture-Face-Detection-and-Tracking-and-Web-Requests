using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GraphQL 
{

    public struct Type {
        public string name;
        public Params[] parameters;
        public Type[] subfield;

        public Type(string name, Params[] parameters = null) {
            this.name = name;
            this.parameters = parameters;
            this.subfield = null;
        }

        public Type(string name, Type[] subfield) {
            this.name = name;
            this.parameters = null;
            this.subfield = subfield;
        }

        public Type(string name, Params[] parameters, Type[] subfield) {
            this.name = name;
            this.parameters = parameters;
            this.subfield = subfield;
        }


    }

    public struct Params {
        public string name;
        public string value;

        public Params(string name, string value) {
            this.name = name;
            this.value = value;
        }
    }
}
