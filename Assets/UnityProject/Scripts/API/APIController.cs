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

using Microsoft.MixedReality.Toolkit.Extensions;
using Microsoft.MixedReality.Toolkit;

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
    public GameObject cubeForTest;
    private DetectionsMapping detectionsMapping;
    async void Start()
    {

        ws = new WebSocket(new Uri(address));
        debugText.text = debugText.text + address;

        detectionsMapping = new DetectionsMapping(cubeForTest);
        

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


        Camera cam = Camera.main;

        debugText.text = debugText.text + "\nCamera Height: " + cam.pixelHeight + " Width: " + cam.pixelWidth;

        //detectionsMapping.MapDetection(new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2), Camera.main.transform.position, Camera.main.transform.rotation, debugText);


#if ENABLE_WINMD_SUPPORT
        
        frameHandler = await FrameHandler.CreateAsync(1504, 846);

#endif

        debugText.text = debugText.text + "\nCamera After Height: " + cam.pixelHeight + " Width: " + cam.pixelWidth;

        /*
       var camera = GameObject.FindGameObjectWithTag("Terminator").GetComponent<Camera>();
       Vector3[] frustumCorners = new Vector3[4];

       camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);

       GameObject topLeft = null;
       GameObject bottomRight = null;


       for (int i = 0; i < 4; i++)
       {
           var worldSpaceCorner = camera.transform.TransformVector(frustumCorners[i]);

           if (i == 1)
               topLeft = Instantiate(cubeForTest, worldSpaceCorner, Quaternion.identity);
           else if (i == 3)
               bottomRight = Instantiate(cubeForTest, worldSpaceCorner, Quaternion.identity);
           else
               Instantiate(cubeForTest, worldSpaceCorner, Quaternion.identity);

           Debug.Log(worldSpaceCorner);
           Debug.DrawRay(camera.transform.position, worldSpaceCorner, Color.blue);

   }

       //Instantiate(cubeForTest, new Vector3(topLeft.transform.position.x + (bottomRight.transform.position.x - topLeft.transform.position.x) / 2, topLeft.transform.position.y + (bottomRight.transform.position.y - topLeft.transform.position.y) / 2, camera.nearClipPlane), Quaternion.identity);
       

        trial = Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, cam.nearClipPlane)), Quaternion.identity);

        Debug.Log(camera.pixelWidth / 2);
        Debug.Log(camera.pixelHeight / 2);

        trial.transform.rotation = camera.transform.rotation;

        RaycastHit hit;

        if (Physics.Raycast(camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2, cam.nearClipPlane)), camera.transform.forward, out hit, Mathf.Infinity, 1 << 31)) // TODO: Check -1
        {
            Instantiate(cubeForTest, hit.point, Quaternion.identity);
        }


        Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(0, camera.pixelHeight, cam.nearClipPlane)), Quaternion.identity);
        Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, cam.nearClipPlane)), Quaternion.identity);

        Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, 0, cam.nearClipPlane)), Quaternion.identity);

        Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(0, 0, cam.nearClipPlane)), Quaternion.identity);
        /*
       float piX = 566;
       float piY = 377;

       Instantiate(cubeForTest, PixelConverter.ToWorldUnits(new Vector2(1133, 0), camera.transform.position), Quaternion.identity);
       Instantiate(cubeForTest, PixelConverter.ToWorldUnits(new Vector2(0, 755), camera.transform.position), Quaternion.identity);
       Instantiate(cubeForTest, PixelConverter.ToWorldUnits(new Vector2(1133, 755), camera.transform.position), Quaternion.identity);
       Instantiate(cubeForTest, PixelConverter.ToWorldUnits(new Vector2(0, 0), camera.transform.position), Quaternion.identity);
       Instantiate(cubeForTest, PixelConverter.ToWorldUnits(new Vector2(566, 377), camera.transform.position), Quaternion.identity);

       piX = piX >  camera.pixelWidth * 0.5f ? piX : (camera.pixelWidth * 0.5f) + (piX - camera.pixelWidth * 0.5f);
        piY = 0;
       
        Instantiate(cubeForTest, camera.ScreenToWorldPoint(new Vector3(/*piX >= camera.pixelWidth / 2 ? camera.pixelWidth / 2 + piX : camera.pixelWidth / 2 - piX piX,
            piY >= camera.pixelHeight / 2 ? camera.pixelHeight / 2 + piY : camera.pixelHeight / 2 - piY
            , cam.nearClipPlane)), Quaternion.identity); */

    }

    private void Update()
    {

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

        //List<DetectionsList> results = JsonConvert.DeserializeObject<List<DetectionsList>>(predictions);
        //debugText.text = debugText.text + "\n" + predictions;
        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());

        debugText.text = debugText.text + "\nDataObject";
        if (results[0].list.Count <= 0)
        {
            debugText.text = debugText.text + "\nExiting Definition";
            return;
        }

        debugText.text = debugText.text + "\nMessage: " + results[0].cameraLocation.position.ToString();

        //  Danger Zone  //
       

        BoundingBox b = results[0].list[0].box;
        
        debugText.text = debugText.text + "\nBoundingBox Center X: " + b.centerX.ToString() + " Y: " + b.centerY.ToString();
        OpenCVForUnity.CoreModule.Rect2d rect = new OpenCVForUnity.CoreModule.Rect2d(b.x1, b.y1, b.x2 - b.x1, b.y2 - b.y1);
        Vector3 unprojectionOffset = Vector3.zero;


        detectionsMapping.MapDetection(new Vector2(b.centerX, b.centerY), results[0].cameraLocation.position, results[0].cameraLocation.rotation, debugText);
        debugText.text = debugText.text + "\nThis is but a test";
        return;
#if ENABLE_WINMD_SUPPORT
        /*
        debugText.text = debugText.text + "\n TRACK ONE";

        Windows.Foundation.Point target = WorldOrigin.GetBoundingBoxTarget(rect, results[0].cameraLocation.forward);
        Vector2 unprojection = CameraIntrinsic.UnprojectAtUnitDepth(target, frameHandler.LastFrame.intrinsic);
        unprojection = CameraIntrinsic.UnprojectAtUnitDepth(new Windows.Foundation.Point(b.centerX, 936 - b.centerY), frameHandler.LastFrame.intrinsic);
        Vector3 correctedUnprojection = new Vector3(unprojection.x * 0.50f, unprojection.y * 0.50f, 1.0f);
        // debugText.text = debugText.text + "\n correctedUnprojection: " + correctedUnprojection.y.ToString();

        Quaternion rotation = Quaternion.LookRotation(-results[0].cameraLocation.forward, results[0].cameraLocation.upwards);
        Vector3 v = GetPosition(results[0].cameraLocation.position, Vector3.Normalize(rotation * correctedUnprojection));
        debugText.text = debugText.text + "\n TRACK TWO";
        // = Instantiate(cubeForTest, results[0].cameraLocation.position , Quaternion.identity);

        //Vector3 pos = PixelConverter.ToWorldUnits(new Vector2(720, 468), results[0].cameraLocation.position); 
        
        GameObject obj = Instantiate(cubeForTest, v, Quaternion.identity);
        */
        /*
        unprojection = CameraIntrinsic.UnprojectAtUnitDepth(new Windows.Foundation.Point(b.centerX, 936 - b.centerY), frameHandler.LastFrame.intrinsic);
        correctedUnprojection = new Vector3(unprojection.x, unprojection.y, 1.0f);
        v = GetPosition(results[0].cameraLocation.position, Vector3.Normalize(rotation * correctedUnprojection));
         obj = Instantiate(cubeForTest, v, Quaternion.identity);

*/


        /*

        foreach (Detection d in results[0].list)
        {
            debugText.text = debugText.text + "\n Detection " + d.name + " | Center X: " + d.box.centerX + " Center Y: " + d.box.centerY;
            obj = Instantiate(cubeForTest, new Vector3(d.box.centerX, d.box.centerY, results[0].cameraLocation.position.z) , Quaternion.identity);
        }
        */
        //GameObject obj = Instantiate(cubeForTest, v , Quaternion.identity);
        debugText.text = debugText.text + "\n TRACK THREE";
         


        /*
        RaycastHit hit;
        //obj.transform.rotation = Quaternion.LookRotation(results[0].cameraLocation.forward, results[0].cameraLocation.upwards);
        if (Physics.Raycast(obj.transform.position, obj.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, 1 << 31)) {
         debugText.text = debugText.text + "\n hit X: " + hit.point.x.ToString();
        debugText.text = debugText.text + "\n hit Y: " + hit.point.y.ToString();
        debugText.text = debugText.text + "\n hit Z: " + hit.point.z.ToString();
        }

        debugText.text = debugText.text + "\n obj X: " + obj.transform.position.x.ToString();
        debugText.text = debugText.text + "\n obj Y: " + obj.transform.position.y.ToString();
        debugText.text = debugText.text + "\n obj Z: " + obj.transform.position.z.ToString();
        //debugText.text = debugText.text + "\n GetPosition X: " + v.x.ToString();
        //debugText.text = debugText.text + "\n GetPosition Y: " + v.y.ToString();
        //debugText.text = debugText.text + "\n GetPosition Z: " + v.z.ToString();

         Vector3 pos = PixelConverter.ToWorldUnits(new Vector2(720, 468), results[0].cameraLocation.position); 

         obj = Instantiate(cubeForTest, pos, Quaternion.identity);
        
        debugText.text = debugText.text + "\n obj2 X: " + obj.transform.position.x.ToString();
        debugText.text = debugText.text + "\n obj2 Y: " + obj.transform.position.y.ToString();
        debugText.text = debugText.text + "\n obj2 Z: " + obj.transform.position.z.ToString();
        */
#endif
        debugText.text = debugText.text + "\n TRACK FOUR";

        //  You're safe now :3  //

    }   



    public async void ObjectPrediction()
    {

        Instantiate(cubeForTest, Camera.main.ScreenToWorldPoint(new Vector3(0, 846, Camera.main.nearClipPlane)), Quaternion.identity);
        Instantiate(cubeForTest, Camera.main.ScreenToWorldPoint(new Vector3(1504, 846, Camera.main.nearClipPlane)), Quaternion.identity);
        Instantiate(cubeForTest, Camera.main.ScreenToWorldPoint(new Vector3(1504, 0, Camera.main.nearClipPlane)), Quaternion.identity);
        Instantiate(cubeForTest, Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane)), Quaternion.identity);
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
                      
                        
                        /*
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)), new CameraLocation(lastFrame.extrinsic.Position, Quaternion.LookRotation(lastFrame.extrinsic.Forward, lastFrame.extrinsic.Upwards)));
                        */

                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)), new CameraLocation(Camera.main.transform.position, Camera.main.transform.rotation));

                        debugText.text = debugText.text + "\n extrinsic: " + lastFrame.extrinsic.ToString();
                        debugText.text = debugText.text + "\n raw Position: " + Camera.main.transform.position.ToString("f9");
                        debugText.text = debugText.text + "\n raw Rotation: " + Camera.main.transform.rotation.ToString("f9");
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
