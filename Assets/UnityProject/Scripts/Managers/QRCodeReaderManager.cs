using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using PersonAndEmotionsInferenceReply;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif

public static class QRCodeReaderManager
{

    /// <summary>
    /// The QRCode detector.
    /// </summary>
    private static QRCodeDetector detector = new QRCodeDetector();

    /// <summary>
    /// The decoded info
    /// </summary>
    private static List<string> decodedInfo = new List<string>();

    /// <summary>
    /// The straight qrcode
    /// </summary>
    private static List<Mat> straightQrcode = new List<Mat>();

    /// <summary>
    /// The points.
    /// </summary>
    private static Mat points = new Mat();


    private static bool _hasFinished = true;
    private static Mat _grayMat;

    public static bool IsRapidShot;
    private static Coroutine _stopDetectitionCoroutine;
    private static bool _detecting = false;

    async public static void DetectQRCodes(float? waitTime = null) {
        if (_detecting)
            return;

        if (waitTime != null) {
            QRCodeReaderManager.IsRapidShot = false;
            _stopDetectitionCoroutine = AppCommandCenter.Instance.StartCoroutine(StopDetection((float)waitTime));

        } else {
            QRCodeReaderManager.IsRapidShot = true;

        }

        _detecting = true;
        AppCommandCenter.CameraFrameReader.FrameArrived += ProcessDetections;

    }

    static IEnumerator StopDetection(float waitTime) {
        yield return new WaitForSeconds(waitTime);
        AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
        _detecting = false;

    }


    private static void ProcessDetections(object sender, FrameArrivedEventArgs e) {

        if (!_hasFinished)
            return;

        _hasFinished = false;

        try {
            if (_grayMat is null) {
                _grayMat = new Mat(e.Frame.Mat.rows(), e.Frame.Mat.cols(), CvType.CV_8UC1);

            }

            Imgproc.cvtColor(e.Frame.Mat, _grayMat, Imgproc.COLOR_RGBA2GRAY);

            bool result = detector.detectAndDecodeMulti(_grayMat, decodedInfo);

            if (result) {

                foreach (string info in decodedInfo) {
                    
                    Debug.Log("got: " + info);

                }

                AppCommandCenter.Instance.StopCoroutine(_stopDetectitionCoroutine);
                AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
                _detecting = false;

            } 

            _hasFinished = true;

        } catch (Exception ex) { 
            Debug.Log(ex.Message);

        }

        _hasFinished = true;

        if (QRCodeReaderManager.IsRapidShot) { 
            AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
            _detecting = false;
        }

    }


}
