using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.Sec;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Presets;
using UnityEngine;

public static class MRDebug
{
    private static TextMeshPro _debugConsole = null;

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

        switch (logType) {
            case LogType.Info: preText = "|INFO| ";
                _logs.Add(new AppLog(LogType.Info, System.DateTime.Now + " | " + text + "\n"));
                break;

            case LogType.Exception: preText = "|EXCEPTION| ";
                _logs.Add(new AppLog(LogType.Exception, System.DateTime.Now + " | " + text + "\n"));
                break;

            case LogType.Warning:
                preText = "|WARNING| ";
                _logs.Add(new AppLog(LogType.Warning, System.DateTime.Now + " | " + text + "\n"));
                break;

            case LogType.Error:
                preText = "|ERROR| ";
                _logs.Add(new AppLog(LogType.Error, System.DateTime.Now + " | " + text + "\n"));
                break;

            case LogType.Fatal:
                preText = "|FATAL| ";
                _logs.Add(new AppLog(LogType.Fatal, System.DateTime.Now + " | " + text + "\n"));
                break;

        }

        if (_debugConsole != null)
            _debugConsole.text = _debugConsole.text + text;

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

    public static List<AppLog> GetLog(params LogType[] logType) {
        List<AppLog> filteredLogs = new List<AppLog>();
        foreach (AppLog log in _logs) {
            if (logType.Contains<LogType>(log.type))
                filteredLogs.Add(log);

        }

        return filteredLogs;

    }

    public static void ClearConsole()
    {
        _debugConsole.text = "";

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

    public static void BindDebugConsole(TextMeshPro text)
    {
        _debugConsole = text;
        foreach (AppLog log in _logs)
            _debugConsole.text = _debugConsole.text + log.info;

    }

    public static void UnbindDebugConsole() {
        _debugConsole = null;
    }

    public static void DrawFieldView()
    {
        GameObject te = null;
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, AppCommandCenter.cameraMain.pixelHeight, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, 0, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(AppCommandCenter.cameraMain.pixelWidth, 0, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
        te = UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.cameraMain.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, AppCommandCenter.cameraMain.ScreenToWorldPoint(new Vector3(0, 0, AppCommandCenter.cameraMain.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), hit.point, Quaternion.identity);
        LineDrawer.Draw(te.transform.position, two.transform.position, Color.yellow);
    }


}
