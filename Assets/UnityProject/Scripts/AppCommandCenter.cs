using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppCommandCenter : MonoBehaviour
{
    BinaryTree pacients;

    void Start()
    {
        LoadSavedData();

    }

    private void LoadSavedData()
    {
        if (pacients == null)
            pacients = new BinaryTree();

    }

    void Update()
    {
        
    }
}
