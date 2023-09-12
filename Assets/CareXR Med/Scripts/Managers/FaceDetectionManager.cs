// Adapted from the WinML MNIST sample and Rene Schulte's repo 
// https://github.com/microsoft/Windows-Machine-Learning/tree/master/Samples/MNIST
// https://github.com/reneschulte/WinMLExperiments/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;
using static FaceDetectionCVManager;
using UnityEngine.Analytics;
using BestHTTP.WebSocket;
using Debug = XRDebug;
using Newtonsoft.Json.Linq;

#if ENABLE_WINMD_SUPPORT
using Windows.AI.MachineLearning;
using Windows.Storage.Streams;
using Windows.Media;
using Windows.Storage;
using Windows.Media.Capture;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Foundation;
using Windows.Media.FaceAnalysis;
#endif

public struct DetectedFaces {
#if ENABLE_WINMD_SUPPORT
    public SoftwareBitmap originalImageBitmap { get; set; }
#endif
    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }
    public DetectedFaceRect[] Faces { get; set; }
}
public struct DetectedFaceRect {
    public uint X { get; set; }
    public uint Y { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }
}

public static class FaceDetectionManager {

    public static int Counter;

#if ENABLE_WINMD_SUPPORT
    private static FaceDetector detector;
    private static IList<DetectedFace> detectedFaces;

#endif

#if ENABLE_WINMD_SUPPORT

    public static async Task<DetectedFaces> EvaluateVideoFrameAsync(SoftwareBitmap bitmap)
    {
        DetectedFaces result = new DetectedFaces();
      
        try{

            // Perform network model inference using the input data tensor, cache output and time operation
            result = await EvaluateFrame(bitmap);

        return result;
        }

         catch (Exception ex)
        {
            throw;
            return result;
        }

    }

   private static async Task<DetectedFaces> EvaluateFrame(SoftwareBitmap bitmap)
   {
			if (detector == null) {
                detector = await FaceDetector.CreateAsync();

            }

            //use NV12 for detections
			const BitmapPixelFormat faceDetectionPixelFormat = BitmapPixelFormat.Nv12;
            SoftwareBitmap convertedBitmap;
            //if frame not in NV12, convert
            if (bitmap.BitmapPixelFormat != faceDetectionPixelFormat)
            {
                convertedBitmap = SoftwareBitmap.Convert(bitmap, faceDetectionPixelFormat);
            }
            else
            {
                convertedBitmap = bitmap;
            }
			detectedFaces = await detector.DetectFacesAsync(convertedBitmap);
       
            return new DetectedFaces
			{
                originalImageBitmap = bitmap,
			    Faces = detectedFaces.Select(f => 
			        new DetectedFaceRect {X = f.FaceBox.X, Y = f.FaceBox.Y, Width = f.FaceBox.Width, Height = f.FaceBox.Height}).ToArray()
			};

   }
#endif

#if ENABLE_WINMD_SUPPORT

   public static void RGBDetectionToWorldspace(DetectedFaces result, CameraFrame returnFrame)
   {

        //Debug.Log("Number of faces: " + result.Faces.Count());
        //create a trasnform from camera space to world space
        var cameraToWorld = (System.Numerics.Matrix4x4)returnFrame.MediaFrameReference.CoordinateSystem.TryGetTransformTo(MRWorld.WorldOrigin);

        UnityEngine.Matrix4x4 unityCameraToWorld = NumericsConversionExtensions.ToUnity(cameraToWorld);
        var pixelsPerMeterAlongX = returnFrame.Intrinsic.FocalLength.x;
        var averagePixelsForFaceAt1Meter = pixelsPerMeterAlongX * 0.15f;
        
        foreach (DetectedFaceRect face in result.Faces)
        {
            double xCoord = (double)face.X + ((double)face.Width / 2.0F);
            double yCoord = (double)face.Y + ((double)face.Height / 2.0F);
            
            //vetor toward face at 1m
            System.Numerics.Vector2 projectedVector = returnFrame.Intrinsic.UnprojectAtUnitDepth(new Point(xCoord, yCoord));
            UnityEngine.Vector3 normalizedVector = NumericsConversionExtensions.ToUnity(new System.Numerics.Vector3(projectedVector.X, projectedVector.Y, -1.0f));
            normalizedVector.Normalize();
            //calculate estimated depth based on average face width and pixel width in detection
            float estimatedFaceDepth = averagePixelsForFaceAt1Meter / (float)face.Width;
            Vector3 targetPositionInCameraSpace = normalizedVector * estimatedFaceDepth;
            Vector3 bestRectPositionInWorldspace = unityCameraToWorld.MultiplyPoint(targetPositionInCameraSpace);
            //create object at established 3D coords

            var newObject = UnityEngine.Object.Instantiate(Controller.Instance.OutlinedCube, bestRectPositionInWorldspace, Quaternion.identity);

        }
 
    }

#endif


    async public static void OneShotFaceRecognition(object sender, FrameArrivedEventArgs args) {
        Debug.Log("OneShotFaceRecognition");
        Task thread = Task.Run(async () => {
            try {
                Task<DetectedFaces> evaluateVideoFrameTask = null;
                Debug.Log("OneShotFaceRecognition A");
                DetectedFaces result = new DetectedFaces();
                Debug.Log("OneShotFaceRecognition B");
#if ENABLE_WINMD_SUPPORT
                Debug.Log("OneShotFaceRecognition C");
                evaluateVideoFrameTask = FaceDetectionManager.EvaluateVideoFrameAsync(args.Frame.Bitmap);
                
                Debug.Log("OneShotFaceRecognition D");
                evaluateVideoFrameTask.Wait();
#endif

                Debug.Log("OneShotFaceRecognition E");
                result = evaluateVideoFrameTask.Result;

                if (result.Faces.Length > 0) {
                    //dfasdasd
                    if (APIManager.GetWebSocket(APIManager.FrameFaceRecognition) == null || !APIManager.GetWebSocket(APIManager.FrameFaceRecognition).IsOpen) {
                        APIManager.CreateWebSocketConnection(APIManager.FrameFaceRecognition, null, null,  (WebSocket ws) => {

                             MLManager.AnalyseFrame(args.Frame);
#if ENABLE_WINMD_SUPPORT
                            FaceDetectionManager.RGBDetectionToWorldspace(result, args.Frame);
/*
                        try {
                            byte[] byteArray = await Parser.ToByteArray(args.Frame.Bitmap);
                            ProtoImage request = new ProtoImage();
                            request.image = byteArray;

                            Debug.Log("Sending");
                          
                            APIManager.GetWebSocket(APIManager.FrameFaceRecognition).Send(Parser.ProtoSerialize<ProtoImage>(request));
                            Debug.Log("Sended");

                        } catch (Exception ex) {
                            Debug.Log(ex.Message);
                        }*/
#endif

                        }).Open();

                    } else {
                        Debug.Log("Not null?");

                    }

                    

                    Debug.Log("OneShotFaceRecognition F");


                  /*
                    APIManager.CreateWebSocketConnection(APIManager.FrameFullInference,
                            onOpen: (WebSocket ws) => {
                                Debug.Log("WebSocket Opened");
                                MLManager.AnalyseFrame(args.Frame);

                            },
                            onBinary: (byte[] response) => {
                                UnityEngine.WSA.Application.InvokeOnAppThread(() => {
                                    Debug.Log("Number of faces: " + result.Faces.Count());
#if ENABLE_WINMD_SUPPORT
                                    FaceDetectionManager.RGBDetectionToWorldspace(result, args.Frame);

#endif
                                }, true);

                            }
                        );
                    */
                }

            } catch (Exception ex) {
                Debug.Log("Exception:" + ex.Message);
            }

        });

        thread.Wait();

        if (MediaCaptureManager.PassiveServicesUsing == 0) {
#if ENABLE_WINMD_SUPPORT
            await MediaCaptureManager.StopMediaFrameReaderAsync();

#endif

        }

        FaceReconMenu.FaceReconActive = false;


    }



    public static void ProcessResults(List<PersonAndEmotionsInferenceReply.Detection> detections) {
        
        
        /*
        analysisResult = new Rect[detections.Count];
        try {

            for (int i = 0; i < detections.Count; i += 1) {
                analysisResult[i] = new Rect(
                    detections[i].faceRect.x1,
                    detections[i].faceRect.y1,
                    detections[i].faceRect.x2 - detections[i].faceRect.x1,
                detections[i].faceRect.y2 - detections[i].faceRect.y1);

                for (int j = 0; i < trackedObjects.Count; i++) {
                    if (trackedObjects[i].rectSnapshot is null)
                        continue;

                    if (RectContainsPoint(trackedObjects[j].rectSnapshot, detections[i].faceRect.x1 + (int)((detections[i].faceRect.x2 - detections[i].faceRect.x1) * 0.5), detections[i].faceRect.y1 + (int)((detections[i].faceRect.y2 - detections[i].faceRect.y1) * 0.5))) {
                        //Debug.Log("Yup");

                        if (trackedObjects[i].trackerEntity is null) {
                            UIWindow newMarker = UIManager.Instance.OpenWindowAt(WindowType.Sp_ML_E_1btn_Pacient, null, Quaternion.identity);
                            Debug.Log("Marker is active? " + (newMarker.gameObject.activeInHierarchy).ToString());


                            newMarker.gameObject.SetActive(true);
                            trackedObjects[i].trackerEntity = newMarker.gameObject.GetComponent<PacientTracker>();
                            (trackedObjects[i].trackerEntity as PacientTracker).Window = newMarker;
                            (trackedObjects[i].trackerEntity as PacientTracker).id = detections[i].uuid;

                            trackedObjects[i].IsNew = true;
                            trackedObjects[i].meshRenderer = newMarker.gameObject.GetComponent<MeshRenderer>();

                            (trackedObjects[i].trackerEntity as PacientTracker).UpdateActiveEmotion(detections[i].emotionsDetected.categorical[0]);
                            Debug.Log("Created");

                        } else {
                            if (trackedObjects[i].meshRenderer.isVisible) {
                                Debug.Log("Updated");
                                (trackedObjects[i].trackerEntity as PacientTracker).UpdateActiveEmotion(detections[i].emotionsDetected.categorical[0]);
                            }
                        }

                        trackedObjects[i].rectSnapshot = null;

                    }

                }

            }

            didUpdateTheDetectionResult = true;
            isAnalysingFrame = false;

        } catch (Exception ex) {
            Debug.Log("Error (ProcessResults): " + ex.Message);

        }
        */

    }

}