using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Sec;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

using Debug = MRDebug;

public static class MRDebug
{

    private static GameObject _cubeForTest;
    private static GameObject _sphereForTest;

    private static List<AppLog> _logs = new List<AppLog>();

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

    }

    public static void LogWarning(object message) {
        string text = message.ToString();
        Log(text, LogType.Warning);

    }

    public static void LogError(object message) {
        string text = message.ToString();
        Log(text, LogType.Error);

    }

    public static void LogException(object message) {
        string text = message.ToString();
        Log(text, LogType.Exception);

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
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.CameraMain.pixelHeight, AppCommandCenter.CameraMain.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.CameraMain.pixelHeight, AppCommandCenter.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.CameraMain.pixelWidth, AppCommandCenter.CameraMain.pixelHeight, AppCommandCenter.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.CameraMain.pixelWidth, AppCommandCenter.CameraMain.pixelHeight, AppCommandCenter.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.CameraMain.pixelWidth, 0, AppCommandCenter.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.CameraMain.pixelWidth, 0, AppCommandCenter.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.CameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.CameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.CameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
    }


}
