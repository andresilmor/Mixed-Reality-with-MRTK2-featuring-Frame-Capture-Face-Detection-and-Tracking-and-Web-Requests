using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.OpenXR.Input;

#if ENABLE_WINMD_SUPPORT
using Windows.Perception.Spatial;
using Microsoft.MixedReality.OpenXR;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices.Core;
#endif

public static class MRWorld {

    public static CameraExtrinsic tempExtrinsic = null;
    public static CameraIntrinsic tempIntrinsic = null;

    //blic static PixelPointRatio pixelPointRatio = new PixelPointRatio();

#if ENABLE_WINMD_SUPPORT
    private static SpatialCoordinateSystem _worldOrigin;
        public static SpatialCoordinateSystem WorldOrigin
        {
            get
            {
                if (_worldOrigin == null)
                {
                    _worldOrigin = CreateWorldOrigin();
                }
                return _worldOrigin;
            }
        }

#endif

    public struct PixelPointRatio {
        public float distPixel;
        public float distPoint;
    }


#if ENABLE_WINMD_SUPPORT
    private static SpatialCoordinateSystem CreateWorldOrigin()
        {
            //IntPtr worldOriginPtr = Microsoft.MixedReality.Toolkit.WindowsMixedReality.WindowsMixedRealityUtilities.UtilitiesProvider.ISpatialCoordinateSystemPtr;
            //WinRTExtensions.GetSpatialCoordinateSystem(coordinateSystemPtr); // https://github.com/microsoft/MixedReality-SpectatorView/blob/7796da6acb0ae41bed1b9e0e9d1c5c683b4b8374/src/SpectatorView.Unity/Assets/PhotoCapture/Scripts/WinRTExtensions.cs#L20
            var worldOriginPtr = SpatialLocator.GetDefault().CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem;
            SpatialCoordinateSystem origin = PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

            return origin;
        }

    private static SpatialCoordinateSystem RetrieveWorldOriginFromPointer(SpatialCoordinateSystem worldOriginPtr)
        {
            
            if (worldOriginPtr == null) throw new InvalidCastException("Failed to retrieve world origin from pointer");
            return worldOriginPtr;
        }


        /// <summary>
        /// Algorithm to approximate the vertical point in the bounding box regarding the user's position.
        /// </summary>
        /// OpenCV uses Row-major order (top-left is 0,0).
        /// Windows UWP uses Cartesian coordinate system (bottom left is 0,0).
        public static Windows.Foundation.Point GetBoundingBoxTarget(Rect2d rect, Vector4 cameraForward)
        {
            var cameraToGroundAngle = Vector3.Angle(cameraForward, Vector3.down);
            var offsetFactor = 0f;
            if (cameraToGroundAngle <= 90)
            {
                offsetFactor = 0.5f + cameraToGroundAngle / 180;
            }

            Point point = new Point(rect.x + rect.width / 2, rect.y + rect.height * offsetFactor);
            return new Windows.Foundation.Point(point.x, AppCommandCenter.cameraMain.pixelHeight - point.y); //846 = Frame Height, static now but we can turn dynamic and get from Intrinsics
        }

#endif


    /// <summary>
    /// Returns the unprojected forward vector. Fallback to default forward vector if UWP is not available.
    /// <see cref="VisualizationManager"/> may override fallback if main camera is available (only available on main thread).
    /// Adapted from https://github.com/abist-co-ltd/hololens-opencv-laserpointer/blob/master/Assets/Script/HololensLaserPointerDetection.cs.
    /// </summary>
#if ENABLE_WINMD_SUPPORT
    public static Vector3 GetLayForward(Vector2 unprojectionOffset, Windows.Foundation.Point target, CameraExtrinsic extrinsic, CameraIntrinsic intrinsic)
    {
        Vector4 forward = Vector4.one;
        Vector4 upwards = Vector4.one;
        forward = -extrinsic.Forward;
        upwards = extrinsic.Upwards;

        //Windows.Foundation.Point target = GetBoundingBoxTarget(extrinsic, boundingBox).ToWindowsPoint();
        Vector2 unprojection = intrinsic.UnprojectAtUnitDepth(target).ToUnity();
        Vector3 correctedUnprojection = new Vector3(unprojection.x + unprojectionOffset.x, unprojection.y + unprojectionOffset.y, 1.0f);
        Quaternion rotation = Quaternion.LookRotation(forward, upwards);
        Vector3 layForward = Vector3.Normalize(rotation * correctedUnprojection);


        if (layForward == Vector3.forward) Debug.LogWarning("Lay forward is forward vector.");
        if (layForward == Vector3.zero) Debug.LogWarning("Lay forward is zero vector.");
        return layForward;

    }
#endif

    // TODO: Dynamic offset, the higher the distance of the point to the camerera the lower it needs to go, actual value ~= 1m 
    public static Vector2 GetUnprojectionOffset(float posY) {
        Vector2 unprojectionOffset = Vector2.zero;


        unprojectionOffset = new Vector2(0, 0);

        /*
        if (posY > AppCommandCenter.cameraMain.pixelHeight / 2) // Got by trial and error / Negative = UP / Positive = Down
        {
            unprojectionOffset = new Vector2(0, -0.12f);
            unprojectionOffset = new Vector2(0, 0);
            Debugger.AddText("unprojectionOffset: " + unprojectionOffset.ToString("0.############"));
            Debugger.AddText("Unprojection A");
        } else {
            Debugger.AddText("Unprojection B");
            Debugger.AddText("distance: " + ((float)distance).ToString("0.############"));
            float distBlocks = (float)distance / 0.5706405f;
            Debugger.AddText("distBlocks: " + distBlocks.ToString("0.############"));

            float prediction = distBlocks * -0.07f;
            unprojectionOffset = new Vector2(0, prediction);

            Debugger.AddText("unprojectionOffset: " + unprojectionOffset.ToString("0.############")); 
            unprojectionOffset = new Vector2(0, 0);
            Debugger.AddText("unprojectionOffset: " + unprojectionOffset.ToString("0.############"));
            //unprojectionOffset = new Vector2(0, -0.07f);
            Debugger.AddText("Unprojection ---------------");
        }
        */
        return unprojectionOffset;

    }

    public static Vector3 GetPosition(Vector3 cameraPosition, Vector3 layForward, int layer) {
        if (!Microsoft.MixedReality.Toolkit.Utilities.SyncContextUtility.IsMainThread) {
            return Vector3.zero;
        }
        RaycastHit hit;
        if (!Physics.Raycast(cameraPosition, layForward * -1f, out hit, Mathf.Infinity, 1 << layer)) // TODO: Check -1
        {
#if ENABLE_WINMD_SUPPORT
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided.");
                return Vector3.positiveInfinity;
#else
            Debug.LogWarning("Raycast failed. Probably no spatial mesh provided. Use Holographic Remoting or HoloLens."); // TODO: Check mesh simulation
#endif
        }
        //frame.Dispose(); // TODO: Check disposal

        //UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), hit.point, Quaternion.identity);
        return hit.point;
    }




    /// <summary>
    /// Algorithm to approximate the vertical point in the bounding box regarding the user's position.
    /// </summary>
    public static Point GetBoundingBoxTarget(CameraExtrinsic extrinsic, BoxRect boundingBox) {
        var cameraForward = extrinsic.viewFromWorld.GetColumn(2);
        var cameraToGroundAngle = Vector3.Angle(cameraForward, Vector3.down);
        var offsetFactor = 0f;
        if (cameraToGroundAngle <= 90) {
            offsetFactor = 0.5f + cameraToGroundAngle / 180;
        }

        return new Point(boundingBox.x1 + ((boundingBox.x2 - boundingBox.x1) / 2), boundingBox.y1 + ((boundingBox.y2 - boundingBox.y1) * offsetFactor));
    }



    public static void UpdateExtInt(CameraExtrinsic cameraExtrinsic, CameraIntrinsic cameraIntrinsic) {
        tempExtrinsic = cameraExtrinsic;
        tempIntrinsic = cameraIntrinsic;

    }

    // ----------------------------------------------------------------------------------------------------------------//
    // ----------------------------------------------------------------------------------------------------------------//
    // ----------------------------------------------------------------------------------------------------------------//
    // ----------------------------------------------------------------------------------------------------------------//
    // ----------------------------------------------------------------------------------------------------------------//    


    // Used ON MACHINE LEARNINGGGGG
    public static void GetWorldPosition(out Vector3 worldPosition, PersonAndEmotionsInferenceReply.Detection detection) {
        // Keep at bay xd
        //Vector3 bodyPos = GetWorldPositionDirection(new OpenCVForUnity.CoreModule.Point(detection.bodyCenter.x, detection.bodyCenter.y));

        Debugger.AddText("ML Get World Position: ");

        Vector3 worldPosDist = GetWorldPositionDistance(detection.faceRect);
        Debugger.AddText("Calculated distance: " + Vector3.Distance(tempExtrinsic.Position, worldPosDist).ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, worldPosDist, UnityEngine.Color.green);


        Debugger.AddText("Modifed height: " + ((detection.faceRect.y2 - detection.faceRect.y1) * 0.5).ToString("0.############"));
        Debugger.AddText("Y2: " + detection.faceRect.y2.ToString("0.############"));
        //detection.faceRect.y2 = detection.faceRect.y2 - (int)((detection.faceRect.y2 - detection.faceRect.y1) * 0.5);

        Vector3 hitPoint = GetWorldPositionDirection(detection.faceRect);
        LineDrawer.Draw(tempExtrinsic.Position, hitPoint, UnityEngine.Color.red);
        Debugger.AddText("Y position: " + hitPoint.y.ToString("0.############"));
        /*if (Vector3.Distance(bodyPos, tempExtrinsic.Position) < Vector3.Distance(hitPoint, tempExtrinsic.Position)) {
            hitPoint.x = bodyPos.x;
            hitPoint.z = bodyPos.z;

        }*/
        //Debugger.AddText("Starting Positions: ");
        //Debugger.AddText("Camera:  " + tempExtrinsic.Position.ToString("0.############"));
        //Debugger.AddText("RayCast Face:  " + hitPoint.ToString("0.############"));
        //Debugger.AddText("RayCast Body:  " + bodyPos.ToString("0.############"));




        //Debugger.AddText("Pos Calculated:  " + worldPosDist.ToString("0.############"));


        worldPosition = new Vector3(hitPoint.x, hitPoint.y, worldPosDist.z);
        //Debugger.AddText("World Position:  " + worldPosition.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, hitPoint, UnityEngine.Color.magenta);

        Vector3 lerpedPosition = LerpByDistance(tempExtrinsic.Position, hitPoint, Vector3.Distance(tempExtrinsic.Position, worldPosition));

        //Debugger.AddText("Lerped  Position:  " + lerpedPosition.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, lerpedPosition, UnityEngine.Color.blue);


        worldPosition = Vector3.Distance(tempExtrinsic.Position, worldPosition) < Vector3.Distance(tempExtrinsic.Position, lerpedPosition) ? LerpByDistance(tempExtrinsic.Position, hitPoint, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)) : lerpedPosition;

        //Debugger.AddText("Lerped  Position 2:  " + LerpByDistance(tempExtrinsic.Position, hitPoint, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)).ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, LerpByDistance(tempExtrinsic.Position, hitPoint, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)), UnityEngine.Color.green);
        Debugger.AddText("Final distance: " + Vector3.Distance(tempExtrinsic.Position, worldPosition).ToString("0.############"));
        Debugger.AddText("Final Position:  " + worldPosition.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, worldPosition, UnityEngine.Color.yellow);
        /*
        Vector3 testWorldPosition = worldPosition;
        if (testWorldPosition.y > AppCommandCenter.cameraMain.pixelHeight / 2) // Got by trial and error / Negative = UP / Positive = Down
        {
            testWorldPosition.y = testWorldPosition.y;
        } else {
            testWorldPosition.y = (Vector3.Distance(tempExtrinsic.Position, testWorldPosition) * -0.07f) / 0.7654663f;
            LineDrawer.Draw(tempExtrinsic.Position, testWorldPosition, UnityEngine.Color.red);


        }*/


    }

    // Used ON the Tracking
    public static void GetWorldPosition(out Vector3 worldPosition, Rect2d boxRect) {
        Debugger.AddText("Tracking Get World Position: ");
        worldPosition = Vector3.zero;

        Vector3 worldPosDist = GetWorldPositionDistance(new BoxRect((int)boxRect.x, (int)boxRect.y, (int)boxRect.x + (int)boxRect.width, (int)boxRect.y + (int)boxRect.height));
        Debugger.AddText("Calculated:  " + worldPosDist.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, worldPosDist, UnityEngine.Color.green);


        Vector3 hitPos = GetWorldPositionDirection(new OpenCVForUnity.CoreModule.Point(boxRect.x + (boxRect.width / 2), boxRect.y + (boxRect.height / 2)));
        LineDrawer.Draw(tempExtrinsic.Position, hitPos, UnityEngine.Color.red);

        Debugger.AddText("Tracker Starting Positions: ");
        Debugger.AddText("Camera:  " + tempExtrinsic.Position.ToString("0.############"));
        Debugger.AddText("RayCast:  " + hitPos.ToString("0.############"));



        


        worldPosition = new Vector3(hitPos.x, hitPos.y, worldPosDist.z);
        //Debugger.AddText("HitPos Z Changed:  " + worldPosition.ToString("0.############"));
        //LineDrawer.Draw(tempExtrinsic.Position, worldPosition, UnityEngine.Color.blue);


        Vector3 lerpedPosition = LerpByDistance(tempExtrinsic.Position, hitPos, Vector3.Distance(tempExtrinsic.Position, worldPosition));
        //Debugger.AddText("LerpedPosition:  " + lerpedPosition.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, lerpedPosition, UnityEngine.Color.yellow);


        worldPosition = Vector3.Distance(tempExtrinsic.Position, worldPosition) < Vector3.Distance(tempExtrinsic.Position, lerpedPosition) ? LerpByDistance(tempExtrinsic.Position, hitPos, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)) : lerpedPosition;

        //Debugger.AddText("Second LerpedPosition:  " + LerpByDistance(tempExtrinsic.Position, hitPos, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)).ToString("0.############"));
        //Debugger.AddText("Final World Positon:  " + worldPosition.ToString("0.############"));
        LineDrawer.Draw(tempExtrinsic.Position, LerpByDistance(tempExtrinsic.Position, hitPos, Vector3.Distance(tempExtrinsic.Position, lerpedPosition)), UnityEngine.Color.blue);




    }

    public static Vector3 LerpByDistance(Vector3 A, Vector3 B, float x) {
        Vector3 P = x * Vector3.Normalize(B - A) + A;
        return P;

    }

    //Base:
    // (C#) https://github.com/cookieofcode/hololens2-unity-uwp-starter/blob/main/Unity/Assets/Scripts/HoloFaceTracker.cs   
    // (C++) https://github.com/microsoft/Windows-universal-samples/tree/main/Samples/HolographicFaceTracking/cpp
    // Old Name: GetWorldPositionByCalculation
    private static Vector3 GetWorldPositionDistance(BoxRect boxRect) {

#if ENABLE_WINMD_SUPPORT
        VideoMediaFrameFormat videoFormat = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.VideoMediaFrame.VideoFormat;
        SpatialCoordinateSystem cameraCoordinateSystem = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.CoordinateSystem;
        CameraIntrinsics cameraIntrinsics = AppCommandCenter.CameraFrameReader.LastFrame.mediaFrameReference.VideoMediaFrame.CameraIntrinsics;

        System.Numerics.Matrix4x4? cameraToWorld = cameraCoordinateSystem.TryGetTransformTo(WorldOrigin);

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


    // Old Name: GetWorldPositionByRaycast
    private static Vector3 GetWorldPositionDirection(OpenCVForUnity.CoreModule.Point boxCenter) {
        Vector2 unprojectionOffset = GetUnprojectionOffset((float)boxCenter.y);
        return GetWorldPositionOfPixel(boxCenter, unprojectionOffset);

    }

    // Old Name: GetWorldPositionByRaycast
    private static Vector3 GetWorldPositionDirection(BoxRect boxRect) {
        Vector2 unprojectionOffset = GetUnprojectionOffset(boxRect.y1 + ((boxRect.y2 - boxRect.y1) * 0.5f));
        return GetWorldPositionOfPixel(GetBoundingBoxTarget(tempExtrinsic, boxRect), unprojectionOffset);

    }




    // ATTENTION TO THIS IS FOR BOUNDIG BOX
    public static Vector3 GetWorldPositionOfPixel(Point pointCV, Vector2 unprojectionOffset, GameObject toInstantiate = null, int layer = 31, bool debug = false, GameObject debugText = null) {
        Debugger.ClearText();

        Vector3 layForward = Vector3.zero;

        Vector3 cameraPosition = tempExtrinsic.Position;
        Vector3 position = Vector3.zero;
        try {
#if ENABLE_WINMD_SUPPORT
        Windows.Foundation.Point target = pointCV.ToWindowsPoint();
        
        if (debug) {
            layForward = GetLayForward( Vector3.zero, target, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
            
            position = GetPosition(cameraPosition, layForward, layer);
            if (toInstantiate != null) {
                UnityEngine.Object.Instantiate(toInstantiate, cameraPosition, Quaternion.identity);
                UnityEngine.Object.Instantiate(toInstantiate, position, Quaternion.identity);
            }
        }
        layForward = GetLayForward(unprojectionOffset, target, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
      

        return GetPosition(cameraPosition, layForward, layer);  
#endif
        } catch (Exception e) {
            Debugger.AddText(e.Message);
        }

        return position;

    }

}
