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
using BestHTTP.Logger;


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
        
    }

    private static void StartApplication() {


        AppCommandCenter.Instance.MineField();

        UIManager.Instance.LoginMenu.gameObject.SetActive(!UIManager.Instance.LoginMenu.gameObject.activeInHierarchy);

        Vector3 position = Camera.main.transform.position + Camera.main.transform.forward * UIManager.Instance.WindowDistance;

        position.y += UIManager.Instance.AxisYOffset;

        UIManager.Instance.LoginMenu.gameObject.transform.position = position;
        UIManager.Instance.LoginMenu.gameObject.transform.LookAt(Camera.main.transform.position);


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
        MRDebug.SetCubeForTest(_cubeForTest);
        MRDebug.SetSphereForTest(_sphereForTest);
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
