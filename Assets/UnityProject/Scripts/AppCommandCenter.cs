using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Net.NetworkInformation;
using UnityEngine.SceneManagement;

using Debug = MRDebug;


[DisallowMultipleComponent]
public class AppCommandCenter : MonoBehaviour {

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
    [SerializeField] GameObject _sphereForTest;
    [SerializeField] GameObject _lineForTest;
    [SerializeField] public GameObject _detectionName;
    [SerializeField] GameObject _general;

    // Attr's for the Machine Learning and detections
    private static CameraFrameReader _cameraFrameReader = null;
    public static CameraFrameReader CameraFrameReader {
        get { return AppCommandCenter._cameraFrameReader; }
        set { if (AppCommandCenter.CameraFrameReader == null) AppCommandCenter._cameraFrameReader = value; }
    }
    
    public byte timeToStop = 0;



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

    private static AppCommandCenter _instance = null;
    public static AppCommandCenter Instance {
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
        MineField();    
        //await MLManager.ToggleLiveDetection();

        StartCoroutine(LoadAdditiveScenes());

#if ENABLE_WINMD_SUPPORT
        if (AppCommandCenter.CameraFrameReader == null)
            AppCommandCenter.CameraFrameReader = await CameraFrameReader.CreateAsync();

#endif

    }

    // TODO: Refactorate code of related to scenes for something alike "SceneManager" ????
    private IEnumerator LoadAdditiveScenes() {
        //Load Additive Scenes
        yield return StartCoroutine(LoadAddititiveScene("UI"));

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
            Debug.Log("Loading progress: " + (asyncOperation.progress * 100) + "%");

            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f) {
                //Change the Text to show the Scene is ready
                Debug.Log("Press the space bar to continue");
                //Wait to you press the space key to activate the Scene
                 asyncOperation.allowSceneActivation = true;
                
            }

            yield return null;

        }

    }


    private void MineField() { // For features test / Out-flow
        //DateTime testDT = DateTime.Now;
        //testDT  = testDT.Add(new TimeSpan(0, 0, 5));
        //Debug.Log("=> " + testDT.ToString());


        /*TimedEventManager.AddUpdateTimedEvent("3c764a20-629c-4be9-b19b-5f87bddd60d5", new TimedEventHandler(testDT, () => {

            UIWindow timerOverNotification = UIManager.Instance.OpenWindow(WindowType.TD_1btn_Cl_00, stackerName: "Time Over Notification", isNotification: true);
            (timerOverNotification.components["Title"] as TextMeshPro).text = "Medication Alert";
            (timerOverNotification.components["Description"] as TextMeshPro).text = "Pacient, Tiago Monteiro, have medication to take at 12:30.";
            (timerOverNotification.components["ActionButtonText"] as TextMeshPro).text = "Locate Pacient";
            (timerOverNotification.components["ActionButton"] as Interactable).OnClick.AddListener(() => {
                Debug.Log("Ok im calling");
                //APIManager.wsLiveDetection.Send("oi");
                //MLManager.AnalyseFrame();

            });
            (timerOverNotification.components["CloseButton"] as Interactable).OnClick.AddListener(() => {
                UIManager.Instance.CloseWindow(timerOverNotification.stacker, (timerOverNotification.components["CloseButton"] as Interactable).gameObject.GetComponent<AudioSource>());

            });

            if (!APIManager.wsLiveDetection.IsOpen) {
                Debug.Log("nop opened");
                APIManager.wsLiveDetection.Open();
            }
            
            Debug.Log(TimedEventManager.GetTimedEventTimeLeft("TEST"));


        }));*/



    }

    private static void StartApplication() {
        UIWindow loginWindow = UIManager.Instance.OpenWindow(WindowType.H_2btn_00, new LoginView(), stackerName: "Login Window");

    }
    
    private void SetDebugger() {
        MRDebug.SetCubeForTest(_cubeForTest);
        MRDebug.SetSphereForTest(_sphereForTest);
        MRDebug.BindDebugConsole(_debugText);
        LineDrawer.SetDrawLine(_lineForTest);

    }

    void Update() {
       /*
        if (timeToStop > 4) {
            if (TrackerManager.TrackersUpdater != null) { 
                Debug.Log("Updater stopped");
                StopCoroutine(TrackerManager.TrackersUpdater);
                TrackerManager.TrackersUpdater = null;

            }

        }*/

    }

    private void OnDestroy() {
        APIManager.CloseAllWebSockets();
        StopAllCoroutines();

    }

}
