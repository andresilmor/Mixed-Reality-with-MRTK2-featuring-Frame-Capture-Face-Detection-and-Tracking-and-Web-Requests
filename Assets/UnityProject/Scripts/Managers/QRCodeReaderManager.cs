
using Microsoft.MixedReality.Toolkit.Experimental.InteractiveElement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using PersonAndEmotionsInferenceReply;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

using Debug = MRDebug;



// Uses the CV Asset
public static class QRCodeReaderManager
{
    public struct QRCodeDetected {
        public float[] PointsArr;
        public string Info;

        public QRCodeDetected(float[] pointsArr, string info) {
            this.PointsArr = pointsArr;
            this.Info = info;
        }
    }

    /// <summary>
    /// The QRCode detector.
    /// </summary>
    private static QRCodeDetector detector = new QRCodeDetector();

    /// <summary>
    /// The decoded Info
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


    private static bool _detecting = false;

    private static Action<List<QRCodeDetected>> _oneShotAction;
    private static Dictionary<int,Action<List<QRCodeDetected>>> _timerActions = new Dictionary<int,Action<List<QRCodeDetected>>>();
    private static List<Action<List<QRCodeDetected>>> _passiveModeActions = new List<Action<List<QRCodeDetected>>>();


    /// <summary>
    /// Detect QR Codes on Frame (One-Shot)
    /// </summary>
    /// <param name="action"></param>
    async public static void DetectQRCodes(DetectionMode detectionMode, Action<List<QRCodeDetected>> action = null) {
#if ENABLE_WINMD_SUPPORT
/*
        if (AppCommandCenter.CameraFrameReader == null) {
            Debug.Log("wtf");
            AppCommandCenter.CameraFrameReader = await CameraFrameReader.CreateAsync();
        } else if (!CameraFrameReader.IsRunning) {
        Debug.Log("START CAPTURE");
            await AppCommandCenter.CameraFrameReader.StartCapture();
        }*/
#endif

        switch (detectionMode) {
            case DetectionMode.OneShot:
                Debug.Log("oNE SHOT");
                _oneShotAction = action;
                AppCommandCenter.CameraFrameReader.FrameArrived += OneShotDetection;
                break;

            case DetectionMode.Passive:
                break;

            case DetectionMode.Timing:
                break;
        
        }

    }

    private static void OneShotDetection(object sender, FrameArrivedEventArgs e) {
        Debug.Log("hhhheeeeerrrrreeeee");
        try {
            if (_grayMat is null) {
                _grayMat = new Mat(e.Frame.Mat.rows(), e.Frame.Mat.cols(), CvType.CV_8UC1);

            }

            Imgproc.cvtColor(e.Frame.Mat, _grayMat, Imgproc.COLOR_RGBA2GRAY);

            bool result = detector.detectAndDecodeMulti(_grayMat, decodedInfo, points);

            if (result) {

                List<QRCodeDetected> detections = new List<QRCodeDetected>();

                for (int i = 0; i < points.rows(); i++) {
                    float[] points_arr = new float[8];
                    points.get(i, 0, points_arr);


                    Debug.Log(decodedInfo[i]);

                    if (decodedInfo.Count > i && decodedInfo[i] != null)
                        detections.Add(new QRCodeDetected(points_arr, decodedInfo[i]));

                }

                _oneShotAction?.Invoke(detections);
                _oneShotAction = null;

            }

        } catch (Exception ex) {
            Debug.Log(ex.Message);

        }

        AppCommandCenter.CameraFrameReader.FrameArrived -= OneShotDetection;

        _hasFinished = true;
    }



    static IEnumerator StopDetection(float waitTime, int actionKey, Action endTimeAction = null) {
        yield return new WaitForSeconds(waitTime);


        _timerActions.Remove(actionKey);
        _detecting = false;

        endTimeAction.Invoke();

    }


    private static void ProcessDetections(object sender, FrameArrivedEventArgs e) {
        /*
        if (!_hasFinished)
            return;

        _hasFinished = false;

        try {
            if (_grayMat is null) {
                _grayMat = new Mat(e.Frame.Mat.rows(), e.Frame.Mat.cols(), CvType.CV_8UC1);

            }

            Imgproc.cvtColor(e.Frame.Mat, _grayMat, Imgproc.COLOR_RGBA2GRAY);

            bool result = detector.detectAndDecodeMulti(_grayMat, decodedInfo, points);

            if (result) {

                List<QRCodeDetected> detections = new List<QRCodeDetected>();   

                for (int i = 0; i < points.rows(); i++) {
                    float[] points_arr = new float[8];
                    points.get(i, 0, points_arr);


                    Debug.Log(decodedInfo[i]);

                    if (decodedInfo.Count > i && decodedInfo[i] != null)
                        detections.Add(new QRCodeDetected(points_arr, decodedInfo[i]));

                }


                AppCommandCenter.Instance.StopCoroutine(_stopDetectitionCoroutine);



                if (IsOneShot) { 
                    _oneShotAction?.Invoke(detections);
                    _oneShotAction = null;

                }

                foreach (KeyValuePair<int, Action<List<QRCodeDetected>>> action in _timerActions) {
                    action.Value?.Invoke(detections);
                    _timerActions.Remove(action.Key);

                }

                if (InPassiveMode) {
                    foreach (Action<List<QRCodeDetected>> action in _passiveModeActions)
                        action?.Invoke(detections);

                }

                if (!InPassiveMode && _timerActions.Count <= 0) {
                    AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
                    _detecting = false;

                }

                _hasFinished = true;
            }

        } catch (Exception ex) { 
            Debug.Log(ex.Message);

        }

        _hasFinished = true;

        */
    }

    public static void CleanPassiveModeActions() {
        _passiveModeActions = new List<Action<List<QRCodeDetected>>>();

    }

    public static void DeactivatePassiveMode() {
        CleanPassiveModeActions();

        if (_timerActions.Count <= 0) {
            AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
            _detecting = false;

        }

    }


}
