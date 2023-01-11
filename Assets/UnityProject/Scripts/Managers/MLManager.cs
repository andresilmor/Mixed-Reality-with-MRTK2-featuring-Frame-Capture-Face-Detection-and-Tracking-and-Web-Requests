using Newtonsoft.Json;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

using BestHTTP.WebSocket;
using UnityEngine.UIElements;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;
using UnityEngine.InputSystem.HID;
using OpenCVForUnity.VideoModule;
using UnityEngine.Playables;

#if ENABLE_WINMD_SUPPORT
using Windows.Foundation;
using Windows.Perception.Spatial;
using Windows.Media.Capture;
using Windows.Graphics.Holographic;
using Windows.Graphics.Imaging;
#endif

public static class MLManager
{
    private static Mat tempFrameMat = null;

    public static async Task<bool> ToggleLiveDetection() {
#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.CameraFrameReader = await CameraFrameReader.CreateAsync();
#endif

        APIManager.CreateWebSocketLiveDetection(APIManager.mlLiveDetection, DetectionType.Person, MLManager.MapDetections);

        return APIManager.wsLiveDetection != null;

    }

    public static async void AnalyseFrame() {
        Debugger.SetFieldView();
#if ENABLE_WINMD_SUPPORT
        var lastFrame = AppCommandCenter.CameraFrameReader.LastFrame;
        if (lastFrame.mediaFrameReference != null)
        {
            try
            {
                using (var videoFrame = lastFrame.mediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        
                        //Debugger.AddText("1 Frame: " + (tempFrameMat is null).ToString());
                        tempFrameMat = CameraFrameReader.GenerateCVMat(lastFrame.mediaFrameReference);
                        //Debugger.AddText("2 Frame: " + (tempFrameMat is null).ToString());


                        videoFrame.SoftwareBitmap.Dispose();
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.transform.position, Quaternion.identity);
                        UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), lastFrame.extrinsic.Position, Quaternion.identity);
                       
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));
                        WebSocket wsTemp = APIManager.GetWebSocket(APIManager.mlLiveDetection);
                        if (wsTemp.IsOpen)
                        {
                        } else
                        {
                            wsTemp.Open();
                        }

                        wsTemp.Send(JsonUtility.ToJson(frame));
                                          

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

    public static async void MapDetections(string predictions, DetectionType detectionType) {
        try {
            List<DetectionsList> results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
        
            Vector3 worldPosition = Vector3.zero;

            switch (detectionType) {
                case DetectionType.Person:
                    foreach (Detection detection in results[0].list) {
                        MRWorld.GetWorldPosition(out worldPosition, detection);

                        try {

                            if (!TrackerManager.LiveTrackers.ContainsKey(detection.id)) {
                                
                                TrackerHandler newTracker = TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, worldPosition, TrackerType.PacientTracker);

                                newTracker.SetIdentifier(detection.id);
                                TrackerManager.LiveTrackers.Add(detection.id, newTracker);

                            } else {

                                lock (TrackerManager.LiveTrackers[detection.id]) {
                                    (TrackerManager.LiveTrackers[detection.id].TrackerEntity as PacientTracker).UpdateActiveEmotion("Anger");

                                    TrackerManager.LiveTrackers[detection.id].UpdateTracker(detection.faceRect, tempFrameMat);

                                    (TrackerManager.LiveTrackers[detection.id].TrackerEntity as PacientTracker).Window.SetPosition(worldPosition, instantMove: false);

                                    

                                }

                            }

                            TimedEventHandler timedEvent = TimedEventManager.GetTimedEvent(detection.id);
                            if (timedEvent != null)
                                ((TrackerManager.LiveTrackers[detection.id].TrackerEntity as PacientTracker).Window.components["NotificationAlert"] as GameObject).SetActive(timedEvent.TimeRunOut);

                        } catch (Exception ex) {
                            Debugger.AddText(ex.Message);

                        }

                    }

                    break;
            
            }
            
        } catch (Exception error) {
            Debugger.AddText("Error: " + error.Message.ToString());

        }

    }

}
