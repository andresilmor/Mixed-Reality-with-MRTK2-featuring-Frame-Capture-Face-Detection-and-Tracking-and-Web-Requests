#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using Rect = OpenCVForUnity.CoreModule.Rect;
using PositionsVector = System.Collections.Generic.List<OpenCVForUnity.CoreModule.Rect>;
using System.Threading.Tasks;
using UnityEngine.XR.ARSubsystems;
using PersonAndEmotionsInferenceReply;
using UnityEngine.Rendering;

#if ENABLE_WINMD_SUPPORT
using Debug = MRDebug;
#else
using Debug = UnityEngine.Debug;
#endif


//Embedded Detection
public static class FaceDetectionManager {
    /// <summary>
    /// The gray mat.
    /// </summary>
    static Mat grayMat = null;




    public static bool testing = true;


    public static bool isAnalysingFrame = false;




    /// <summary>
    /// The cascade.
    /// </summary>
    static CascadeClassifier cascade;

    /// <summary>
    /// LBP_CASCADE_FILENAME
    /// </summary>
    static readonly string LBP_CASCADE_FILENAME = "OpenCVForUnity/objdetect/lbpcascade_frontalface.xml";

    /// <summary>
    /// The lbp cascade filepath.
    /// </summary>
    static string lbp_cascade_filepath;

    /// <summary>
    /// HAAR_CASCADE_FILENAME
    /// </summary>
    static readonly string HAAR_CASCADE_FILENAME = "OpenCVForUnity/objdetect/haarcascade_frontalface_alt.xml";

    /// <summary>
    /// The haar_cascade_filepath.
    /// </summary>
    static string haar_cascade_filepath;

    /// <summary>
    /// The rects where regions.
    /// </summary>
    static Rect[] rectsWhereRegions;

    /// <summary>
    /// The detected objects in regions.
    /// </summary>
    static List<Rect> detectedObjectsInRegions = new List<Rect>();

    /// <summary>
    /// The result objects.
    /// </summary>
    static List<Rect> resultObjects = new List<Rect>();

    // for Thread
    static CascadeClassifier cascade4Thread;
    static Mat grayMat4Thread;
    static MatOfRect detectionResult;
    static System.Object sync = new System.Object();

    static bool _isThreadRunning = false;

    static bool isThreadRunning {
        get {
            lock (sync)
                return _isThreadRunning;
        }
        set {
            lock (sync)
                _isThreadRunning = value;
        }
    }

    static bool _shouldStopThread = false;

    static bool shouldStopThread {
        get {
            lock (sync)
                return _shouldStopThread;
        }
        set {
            lock (sync)
                _shouldStopThread = value;
        }
    }

    static bool _shouldDetectInMultiThread = false;

    static bool shouldDetectInMultiThread {
        get {
            lock (sync)
                return _shouldDetectInMultiThread;
        }
        set {
            lock (sync)
                _shouldDetectInMultiThread = value;
        }
    }

    static bool _didUpdateTheDetectionResult = false;

    static bool didUpdateTheDetectionResult {
        get {
            lock (sync)
                return _didUpdateTheDetectionResult;
        }
        set {
            lock (sync)
                _didUpdateTheDetectionResult = value;
        }
    }

    /// <summary>
    /// The FPS monitor.
    /// </summary>

    // for tracker
    static List<TrackedObject> trackedObjects = new List<TrackedObject>();
    static List<float> weightsPositionsSmoothing = new List<float>();
    static List<float> weightsSizesSmoothing = new List<float>();
    static Parameters parameters;
    static InnerParameters innerParameters;

    private static bool hasFinished = true;

    private static Rect[] analysisResult;

    private static void Setup() {
        // Step 2

        weightsPositionsSmoothing.Add(1);
        weightsSizesSmoothing.Add(0.5f);
        weightsSizesSmoothing.Add(0.3f);
        weightsSizesSmoothing.Add(0.2f);

        //parameters.minObjectSize = 96;
        //parameters.maxObjectSize = int.MaxValue;
        //parameters.scaleFactor = 1.1f;
        //parameters.minNeighbors = 2;
        parameters.maxTrackLifetime = 5;

        innerParameters.numLastPositionsToTrack = 4;
        innerParameters.numStepsToWaitBeforeFirstShow = 6;
        innerParameters.numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown = 3;
        innerParameters.numStepsToShowWithoutDetecting = 3;
        innerParameters.coeffTrackingWindowSize = 2.0f;
        innerParameters.coeffObjectSizeToTrack = 0.85f;
        innerParameters.coeffObjectSpeedUsingInPrediction = 0.8f;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            //webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif
        //webCamTextureToMatHelper.Initialize();
    }

    /// <summary>
    /// Raises the webcam texture to mat helper initialized event.
    /// </summary>
    async public static Task<bool> Initialize() {

        Debug.Log("InitializeFaceDetection");
        // Step 3  // Here i get map

        if (AppCommandCenter.CameraFrameReader is null) {
            Debug.Log("Error (InitializeFaceDetection): CameraFrameReader is null");
            return false;

        }

        try { 

            lbp_cascade_filepath = Utils.getFilePath(LBP_CASCADE_FILENAME);
            haar_cascade_filepath = Utils.getFilePath(HAAR_CASCADE_FILENAME);

            Setup();


            /*
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            Utils.matToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);

            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);



            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }
            */

            //grayMat = new Mat(webCamTextureMat.rows(), webCamTextureMat.cols(), CvType.CV_8UC1);

            if (string.IsNullOrEmpty(lbp_cascade_filepath)) {

                Debug.Log(LBP_CASCADE_FILENAME + " is not loaded. Please move from “OpenCVForUnity/StreamingAssets/OpenCVForUnity/” to “Assets/StreamingAssets/OpenCVForUnity/” folder.");
            } else {
                cascade = new CascadeClassifier(lbp_cascade_filepath);
            }

            Debug.Log("Step (InitializeFaceDetection): InitThread");
            InitThread();

            Debug.Log("Step (InitializeFaceDetection): Assign Event");
            AppCommandCenter.CameraFrameReader.FrameArrived += UpdateDetections;

            return true;

        } catch (Exception ex) {
            Debug.Log("Error (InitializeFaceDetection): " + ex.Message);
            return false;

        }

    }

    /// <summary>
    /// Raises the webcam texture to mat helper disposed event.
    /// </summary>
    public static void Stop() {
        Debug.Log("OnWebCamTextureToMatHelperDisposed");

#if !UNITY_WEBGL
        StopThread();
#else
        StopCoroutine ("ThreadWorker");
#endif

        if (grayMat4Thread != null)
            grayMat4Thread.Dispose();

        if (cascade4Thread != null)
            cascade4Thread.Dispose();

        if (grayMat != null)
            grayMat.Dispose();

        if (cascade != null)
            cascade.Dispose();

        trackedObjects.Clear();
    }


    public static void CreateSnapshot() {
        try {
            for (int i = 0; i < trackedObjects.Count; i++) { 
                trackedObjects[i].rectSnapshot = trackedObjects[i].lastPositions[trackedObjects[i].lastPositions.Count - 1];
                //Debug.Log("Snapshot (" + i + ")|  x1 : " + trackedObjects[i].rectSnapshot.x + "|  y1 : " + trackedObjects[i].rectSnapshot.y + " | x2 : " + (trackedObjects[i].rectSnapshot.x + trackedObjects[i].rectSnapshot.width) + " | y2 : "+ (trackedObjects[i].rectSnapshot.y + trackedObjects[i].rectSnapshot.height));


            }
        } catch (Exception ex) {
            Debug.Log("Error (CreateSnapshot): " + ex.Message);

        }
    }

    public static List<TrackedObject> GetTrackedObjects() {
        return trackedObjects;

    }

    static bool RectContainsPoint(Rect rect, int x, int y) {
        if (x > rect.x && x < (rect.x + rect.width) &&
            y > rect.y && y < (rect.y + rect.height))
            return true;

        return false;
    }


    public static void ProcessResults(List<PersonAndEmotionsInferenceReply.Detection> detections) {

        analysisResult = new Rect[detections.Count];
        try { 

            for (int i = 0; i < detections.Count; i += 1) {
                analysisResult[i] = new Rect(
                    detections[i].faceRect.x1,
                    detections[i].faceRect.y1,
                    detections[i].faceRect.x2 - detections[i].faceRect.x1,
                    detections[i].faceRect.y2 - detections[i].faceRect.y1);

                for (int j = 0; i < trackedObjects.Count; i++) {
                    if (trackedObjects[i].rectSnapshot is null)
                        continue;

                    if (RectContainsPoint(trackedObjects[j].rectSnapshot, detections[i].faceRect.x1 + (int)((detections[i].faceRect.x2 - detections[i].faceRect.x1) * 0.5), detections[i].faceRect.y1 + (int)((detections[i].faceRect.y2 - detections[i].faceRect.y1) * 0.5))) {
                        //Debug.Log("Yup");

                        if (trackedObjects[i].trackerEntity is null) {
                            UIWindow newMarker = UIManager.Instance.OpenWindowAt(WindowType.Sp_ML_E_1btn_Pacient, null, Quaternion.identity);
                            Debug.Log("Marker is active? " + (newMarker.gameObject.activeInHierarchy).ToString());


                            newMarker.gameObject.SetActive(true);
                            trackedObjects[i].trackerEntity = newMarker.gameObject.GetComponent<PacientTracker>();
                            (trackedObjects[i].trackerEntity as PacientTracker).Window = newMarker;
                            (trackedObjects[i].trackerEntity as PacientTracker).id = detections[i].uuid;

                            trackedObjects[i].IsNew = true;
                            trackedObjects[i].meshRenderer = newMarker.gameObject.GetComponent<MeshRenderer>();

                            (trackedObjects[i].trackerEntity as PacientTracker).UpdateActiveEmotion(detections[i].emotionsDetected.categorical[0]);
                            Debug.Log("Created");

                        } else {
                            if (trackedObjects[i].meshRenderer.isVisible) {
                                Debug.Log("Updated");
                                (trackedObjects[i].trackerEntity as PacientTracker).UpdateActiveEmotion(detections[i].emotionsDetected.categorical[0]);
                            }
                        }

                        trackedObjects[i].rectSnapshot = null;

                    } 

                }

            }

            didUpdateTheDetectionResult = true;
            isAnalysingFrame = false;

        } catch (Exception ex) {
            Debug.Log("Error (ProcessResults): " + ex.Message);  

        }


    }


    private static void UpdateDetections(object sender, FrameArrivedEventArgs e) {

        if (!hasFinished)
            return;

        hasFinished = false;

        try {
            if (grayMat is null) {
                grayMat = new Mat(e.Frame.Mat.rows(), e.Frame.Mat.cols(), CvType.CV_8UC1);
                Debug.Log("Setting grayMat Height: " + grayMat.height() + " | Width: " + grayMat.width());
                Debug.Log("grayMat type: " + grayMat.type());
                Debug.Log("FrameMat type: " + e.Frame.Mat.type());
            }

            if (cascade == null || cascade4Thread == null) {
                Debug.Log("Someone is null -_-");
                return;
            }


            Imgproc.cvtColor(e.Frame.Mat, grayMat, Imgproc.COLOR_RGBA2GRAY);
            Imgproc.equalizeHist(grayMat, grayMat);

            if (!shouldDetectInMultiThread) {
                grayMat.copyTo(grayMat4Thread);

                shouldDetectInMultiThread = true;

            }

            OpenCVForUnity.CoreModule.Rect[] rects;

            if (didUpdateTheDetectionResult) {
                didUpdateTheDetectionResult = false;
                //Debug.Log("DetectionBasedTracker::process: get _rectsWhereRegions were got from resultDetect");
                rectsWhereRegions = analysisResult;

                /*
                rects = rectsWhereRegions;
                for (int i = 0; i < rects.Length; i++) {
                    //Imgproc.rectangle(rgbaMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(0, 0, 255, 255), 2);
                }
                */

            } else {
                //Debug.Log("DetectionBasedTracker::process: get _rectsWhereRegions from previous positions");
                rectsWhereRegions = new Rect[trackedObjects.Count];

                for (int i = 0; i < trackedObjects.Count; i++) {
                    int n = trackedObjects[i].lastPositions.Count;
                    //if (n > 0) UnityEngine.Debug.LogError("n > 0 is false");

                    Rect r = trackedObjects[i].lastPositions[n - 1].clone();
                    if (r.area() == 0) {
                        Debug.Log("DetectionBasedTracker::process: ERROR: ATTENTION: strange algorithm's behavior: trackedObjects[i].rect() is empty");
                        continue;
                    }

                    //correction by speed of rectangle
                    if (n > 1) {
                        Point center = CenterRect(r);
                        Point center_prev = CenterRect(trackedObjects[i].lastPositions[n - 2]);
                        Point shift = new Point((center.x - center_prev.x) * innerParameters.coeffObjectSpeedUsingInPrediction,
                                          (center.y - center_prev.y) * innerParameters.coeffObjectSpeedUsingInPrediction);

                        r.x += (int)Math.Round(shift.x);
                        r.y += (int)Math.Round(shift.y);
                    }
                    rectsWhereRegions[i] = r;
                }

                /*
                rects = rectsWhereRegions;
                for (int i = 0; i < rects.Length; i++) {
                    //Imgproc.rectangle(rgbaMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(0, 255, 0, 255), 2);
                }
                */

            }

            detectedObjectsInRegions.Clear();
            if (rectsWhereRegions.Length > 0) {

                int len = rectsWhereRegions.Length;
                for (int i = 0; i < len; i++) {
                    DetectInRegion(grayMat, rectsWhereRegions[i], detectedObjectsInRegions);

                }

            }

            UpdateTrackedObjects(detectedObjectsInRegions);
            GetObjects(resultObjects);

            



            rects = resultObjects.ToArray();


            if (!isAnalysingFrame) { // ???????????????????????????????????
                isAnalysingFrame = true;
                CreateSnapshot();
                MLManager.AnalyseFrame(e.Frame);

            }

            Vector3 worldPosition;
            for (int i = 0; i < rects.Length; i++) {
                if (trackedObjects[i].trackerEntity != null) {
                    if (trackedObjects[i].meshRenderer.isVisible || trackedObjects[i].IsNew) {
                        MRWorld.GetFaceWorldPosition(out worldPosition, new BoxRect((int)rects[i].x, (int)rects[i].y, (int)rects[i].x + (int)rects[i].width, (int)rects[i].y + (int)rects[i].height), e.Frame);
                        (trackedObjects[i].trackerEntity as PacientTracker).UpdatePosition(worldPosition, e.Frame);

                        if (trackedObjects[i].IsNew) 
                            trackedObjects[i].IsNew = false;

                    }                    

                }

            }
            /*
            for (int i = 0; i < rects.Length; i++) {
                //Debug.Log ("detect faces " + rects [i]);
                //Imgproc.rectangle(rgbaMat, new Point(rects[i].x, rects[i].y), new Point(rects[i].x + rects[i].width, rects[i].y + rects[i].height), new Scalar(255, 0, 0, 255), 2);
                Vector3 worldPosition = Vector3.zero;
                //Debug.Log("Box x1: " + rects[i].x + " | y1: " + rects[i].y + " | Width: " + rects[i].width + " | Height: " + rects[i].height );

                if (testing) { 
                    MRWorld.GetFaceWorldPosition(out worldPosition, new BoxRect((int)rects[i].x, (int)rects[i].y, (int)rects[i].x + (int)rects[i].width, (int)rects[i].y + (int)rects[i].height), e.Frame);
                    //Debug.Log("Box x1: " + rects[i].x + " | y1: " + rects[i].y + " | Width: " + rects[i].width + " | Height: " + rects[i].height );
                    //Debug.Log("Camera Position when detected: " + e.Frame.Extrinsic.Position );
                    UIWindow newVisualTracker = UIManager.Instance.OpenWindowAt(WindowType.Sp_ML_E_1btn_Pacient, worldPosition, Quaternion.identity);
                    testing = false;

                }
            }*/

        } catch (Exception ex) {
            Debug.Log("Error A: " + ex.Message);
        
        }

        hasFinished = true;

    }


    private static void DetectInRegion(Mat img, Rect r, List<Rect> detectedObjectsInRegions) {
        Rect r0 = new Rect(new Point(), img.size());
        Rect r1 = new Rect(r.x, r.y, r.width, r.height);
        Rect.inflate(r1, (int)((r1.width * innerParameters.coeffTrackingWindowSize) - r1.width) / 2,
            (int)((r1.height * innerParameters.coeffTrackingWindowSize) - r1.height) / 2);
        r1 = Rect.intersect(r0, r1);

        if (r1 != null && (r1.width <= 0) || (r1.height <= 0)) {
            Debug.Log("DetectionBasedTracker::detectInRegion: Empty intersection");
            return;
        }


        int d = Math.Min(r.width, r.height);
        d = (int)Math.Round(d * innerParameters.coeffObjectSizeToTrack);


        MatOfRect tmpobjects = new MatOfRect();

        Mat img1 = new Mat(img, r1);//subimage for rectangle -- without data copying

        cascade.detectMultiScale(img1, tmpobjects, 1.1, 2, 0 | Objdetect.CASCADE_DO_CANNY_PRUNING | Objdetect.CASCADE_SCALE_IMAGE | Objdetect.CASCADE_FIND_BIGGEST_OBJECT, new Size(d, d), new Size());


        Rect[] tmpobjectsArray = tmpobjects.toArray();
        int len = tmpobjectsArray.Length;
        for (int i = 0; i < len; i++) {
            Rect tmp = tmpobjectsArray[i];
            Rect curres = new Rect(new Point(tmp.x + r1.x, tmp.y + r1.y), tmp.size());
            detectedObjectsInRegions.Add(curres);

        }

    }

    public static Point CenterRect(Rect r) {
        return new Point(r.x + (r.width / 2), r.y + (r.height / 2));
    }

    private static void InitThread() {
        // Step 4

        StopThread();

        grayMat4Thread = new Mat();

        if (string.IsNullOrEmpty(haar_cascade_filepath)) {
            Debug.Log(HAAR_CASCADE_FILENAME + " is not loaded. Please move from “OpenCVForUnity/StreamingAssets/OpenCVForUnity/” to “Assets/StreamingAssets/OpenCVForUnity/” folder.");
        } else {
            cascade4Thread = new CascadeClassifier(haar_cascade_filepath);
        }

        shouldDetectInMultiThread = false;

#if !UNITY_WEBGL
        StartThread(ThreadWorker);
#else
            StartCoroutine ("ThreadWorker");
#endif
    }

    private static void StartThread(Action action) {
        // Step 5

        shouldStopThread = false;

#if UNITY_METRO && NETFX_CORE
            System.Threading.Tasks.Task.Run(() => action());
#elif UNITY_METRO
        action.BeginInvoke(ar => action.EndInvoke(ar), null);
#else
            ThreadPool.QueueUserWorkItem(_ => action());
#endif

        Debug.Log("Thread Start");
    }

    private static void StopThread() {
        if (!isThreadRunning)
            return;

        shouldStopThread = true;

        while (isThreadRunning) {
            //Wait threading stop
        }
        Debug.Log("Thread Stop");
    }

#if !UNITY_WEBGL
    private static void ThreadWorker() {
        isThreadRunning = true;
        return;
        while (!shouldStopThread) {
            if (!shouldDetectInMultiThread)
                continue;

            Detect();

            shouldDetectInMultiThread = false;
            didUpdateTheDetectionResult = true;
        }

        isThreadRunning = false;
    }


#else
        private IEnumerator ThreadWorker ()
        {
            while (true) {
                while (!shouldDetectInMultiThread) {
                    yield return null;
                }

                Detect ();

                shouldDetectInMultiThread = false;
                didUpdateTheDetectionResult = true;
            }
        }
#endif


    private static void Detect() {
        MatOfRect objects = new MatOfRect();
        if (cascade4Thread != null) { 
            cascade4Thread.detectMultiScale(grayMat4Thread, objects, 1.1, 2, Objdetect.CASCADE_SCALE_IMAGE, // TODO: objdetect.CV_HAAR_SCALE_IMAGE
                new Size(grayMat4Thread.height() * 0.2, grayMat4Thread.height() * 0.2), new Size());
        } else {
            //Debug.Log("Detecting IS NULL? ");

        }


        detectionResult = objects;

        /*
        if(detectionResult.toArray().Length >0 ) {
            Debug.Log("Detected: " + detectionResult.toArray().Length);
        }*/

        Thread.Sleep(500);
    }


    //
    // tracker
    //
    private static void GetObjects(List<Rect> result) {
        result.Clear();

        for (int i = 0; i < trackedObjects.Count; i++) {
            Rect r = CalcTrackedObjectPositionToShow(i);
            if (r.area() == 0) {
                continue;
            }
            result.Add(r);
            //LOGD("DetectionBasedTracker::process: found a object with SIZE %d x %d, rect={%d, %d, %d x %d}", r.width, r.height, r.x, r.y, r.width, r.height);
        }
    }

    private enum TrackedState : int {
        NEW_RECTANGLE = -1,
        INTERSECTED_RECTANGLE = -2
    }

    private static void UpdateTrackedObjects(List<Rect> detectedObjects) {
        int N1 = (int)trackedObjects.Count;
        int N2 = (int)detectedObjects.Count;

        for (int i = 0; i < N1; i++) {
            trackedObjects[i].numDetectedFrames++;
        }

        int[] correspondence = new int[N2];
        for (int i = 0; i < N2; i++) {
            correspondence[i] = (int)TrackedState.NEW_RECTANGLE;
        }


        for (int i = 0; i < N1; i++) {
            TrackedObject curObject = trackedObjects[i];

            int bestIndex = -1;
            int bestArea = -1;

            int numpositions = (int)curObject.lastPositions.Count;
            //Debug.Log("ID: " + curObject.id);

            //if (numpositions > 0) UnityEngine.Debug.LogError("numpositions > 0 is false");

            Rect prevRect = curObject.lastPositions[numpositions - 1];

            for (int j = 0; j < N2; j++) {
                if (correspondence[j] >= 0) {
                    //Debug.Log("DetectionBasedTracker::updateTrackedObjects: j=" + i + " is rejected, because it has correspondence=" + correspondence[j]);
                    continue;
                }
                if (correspondence[j] != (int)TrackedState.NEW_RECTANGLE) {
                    //Debug.Log("DetectionBasedTracker::updateTrackedObjects: j=" + j + " is rejected, because it is intersected with another rectangle");
                    continue;
                }

                Rect r = Rect.intersect(prevRect, detectedObjects[j]);
                if (r != null && (r.width > 0) && (r.height > 0)) {
                    //LOGD("DetectionBasedTracker::updateTrackedObjects: There is intersection between prevRect and detectedRect, r={%d, %d, %d x %d}",
                    //        r.x, r.y, r.width, r.height);
                    correspondence[j] = (int)TrackedState.INTERSECTED_RECTANGLE;

                    if (r.area() > bestArea) {
                        //LOGD("DetectionBasedTracker::updateTrackedObjects: The area of intersection is %d, it is better than bestArea=%d", r.area(), bestArea);
                        bestIndex = j;
                        bestArea = (int)r.area();
                    }
                }
            }

            if (bestIndex >= 0) {
                //LOGD("DetectionBasedTracker::updateTrackedObjects: The best correspondence for i=%d is j=%d", i, bestIndex);
                correspondence[bestIndex] = i;

                for (int j = 0; j < N2; j++) {
                    if (correspondence[j] >= 0)
                        continue;

                    Rect r = Rect.intersect(detectedObjects[j], detectedObjects[bestIndex]);
                    if (r != null && (r.width > 0) && (r.height > 0)) {
                        //LOGD("DetectionBasedTracker::updateTrackedObjects: Found intersection between "
                        //    "rectangles j=%d and bestIndex=%d, rectangle j=%d is marked as intersected", j, bestIndex, j);
                        correspondence[j] = (int)TrackedState.INTERSECTED_RECTANGLE;
                    }
                }
            } else {
                //LOGD("DetectionBasedTracker::updateTrackedObjects: There is no correspondence for i=%d ", i);
                curObject.numFramesNotDetected++;
            }
        }

        //LOGD("DetectionBasedTracker::updateTrackedObjects: start second cycle");
        for (int j = 0; j < N2; j++) {
            int i = correspondence[j];
            if (i >= 0) {//add position
                         //Debug.Log("DetectionBasedTracker::updateTrackedObjects: add position");
                trackedObjects[i].lastPositions.Add(detectedObjects[j]);
                while ((int)trackedObjects[i].lastPositions.Count > (int)innerParameters.numLastPositionsToTrack) {
                    trackedObjects[i].lastPositions.Remove(trackedObjects[i].lastPositions[0]);
                }
                trackedObjects[i].numFramesNotDetected = 0;
            } else if (i == (int)TrackedState.NEW_RECTANGLE) { //new object
                                                               //Debug.Log("DetectionBasedTracker::updateTrackedObjects: new object");
                trackedObjects.Add(new TrackedObject(detectedObjects[j]));
            } else {
                //Debug.Log ("DetectionBasedTracker::updateTrackedObjects: was auxiliary intersection");
            }
        }

        int t = 0;
        TrackedObject it;
        while (t < trackedObjects.Count) {
            it = trackedObjects[t];

            if ((it.numFramesNotDetected > parameters.maxTrackLifetime)
                ||
                ((it.numDetectedFrames <= innerParameters.numStepsToWaitBeforeFirstShow)
                &&
                (it.numFramesNotDetected > innerParameters.numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown))) {
                //int numpos = (int)it.lastPositions.Count;
                //if (numpos > 0) UnityEngine.Debug.LogError("numpos > 0 is false");
                //Rect r = it.lastPositions [numpos - 1];
                //Debug.Log("DetectionBasedTracker::updateTrackedObjects: deleted object " + r.x + " " + r.y + " " + r.width + " " + r.height);

                UIManager.Instance.CloseWindow((it.trackerEntity as PacientTracker).Window.stacker);
                trackedObjects.Remove(it);
                Debug.Log("Removed");

            } else {
                t++;
            }
        }
    }

    private static Rect CalcTrackedObjectPositionToShow(int i) {
        if ((i < 0) || (i >= trackedObjects.Count)) {
            Debug.Log("DetectionBasedTracker::calcTrackedObjectPositionToShow: ERROR: wrong i=" + i);
            return new Rect();
        }
        if (trackedObjects[i].numDetectedFrames <= innerParameters.numStepsToWaitBeforeFirstShow) {
            //Debug.Log("DetectionBasedTracker::calcTrackedObjectPositionToShow: " + "trackedObjects[" + i + "].numDetectedFrames=" + trackedObjects[i].numDetectedFrames + " <= numStepsToWaitBeforeFirstShow=" + innerParameters.numStepsToWaitBeforeFirstShow + " --- return empty Rect()");
            return new Rect();
        }
        if (trackedObjects[i].numFramesNotDetected > innerParameters.numStepsToShowWithoutDetecting) {
            return new Rect();
        }

        List<Rect> lastPositions = trackedObjects[i].lastPositions;

        int N = lastPositions.Count;
        if (N <= 0) {
            Debug.Log("DetectionBasedTracker::calcTrackedObjectPositionToShow: ERROR: no positions for i=" + i);
            return new Rect();
        }

        int Nsize = Math.Min(N, (int)weightsSizesSmoothing.Count);
        int Ncenter = Math.Min(N, (int)weightsPositionsSmoothing.Count);

        Point center = new Point();
        double w = 0, h = 0;
        if (Nsize > 0) {
            double sum = 0;
            for (int j = 0; j < Nsize; j++) {
                int k = N - j - 1;
                w += lastPositions[k].width * weightsSizesSmoothing[j];
                h += lastPositions[k].height * weightsSizesSmoothing[j];
                sum += weightsSizesSmoothing[j];
            }
            w /= sum;
            h /= sum;
        } else {
            w = lastPositions[N - 1].width;
            h = lastPositions[N - 1].height;
        }

        if (Ncenter > 0) {
            double sum = 0;
            for (int j = 0; j < Ncenter; j++) {
                int k = N - j - 1;
                Point tl = lastPositions[k].tl();
                Point br = lastPositions[k].br();
                Point c1;
                //c1=tl;
                //c1=c1* 0.5f;//
                c1 = new Point(tl.x * 0.5f, tl.y * 0.5f);
                Point c2;
                //c2=br;
                //c2=c2*0.5f;
                c2 = new Point(br.x * 0.5f, br.y * 0.5f);
                //c1=c1+c2;
                c1 = new Point(c1.x + c2.x, c1.y + c2.y);

                //center=center+  (c1  * weightsPositionsSmoothing[j]);
                center = new Point(center.x + (c1.x * weightsPositionsSmoothing[j]), center.y + (c1.y * weightsPositionsSmoothing[j]));
                sum += weightsPositionsSmoothing[j];
            }
            //center *= (float)(1 / sum);
            center = new Point(center.x * (1 / sum), center.y * (1 / sum));
        } else {
            int k = N - 1;
            Point tl = lastPositions[k].tl();
            Point br = lastPositions[k].br();
            Point c1;
            //c1=tl;
            //c1=c1* 0.5f;
            c1 = new Point(tl.x * 0.5f, tl.y * 0.5f);
            Point c2;
            //c2=br;
            //c2=c2*0.5f;
            c2 = new Point(br.x * 0.5f, br.y * 0.5f);

            //center=c1+c2;
            center = new Point(c1.x + c2.x, c1.y + c2.y);
        }
        //Point2f tl=center-(Point2f(w,h)*0.5);
        Point tl2 = new Point(center.x - (w * 0.5f), center.y - (h * 0.5f));
        //Rect res(cvRound(tl.x), cvRound(tl.y), cvRound(w), cvRound(h));
        Rect res = new Rect((int)Math.Round(tl2.x), (int)Math.Round(tl2.y), (int)Math.Round(w), (int)Math.Round(h));
        //LOGD("DetectionBasedTracker::calcTrackedObjectPositionToShow: Result for i=%d: {%d, %d, %d x %d}", i, res.x, res.y, res.width, res.height);

        return res;
    }

    private struct Parameters {
        //public int minObjectSize;
        //public int maxObjectSize;
        //public float scaleFactor;
        //public int minNeighbors;

        public int maxTrackLifetime;
        //public int minDetectionPeriod; //the minimal time between run of the big object detector (on the whole frame) in ms (1000 mean 1 sec), default=0
    };

    private struct InnerParameters {
        public int numLastPositionsToTrack;
        public int numStepsToWaitBeforeFirstShow;
        public int numStepsToTrackWithoutDetectingIfObjectHasNotBeenShown;
        public int numStepsToShowWithoutDetecting;
        public float coeffTrackingWindowSize;
        public float coeffObjectSizeToTrack;
        public float coeffObjectSpeedUsingInPrediction;
    };

    public class TrackedObject {
        public PositionsVector lastPositions;
        public int numDetectedFrames;
        public int numFramesNotDetected;
        public int id;
        static private int _id = 0;

        public bool IsNew = true;
        public Rect rectSnapshot = null;
        public ITrackerEntity trackerEntity = null;

        public MeshRenderer meshRenderer = null;


        public TrackedObject(OpenCVForUnity.CoreModule.Rect rect) {
            lastPositions = new PositionsVector();

            numDetectedFrames = 1;
            numFramesNotDetected = 0;

            lastPositions.Add(rect.clone());

            _id = GetNextId();
            id = _id;
        }

        static int GetNextId() {
            _id++;
            return _id;
        }
    }
}

#endif