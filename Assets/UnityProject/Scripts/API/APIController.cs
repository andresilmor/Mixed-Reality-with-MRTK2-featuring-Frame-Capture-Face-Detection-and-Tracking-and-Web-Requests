using System;
using System.Text.RegularExpressions;
using UnityEngine;

using TMPro;
using BestHTTP.WebSocket;
//using Newtonsoft.Json;

//These are needed, trust me ^-^ 
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
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
            debugText.text = debugText.text + "\nMessage: " + message;
            //Debug.Log("Text Message received from server: " + message);
            // JObject results = JObject.Parse(@message);
            if (message.Length > 0)
                DefinePredictions(message);
        
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


        FrameCapture f = new FrameCapture("bytes", new CameraLocation(new Vector4(3, 3, 3, 3), new Quaternion()));

        Debug.Log(JsonUtility.ToJson(f));

        Debug.Log(JsonUtility.FromJson<FrameCapture>(JsonUtility.ToJson(f)).cameraLocation.position);
   

        debugText.text = debugText.text + "\nWS Created/Opened";


#if ENABLE_WINMD_SUPPORT
        frameHandler = await FrameHandler.CreateAsync(1504, 846);

#endif
    }


    private async void DefinePredictions(string predictions)
    {
        //List<DetectionsList> results = 
        //    JsonConvert.DeserializeObject<List<DetectionsList>>(
        //        JsonConvert.DeserializeObject(predictions).ToString()
        //        );



        DetectionsList results = JsonUtility.FromJson<DetectionsList>(predictions);



        
        /*CameraExtrinsic Extrinsic = new CameraExtrinsic(trackedObject.Extrinsic);
        Vector3 cameraLocation = Extrinsic.Position;
        if (cameraLocation == Vector3.forward) Debug.LogWarning("Camera position is forward vector.");
        if (cameraLocation == Vector3.zero) Debug.LogWarning("Camera position is zero vector.");

        Debug.Log(GetPosition(cameraLocation,));
        */
    }
    /*
    public Vector3 GetPosition(Vector3 cameraLocation, Vector3 layForward)
    {
        if (!Microsoft.MixedReality.Toolkit.Utilities.SyncContextUtility.IsMainThread)
        {
            return Vector3.zero;
        }

        RaycastHit hit;
        if (!Physics.Raycast(cameraLocation, layForward * -1f, out hit, Mathf.Infinity, 1 << 31)) // TODO: Check -1
        {
#if ENABLE_WINMD_SUPPORT
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided.");
                return Vector3.positiveInfinity;
#else
            Debug.LogWarning("Raycast failed. Probably no spatial mesh provided. Use Holographic Remoting or HoloLens."); // TODO: Check mesh simulation
#endif
        }
        //frame.Dispose(); // TODO: Check disposal
        return hit.point;
    }

    */

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
                        byte[] byteArray = await toByteArray(videoFrame.SoftwareBitmap);
                        Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                        
                        //ws.Send(Convert.ToBase64String(byteArray));



                        //  Danger Zone  //


                        //CameraExtrinsic extrinsic = new CameraExtrinsic(lastFrame.mediaFrameReference.CoordinateSystem, WorldOrigin);
                        
                        
                        //debugText.text = "(" + (testNum).ToString() + ") intrinsic:" + intrinsic.ToString() +"\n \n";
                        //debugText.text = debugText.text + "(" + (testNum++).ToString() + ") extrinsic:" + extrinsic.ToString() +"\n \n";


                        FrameCapture frame = new FrameCapture(Convert.ToBase64String(byteArray), 
                                                                new CameraLocation(lastFrame.extrinsic.GetPosition(),lastFrame.extrinsic.GetRotation()));


                        Regex.Replace(frame.bytes, @"/\=+$/", "");
                        Regex.Replace(frame.bytes, @"/\//g", "_");
                        Regex.Replace(frame.bytes, @"/\+/g", "-");


                        ws.Send("Sending");
                        ws.Send(JsonUtility.ToJson(frame));
                        ws.Send("Sended");


                        //  You're safe now :3  //




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

#if ENABLE_WINMD_SUPPORT
    public async Task<byte[]> toByteArray(SoftwareBitmap sftBitmap_c)
    {
        SoftwareBitmap sftBitmap = SoftwareBitmap.Convert(sftBitmap_c, BitmapPixelFormat.Bgra8);
        InMemoryRandomAccessStream mss = new InMemoryRandomAccessStream();
        Windows.Graphics.Imaging.BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, mss);

        encoder.SetSoftwareBitmap(sftBitmap);
        await encoder.FlushAsync();

        IBuffer bufferr = new Windows.Storage.Streams.Buffer((uint)mss.Size);
        await mss.ReadAsync(bufferr, (uint)mss.Size, InputStreamOptions.None);

        DataReader dataReader = DataReader.FromBuffer(bufferr);
        byte[] bytes = new byte[bufferr.Length];
        dataReader.ReadBytes(bytes);
        return bytes;
    }
#endif


    void OnDestroy()
    {
        if (this.ws != null)
        {
            this.ws.Close();
            this.ws = null;
        }
    }
}
