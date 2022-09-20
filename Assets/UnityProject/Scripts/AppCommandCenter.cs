using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AppCommandCenter : MonoBehaviour
{
    //
    BinaryTree pacients;

    // For Debug
    [Header("Debugger")]
    public TextMeshPro debugText;
    public GameObject cubeForTest;
    public GameObject sphereForTest;
    public GameObject lineForTest;


    // Attr's for the Machine Learning and detections
    private FrameHandler frameHandler;
    private Mat tempFrameMat;



    void Start()
    {
        LoadSavedData();
        SetDebugger();



#if ENABLE_WINMD_SUPPORT
        frameHandler = await FrameHandler.CreateAsync();
#endif

    }

    private void SetDebugger()
    {
        Debugger.SetCubeForTest(cubeForTest);
        Debugger.SetSphereForTest(sphereForTest);
        Debugger.SetDebugText(debugText);
        LineDrawer.SetDrawLine(lineForTest);

    }

    /// <summary>
    /// Function to load save files, like the registry of pacients and their data
    /// </summary>
    private void LoadSavedData()
    {
        if (pacients == null)
            pacients = new BinaryTree();

    }

    void Update()
    {
        
    }



}
