using System;
using UnityEngine;

using TMPro;
using BestHTTP.WebSocket;
//using Newtonsoft.Json;

//These are needed, trust me ^-^ 
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Security.Cryptography;

using Windows.Media.Capture.Frames;
#endif



public class APIController : MonoBehaviour
{

    public string IP = "127.0.0.1";



    private FrameHandler frameHandler;

    public TextMeshPro debugText;

    private WebSocket ws;

    private byte[] recvBuffer = new byte[(int)1e5];


    private int testNum = 0;
    

    string address = "ws://192.168.1.238:8000/ws";



    async void Start()
    {

        ws = new WebSocket(new Uri(address));
        debugText.text = debugText.text + address;


        ws.OnMessage += (WebSocket webSocket, string message) =>
        {
            //Debug.Log("Text Message received from server: " + message);
            // JObject results = JObject.Parse(@message);


            if(message.Length > 0) {
                debugText.text = debugText.text + "\nJsonText";
                DefinePredictions(message);
                
            } else
                debugText.text = debugText.text + "\nOnMessage";

        };

        ws.OnClosed += (WebSocket webSocket, UInt16 code, string message) =>
        {
            Debug.Log("WebSocket is now Closed!");
        };

        ws.OnError += (WebSocket ws, string error) =>
        {
            debugText.text = debugText.text + "\nErro: " + error;
            Debug.LogError("Error: " + error);
        };

        ws.OnOpen += (WebSocket ws) =>
        {
            debugText.text = debugText.text + "\nOpen (Inside)";


        };

        ws.Open();


#if ENABLE_WINMD_SUPPORT
        frameHandler = await FrameHandler.CreateAsync(1504, 846);

#endif
    }

    public Vector3 GetPosition(Vector3 cameraPosition, Vector3 layForward)
    {
        if (!Microsoft.MixedReality.Toolkit.Utilities.SyncContextUtility.IsMainThread)
        {
            return Vector3.zero;
        }
        debugText.text = debugText.text + "\nCHOCKED";
        RaycastHit hit;
        if (!Physics.Raycast(cameraPosition, layForward * -1f, out hit, Mathf.Infinity, 1 << 31)) // TODO: Check -1
        {
#if ENABLE_WINMD_SUPPORT
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided.");
                debugText.text = debugText.text + "\nRaycast failed. Probably no spatial mesh provided.";
                return Vector3.positiveInfinity;
#else
            Debug.LogWarning("Raycast failed. Probably no spatial mesh provided. Use Holographic Remoting or HoloLens."); // TODO: Check mesh simulation
#endif
        }
        //frame.Dispose(); // TODO: Check disposal
        return hit.point;
    }



    private async void DefinePredictions(string predictions)
    {
        debugText.text = debugText.text + "\nInside Definition";

        List<DetectionsList> results = JsonConvert.DeserializeObject<List<DetectionsList>>(predictions);

        debugText.text = debugText.text + "\nDataObject";
        if (results[0].list.Count <= 0)
        {
            debugText.text = debugText.text + "\nExiting Definition";
            return;
        }

        debugText.text = debugText.text + "\nMessage: " + results[0].cameraLocation.position.ToString();

        //  Danger Zone  //
       

        BoundingBox b = results[0].list[0].box;
        
        debugText.text = debugText.text + "\nBoundingBox: " + b.x1.ToString();
        OpenCVForUnity.CoreModule.Rect2d rect = new OpenCVForUnity.CoreModule.Rect2d(b.x1, b.y1, b.x2 - b.x1, b.x2 - b.y1);


#if ENABLE_WINMD_SUPPORT
debugText.text = debugText.text + "\n rect: " + rect.x.ToString();
        Windows.Foundation.Point target = WorldOrigin.GetBoundingBoxTarget(rect, results[0].cameraLocation.forward);
        Vector2 unprojection = CameraIntrinsic.UnprojectAtUnitDepth(target, frameHandler.LastFrame.intrinsic);
        Vector3 correctedUnprojection = new Vector3(unprojection.x, unprojection.y, 1.0f);
        debugText.text = debugText.text + "\n correctedUnprojection: " + correctedUnprojection.y.ToString();

        Quaternion rotation = Quaternion.LookRotation(-results[0].cameraLocation.forward, results[0].cameraLocation.upwards);
        Vector3 v = GetPosition(results[0].cameraLocation.position, Vector3.Normalize(rotation * correctedUnprojection));

        
        debugText.text = debugText.text + "\n GetPosition: " + v.y.ToString();
#endif


        //  You're safe now :3  //

    }



    public async void ObjectPrediction()
    {

#if ENABLE_WINMD_SUPPORT
        var lastFrame = frameHandler.LastFrame;
        if (lastFrame.mediaFrameReference != null)
        {
            try
            {
                using (var videoFrame = lastFrame.mediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        //FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)), new CameraLocation(lastFrame.extrinsic.Position,lastFrame.extrinsic.Upwards,lastFrame.extrinsic.Forward));
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)), new CameraLocation(lastFrame.extrinsic.Position, lastFrame.extrinsic.Upwards, lastFrame.extrinsic.Forward));

                        debugText.text = debugText.text + "\n extrinsic: " + lastFrame.extrinsic.ToString();
                        debugText.text = debugText.text + "\n intrinsic: " + lastFrame.intrinsic.ToString();


                        ws.Send("Sending");
                        ws.Send(JsonUtility.ToJson(frame));
                        ws.Send("Sended");

                    }
                    else
                    { Debug.Log("videoFrame or SoftwareBitmap = null"); }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[### Deebug ###] Update Error: {ex.Message}");
            }
        }
        else
        { Debug.Log("lastFrame.mediaFrameReference = null"); }
#endif

    }

    void OnDestroy()
    {
        if (this.ws != null)
        {
            this.ws.Close();
            this.ws = null;
        }
    }
}
