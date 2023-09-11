using System;
using UnityEngine;

using BestHTTP.WebSocket;
//using Newtonsoft.Json;

//These are needed, trust me ^-^ 
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Security.Cryptography;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
#endif


//For Tracking
using BestHTTP;
using System.Text;
using BestHTTP.Caching;

using Debug = XRDebug;
using System.Text.RegularExpressions;

public static class APIManager {

    #region API Meta Data

    [Header("Protocols:")]
    [SerializeField] static string _websocketProtocol = "wss://";
    [SerializeField] static string _httpProtocol = "https://";

    [Header("API Address:")]
    //string address = "_websocketProtocol://192.168.1.238:8000";
    [SerializeField] static string _ip = "473b-193-136-194-58.ngrok-free.app";
    [SerializeField] static string _port = ""; //For when used with localhost server :8000

    [Header("Root Paths:")]
    [SerializeField] private static string _websocketPath = "/ws";
    public static string WebsocketPath { get { return _websocketPath; } }

    [SerializeField] private static string _graphqlPath = "/api";
    public static string GraphqlPath { get { return _graphqlPath; } }

    #endregion

    public static void SetupAPI() {
        if (!HTTPManager.IsCachingDisabled) {
            HTTPCacheService.BeginClear();
            HTTPManager.IsCachingDisabled = true;

        }

        HTTPManager.HTTP2Settings.EnableConnectProtocol = true;


    }

    #region WebSockets Data

    // Machine Learning Routes
    private const string _mlRoute = "/ml";

    private const string _frameFullInference = "/emotionFaceRecon";
    private const string _frameFaceRecognition = "/face-recognition";
    public static string FrameFullInference { get { return _mlRoute + _frameFullInference; } }
    public static string FrameFaceRecognition { get { return _mlRoute + _frameFaceRecognition; } }

    // QRCode Routes
    private const string _qrRoute = "/qr";

    private const string _qrDecode = "/decode";
    public static string QRDecode { get { return _qrRoute + _qrDecode; } }

    private const string _qrAuth = "/auth";
    public static string QRAuth { get { return _qrRoute + _qrAuth; } }


    private static List<string> _wsConnectionsPath;

    private static Dictionary<string, WebSocket> _wsConnections = new Dictionary<string, WebSocket>();
    public static Dictionary<string, WebSocket> WebSocketConnections { get { return _wsConnections; } }


    // WS Connections
    public static WebSocket wsFrameInference { get; private set; }


    #endregion

    #region GraphQL Field

    

    #endregion

    #region WebSocket

    public static WebSocket GetWebSocket(string path) {
        if (_wsConnections.ContainsKey(path)) {
            return _wsConnections[path];
        }
        Debug.Log("Nop Contains");
        return null;

    }

    private static void AddWebSocket(string path, WebSocket webSocket) {
        _wsConnections.Add(path, webSocket);

        /*
        switch (path) {
            case _frameFullInference:
                wsFrameInference = webSocket;
                break;

            default:
                _wsConnections.Add(webSocket);
                _wsConnectionsPath.Add(path);
                break;

        }*/
    }


    public static WebSocket CreateWebSocketConnection(string path, Action<WebSocket, string> onMessage = null, Action<byte[]> onBinary = null, Action<WebSocket> onOpen = null) {
        try {
            _wsConnections[path] = new WebSocket(new Uri(_websocketProtocol + _ip + _websocketPath + path));
            Debug.Log("Opening "  + (_websocketProtocol + _ip + _websocketPath + path));
            _wsConnections[path].OnMessage += (WebSocket webSocket, string data) => {
                Debug.Log("API Manager: " + data);
                onMessage?.Invoke(webSocket, data);
            };

            _wsConnections[path].OnBinary += (WebSocket webSocket, byte[] data) => {
                onBinary?.Invoke(data);

            };

            _wsConnections[path].OnClosed += (WebSocket webSocket, UInt16 code, string message) => {
                Debug.Log("Connection [" + path + "] closed.");
            };

            _wsConnections[path].OnOpen += (WebSocket webSocket) => {
                onOpen?.Invoke(webSocket);
                Debug.Log("Connection [" + path + "] opened.");
            
            };


            return _wsConnections[path];

        } catch (Exception e) {
            Debug.Log("Error: " + e.Message.ToString());
        }

        return null;

    }



    public static void CreateWebSocketLiveDetection(string path, DetectionType detectionType, Action<List<PersonAndEmotionsInferenceReply.Detection>> action) {
        try {
            Debug.Log(_websocketProtocol + _ip + _websocketPath + _mlRoute + path);

            _wsConnections[path] = new WebSocket(new Uri(_websocketProtocol + _ip + _websocketPath + "/ml" + path));
            // "ws://34.254.162.143/ws" 

            _wsConnections[path].OnMessage += (WebSocket webSocket, string message) => {

                if (message.Length > 6) {
                    if (detectionType == DetectionType.Person) {
                        action?.Invoke(JsonConvert.DeserializeObject<PersonAndEmotionsInferenceReply.DetectionsList>(
                JsonConvert.DeserializeObject(message).ToString()).detections);

                    }
                }

            };
            _wsConnections[path].OnBinary += (WebSocket webSocket, byte[] data) => {
                Debug.Log("Binary");
                /*
                try { 
                Debug.Log("r: " + ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray().Length);
                Debug.Log("r: " + ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray()[0].uuid);
                Debug.Log("r: " + ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray()[0].faceRect.x1);
                Debug.Log("r: " + ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray()[0].emotionsDetected.continuous["Valence"].ToString());
                Debug.Log("r: " + ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray()[0].emotionsDetected.categorical.ToArray()[0]);
                    Debug.Log(ProtoBuf.Serializer.Deserialize<ProtoClasses.PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).GetType());
                } catch (Exception ex) {
                    Debug.Log(ex.Message);
                }




              
                List<Detection> detections = new List<Detection>();

                foreach (PacientAndEmotionDetected pacient in ProtoBuf.Serializer.Deserialize<PacientsAndEmotionsInferenceReply>(new MemoryStream(Data)).detections.ToArray()) {
                    detections.Add(new Detection(
                            id: pacient.uuid,
                            bodyCenter: new BodyCenter(
                                    x: (int)pacient.bodyCenter.x,
                                    y: (int)pacient.bodyCenter.y
                                ),
                            faceRect: new FaceRectOld(
                                    x1: (int)pacient.faceRect.x1, 
                                    x2: (int)pacient.faceRect.x2,
                                    y1: (int)pacient.faceRect.y1,
                                    y2: (int)pacient.faceRect.y2
                                ),
                            emotions: new EmotionsDetected(
                                    continuous: new string[] { "eqweqw" },
                                    categorical: new string[] { "Anger" }
                                )
                        ));
                }
                action?.Invoke(new DetectionsList("Pacients", list: detections), detectionType);
                */
            };
            _wsConnections[path].Open();
            

            /*
            WebSocket newConnection = new WebSocket(new Uri(_websocketProtocol + _ip + _port + WebsocketPath + path));

            newConnection.OnMessage += (WebSocket webSocket, string message) => {
                Debug.Log(message);
                if (message.Length > 6)
                    action?.Invoke(message, detectionType);
            };

            ConfigWebsocketGeneric(path, newConnection);
            */

        } catch (Exception e) {
            Debug.Log("Error: " + e.Message.ToString());
        }

    }

    

    public static void CloseAllWebSockets() {
        foreach (KeyValuePair<string,WebSocket> connection in _wsConnections)
            connection.Value.Close();   

        /*
        if (wsFrameInference != null && wsFrameInference.IsOpen) { 
            wsFrameInference.Close(); 
        }

        if (_wsConnections != null && _wsConnections.Count > 0) {
            foreach (WebSocket ws in _wsConnections)
                ws.Close();
        }*/


    }

    #endregion

    #region HTTP Request General

    private static void OnRequestFinished(Action<string, bool> action, HTTPRequest request, HTTPResponse response) {
        switch (request.State) {
            // The request finished without any problem.
            case HTTPRequestStates.Finished:
                action?.Invoke(response.DataAsText, true);
                break;

            // The request finished with an unexpected error. The request's Exception property may contain more Info about the error.
            case HTTPRequestStates.Error:
                action?.Invoke(response.DataAsText, false);
                Debug.Log("Request Finished with Error! " + (request.Exception != null ? (request.Exception.Message + "\n" + request.Exception.StackTrace) : "No Exception", LogType.Error));
                break;

            // The request aborted, initiated by the user.
            case HTTPRequestStates.Aborted:
                Debug.Log("Request Aborted!", LogType.Warning);
                break;

            // Connecting to the server is timed out.
            case HTTPRequestStates.ConnectionTimedOut:
                Debug.Log("Connection Timed Out!", LogType.Error);
                break;

            // The request didn't finished in the given time.
            case HTTPRequestStates.TimedOut:
                Debug.Log("Processing the request Timed Out!", LogType.Fatal);
                break;
        }
    }

    #endregion

    #region GraphQL Query Functions
    private static void MountQuery(GraphQL.Type[] args, ref string query, byte identationLevel = 2) {
        foreach (GraphQL.Type field in args) {
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

    public static async Task ExecuteRequest(string token, GraphQL.Type type, Action<string, bool> action, params GraphQL.Type[] args) {

        await Task.Run(() => {
            string query = ""; //query {\r\n
            query += (new string('\t', 1) + type.name);
            if (type.parameters != null) {
                query += " ( ";
                foreach (GraphQL.Params parameter in type.parameters)
                    query += (parameter.name + ": " + parameter.value + ", ");

                if (type.subfield != null)
                    query += " ) {\r\n";

            }
            if (type.subfield != null) {
                query += " ( ";
                foreach (GraphQL.Type subfield in type.subfield) {
                    query += subfield.name + ": { ";

                    if (subfield.parameters != null) {
                        foreach (GraphQL.Params parameter in subfield.parameters)
                            query += (parameter.name + ": " + parameter.value + ", ");



                    }

                    query += " } ";
                }
                query += " ) { \r\n";
            } else
                query += " ) { \r\n";

            MountQuery(args, ref query, 2);
            query += (new string('\t', 1) + "}\r\n");
            query = "query { " + query + "}";

            query = query.Replace("\n", " ").Replace("\t", " ").Replace("\r", " ");
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex(@"[ ]{2,}", options);
            query = regex.Replace(query, @" ");

            string jsonData = JsonConvert.SerializeObject(new { query });



            byte[] postData = Encoding.ASCII.GetBytes(jsonData);


            using (HTTPRequest request = new HTTPRequest(new Uri(_httpProtocol + _ip + _port + GraphqlPath), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) => OnRequestFinished(action, request, response))) {
                request.DisableCache = true;

                request.SetHeader("Content-Type", "application/json");
                request.SetHeader("Accept", "application/json");
                request.SetHeader("Keep-Alive", "timeout = 2, max = 20");
               
                if (token != "")
                    request.SetHeader("Authorization", token.Trim());

                request.RawData = Encoding.UTF8.GetBytes(jsonData);
                request.Send();

            }

        });
    }

    #endregion

}
