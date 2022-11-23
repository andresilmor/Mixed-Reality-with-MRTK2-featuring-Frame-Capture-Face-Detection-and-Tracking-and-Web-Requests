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

public static class APIController 
{

    #region API Meta Data

    [Header("Protocols:")]
    [SerializeField] static string websocketProtocol = "ws://";
    [SerializeField] static string httpProtocol = "http://";


    [Header("API Address:")]
    //string address = "websocketProtocol://192.168.1.238:8000";
    [SerializeField] static string ip = "192.168.1.119";
    [SerializeField] static string port = "8000";

    [Header("Root Paths:")]
    [SerializeField] private static string _websocketPath = "/ws";
    public static string websocketPath
    {
        get
        {
            return _websocketPath;
        }
    }
    [SerializeField] private static string _graphqlPath = "/api";
    public static string graphqlPath
    {
        get
        {
            return _graphqlPath;
        }
    }

    #endregion

    private static string READ = "3466fab4975481651940ed328aa990e4";
    private static string UPDATE = "15a8022d0ed9cd9c2a2e756822703eb4";
    private static string CREATE = "294ce20cdefa29be3be0735cb62e715d";
    private static string DELETE = "32f68a60cef40faedbc6af20298c1a1e";

    #region WebSockets Data

    private const string _pacientsDetection = "/live";
    public static string pacientsDetection
    {
        get
        {
            return _pacientsDetection;
        }
    }

    private static List<WebSocket> wsConnections;
    private static List<string> wsConnectionsPath;
    private static WebSocket pacientMapping;

    #endregion

    #region GraphQL Field

    public struct Field
    {
        public string name;
        public FieldParams[] parameters;
        public Field[] subfield;

        public Field(string name, FieldParams[] parameters = null)
        {
            this.name = name;
            this.parameters = parameters;
            this.subfield = null;
        }

        public Field(string name, Field[] subfield)
        {
            this.name = name;
            this.parameters = null;
            this.subfield = subfield;
        }

        public Field(string name, FieldParams[] parameters, Field[] subfield)
        {
            this.name = name;
            this.parameters = parameters;
            this.subfield = subfield;
        }


    }

    public struct FieldParams
    {
        public string name;
        public string value;

        public FieldParams(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }

    #endregion

    #region WebSocket Functions

    public static WebSocket GetWebSocket(string path)
    {
        switch (path)
        {
            case _pacientsDetection:
                return pacientMapping;


            default:
                for (int index = 0; index >= wsConnections.Count; index++)
                {
                    if (wsConnectionsPath[index].Equals(path))
                        return wsConnections[index];
                }
                return null;

        }
        
    }

    private static void AddWebSocket(string path, WebSocket webSocket)
    {
        switch (path)
        {
            case _pacientsDetection:
                pacientMapping = webSocket;
                break;

            default:
                wsConnections.Add(webSocket);
                wsConnectionsPath.Add(path);
                break;

        }
    }

    public static void CreateWebSocketConnection(string path, Action<string> callback)
    {
        try { 
            WebSocket newConnection = new WebSocket(new Uri(websocketProtocol + ip + ":" + port + websocketPath + path));
       
            newConnection.OnMessage += (WebSocket webSocket, string message) =>
            {
                if(message.Length > 6)
                    callback?.Invoke(message);
            };

            newConnection.OnOpen += (WebSocket webSocket) =>
            {
                webSocket.Send("Connection Opened");
            };

            newConnection.OnClosed += (WebSocket webSocket, UInt16 code, string message) =>
            {
                wsConnections.Remove(newConnection);

            };

            newConnection.Open();

            AddWebSocket(path, newConnection);

        } catch(Exception e)
        {
            Debugger.AddText("Error: " + e.Message.ToString());
        }

    }

    public static void CloseAllWebSockets()
    {
        if (wsConnections != null) { 
            foreach (WebSocket ws in wsConnections)
                ws.Close();
        }

    }

    #endregion

    #region GraphQL Query Functions
    private static void MountQuery(Field[] args, ref string query, byte identationLevel = 2)
    {
        foreach (Field field in args)
        {
            query += (new string('\t', identationLevel) + field.name);
            if (field.parameters != null)
            {
                query += " (";
                for (byte index = 0; index < field.parameters.Length; index++)
                    query += (field.parameters[index].name + ": " + field.parameters[index].value + (index >= field.parameters.Length ? ", " : ""));

                query += ") {";
            }

            if (field.subfield != null)
            {
                query += " {\r\n";
                MountQuery(field.subfield, ref query, identationLevel += 1);

                query += (new string('\t', identationLevel - 1) + "}\r\n");
            }
            else
                query += "\r\n";

        }
    }

    public static async Task ExecuteQuery(string operation, string token, Field type,  Action<string> callback, params Field[] args)
    {
        if (!HTTPManager.IsCachingDisabled) {
            HTTPCacheService.BeginClear();
            HTTPManager.IsCachingDisabled = true;

        }

        await Task.Run(() => { 
            string query = "query {\r\n";
            query += (new string('\t', 1) + type.name);
            if (type.parameters != null)
            {
                query += " (";
                foreach (FieldParams parameter in type.parameters)
                    query += (parameter.name + ": " + parameter.value + ", ");

                query += ") {\r\n";

            }

            MountQuery(args, ref query, 2);
            query += (new string('\t', 1) + "}\r\n");
            query += "}";

            string jsonData = JsonConvert.SerializeObject(new {query});
            byte[] postData = Encoding.ASCII.GetBytes(jsonData);


            using (HTTPRequest request = new HTTPRequest(new Uri(httpProtocol + ip + ':' + port + graphqlPath), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) => {
                callback?.Invoke(response.DataAsText);
            }))
            {
                request.DisableCache = true;

                request.SetHeader("Content-Type", "application/json; charset=UTF-8");
                if (token != null)
                    request.SetHeader("Authorization", token);
                request.SetHeader("Operation", operation);

                request.RawData = Encoding.UTF8.GetBytes(jsonData);
                request.Send();
            }
                //HTTPRequest request = new HTTPRequest(new Uri(httpProtocol + ip + ':' + port + graphqlPath), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) => {
                  //  callback?.Invoke(response.DataAsText);
                //});

            
        });
    }

    #endregion

    
}
