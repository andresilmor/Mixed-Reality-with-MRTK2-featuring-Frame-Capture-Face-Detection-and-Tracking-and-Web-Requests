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

using Debug = MRDebug;

public static class MLManager
{
    private static Mat tempFrameMat = null;

    private static bool stop = false;

    public static async Task<bool> ToggleLiveDetection() {

        //APIManager.CreateWebSocketLiveDetection(APIManager.FrameFullInference, DetectionType.Person, FaceDetectionCVManager.ProcessResults);


#if ENABLE_WINMD_SUPPORT

        //await FaceDetectionManager.Initialize();

#endif

        //AppCommandCenter.MediaCaptureManager.FrameArrived += CameraServiceOnFrameArrivedSync;
        Debug.Log("Back");
        return APIManager.wsFrameInference != null;

    }


    public static async void AnalyseFrame(CameraFrame cameraFrame) {
        //MRDebug.DrawFieldView();
        //Debug.Log("Starting analyse");


#if ENABLE_WINMD_SUPPORT
        //var lastFrame = AppCommandCenter.MediaCaptureManager.LastFrame;



        if (cameraFrame.MediaFrameReference != null)
        {
            try
            {
                using (var videoFrame = cameraFrame.MediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        
                        videoFrame.SoftwareBitmap.Dispose();
                       
                        //UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.transform.position, Quaternion.identity);
                        //UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), cameraFrame.Extrinsic.Position, Quaternion.identity);
                       
                        ImageInferenceRequest request = new ImageInferenceRequest();
                        request.image = byteArray;
                        
                        APIManager.wsFrameInference.Send(Parser.ProtoSerialize<ImageInferenceRequest>(request));

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

            Debug.Log("Response: " + results.ToString());
        
            //Vector3 worldPosition = Vector3.zero;
            //Debug.Log("Test: " + results.detections[0].uuid);
            //TrackerManager.ToUpdate = false;

            //FaceDetectionCVManager.isAnalysingFrame = false;

            return;


            Vector3 worldPosition = Vector3.zero;

            switch (detectionType) {
                case DetectionType.Person:
                    foreach (PersonAndEmotionsInferenceReply.Detection detection in results.detections) {
                        //MRWorld.GetFaceWorldPosition(out worldPosition, detection.faceRect);
                        //Debug.Log("1 Position: " + worldPosition.ToString("0.############"));
                        try {
                            Debug.Log("1 Width: " + (detection.faceRect.x2 - detection.faceRect.x1) );
                            Debug.Log("1 Height: " + (detection.faceRect.y2 - detection.faceRect.y1) );
                            Debug.Log("1 X1: " + (detection.faceRect.x1));
                            Debug.Log("1 Y1: " + (detection.faceRect.y1) );
                            Debug.Log("1 X2: " + (detection.faceRect.x2));
                            Debug.Log("1 Y2: " + (detection.faceRect.y2));
                            Debug.Log("1 Mat Widht " + tempFrameMat.width());
                            Debug.Log("1 Mat Height " + tempFrameMat.height());

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

                                //Debug.Log("Start save");

                                //var bytes = texture.EncodeToPNG();

                                
                                // ----------------------------------------------------------------------------------------------

                                //Texture2D tex = new Texture2D(forTest.cols(), forTest.rows(), TextureFormat.RGB24, false);
                                //tex.LoadImage(bytes);
                                //AppCommandCenter.Instance.screenTest.GetComponent<Renderer>().material.mainTexture = tex;
                                Debug.Log("Saved! ");


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
                            Debug.Log(ex.Message);

                        }

                    }

                    break;
            
            }

            TrackerManager.ToUpdate = true;
            if (TrackerManager.TrackersUpdater == null) {
                TrackerManager.TrackersUpdater = AppCommandCenter.Instance.StartCoroutine(TrackerManager.UpdateTrackers());
                Debug.Log("Updater Started");

            }

        } catch (Exception error) {
            Debug.Log("Error: " + error.Message.ToString());

        }

    }

}
