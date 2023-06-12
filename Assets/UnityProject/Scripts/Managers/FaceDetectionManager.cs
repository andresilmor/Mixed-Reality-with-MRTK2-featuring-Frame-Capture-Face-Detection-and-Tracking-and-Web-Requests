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

            var newObject = UnityEngine.Object.Instantiate(AppCommandCenter.Instance.OutlinedCube, bestRectPositionInWorldspace, Quaternion.identity);

        }
 
    }

#endif

}