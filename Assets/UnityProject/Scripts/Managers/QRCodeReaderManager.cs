using Microsoft.MixedReality.SampleQRCodes;
using Microsoft.MixedReality.Toolkit.Experimental.InteractiveElement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ObjdetectModule;
using PersonAndEmotionsInferenceReply;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

using Debug = MRDebug;




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

    public static bool IsOneShot;
    public static bool InPassiveMode;

    private static Coroutine _stopDetectitionCoroutine;
    private static bool _detecting = false;

    private static Action<List<QRCodeDetected>> _oneShotAction;
    private static Dictionary<int,Action<List<QRCodeDetected>>> _timerActions = new Dictionary<int,Action<List<QRCodeDetected>>>();
    private static List<Action<List<QRCodeDetected>>> _passiveModeActions = new List<Action<List<QRCodeDetected>>>();


    async public static void DetectQRCodes(Action<List<QRCodeDetected>> action = null, float? waitSeconds = null) {
        if (_detecting) {
            if (waitSeconds != null) {
                if (waitSeconds == 0) {
                    _oneShotAction = action;

                } else {
                    _timerActions.Add(_timerActions.Count,action);
                    _stopDetectitionCoroutine = AppCommandCenter.Instance.StartCoroutine(StopDetection((float)waitSeconds, _timerActions.Count - 1));

                }

            } else {
                _passiveModeActions.Add(action);

            }

            return;
        
        }

        if (waitSeconds != null) {
            if (waitSeconds == 0) {
                QRCodeReaderManager.IsOneShot = true;
                _oneShotAction = action;

            } else {
                _timerActions.Add(_timerActions.Count, action);
                _stopDetectitionCoroutine = AppCommandCenter.Instance.StartCoroutine(StopDetection((float)waitSeconds, _timerActions.Count - 1));

            }

        } else {
            QRCodeReaderManager.IsOneShot = false;
            QRCodeReaderManager.InPassiveMode = true;

            _passiveModeActions.Add(action);

        }

        _detecting = true;
        AppCommandCenter.CameraFrameReader.FrameArrived += ProcessDetections;

    }

    static IEnumerator StopDetection(float waitTime, int actionKey) {
        yield return new WaitForSeconds(waitTime);

        if (!InPassiveMode)
            AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;

        _timerActions.Remove(actionKey);
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

            bool result = detector.detectAndDecodeMulti(_grayMat, decodedInfo, points);

            if (result) {

                List<QRCodeDetected> detections = new List<QRCodeDetected>();   

                for (int i = 0; i < points.rows(); i++) {
                    float[] points_arr = new float[8];
                    points.get(i, 0, points_arr);

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


    }

    public static void CleanPassiveModeActions() {
        _passiveModeActions = new List<Action<List<QRCodeDetected>>>();

    }

    public static void DeactivatePassiveMode() {
        CleanPassiveModeActions();
        InPassiveMode = false;

        if (_timerActions.Count <= 0) {
            AppCommandCenter.CameraFrameReader.FrameArrived -= ProcessDetections;
            _detecting = false;

        }

    }


}
