using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

#if ENABLE_WINMD_SUPPORT
using Windows.Perception.Spatial;
using Microsoft.MixedReality.OpenXR;
using Windows.Graphics.Imaging;
#endif

public static class MRWorld {

    public static CameraExtrinsic tempExtrinsic = null;
    public static CameraIntrinsic tempIntrinsic = null;

    //blic static PixelPointRatio pixelPointRatio = new PixelPointRatio();

#if ENABLE_WINMD_SUPPORT
    private static SpatialCoordinateSystem _worldOrigin;
        public static SpatialCoordinateSystem worldOrigin
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


    public static Vector2 GetUnprojectionOffset(float posY) {
        Vector2 unprojectionOffset = Vector2.zero;
        if (posY > AppCommandCenter.cameraMain.pixelHeight / 2) // Got by trial and error
        {
            unprojectionOffset = new Vector2(0, -0.05f);
            Debugger.AddText("Unprojection A");
        } else {
            unprojectionOffset = new Vector2(0, -0.08f);
            Debugger.AddText("Unprojection B");

        }

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
    public static Point GetBoundingBoxTarget(CameraExtrinsic extrinsic, FaceRect boundingBox) {
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


    public static float ConvertPixelDistToPoint(float pixelDist) {
        return 0; ///elDist * pixelPointRatio.distPoint) / pixelPointRatio.distPixel) * 0.25f;
    }


    // ATTENTION TO THIS IS FOR BOUNDIG BOX
    public static Vector3 GetWorldPositionOfPixel(Point pointCV, Vector2 unprojectionOffset, uint boundigBoxWidth, GameObject toInstantiate = null, int layer = 31, bool debug = false, GameObject debugText = null) {
        Debugger.ClearText();

        Vector3 layForward = Vector3.zero;

        Vector3 cameraPosition = MRWorld.tempExtrinsic.Position;
        Vector3 position = Vector3.zero;
        Debugger.AddText("yo");
        try {
#if ENABLE_WINMD_SUPPORT
        Windows.Foundation.Point target = pointCV.ToWindowsPoint();
        
        if (debug) {
            layForward = MRWorld.GetLayForward( Vector3.zero, target, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
            
            position = MRWorld.GetPosition(cameraPosition, layForward, layer);
            if (toInstantiate != null) {
                UnityEngine.Object.Instantiate(toInstantiate, cameraPosition, Quaternion.identity);
                UnityEngine.Object.Instantiate(toInstantiate, position, Quaternion.identity);
            }
        }
        layForward = MRWorld.GetLayForward(unprojectionOffset, target, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
        Debugger.AddText("yo");

        return MRWorld.GetPosition(cameraPosition, layForward, layer);




        layForward = MRWorld.GetLayForward(unprojectionOffset, target, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);







        //Second method test FOLLOWING THE METHOD FROM FACE TRACKIN UNITY IT MAY BE NEEDED IN THE FUTURE, SO LETS KEEP ....  <======================

      

        System.Numerics.Matrix4x4? cameraToWorld = MRWorld.tempExtrinsic.cameraToWorld;


        float textureWidthInv = 1.0f / MRWorld.tempIntrinsic.videoFormat.Width;
        float textureHeightInv = 1.0f / MRWorld.tempIntrinsic.videoFormat.Height;
        int paddingForFaceRect = 24;
        float averageFaceWidthInMeters = 0.15f;

        float pixelsPerMeterAlongX = MRWorld.tempIntrinsic.FocalLength.x;
        float averagePixelsForFaceAt1Meter = pixelsPerMeterAlongX * averageFaceWidthInMeters;
         Debugger.AddText("6");
        System.Numerics.Vector3 cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.25f, 0.0f);
        BitmapBounds bestRect = new BitmapBounds();
        System.Numerics.Vector3 bestRectPositionInCameraSpace = System.Numerics.Vector3.Zero;
        float bestDotProduct = -1.0f;
        Windows.Foundation.Point faceRectCenterPoint = target;

        System.Numerics.Vector2 centerOfFace = MRWorld.tempIntrinsic.UnprojectAtUnitDepth(faceRectCenterPoint);

        System.Numerics.Vector3 vectorTowardsFace = System.Numerics.Vector3.Normalize(new System.Numerics.Vector3(centerOfFace.X, centerOfFace.Y, -1.0f));

        float estimatedFaceDepth = averagePixelsForFaceAt1Meter / boundigBoxWidth;

        float dotFaceWithGaze = System.Numerics.Vector3.Dot(vectorTowardsFace, -System.Numerics.Vector3.UnitZ);

        System.Numerics.Vector3 targetPositionInCameraSpace = vectorTowardsFace * estimatedFaceDepth;
        if (dotFaceWithGaze > bestDotProduct)
            {
             Debugger.AddText("Inside IF");
                bestDotProduct = dotFaceWithGaze;
                //bestRect = faceRect;
                bestRectPositionInCameraSpace = targetPositionInCameraSpace;
            }
        System.Numerics.Vector3 bestRectPositionInWorldspace = System.Numerics.Vector3.Transform(bestRectPositionInCameraSpace, cameraToWorld.Value);
        Vector3 bestRectPositionInWorldspaceUnity = (bestRectPositionInWorldspace + cubeOffsetInWorldSpace).ToUnity();
        position = MRWorld.GetPosition(MRWorld.tempExtrinsic.Position, layForward, layer);
        UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), bestRectPositionInWorldspaceUnity, Quaternion.identity);
        if (Vector3.Distance(bestRectPositionInWorldspaceUnity, MRWorld.tempExtrinsic.Position) < Vector3.Distance(position, MRWorld.tempExtrinsic.Position))
        {
            //layForward = MRWorld.GetLayForward(unprojectionOffset, bestRectPositionInWorldspaceUnity, MRWorld.tempExtrinsic, MRWorld.tempIntrinsic);
            return bestRectPositionInWorldspaceUnity;
        }
        
#endif
        } catch (Exception e) {
            Debugger.AddText(e.Message);
        }

        return position;

        /*

        if (results[0].list[0].box.centerY > AppCommandCenter.cameraMain.pixelHeight / 2) {
            cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.12f, 0.0f);
            position = (bestRectPositionInWorldspace - cubeOffsetInWorldSpace).ToUnity();
            debugText.text = debugText.text + "\nNew PositionOffset X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");
            two = Instantiate(Debugger.GetCubeForTest(), position, Quaternion.identity);
            LineDrawer.Draw(cameraPosition,  position, Color.cyan);

        } else {
        
            cubeOffsetInWorldSpace = new System.Numerics.Vector3(0.0f, 0.25f, 0.0f);
            position = (bestRectPositionInWorldspace - cubeOffsetInWorldSpace).ToUnity();
            debugText.text = debugText.text + "\nNew PositionOffset X: " + position.x.ToString("f9") + " | Y: " + position.y.ToString("f9") + " | Z: " + position.z.ToString("f9");
            two = Instantiate(Debugger.GetCubeForTest(), position, Quaternion.identity);
            LineDrawer.Draw(cameraPosition,  position, Color.green);
        }
        */




        // tEST END



        //MRWorld.tempExtrinsic = null;

        //MRWorld.tempIntrinsic = null;







    }

}
