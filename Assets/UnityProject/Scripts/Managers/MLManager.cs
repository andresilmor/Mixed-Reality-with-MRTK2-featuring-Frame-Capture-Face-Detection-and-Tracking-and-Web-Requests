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
using Microsoft.MixedReality.Toolkit.UI;
using OpenCVForUnity.ImgprocModule;
using UnityEngine.Experimental.GlobalIllumination;
using System.IO;
using UnityEngine.XR.ARSubsystems;

#if ENABLE_WINMD_SUPPORT
using Windows.Foundation;
using Windows.Perception.Spatial;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Graphics.Holographic;
using Windows.Graphics.Imaging;
#endif

public static class MLManager
{
    private static Mat tempFrameMat = null;

    private static bool stop = false;

    public static async Task<bool> ToggleLiveDetection() {
#if ENABLE_WINMD_SUPPORT
        Debugger.AddText("Step One");
        AppCommandCenter.CameraFrameReader = await CameraFrameReader.CreateAsync();
        Debugger.AddText("Step Two");

        await FaceDetectionManager.Initialize();

#endif

        //AppCommandCenter.CameraFrameReader.FrameArrived += CameraServiceOnFrameArrivedSync;

        APIManager.CreateWebSocketLiveDetection(APIManager.mlLiveDetection, DetectionType.Person, MLManager.MapDetections);
        Debugger.AddText("Back");
        return APIManager.wsLiveDetection != null;

    }


    public static async void AnalyseFrame() {
        Debugger.SetFieldView();
        Debugger.AddText("Before snapshot");

        List<FaceDetectionManager.TrackedObject> trackedObjects = FaceDetectionManager.GetTrackedObjects();

        foreach (FaceDetectionManager.TrackedObject tracked in trackedObjects)
            Debugger.AddText("Snapshot rect is null? " + (tracked.rectSnapshot is null));

        FaceDetectionManager.CreateSnapshot();
        Debugger.AddText("After snapshot");

        trackedObjects = FaceDetectionManager.GetTrackedObjects();


        foreach (FaceDetectionManager.TrackedObject tracked in trackedObjects)
            Debugger.AddText("Snapshot rect is null? " + (tracked.rectSnapshot is null));


        Debugger.AddText("Done snapshot");

        return;

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
                        Debugger.AddText("1");
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        
                        //Debugger.AddText("1 Frame: " + (tempFrameMat is null).ToString());
                        Debugger.AddText("2");
                        tempFrameMat = CameraFrameReader.GenerateCVMat(lastFrame.mediaFrameReference);
                        Debugger.AddText("2 Frame: " + (tempFrameMat is null).ToString());


                        videoFrame.SoftwareBitmap.Dispose();
                         Debugger.AddText("3");
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.transform.position, Quaternion.identity);
                        UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), lastFrame.extrinsic.Position, Quaternion.identity);
                       
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        Debugger.AddText("4");
                        /*
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));
                        WebSocket wsTemp = APIManager.GetWebSocket(APIManager.mlLiveDetection);
                        if (wsTemp.IsOpen)
                        {
                        } else
                        {
                            wsTemp.Open();
                        }
                        wsTemp.Send("Sended");
                        Debugger.AddText("Sended");
                        Debugger.AddText(JsonUtility.ToJson(frame));
                        wsTemp.Send(JsonUtility.ToJson(frame));
                        */
                        ImageInferenceRequest message = new ImageInferenceRequest();
                        message.image = byteArray;
                        
                        APIManager.wsLiveDetection.Send(Parser.ProtoSerialize<ImageInferenceRequest>(message));

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
            PersonAndEmotionsInferenceReply.DetectionsList results = JsonConvert.DeserializeObject<PersonAndEmotionsInferenceReply.DetectionsList>(
                JsonConvert.DeserializeObject(predictions).ToString());

            Debugger.AddText(results.ToString());
        
            Vector3 worldPosition = Vector3.zero;
            Debugger.AddText("Test: " + results.detections[0].uuid);
            TrackerManager.ToUpdate = false;

            switch (detectionType) {
                case DetectionType.Person:
                    foreach (PersonAndEmotionsInferenceReply.Detection detection in results.detections) {
                        //MRWorld.GetWorldPosition(out worldPosition, detection.faceRect);
                        //Debugger.AddText("1 Position: " + worldPosition.ToString("0.############"));
                        try {
                            Debugger.AddText("1 Width: " + (detection.faceRect.x2 - detection.faceRect.x1) );
                            Debugger.AddText("1 Height: " + (detection.faceRect.y2 - detection.faceRect.y1) );
                            Debugger.AddText("1 X1: " + (detection.faceRect.x1));
                            Debugger.AddText("1 Y1: " + (detection.faceRect.y1) );
                            Debugger.AddText("1 X2: " + (detection.faceRect.x2));
                            Debugger.AddText("1 Y2: " + (detection.faceRect.y2));
                            Debugger.AddText("1 Mat Widht " + tempFrameMat.width());
                            Debugger.AddText("1 Mat Height " + tempFrameMat.height());

                            return;
                            if (!TrackerManager.LiveTrackers.ContainsKey(detection.uuid)) {
                                
                                TrackerHandler newTracker = TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, worldPosition, TrackerType.PacientTracker);

                                newTracker.SetIdentifier(detection.uuid);
                                TrackerManager.LiveTrackers.Add(detection.uuid, newTracker);

                                // ----------------------------------------------------------------------------------------------
                                // DANGER ZONE

                                //Texture2D texture = new Texture2D(forTest.cols(), forTest.rows(), TextureFormat.RGB24, false);

                                //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(forTest, texture);

                                //AppCommandCenter.Instance.screenTest.GetComponent<Renderer>().material.mainTexture = texture;
                                //AppCommandCenter.Instance.screenTest.transform.localScale = new Vector3(forTest.cols(), forTest.rows(), 1);


                                //Imgproc.rectangle(forTest, newTracker.TrackerSettings.boundingBox.tl(), newTracker.TrackerSettings.boundingBox.br(), new Scalar(255, 0, 255), 2, 1, 0);
                                //OpenCVForUnity.UnityUtils.Utils.matToTexture2D(forTest, texture);

                                //Debugger.AddText("Start save");

                                //var bytes = texture.EncodeToPNG();

                                
                                // ----------------------------------------------------------------------------------------------

                                //Texture2D tex = new Texture2D(forTest.cols(), forTest.rows(), TextureFormat.RGB24, false);
                                //tex.LoadImage(bytes);
                                //AppCommandCenter.Instance.screenTest.GetComponent<Renderer>().material.mainTexture = tex;
                                Debugger.AddText("Saved! ");


                            } else {

                                lock (TrackerManager.LiveTrackers[detection.uuid]) {
                                    (TrackerManager.LiveTrackers[detection.uuid].TrackerEntity as PacientTracker).UpdateActiveEmotion("Anger");

                                    TrackerManager.LiveTrackers[detection.uuid].RestartTracker(detection.faceRect, tempFrameMat);

                                    (TrackerManager.LiveTrackers[detection.uuid].TrackerEntity as PacientTracker).Window.SetPosition(worldPosition, instantMove: false);

                                    

                                }

                            }

                            TimedEventHandler timedEvent = TimedEventManager.GetTimedEvent(detection.uuid);
                            if (timedEvent != null)
                                ((TrackerManager.LiveTrackers[detection.uuid].TrackerEntity as PacientTracker).Window.components["NotificationAlert"] as GameObject).SetActive(timedEvent.TimeRunOut);

                        } catch (Exception ex) {
                            Debugger.AddText(ex.Message);

                        }

                    }

                    break;
            
            }

            TrackerManager.ToUpdate = true;
            if (TrackerManager.TrackersUpdater == null) {
                TrackerManager.TrackersUpdater = AppCommandCenter.Instance.StartCoroutine(TrackerManager.UpdateTrackers());
                Debugger.AddText("Updater Started");

            }

        } catch (Exception error) {
            Debugger.AddText("Error: " + error.Message.ToString());

        }

    }

}
