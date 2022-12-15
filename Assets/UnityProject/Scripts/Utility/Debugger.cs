using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class Debugger
{
    public static TextMeshPro debugText;

    public static GameObject _cubeForTest;
    public static GameObject _sphereForTest;

    public static void AddText(string text)
    {
        debugText.text = debugText.text + "\n" + text;
    }

    public static void ClearText()
    {
        //_debugText.text = "";
    }

    public static GameObject GetCubeForTest()
    {
        return _cubeForTest;
    }

    public static GameObject GetSphereForTest()
    {
        return _sphereForTest;
    }

    public static void SetCubeForTest(GameObject cube)
    {
        _cubeForTest = cube;
    }

    public static void SetSphereForTest(GameObject sphere)
    {
        _sphereForTest = sphere;
    }

    public static void SetDebugText(TextMeshPro text)
    {
        debugText = text;
    }

    public static void SetFieldView()
    {
        GameObject te = null;
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, 0, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, 0, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
    }


}
