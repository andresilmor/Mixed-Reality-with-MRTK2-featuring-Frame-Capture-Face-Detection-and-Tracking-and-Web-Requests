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
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.VideoModule;
using RectCV = OpenCVForUnity.CoreModule.Rect;



public class APIController : MonoBehaviour
{

    private FrameHandler frameHandler;

    private Mat tempFrameMat;

    public GameObject detectionName;

    private WebSocket ws;

    private bool justStop = false;
    private byte timeToStop = 0;

    
    string address = "ws://192.168.1.238:8000/ws";
    public GameObject cubeForTest;
    public GameObject sphereForTest;
    async void Start()
    {
        ws = new WebSocket(new Uri(address));

        

        ws.OnMessage += (WebSocket webSocket, string message) =>
        {
    

            if(message.Length > 0) {
                MapPredictions(message);

            } 

        };

        ws.OnClosed += (WebSocket webSocket, UInt16 code, string message) =>
        {
            Debug.Log("WebSocket is now Closed!");
        };

        ws.OnError += (WebSocket ws, string error) =>
        {
            Debug.LogError("Error: " + error);
        };

        ws.OnOpen += (WebSocket ws) =>
        {
            Debugger debugger = GameObject.FindObjectOfType<Debugger>();
            debugger.AddText("Connection Opened");
            ws.Send("Connection Opened");
            debugger.AddText("Warning Sended");
        };
        
        ws.Open();


        Camera cam = Camera.main;




#if ENABLE_WINMD_SUPPORT
        
        frameHandler = await FrameHandler.CreateAsync();

#endif


    }



    private async void MapPredictions(string predictions)
    {
        
        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());

        // --------------------------- DANGER ZONE ---------------------------- //

        NewTracker(results[0].list[0].faceRect);


        // ------------------------ You are safe now -------------------------- //


        MRWorld.Project2DBoundingBox(results[0].list[0], true, cubeForTest, detectionName);

        return;


    }

    // --------------------------- DANGER ZONE ---------------------------- //

    /// <summary>
    /// The trackers.
    /// </summary>


    private void NewTracker(FaceRect faceRect)
    {
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        
        debugger.AddText("Tracker");
        /*
        trackers = new List<TrackerSetting>();
        Point top = new Point(faceRect.x1, faceRect.y1);
        Point bottom = new Point(faceRect.x2, faceRect.y2);
        debugger.AddText("Top: " + top.ToString());
        debugger.AddText("Bottom: " + bottom.ToString());
        //ConvertScreenPointToTexturePoint(top, top, gameObject, 1440, 936);
        debugger.AddText("1");
        RectCV region = new RectCV(top, bottom);
        debugger.AddText("2");
        TrackerCSRT trackerCSRT = TrackerCSRT.create(new TrackerCSRT_Params());
        debugger.AddText("3");
        trackerCSRT.init(tempFrameMat, region);
        debugger.AddText("4");
        trackers.Add(new TrackerSetting(trackerCSRT, trackerCSRT.GetType().Name.ToString(), new Scalar(0, 255, 0)));
        debugger.AddText("5");*/

        TrackingManager.CreateTracker(faceRect, tempFrameMat);
        debugger.AddText("1");


    }

    void Update()
    {
        if (!justStop)
        {
            
#if ENABLE_WINMD_SUPPORT
            bool wasUpdated = TrackingManager.UpdateTrackers(frameHandler.LastFrame.frameMat);
            if (wasUpdated) {
                timeToStop++;
                if (timeToStop >= 20)
                    justStop = true;
            }
#endif
            
        }

        /*
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
            debugger.AddText("trackers count: " + trackers.Count);
            for (int i = 0; i < trackers.Count; i++)
        {
            Tracker tracker = trackers[i].tracker;
            RectCV boundingBox = trackers[i].boundingBox;
#if ENABLE_WINMD_SUPPORT
            var lastFrame = frameHandler.LastFrame;
            tracker.update(lastFrame.frameMat, boundingBox);

            if (tracker is TrackerCSRT)
            {

                debugger.AddText(boundingBox.ToString());
         
            }
            //Imgproc.rectangle(rgbMat, boundingBox.tl(), boundingBox.br(), lineColor, 2, 1, 0);
            

        }
        */
    }


    
    class TrackerSetting
    {
        public Tracker tracker;
        public string label;
        public Scalar lineColor;
        public RectCV boundingBox;

        public TrackerSetting(Tracker tracker, string label, Scalar lineColor)
        {
            this.tracker = tracker;
            this.label = label;
            this.lineColor = lineColor;
            this.boundingBox = new RectCV();
        }

        public void Dispose()
        {
            if (tracker != null)
            {
                tracker.Dispose();
                tracker = null;
            }
        }
    }


    // ------------------------ You are safe now -------------------------- //



    public async void ObjectPrediction()
    {
        
        GameObject te = null;
        te = Instantiate(sphereForTest, Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.nearClipPlane)), Quaternion.identity);
        RaycastHit hit;
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        GameObject two = Instantiate(cubeForTest, hit.point, Quaternion.identity);
        gameObject.GetComponent<LineDrawer>().Draw(te.transform.position, two.transform.position, Color.yellow);
        te = Instantiate(sphereForTest, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.nearClipPlane)), Quaternion.identity);

        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = Instantiate(cubeForTest, hit.point, Quaternion.identity);
        gameObject.GetComponent<LineDrawer>().Draw(te.transform.position, two.transform.position, Color.yellow);
        te = Instantiate(sphereForTest, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, Camera.main.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = Instantiate(cubeForTest, hit.point, Quaternion.identity);
        gameObject.GetComponent<LineDrawer>().Draw(te.transform.position, two.transform.position, Color.yellow);
        te = Instantiate(sphereForTest, Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)), Quaternion.identity);
        Physics.Raycast(te.transform.position, Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.farClipPlane)), out hit, Mathf.Infinity, 1 << 31);
        two = Instantiate(cubeForTest, hit.point, Quaternion.identity);
        gameObject.GetComponent<LineDrawer>().Draw(te.transform.position, two.transform.position, Color.yellow);
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        debugger.AddText("So it starts");

        debugger.AddText("Height: " + Camera.main.pixelHeight);
        debugger.AddText("Width: " + Camera.main.pixelWidth);
        debugger.AddText("Camera Rect: " + Camera.main.pixelRect.ToString());

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
                        
                        
                        tempFrameMat = lastFrame.frameMat;

                        // --------------------------- DANGER ZONE ---------------------------- //



                        // ------------------------ You are safe now -------------------------- //


                        videoFrame.SoftwareBitmap.Dispose();
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        
                        Instantiate(sphereForTest, Camera.main.transform.position, Quaternion.identity);
                        Instantiate(cubeForTest, lastFrame.extrinsic.Position, Quaternion.identity);
                       
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));

                        
                        

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
        { Debug.Log("lastFrame.mediaFrameReference = null"); 
        }
#endif

    }

    void OnDestroy()
    {
        if (this.ws != null)
        {
            //this.ws.Close();
            //this.ws = null;
        }
    }
}
