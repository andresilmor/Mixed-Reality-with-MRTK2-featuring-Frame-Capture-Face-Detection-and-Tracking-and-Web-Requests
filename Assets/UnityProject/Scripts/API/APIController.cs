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

public class APIController : MonoBehaviour
{

    [Header("Protocols:")]
    [SerializeField] string websocketProtocol = "ws://";
    [SerializeField] string httpProtocol = "http://";


    [Header("API Address:")]
    //string address = "websocketProtocol://192.168.1.238:8000";
    [SerializeField] string ip = "192.168.1.50";
    [SerializeField] string port = "8000";

    [Header("Root Paths:")]
    [SerializeField] private string _websocketPath = "/ws";
    public string websocketPath
    {
        get
        {
            return _websocketPath;
        }
    }
    [SerializeField] private string _graphqlPath = "/api";
    public string graphqlPath
    {
        get
        {
            return _graphqlPath;
        }
    }

    private string READ = "3466fab4975481651940ed328aa990e4";
    private string UPDATE = "15a8022d0ed9cd9c2a2e756822703eb4";
    private string CREATE = "294ce20cdefa29be3be0735cb62e715d";
    private string DELETE = "32f68a60cef40faedbc6af20298c1a1e";


    [Header("WebSockets Paths:")]
    [SerializeField]  private string _pacientsDetection = "/live";
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
                Destroy(value);
            }
        }
    }


    // For GraphQL Schema
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
            Debugger.AddText("Connection State: " + pacientMapping.State);
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
        Debugger.AddText("Address: " + (websocketProtocol + ip + ":" + port + websocketPath + path));
        try { 
            WebSocket newConnection = new WebSocket(new Uri(websocketProtocol + ip + ":" + port + websocketPath + path));
       
            newConnection.OnMessage += (WebSocket webSocket, string message) =>
            {
                Debugger.AddText("Message Received");
                if(message.Length > 6)
                    callback?.Invoke(message);
           

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
            Debugger.AddText("Connection State: " + newConnection.State);
          
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


    public void ExecuteQuery(string operation, Field type,  Action<string> callback, params Field[] args)
    {
        string query = "query {\r\n";
        query += (new string('\t', 1) + type.name);
        if (type.parameters != null)
        {
            query += " (";
            foreach (FieldParams parameter in type.parameters)
                query += (parameter.name + ": " + parameter.value);

            query += ") {\r\n";

        }

        MountQuery(args, ref query, 2);
        query += (new string('\t', 1) + "}\r\n");
        query += "}";

        string jsonData = JsonConvert.SerializeObject(new {query});
        byte[] postData = Encoding.ASCII.GetBytes(jsonData);

        HTTPRequest request = new HTTPRequest(new Uri(httpProtocol + ip + ':' + port + graphqlPath), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) => {
            callback?.Invoke(response.DataAsText);
        });

        request.SetHeader("Content-Type", "application/json; charset=UTF-8");
        request.SetHeader("Hash", READ);
        
        request.RawData = Encoding.UTF8.GetBytes(jsonData);
        request.Send();
      
    }

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

    


    void OnDestroy()
    {
        foreach (WebSocket ws in wsConnections)
        {
            ws.Close();
        }


    }
}
