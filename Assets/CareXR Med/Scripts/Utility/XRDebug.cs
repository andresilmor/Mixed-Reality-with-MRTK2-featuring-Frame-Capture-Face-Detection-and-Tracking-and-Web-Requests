using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Sec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using Debug = XRDebug;

public static class XRDebug
{

    private static GameObject _cubeForTest;
    private static GameObject _sphereForTest;

    private static List<AppLog> _logs = new List<AppLog>();

    public static TextMeshPro Console = null;

    public struct AppLog {
        public LogType type;
        public string info;

        public AppLog(LogType type, string info) {
            this.type = type;
            this.info = info;
        }
    }

    public static void Log(object message, LogType logType = LogType.Info)
    {
        string text = message.ToString();
        string preText = "";

        _logs.Add(new AppLog(logType, System.DateTime.Now + " | " + Enum.GetName(typeof(LogType), logType) + " | " + text + "\n"));
        UnityEngine.Debug.Log(Enum.GetName(typeof(LogType), logType) + " | " + text + "\n");

        if (Controller.Instance.AdditiveScenesLoaded && UIManager.Instance.DebugMenu.gameObject.activeInHierarchy)
            UIManager.Instance.DebugMenu.UpdateConsole();

    }

    public static List<AppLog> GetLog(bool filterInfo, bool filterWarning, bool filterException, bool filterError, bool filterFatal) {
        List<AppLog> filteredLogs = new List<AppLog>();

        foreach (AppLog log in _logs) {
            if (
                (log.type is LogType.Info && filterInfo) ||
                (log.type is LogType.Warning && filterWarning) ||
                (log.type is LogType.Exception && filterException) ||
                (log.type is LogType.Error && filterError) ||
                (log.type is LogType.Fatal && filterFatal)
                )
                filteredLogs.Add(log);

        }

        return filteredLogs;

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


    public static void DrawFieldView()
    {
        GameObject te = null;
        te = UnityEngine.Object.Instantiate(XRDebug.GetSphereForTest(), Controller.CameraMain.ScreenToWorldPoint(new Vector3(0, Controller.CameraMain.pixelHeight, Controller.CameraMain.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, Controller.CameraMain.ScreenToWorldPoint(new Vector3(0, Controller.CameraMain.pixelHeight, Controller.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = UnityEngine.Object.Instantiate(XRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(XRDebug.GetSphereForTest(), Controller.CameraMain.ScreenToWorldPoint(new Vector3(Controller.CameraMain.pixelWidth, Controller.CameraMain.pixelHeight, Controller.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Controller.CameraMain.ScreenToWorldPoint(new Vector3(Controller.CameraMain.pixelWidth, Controller.CameraMain.pixelHeight, Controller.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(XRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(XRDebug.GetSphereForTest(), Controller.CameraMain.ScreenToWorldPoint(new Vector3(Controller.CameraMain.pixelWidth, 0, Controller.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Controller.CameraMain.ScreenToWorldPoint(new Vector3(Controller.CameraMain.pixelWidth, 0, Controller.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(XRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(XRDebug.GetSphereForTest(), Controller.CameraMain.ScreenToWorldPoint(new Vector3(0, 0, Controller.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Controller.CameraMain.ScreenToWorldPoint(new Vector3(0, 0, Controller.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(XRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
    }


}
