using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.UI;
using Newtonsoft.Json;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class AppCommandCenter : MonoBehaviour
{
    //
    BinaryTree pacients;

    // For Debug
    [Header("Debugger:")]
    public TextMeshPro debugText;
    public GameObject cubeForTest;
    public GameObject sphereForTest;
    public GameObject lineForTest;
    public GameObject detectionName;

    // Controllers
    APIController apiController;


    // Attr's for the Machine Learning and detections
    private FrameHandler frameHandler;
    private Mat tempFrameMat;




    private bool justStop = false;
    private byte timeToStop = 0;


    async void Start()
    {
        LoadSavedData();
        SetDebugger();
        Debugger.SetFieldView();

        apiController = FindObjectOfType<APIController>();

#if ENABLE_WINMD_SUPPORT
        frameHandler = await FrameHandler.CreateAsync();
#endif

        apiController.CreateWebSocketConnection(apiController.pacientsDetection, MapPredictions);


    }


    private async void MapPredictions(string predictions)
    {
        Debugger.AddText("MapPredctions Called");
        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());

        //NewTracker(results[0].list[0].faceRect);
        foreach (Detection detection in results[0].list)
            TrackingManager.CreateTracker(detection.faceRect, tempFrameMat);

        FaceRect faceRect = results[0].list[0].faceRect;

        Vector2 unprojectionOffset = Vector2.zero; // Use 0,0 as default
        if ((faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f)) > Camera.main.pixelHeight / 2) // Got by trial and error
        {
            unprojectionOffset = new Vector2(0, -0.08f);

        }
        else
        {
            unprojectionOffset = new Vector2(0, -0.05f);
         

        }
        Debugger.AddText(unprojectionOffset.ToString());
        Vector3 position = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, Debugger.GetCubeForTest(), true, detectionName);
        if (!position.Equals(Vector3.zero)) { 
            GameObject two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), position, Quaternion.identity);
            GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, position + new Vector3(0, 0.10f, 0), Quaternion.identity);
            detectionTooltip.GetComponent<TextMeshPro>().SetText(results[0].list[0].id);
        }
        else
        {
            Debugger.AddText("Ya, no XD");
        }


        return;
    }

    private void SetDebugger()
    {
        Debugger.SetCubeForTest(cubeForTest);
        Debugger.SetSphereForTest(sphereForTest);
        Debugger.SetDebugText(debugText);
        LineDrawer.SetDrawLine(lineForTest);

    }

    private void LoadSavedData()
    {
        if (pacients == null)
            pacients = new BinaryTree();

    }

    public async void DetectPacients()
    {

        Debugger.AddText("So it starts");

        Debugger.AddText("Height: " + Camera.main.pixelHeight);
        Debugger.AddText("Width: " + Camera.main.pixelWidth);
        Debugger.AddText("Camera Rect: " + Camera.main.pixelRect.ToString());
        WebSocket wsTemp = apiController.GetWebSocket(apiController.pacientsDetection);
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

                        videoFrame.SoftwareBitmap.Dispose();
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        Instantiate(Debugger.GetSphereForTest(), Camera.main.transform.position, Quaternion.identity);
                        Instantiate(Debugger.GetCubeForTest(), lastFrame.extrinsic.Position, Quaternion.identity);
                       
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));
                        Debugger.AddText("Till the frame is ok");
                        WebSocket wsTemp = apiController.GetWebSocket(apiController.pacientsDetection);
                        if (wsTemp.IsOpen)
                        {
                            Debugger.AddText("Is Open");
                        } else
                        {
                            Debugger.AddText("Is not open");
                            wsTemp.Open();
                        }
                        wsTemp.Send("Sending");
                        wsTemp.Send(JsonUtility.ToJson(frame));
                        wsTemp.Send("Sended");
                        Debugger.AddText("And keeps ok XD");

                        /*
                        ws.Send("Sending");
                        ws.Send(JsonUtility.ToJson(frame));
                        ws.Send("Sended");
                        */
                        

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

    }



}
