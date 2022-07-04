using System;
using System.Collections;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using
UnityEngine.Windows.WebCam;

public class FrameCapture : MonoBehaviour

{

    PhotoCapture photoCaptureObject = null;

    WebSocketSharp.WebSocket ws;

    public void CaptureFrame(WebSocketSharp.WebSocket ws)
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

        this.ws = ws;
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)

    {

        photoCaptureObject = captureObject;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        CameraParameters c = new CameraParameters();

        c.hologramOpacity = 0.0f;

        c.cameraResolutionWidth = cameraResolution.width;

        c.cameraResolutionHeight = cameraResolution.height;

        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);

    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)

    {

        photoCaptureObject.Dispose();

        photoCaptureObject = null;

    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)

    {

        if (result.success)

        {

            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);

        }

        else

        {

            Debug.LogError("Unable to start photo mode!");

        }

    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Copy the raw IMFMediaBuffer data into our empty byte list.
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height, TextureFormat.BGRA32, false);
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);


            byte[] imageBufferList = targetTexture.EncodeToPNG();

            string base64String = Convert.ToBase64String(imageBufferList);
            Debug.Log(base64String);

            ws.Send(base64String);

            System.IO.File.WriteAllBytes("D:\\PC\\Desktop\\testUNITY.png", imageBufferList);
            // In this example, we captured the image using the BGRA32 format.
            // So our stride will be 4 since we have a byte for each rgba channel.
            // The raw image data will also be flipped so we access our pixel data
            // in the reverse order.
            /*int stride = 4;
            float denominator = 1.0f / 255.0f;
            List<Color> colorArray = new List<Color>();
            for (int i = imageBufferList.Count - 1; i >= 0; i -= stride)
            {
                float a = (int)(imageBufferList[i - 0]) * denominator;
                float r = (int)(imageBufferList[i - 1]) * denominator;
                float g = (int)(imageBufferList[i - 2]) * denominator;
                float b = (int)(imageBufferList[i - 3]) * denominator;

                colorArray.Add(new Color(r, g, b, a));
            }
            */
            // Now we could do something with the array such as texture.SetPixels() or run image processing on the list
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }


}