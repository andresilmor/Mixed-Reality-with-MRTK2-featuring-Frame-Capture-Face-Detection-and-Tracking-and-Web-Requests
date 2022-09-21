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

    public static void SetFieldView()
    {
        GameObject te = null;
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, Camera.main.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
    }


}
