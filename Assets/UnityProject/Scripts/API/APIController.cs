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



public class APIController : MonoBehaviour
{

    private FrameHandler frameHandler;

    public GameObject detectionName;

    private WebSocket ws;


    
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


        MRWorld.Project2DBoundingBox(results[0].list[0], true, cubeForTest, detectionName);

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
        Debugger debugger = GameObject.FindObjectOfType<Debugger>();
        debugger.AddText("So it starts");

#if ENABLE_WINMD_SUPPORT
        var lastFrame = frameHandler.LastFrame;
        if (lastFrame.mediaFrameReference != null)
        {debugger.AddText("1");
            try
            {debugger.AddText("1.1");
                using (var videoFrame = lastFrame.mediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {debugger.AddText("1.2");
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {debugger.AddText("2");
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        
                        debugger.AddText("2.1");
                        videoFrame.SoftwareBitmap.Dispose();
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        
                        Instantiate(sphereForTest, Camera.main.transform.position, Quaternion.identity);
                        Instantiate(cubeForTest, lastFrame.extrinsic.Position, Quaternion.identity);
                        debugger.AddText("2.2");
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        debugger.AddText("3");
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));

                        debugger.AddText("4");
                        

                        ws.Send("Sending");
                        ws.Send(JsonUtility.ToJson(frame));
                        ws.Send("Sended");
                        debugger.AddText("5");
                        //debugger.AddText(frame.bytes.ToString());

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
        debugger.AddText("0");}
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
