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
using OpenCVForUnity.CoreModule;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Security.Cryptography;
using System.Windows;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
#endif



public class APIController : MonoBehaviour
{

    private FrameHandler frameHandler;

    public TextMeshPro debugText;
    public GameObject detectionName;

    private WebSocket ws;


    private CameraExtrinsic tempExtrinsic = null;
    private CameraIntrinsic tempIntrinsic = null;

    
    string address = "ws://192.168.1.238:8000/ws";
    public GameObject cubeForTest;
    public GameObject sphereForTest;
    async void Start()
    {

        ws = new WebSocket(new Uri(address));

        

        ws.OnMessage += (WebSocket webSocket, string message) =>
        {
    

            if(message.Length > 0) {
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



#if ENABLE_WINMD_SUPPORT
        
        frameHandler = await FrameHandler.CreateAsync();

#endif


    }

    private void Update()
    {
    }





    private async void DefinePredictions(string predictions)
    {
        
        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());


#if ENABLE_WINMD_SUPPORT


        Vector3 cameraPosition = this.tempExtrinsic.Position;
        
        Vector3 layForward = MRWorld.GetLayForward(new Vector2(0,0), results[0].list[0].faceRect, this.tempExtrinsic, this.tempIntrinsic);

        Vector3 position = MRWorld.GetPosition(cameraPosition, layForward);

        GameObject one = Instantiate(cubeForTest, cameraPosition, Quaternion.identity);

        GameObject two = Instantiate(cubeForTest, position, Quaternion.identity);
        

        
        if ((results[0].list[0].faceRect.y1 + ((results[0].list[0].faceRect.y2 - results[0].list[0].faceRect.y1) * 0.5f)) > Camera.main.pixelHeight / 2) {

            layForward = MRWorld.GetLayForward(new Vector2(0,-0.08f), results[0].list[0].faceRect, this.tempExtrinsic, this.tempIntrinsic);
            position = MRWorld.GetPosition(cameraPosition, layForward);
            two = Instantiate(cubeForTest, position, Quaternion.identity);
            gameObject.GetComponent<LineDrawer>().Draw(cameraPosition, position, Color.black);
    

        } else {

            layForward = MRWorld.GetLayForward(new Vector2(0,-0.05f), results[0].list[0].faceRect, this.tempExtrinsic, this.tempIntrinsic);
            position = MRWorld.GetPosition(cameraPosition, layForward);
            two = Instantiate(cubeForTest, position, Quaternion.identity);
            gameObject.GetComponent<LineDrawer>().Draw(cameraPosition, position, Color.black); // < Seems to work so far

        }

        GameObject detectionTooltip = Instantiate(detectionName, position + new Vector3(0, 0.10f, 0), Quaternion.identity);
        detectionTooltip.GetComponent<TextMeshPro>().SetText(results[0].list[0].id);






        //Second method test FOLLOWING THE METHOD FROM FACE TRACKIN UNITY IT MAY BE NEEDED IN THE FUTURE, SO LETS KEEP ....  <======================
        /*
        debugText.text = "";

        debugText.text = debugText.text + "\nPrev Position X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");

        System.Numerics.Matrix4x4? cameraToWorld = this.tempExtrinsic.cameraToWorld;

        // If we can't locate the world, this transform will be null.
        if (!cameraToWorld.HasValue)
        {
        debugText.text = debugText.text + "\n No Value";
            return;
        }


        float textureWidthInv = 1.0f / 1504;
        float textureHeightInv = 1.0f / 846;

        int paddingForFaceRect = 24;
        float averageFaceWidthInMeters = 0.15f;

        float pixelsPerMeterAlongX = this.tempIntrinsic.FocalLength.x;
        debugText.text = debugText.text + "\n FocalLength.x " + this.tempIntrinsic.FocalLength.x.ToString("f9");
        float averagePixelsForFaceAt1Meter = pixelsPerMeterAlongX * averageFaceWidthInMeters;

        System.Numerics.Vector3 cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.25f, 0.0f);
        BitmapBounds bestRect = new BitmapBounds();
        System.Numerics.Vector3 bestRectPositionInCameraSpace = System.Numerics.Vector3.Zero;
        float bestDotProduct = -1.0f;

        Windows.Foundation.Point faceRectCenterPoint = new Windows.Foundation.Point(results[0].list[0].box.x1 + ((results[0].list[0].box.x2 - results[0].list[0].box.x1) / 2u), results[0].list[0].box.y1 + ((results[0].list[0].box.y2 - results[0].list[0].box.x1) / 2u));

        System.Numerics.Vector2 centerOfFace = this.tempIntrinsic.UnprojectAtUnitDepth(faceRectCenterPoint);

        System.Numerics.Vector3 vectorTowardsFace = System.Numerics.Vector3.Normalize(new System.Numerics.Vector3(centerOfFace.X, centerOfFace.Y, -1.0f));

        float estimatedFaceDepth = averagePixelsForFaceAt1Meter / (results[0].list[0].box.x2 - results[0].list[0].box.x1);

        float dotFaceWithGaze = System.Numerics.Vector3.Dot(vectorTowardsFace, -System.Numerics.Vector3.UnitZ);

        System.Numerics.Vector3 targetPositionInCameraSpace = vectorTowardsFace * estimatedFaceDepth;

        if (dotFaceWithGaze > bestDotProduct)
            {
                bestDotProduct = dotFaceWithGaze;
                //bestRect = faceRect;
                bestRectPositionInCameraSpace = targetPositionInCameraSpace;
            }

        System.Numerics.Vector3 bestRectPositionInWorldspace = System.Numerics.Vector3.Transform(bestRectPositionInCameraSpace, cameraToWorld.Value);

        if (results[0].list[0].box.centerY > Camera.main.pixelHeight / 2) {
            cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.12f, 0.0f);
            position = (bestRectPositionInWorldspace - cubeOffsetInWorldSpace).ToUnity();
            debugText.text = debugText.text + "\nNew PositionOffset X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");
            two = Instantiate(cubeForTest, position, Quaternion.identity);
            gameObject.GetComponent<LineDrawer>().Draw(cameraPosition,  position, Color.cyan);

        } else {
        
            cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.25f, 0.0f);
            position = (bestRectPositionInWorldspace - cubeOffsetInWorldSpace).ToUnity();
            debugText.text = debugText.text + "\nNew PositionOffset X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");
            two = Instantiate(cubeForTest, position, Quaternion.identity);
            gameObject.GetComponent<LineDrawer>().Draw(cameraPosition,  position, Color.green);
        }

        


        position = bestRectPositionInWorldspace.ToUnity();
        debugText.text = debugText.text + "\nNew Position X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");
        two = Instantiate(cubeForTest, position, Quaternion.identity);
        gameObject.GetComponent<LineDrawer>().Draw(cameraPosition,  position, Color.yellow);


        // tEST END
        */


        this.tempExtrinsic = null;

        this.tempIntrinsic = null;




        

#endif

        return;


    }   



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

        if (tempIntrinsic != null || tempExtrinsic != null)
        {
            debugText.text = debugText.text + "\n FUUUCCCCCKKKKKK";
            return;
        }

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
                      
                        
                        Instantiate(sphereForTest, Camera.main.transform.position, Quaternion.identity);
                        Instantiate(cubeForTest, lastFrame.extrinsic.Position, Quaternion.identity);
                        this.tempExtrinsic = lastFrame.extrinsic;
                        this.tempIntrinsic = lastFrame.intrinsic;
                        
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
