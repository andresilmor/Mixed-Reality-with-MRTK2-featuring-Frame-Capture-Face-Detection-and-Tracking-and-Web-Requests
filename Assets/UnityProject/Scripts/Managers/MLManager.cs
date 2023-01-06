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

#if ENABLE_WINMD_SUPPORT
using Windows.Media.Capture.Frames;
using Windows.Foundation;
using Windows.Perception.Spatial;
using Windows.Media.Devices.Core;
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
                        
                        Debugger.AddText("1 Frame: " + (tempFrameMat is null).ToString());
                        tempFrameMat = CameraFrameReader.GenerateCVMat(lastFrame.mediaFrameReference);
                        Debugger.AddText("2 Frame: " + (tempFrameMat is null).ToString());


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
        Debugger.AddText("HERE");
        try {
            List<DetectionsList> results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
        
            Vector3 worldPosition = Vector3.zero;

            switch (detectionType) {
                case DetectionType.Person:
                    foreach (Detection detection in results[0].list) {
                        GetWorldPosition(out worldPosition, detection);

                        try {

                            if (!TrackerManager.LiveTrackers.ContainsKey(detection.id)) {

                                Debugger.AddText("NEW ON TREE");

                                TrackerHandler newTracker = TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, worldPosition, TrackerType.PacientTracker);

                                newTracker.SetIdentifier(detection.id);
                                TrackerManager.LiveTrackers.Add(detection.id, newTracker);


                            } else {
                                Debugger.AddText("ALREADY EXISTS ON TREE");

                                //TrackerManager.LiveTrackers[detection.id]

                                /*SEARCH BINARY TREE BY IDENTIFIER, ALSO LOCK WHILE CHANGING
                                if (node.data is PacientTracker) {
                                    (node.data as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0]);
                                    //(node.GraphQLData as Pacient).UpdateOneTracker(detection.faceRect, tempFrameMat);

                                }
                                */

                            }
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

    private static void GetWorldPosition(out Vector3 worldPosition, Detection detection) {
        Vector3 bodyPos = GetWorldPositionByRaycast(new OpenCVForUnity.CoreModule.Point(detection.bodyCenter.x, detection.bodyCenter.y));

        Vector3 facePos = GetWorldPositionByRaycast(detection.faceRect);

        if (Vector3.Distance(bodyPos, MRWorld.tempExtrinsic.Position) < Vector3.Distance(facePos, MRWorld.tempExtrinsic.Position)) {
            facePos.x = bodyPos.x;
            facePos.z = bodyPos.z;

        }

        LineDrawer.Draw(MRWorld.tempExtrinsic.Position, facePos, UnityEngine.Color.green);


        Vector3 worldPosCalculated = GetWorldPositionCalculation(detection.faceRect);


        worldPosition = new Vector3(facePos.x, facePos.y, worldPosCalculated.z);
        /*worldPosition.x += facePos.x;
        worldPosition.y += facePos.y;

        worldPosition.z += worldPosCalculated.z;
        */
        LineDrawer.Draw(MRWorld.tempExtrinsic.Position, worldPosition, UnityEngine.Color.blue);

    }


    //Base:
    // (C#) https://github.com/cookieofcode/hololens2-unity-uwp-starter/blob/main/Unity/Assets/Scripts/HoloFaceTracker.cs   
    // (C++) https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/HolographicFaceTracking/cpp
    private static Vector3 GetWorldPositionCalculation(BoxRect boxRect) {

#if ENABLE_WINMD_SUPPORT
        VideoMediaFrameFormat videoFormat = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.VideoMediaFrame.VideoFormat;
        SpatialCoordinateSystem cameraCoordinateSystem = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.CoordinateSystem;
        CameraIntrinsics cameraIntrinsics = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.VideoMediaFrame.CameraIntrinsics;

        System.Numerics.Matrix4x4? cameraToWorld = cameraCoordinateSystem.TryGetTransformTo(MRWorld.WorldOrigin);

        if (!cameraToWorld.HasValue) {
            return Vector3.zero;

        }

        float textureWidthInv = 1.0f / videoFormat.Width;
        float textureHeightInv = 1.0f / videoFormat.Height;

#endif

        int paddingForFaceRect = 24;
        float averageFaceWidthInMeters = 0.132f;

#if ENABLE_WINMD_SUPPORT
        float pixelsPerMeterAlongX = cameraIntrinsics.FocalLength.X;
        float averagePixelsForFaceAt1Meter = pixelsPerMeterAlongX * averageFaceWidthInMeters;

        BitmapBounds bestRect = new BitmapBounds();
        System.Numerics.Vector3 bestRectPositionInCameraSpace = System.Numerics.Vector3.Zero;

        float bestDotProduct = -1.0f;

#endif

        long faceWidth = Convert.ToInt64(boxRect.x2 - boxRect.x1);
        Debugger.AddText("Face Width: " + faceWidth);
        long centerX = Convert.ToInt64(boxRect.x1) + faceWidth / 2u;

        long faceHeight = Convert.ToInt64(boxRect.y2 - boxRect.y1);
        long centerY = Convert.ToInt64(boxRect.y1) + faceHeight / 2u;

#if ENABLE_WINMD_SUPPORT
        Windows.Foundation.Point faceRectCenterPoint = new Windows.Foundation.Point(centerX, centerY);

        System.Numerics.Vector2 centerOfFace = cameraIntrinsics.UnprojectAtUnitDepth(faceRectCenterPoint);

        System.Numerics.Vector3 vectorTowardsFace = System.Numerics.Vector3.Normalize(new System.Numerics.Vector3(centerOfFace.X, centerOfFace.Y, -1.0f));
        
        float estimatedFaceDepth = averagePixelsForFaceAt1Meter / faceWidth;

        float dotFaceWithGaze = System.Numerics.Vector3.Dot(vectorTowardsFace, -System.Numerics.Vector3.UnitZ);

        System.Numerics.Vector3 targetPositionInCameraSpace = vectorTowardsFace * estimatedFaceDepth;

        if (dotFaceWithGaze > bestDotProduct) {
            bestDotProduct = dotFaceWithGaze;
            //bestRect = faceRect;
            bestRectPositionInCameraSpace = targetPositionInCameraSpace;
            
        }

        System.Numerics.Vector3 bestRectPositionInWorldSpace = System.Numerics.Vector3.Transform(bestRectPositionInCameraSpace, cameraToWorld.Value);

        Vector3 positon = NumericsConversionExtensions.ToUnity(bestRectPositionInWorldSpace);
        
        
        return positon;

#endif

        return Vector3.zero;
    }


   
    private static Vector3 GetWorldPositionByRaycast(OpenCVForUnity.CoreModule.Point boxCenter) {
        Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset((float)boxCenter.y);
        return MRWorld.GetWorldPositionOfPixel(boxCenter, unprojectionOffset);

    }

    private static Vector3 GetWorldPositionByRaycast(BoxRect boxRect) {
        Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(boxRect.y1 + ((boxRect.y2 - boxRect.y1) * 0.5f));
        return MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, boxRect), unprojectionOffset);

    }

}
