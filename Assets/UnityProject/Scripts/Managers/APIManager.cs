using System;
using UnityEngine;

using BestHTTP.WebSocket;
//using Newtonsoft.Json;

//These are needed, trust me ^-^ 
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

using Microsoft.MixedReality.Toolkit.Extensions;
using Microsoft.MixedReality.Toolkit;
using OpenCVForUnity.CoreModule;
using TMPro;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Security.Cryptography;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
#endif


//For Tracking
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.VideoModule;
using RectCV = OpenCVForUnity.CoreModule.Rect;
using BestHTTP.JSON;
using System.Collections;
using Newtonsoft.Json.Linq;
using BestHTTP;
using UnityEngine.Networking;
using System.Text;
using static BestHTTP.SecureProtocol.Org.BouncyCastle.Crypto.Digests.SkeinEngine;
using BestHTTP.Caching;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading;
using UnityEngine.UIElements;

public static class APIManager {

    #region API Meta Data

    [Header("Protocols:")]
    [SerializeField] static string websocketProtocol = "wss://";
    [SerializeField] static string httpProtocol = "https://";


    [Header("API Address:")]
    //string address = "websocketProtocol://192.168.1.238:8000";
    [SerializeField] static string ip = "c9ce-193-136-194-58.eu.ngrok.io";
    [SerializeField] static string port = ""; //For when used with localhost server :8000

    [Header("Root Paths:")]
    [SerializeField] private static string _websocketPath = "/ws";
    public static string websocketPath {
        get {
            return _websocketPath;
        }
    }
    [SerializeField] private static string _graphqlPath = "/api";
    public static string graphqlPath {
        get {
            return _graphqlPath;
        }
    }

    #endregion

    public static void SetupAPI() {
        if (!HTTPManager.IsCachingDisabled) {
            HTTPCacheService.BeginClear();
            HTTPManager.IsCachingDisabled = true;

        }

        HTTPManager.HTTP2Settings.EnableConnectProtocol = true;


    }

    #region WebSockets Data

    private const string _mlLiveDetection = "/live";
    public static string mlLiveDetection {
        get {
            return _mlLiveDetection;
        }
    }

    private static List<WebSocket> wsConnections;
    private static List<string> wsConnectionsPath;
    public static WebSocket wsLiveDetection { get; private set; }

    #endregion

    #region GraphQL Field

    public struct Field {
        public string name;
        public FieldParams[] parameters;
        public Field[] subfield;

        public Field(string name, FieldParams[] parameters = null) {
            this.name = name;
            this.parameters = parameters;
            this.subfield = null;
        }

        public Field(string name, Field[] subfield) {
            this.name = name;
            this.parameters = null;
            this.subfield = subfield;
        }

        public Field(string name, FieldParams[] parameters, Field[] subfield) {
            this.name = name;
            this.parameters = parameters;
            this.subfield = subfield;
        }


    }

    public struct FieldParams {
        public string name;
        public string value;

        public FieldParams(string name, string value) {
            this.name = name;
            this.value = value;
        }
    }

    #endregion

    #region WebSocket

    public static WebSocket GetWebSocket(string path) {
        switch (path) {
            case _mlLiveDetection:
                return wsLiveDetection;


            default:
                for (int index = 0; index >= wsConnections.Count; index++) {
                    if (wsConnectionsPath[index].Equals(path))
                        return wsConnections[index];
                }
                return null;

        }

    }

    private static void AddWebSocket(string path, WebSocket webSocket) {
        switch (path) {
            case _mlLiveDetection:
                wsLiveDetection = webSocket;
                break;

            default:
                wsConnections.Add(webSocket);
                wsConnectionsPath.Add(path);
                break;

        }
    }

    public static void CreateWebSocketConnection(string path, Action<string> action) {
        try {
            WebSocket newConnection = new WebSocket(new Uri(websocketProtocol + ip + port + websocketPath + path));

            newConnection.OnMessage += (WebSocket webSocket, string message) => {
                if (message.Length > 6)
                    action?.Invoke(message);
            };

            newConnection.OnOpen += (WebSocket webSocket) => {
                webSocket.Send("Connection Opened");
            };

            newConnection.OnClosed += (WebSocket webSocket, UInt16 code, string message) => {
                wsConnections.Remove(newConnection);

            };

            newConnection.Open();

            AddWebSocket(path, newConnection);

        } catch (Exception e) {
            Debugger.AddText("Error: " + e.Message.ToString());
        }

    }

    public static void CloseAllWebSockets() {
        if (wsLiveDetection.IsOpen) { 
            wsLiveDetection.Close(); 
        }

        if (wsConnections != null) {
            foreach (WebSocket ws in wsConnections)
                ws.Close();
        }


    }

    #endregion

    #region HTTP Request General

    private static void OnRequestFinished(Action<string, bool> action, HTTPRequest request, HTTPResponse response) {
        switch (request.State) {
            // The request finished without any problem.
            case HTTPRequestStates.Finished:
                action?.Invoke(response.DataAsText, true);
                break;

            // The request finished with an unexpected error. The request's Exception property may contain more info about the error.
            case HTTPRequestStates.Error:
                action?.Invoke(response.DataAsText, false);
                Debug.LogError("Request Finished with Error! " + (request.Exception != null ? (request.Exception.Message + "\n" + request.Exception.StackTrace) : "No Exception"));
                break;

            // The request aborted, initiated by the user.
            case HTTPRequestStates.Aborted:
                Debug.LogWarning("Request Aborted!");
                break;

            // Connecting to the server is timed out.
            case HTTPRequestStates.ConnectionTimedOut:
                Debug.LogError("Connection Timed Out!");
                break;

            // The request didn't finished in the given time.
            case HTTPRequestStates.TimedOut:
                Debug.LogError("Processing the request Timed Out!");
                break;
        }
    }

    #endregion

    #region GraphQL Query Functions
    private static void MountQuery(Field[] args, ref string query, byte identationLevel = 2) {
        foreach (Field field in args) {
            query += (new string('\t', identationLevel) + field.name);
            if (field.parameters != null) {
                query += " (";
                for (byte index = 0; index < field.parameters.Length; index++)
                    query += (field.parameters[index].name + ": " + field.parameters[index].value + (index >= field.parameters.Length ? ", " : ""));

                query += ") {";
            }

            if (field.subfield != null) {
                query += " {\r\n";
                MountQuery(field.subfield, ref query, identationLevel += 1);

                query += (new string('\t', identationLevel - 1) + "}\r\n");
            } else
                query += "\r\n";

        }
    }

    public static async Task ExecuteRequest(string token, Field type, Action<string, bool> action, params Field[] args) {

        await Task.Run(() => {
            string query = "query {\r\n";
            query += (new string('\t', 1) + type.name);
            if (type.parameters != null) {
                query += " (";
                foreach (FieldParams parameter in type.parameters)
                    query += (parameter.name + ": " + parameter.value + ", ");

                query += ") {\r\n";

            }

            MountQuery(args, ref query, 2);
            query += (new string('\t', 1) + "}\r\n");
            query += "}";

            string jsonData = JsonConvert.SerializeObject(new { query });
            byte[] postData = Encoding.ASCII.GetBytes(jsonData);


            using (HTTPRequest request = new HTTPRequest(new Uri(httpProtocol + ip + port + graphqlPath), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) => OnRequestFinished(action, request, response))) {
                request.DisableCache = true;

                request.SetHeader("Content-Type", "application/json; charset=UTF-8");
                request.SetHeader("Accept", "application/json");
                request.SetHeader("Keep-Alive", "timeout = 2, max = 20");

                if (token != null)
                    request.SetHeader("Authorization", token);

                request.RawData = Encoding.UTF8.GetBytes(jsonData);
                request.Send();

            }

        });
    }

    #endregion

}
