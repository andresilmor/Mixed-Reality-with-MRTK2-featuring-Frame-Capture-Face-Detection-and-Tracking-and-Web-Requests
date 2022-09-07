using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_WINMD_SUPPORT
using Windows.Perception.Spatial;
using Microsoft.MixedReality.OpenXR;

#endif

public static class MRWorld //TODO: Change for something more general, like MRWorld, and save Intrinsics info also, since that dont change a lot
{
    private static string _test;
    public static string test
    {
        get
        {
            if (_test == "")
            {
                _test = "Dasdsa";
            }
            return _test;
        }
    }
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
            return new Windows.Foundation.Point(point.x, Camera.main.pixelHeight - point.y); //846 = Frame Height, static now but we can turn dynamic and get from Intrinsics
        }

#endif

    /// <summary>
    /// Returns the unprojected forward vector. Fallback to default forward vector if UWP is not available.
    /// <see cref="VisualizationManager"/> may override fallback if main camera is available (only available on main thread).
    /// Adapted from https://github.com/abist-co-ltd/hololens-opencv-laserpointer/blob/master/Assets/Script/HololensLaserPointerDetection.cs.
    /// </summary>
    public static Vector3 GetLayForward(Vector2 unprojectionOffset, FaceRect boundingBox, CameraExtrinsic extrinsic, CameraIntrinsic intrinsic)
    {
        Vector4 forward = Vector4.one;
        Vector4 upwards = Vector4.one;
        forward = -extrinsic.Forward;
        upwards = extrinsic.Upwards;



#if ENABLE_WINMD_SUPPORT
            Windows.Foundation.Point target = GetBoundingBoxTarget(extrinsic, boundingBox).ToWindowsPoint();
            Vector2 unprojection = intrinsic.UnprojectAtUnitDepth(target).ToUnity();
            Vector3 correctedUnprojection = new Vector3(unprojection.x + unprojectionOffset.x, unprojection.y + unprojectionOffset.y, 1.0f);
            Quaternion rotation = Quaternion.LookRotation(forward, upwards);
            Vector3 layForward = Vector3.Normalize(rotation * correctedUnprojection);
#else
        // Fallback if using Mono. Main camera needs to be executed on main thread.
        Vector3 layForward = Camera.main.transform.forward * -1f;
#endif
        if (layForward == Vector3.forward) Debug.LogWarning("Lay forward is forward vector.");
        if (layForward == Vector3.zero) Debug.LogWarning("Lay forward is zero vector.");
        return layForward;

    }

    public static Vector3 GetPosition(Vector3 cameraPosition, Vector3 layForward)
    {
        if (!Microsoft.MixedReality.Toolkit.Utilities.SyncContextUtility.IsMainThread)
        {
            return Vector3.zero;
        }
        RaycastHit hit;
        if (!Physics.Raycast(cameraPosition, layForward * -1f, out hit, Mathf.Infinity, 1 << 31)) // TODO: Check -1
        {
#if ENABLE_WINMD_SUPPORT
                Debug.LogWarning("Raycast failed. Probably no spatial mesh provided.");
                return Vector3.positiveInfinity;
#else
            Debug.LogWarning("Raycast failed. Probably no spatial mesh provided. Use Holographic Remoting or HoloLens."); // TODO: Check mesh simulation
#endif
        }
        //frame.Dispose(); // TODO: Check disposal
        return hit.point;
    }


    /// <summary>
    /// Algorithm to approximate the vertical point in the bounding box regarding the user's position.
    /// </summary>
    public static Point GetBoundingBoxTarget(CameraExtrinsic extrinsic, FaceRect boundingBox)
    {
        var cameraForward = extrinsic.viewFromWorld.GetColumn(2);
        var cameraToGroundAngle = Vector3.Angle(cameraForward, Vector3.down);
        var offsetFactor = 0f;
        if (cameraToGroundAngle <= 90)
        {
            offsetFactor = 0.5f + cameraToGroundAngle / 180;
        }

        return new Point(boundingBox.x1 + ((boundingBox.x2 - boundingBox.x1) / 2), boundingBox.y1 + ((boundingBox.y2 - boundingBox.y1) * offsetFactor));
    }

}
