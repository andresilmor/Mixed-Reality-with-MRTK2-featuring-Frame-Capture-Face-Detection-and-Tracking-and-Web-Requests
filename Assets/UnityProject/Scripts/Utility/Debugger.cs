using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class Debugger
{
    public static TextMeshPro debugText;

    public static GameObject cubeForTest;
    public static GameObject sphereForTest;

    public static void AddText(string text)
    {
        debugText.text = debugText.text + "\n" + text;
    }

    public static void ClearText()
    {
        debugText.text = "";
    }

    public static GameObject GetCubeForTest()
    {
        return cubeForTest;
    }

    public static GameObject GetSphereForTest()
    {
        return sphereForTest;
    }

    public static void SetCubeForTest(GameObject cube)
    {
        cubeForTest = cube;
    }

    public static void SetSphereForTest(GameObject sphere)
    {
        sphereForTest = sphere;
    }

    public static void SetDebugText(TextMeshPro text)
    {
        debugText = text;
    }

}
