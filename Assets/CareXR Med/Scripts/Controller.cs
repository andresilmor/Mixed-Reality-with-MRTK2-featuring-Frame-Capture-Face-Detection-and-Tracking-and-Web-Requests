using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Net.NetworkInformation;
using UnityEngine.SceneManagement;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Graphics.Imaging;
using Windows.Foundation;
using Windows.Media.Devices.Core;
using Windows.Perception.Spatial;
using Windows.Perception.Spatial.Preview;

using HL2UnityPlugin;

#endif

using Debug = XRDebug;
using BestHTTP.Logger;


[DisallowMultipleComponent]
public class Controller : MonoBehaviour {

    private static Camera _cameraMain;
    public static Camera CameraMain {
        get {
            if (_cameraMain == null)
                CameraMain = Camera.main;
            return _cameraMain;
        }
        private set { _cameraMain = value; }

    }

    [Header("Config:")]
    [SerializeField] GameObject controllers;
    public Transform DetectionDistanceLimit;


    [Header("Debugger:")]
    [SerializeField] TextMeshPro _debugText;
    [SerializeField] GameObject _cubeForTest;
    [SerializeField] public GameObject OutlinedCube;
    [SerializeField] GameObject _sphereForTest;
    [SerializeField] GameObject _lineForTest;
    [SerializeField] public GameObject _detectionName;
    [SerializeField] GameObject _general;
    /*
    // Attr's for the Machine Learning and detections
    private static MediaCaptureManager _cameraFrameReader = null;
    public static MediaCaptureManager MediaCaptureManager {
        get { return null; }
        set { if (Controller.MediaCaptureManager == null) Controller._cameraFrameReader = value; }
    }
    */
    public byte timeToStop = 0;

    private bool _additiveScenesLoaded = false;
    public bool AdditiveScenesLoaded {
        private set {
            _additiveScenesLoaded = value;
        }
        get {
            return _additiveScenesLoaded;
        }
    }



    public string ShowNetworkInterfaces() {
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = "";
        foreach (NetworkInterface adapter in nics) {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string mac = null;
            for (int i = 0; i < bytes.Length; i++) {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1) {
                    mac = string.Concat(mac + "-");
                }
            }
            info += mac + "\n";

            info += "\n";
        }
        return info;
    }

    private static Controller _instance = null;
    public static Controller Instance {
        get { return _instance; }
        set {
            if (_instance == null) {
                _instance = value;
            } else {
                Destroy(value);
            }
        }
    }

    void OnEnable() {
        BestHTTP.HTTPManager.Setup();

        RealmManager.BulldozeRealm();
        Debug.Log(JObject.Parse("{ \"channel\": \"" + "dasdas" + "\", \"confirmation\": " + false.ToString().ToLower() + " }").ToString());
    }

    void OnDisable() {
        RealmManager.realm.Dispose();

    }

    void Awake() {
        Instance = this;
    }


#if ENABLE_WINMD_SUPPORT
    async void Start()
#else
    async void Start()
#endif
    {

        SetDebugger();  
        //await MLManager.ToggleLiveDetection();

        StartCoroutine(LoadAdditiveScenes());


    }

    // TODO: Refactorate code of related to scenes for something alike "SceneManager" ????
    private IEnumerator LoadAdditiveScenes() {
        //Load Additive Scenes
        yield return StartCoroutine(LoadAddititiveScene("UI"));

        AdditiveScenesLoaded = true;
        StartApplication();

    }

    private IEnumerator LoadAddititiveScene(string scene) {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;

        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone) {
            //Output the current progress
            //Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f) {
                //Change the Text to show the Scene is ready
                //Debug.Log("Press the space bar to continue");
                //Wait to you press the space key to activate the Scene
                 asyncOperation.allowSceneActivation = true;
                
            }

            yield return null;

        }

    }


    async private void MineField() {
        /*
        APIManager.CreateWebSocketConnection(APIManager.FrameFaceRecognition, null, null, (WebSocket ws) => {


            ProtoImage request = new ProtoImage();
            Debug.Log("AnalyseFrame 3");
            request.image = null;
            Debug.Log("AnalyseFrame 4");

            Debug.Log("AnalyseFrame 4.1");
            using (var memoryStream = new System.IO.MemoryStream()) {

                Debug.Log("AnalyseFrame 4.2");
                ProtoBuf.Serializer.Serialize(memoryStream, request);

                Debug.Log("AnalyseFrame 4.3");

                byte[] message = memoryStream.ToArray();
                Debug.Log("AnalyseFrame 4.4");


                APIManager.GetWebSocket(APIManager.FrameFaceRecognition).Send(message);

                Debug.Log("AnalyseFrame 4.5 ");

            }
        }).Open();*/
    }

    private static void StartApplication() {


        Controller.Instance.MineField();

        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

        position.y += UIManager.Instance.AxisYOffset;

        if (RealmManager.FindActiveUser(true) == null) {
            UIManager.Instance.LoginMenu.gameObject.SetActive(true);
            UIManager.Instance.LoginMenu.gameObject.transform.position = position;
            UIManager.Instance.LoginMenu.gameObject.transform.LookAt(Camera.main.transform.position);

        } else {
            AccountManager.OnSucessfullLogin(true);
            AccountManager.IsLogged = true;
            
        }

    }

    public static void StopApplication() {
        // save any game Data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    private void SetDebugger() {
        XRDebug.SetCubeForTest(_cubeForTest);
        XRDebug.SetSphereForTest(_sphereForTest);
        LineDrawer.SetDrawLine(_lineForTest);

    }

    async void Update() {
        /*
#if ENABLE_WINMD_SUPPORT
        FaceDetectionManager.Counter += 1;
        if (MediaCaptureManager.IsCapturing)
        {
            CameraFrame returnFrame = await MediaCaptureManager.GetLatestFrame();


            if (returnFrame != null)
            {

                if (FaceDetectionManager.Counter >= 20)
                {
                    FaceDetectionManager.Counter = 0;

                    Task thread = Task.Run(async () =>
                    {
                        try
                        {
                            // Get the prediction from the model
                            var result = await FaceDetectionManager.EvaluateVideoFrameAsync(returnFrame.Bitmap);
                            //If faces exist in frame, identify in 3D
                            if (result.Faces != null && result.Faces.Length > 0)
                            {
                                UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                                {
                                    //Visualize the detections in 3D to create GameObjects for eye gaze to interact with
                                    Debug.Log("Face Detected: " + result.Faces.Length);
                                }, true);
                            }

                        }
                        catch (Exception ex)
                        {
                            Debug.Log("Exception:" + ex.Message);
                        }

                    });
                }

            }

        }
#endif
        */
    }

    private void OnDestroy() {
        APIManager.CloseAllWebSockets();
        StopAllCoroutines();

    }

}
