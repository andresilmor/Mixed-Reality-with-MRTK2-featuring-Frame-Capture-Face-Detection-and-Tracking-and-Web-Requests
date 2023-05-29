
using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.Experimental.InteractiveElement;
using Newtonsoft.Json;
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


    private static bool _hasFinishedOneShot = true;
    private static bool _wsConnected = false;
    private static int _oneShotTentatives = 0;


    private static Mat _grayMat;


    private static bool _detecting = false;

    private static List<Action<List<QRCodeDecodeReply.Detection>>> _oneShotAction = new List<Action<List<QRCodeDecodeReply.Detection>>>();
    private static Dictionary<int,Action<List<QRCodeDetected>>> _timerActions = new Dictionary<int,Action<List<QRCodeDetected>>>();
    private static List<Action<List<QRCodeDetected>>> _passiveModeActions = new List<Action<List<QRCodeDetected>>>();



    /// <summary>
    /// Detect QR Codes on Frame (One-Shot)
    /// </summary>
    /// <param name="action"></param>
    async public static void DetectQRCodes(DetectionMode detectionMode, List<Action<List<QRCodeDecodeReply.Detection>>> actions = null) {
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
                foreach (Action<List<QRCodeDecodeReply.Detection>> action in actions)
                    _oneShotAction.Add(action);

                AppCommandCenter.CameraFrameReader.FrameArrived += OneShotDetection;
                break;

            case DetectionMode.Passive:
                break;

            case DetectionMode.Timing:
                break;
        
        }

    }

    async public static void DetectQRCodes(DetectionMode detectionMode, Action<List<QRCodeDecodeReply.Detection>> action = null) {
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
                _oneShotAction.Add(action);

                AppCommandCenter.CameraFrameReader.FrameArrived += OneShotDetection;
                break;

            case DetectionMode.Passive:
                break;

            case DetectionMode.Timing:
                break;

        }

    }

    private async static void OneShotDetection(object sender, FrameArrivedEventArgs e) {

        if (_wsConnected) {
            WebSocket wsTemp;
            if (_hasFinishedOneShot)
                wsTemp = APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRDecode);
            else 
                return;

            if (wsTemp.IsOpen) {
                if (!_hasFinishedOneShot)
                    return;

                if (_oneShotTentatives >= 10) {
                    StopOneShotDetection(null);
                    return;

                }

                _hasFinishedOneShot = false;
                _oneShotTentatives += 1;

#if ENABLE_WINMD_SUPPORT
        if (e.Frame.MediaFrameReference != null) {
            try {
                using (var videoFrame = e.Frame.MediaFrameReference.VideoMediaFrame.GetVideoFrame()) {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null) {
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);

                        videoFrame.SoftwareBitmap.Dispose();

                        //UnityEngine.Object.Instantiate(MRDebug.GetSphereForTest(), AppCommandCenter.CameraMain.transform.position, Quaternion.identity);
                        //UnityEngine.Object.Instantiate(MRDebug.GetCubeForTest(), cameraFrame.Extrinsic.Position, Quaternion.identity);

                        ImageInferenceRequest request = new ImageInferenceRequest();
                        request.image = byteArray;
                        Debug.Log("Sending");
                        if (!wsTemp.IsOpen) {
                            Debug.Log("Not opened");
                        }
                        wsTemp.Send(Parser.ProtoSerialize<ImageInferenceRequest>(request));
                        Debug.Log("Sended");

                    } else { Debug.Log("videoFrame or SoftwareBitmap = null"); }
                }
            } catch (Exception ex) {
                Debug.Log($"[### Deebug ###] Update Error: {ex.Message}");
            }
        } else {
            Debug.Log("lastFrame.mediaFrameReference = null");
        }

#endif
            }

            return;

        }

        _wsConnected = true;

        Debug.Log("HERE .....");
        WebSocket ws = APIManager.CreateWebSocketConnection(APIManager.QRRoute + APIManager.QRDecode, (WebSocket ws, string message) => {
            Debug.Log("Action .....");
            try {
                QRCodeDecodeReply.DetectionsList results = JsonConvert.DeserializeObject<QRCodeDecodeReply.DetectionsList>(
                    JsonConvert.DeserializeObject(message).ToString());
                if (results.qrCodes.Count > 0) {
                    StopOneShotDetection(results.qrCodes);
                    Debug.Log("Response ..." + results.qrCodes[0].content);


                }

            } catch (Exception e) {
                Debug.Log("B: " + e.Message, LogType.Exception);

            }

            _hasFinishedOneShot = true;


            /*
            StopOneShotDetection(null);
            APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRDecode).Close();
            _hasFinishedOneShot = true;
            _wsConnected = false;*/

        });

        ws.Open();





        try {


            /* TRIAL WITH "EMBEDDED" COMPUTOR VISION (OpenCV For Unity
                if (_grayMat is null) {
                    _grayMat = new Mat(e.Frame.Mat.rows(), e.Frame.Mat.cols(), CvType.CV_8UC1);

                }

                Imgproc.cvtColor(e.Frame.Mat, _grayMat, Imgproc.COLOR_RGBA2GRAY);

                bool result = detector.detectAndDecodeMulti(_grayMat, decodedInfo, points);

                if (result) {

                    Debug.Log("Result: " + result);
                    List<QRCodeDetected> detections = new List<QRCodeDetected>();

                    for (int i = 0; i < points.rows(); i++) {
                        float[] points_arr = new float[8];
                        points.get(i, 0, points_arr);

                        if (decodedInfo.Count > i && decodedInfo[i] != null)
                            detections.Add(new QRCodeDetected(points_arr, decodedInfo[i]));

                    }

                    if (detections.Count > 0) {
                        Debug.Log("SUCCESSSSSSSS ");
                        StopOneShotDetection(detections);

                    } else {
                        _triesOneShot++;

                    }

                } else {
                    _triesOneShot++;

                }
                */

            } catch (Exception ex) {
                Debug.Log("Error in QR Code reader : " + ex.Message);
            

            }

         


        }

    private static void StopOneShotDetection(List<QRCodeDecodeReply.Detection> qrCodes) {
        AppCommandCenter.CameraFrameReader.FrameArrived -= OneShotDetection;
        APIManager.GetWebSocket(APIManager.QRRoute + APIManager.QRDecode).Close();

        foreach (Action<List<QRCodeDecodeReply.Detection>> action in _oneShotAction)
            action?.Invoke(qrCodes);

        _hasFinishedOneShot = true;
        _wsConnected = false;
        _oneShotTentatives = 0;
        _oneShotAction.Clear();

    }




        static IEnumerator StopDetection(float waitTime, int actionKey, Action endTimeAction = null) {
            yield return new WaitForSeconds(waitTime);


            _timerActions.Remove(actionKey);
            _detecting = false;

            endTimeAction.Invoke();

        }


        private static void ProcessDetections(object sender, FrameArrivedEventArgs e) {
            /*
            if (!_hasFinishedOneShot)
                return;

            _hasFinishedOneShot = false;

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

                    _hasFinishedOneShot = true;
                }

            } catch (Exception ex) { 
                Debug.Log(ex.Message);

            }

            _hasFinishedOneShot = true;

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
