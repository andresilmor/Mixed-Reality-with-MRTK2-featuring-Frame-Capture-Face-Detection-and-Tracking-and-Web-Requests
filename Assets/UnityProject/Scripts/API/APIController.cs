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

public class APIController : MonoBehaviour
{
    [Header("Protocols:")]
    [SerializeField] string ws = "ws://";


    [Header("API Address:")]
    //string address = "ws://192.168.1.238:8000";
    [SerializeField] string ip = "192.168.1.238";
    [SerializeField] string port = "8000";

    [Header("API Paths")]
    [SerializeField]  private string _pacientsDetection = "/ws";
    public string pacientsDetection
    {
        get
        {
            return _pacientsDetection;
        }
    }





    private List<WebSocket> wsConnections;
    private List<string> wsConnectionsPath;

    private WebSocket pacientMapping;



    private static APIController _instance;
    public static APIController Instance
    {
        get { return _instance; }
        set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else
            {
                Debugger.AddText("I was called ?_?");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Instance = this;

    }



    void Start()
    {
        wsConnections = new List<WebSocket>(); 

    }

    public WebSocket GetWebSocket(string path)
    {
        


        Debugger.AddText("Get WebSocket");
        if (path.Equals(this.pacientsDetection))
        {
            Debugger.AddText("WebSocket Getted Var");
            return pacientMapping;

        }
        else
        {
            Debugger.AddText("Searching in List");
            for (int index = 0; index >= wsConnections.Count; index++)
            {
                if (wsConnectionsPath[index].Equals(path))
                    return wsConnections[index];
            }

        }
        return null;
    }


    public void CreateWebSocketConnection(string path, Action<string> callback)
    {
        Debugger.AddText("Address: " + (ws + ip + ":" + port + path));
        Debugger.AddText(new Uri(ws + ip + ":" + port + path).ToString());
        try { 
            WebSocket newConnection = new WebSocket(new Uri(ws + ip + ":" + port + path));
       
            newConnection.OnMessage += (WebSocket webSocket, string message) =>
            {
                Debugger.AddText("Message Received");
                if(message.Length > 6)
                    callback?.Invoke(message);
                else
                    Debugger.AddText("Message Size: " + message.Length);

            };

            newConnection.OnOpen += (WebSocket webSocket) =>
            {
                Debugger.AddText("Connection Opened (CreateWebSocketConnection)");
                webSocket.Send("Connection Opened");
                Debugger.AddText("Warning Sended (CreateWebSocketConnection)");
            };

            newConnection.OnClosed += (WebSocket webSocket, UInt16 code, string message) =>
            {
                wsConnections.Remove(newConnection);

            };

            newConnection.Open();

            if (path.Equals(_pacientsDetection))
            {
                pacientMapping = newConnection;
                Debugger.AddText("Connection Added Var");

            }
            else
            {
                wsConnections.Add(newConnection);
                wsConnectionsPath.Add(path);
                Debugger.AddText("Connection Added List");
            }

        } catch(Exception e)
        {
            Debugger.AddText("Error: " + e.Message.ToString());
        }

        

    }
    

    void OnDestroy()
    {
        foreach (WebSocket ws in wsConnections)
        {
            //this.ws.Close();
        }


    }
}
