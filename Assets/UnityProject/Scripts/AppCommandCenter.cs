using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.TrackingModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

using System.Net.NetworkInformation;
using Microsoft.MixedReality.SampleQRCodes;
using System.Linq;
using static BestHTTP.SecureProtocol.Org.BouncyCastle.Math.EC.ECCurve;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;


[DisallowMultipleComponent]
public class AppCommandCenter : MonoBehaviour {

    // TODO change variable names: PublicVariable, _privateVariable, normalVariable

    public BinaryTree liveTrackers { get; private set; }

    private static Camera _cameraMain;
    public static Camera cameraMain {
        get {
            if (_cameraMain == null)
                cameraMain = Camera.main;
            return _cameraMain;
        }

        private set { _cameraMain = value; }

    }

    [Header("Config:")]
    [SerializeField] GameObject controllers;


    //[field:SerializeField] x {get; private set;}

    [Header("Debugger:")]
    [SerializeField] TextMeshPro _debugText;
    [SerializeField] GameObject _cubeForTest;
    [SerializeField] GameObject _sphereForTest;
    [SerializeField] GameObject _lineForTest;
    [SerializeField] public GameObject _detectionName;
    [SerializeField] GameObject _general;


    // Attr's for the Machine Learning and detections
    private static FrameHandler _frameHandler = null;
    public static FrameHandler frameHandler {
        get { return AppCommandCenter._frameHandler; }
        set { if (AppCommandCenter.frameHandler == null) AppCommandCenter._frameHandler = value; }
    }
    

    private bool justStop = false;
    private byte timeToStop = 0;

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
        Debug.Log(AppCommandCenter.cameraMain.transform.position.ToString());
        SetDebugger();

        Debugger.AddText("Debug 2");
        liveTrackers = new BinaryTree();


        if (!SceneManager.GetSceneByName("UI").isLoaded)
            await SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);

        MineField();

        await MLManager.ToggleLiveDetection();
        StartCoroutine(WarmApplication());

    }

    private IEnumerator WarmApplication() {
        yield return new WaitForSeconds(1);
        StartApplication();

    }

    //Test start code
    private void MineField() {
        DateTime testDT = DateTime.Now;
        testDT  = testDT.Add(new TimeSpan(0, 0, 5));
        Debug.Log("=> " + testDT.ToString());
        TimedEventManager.AddUpdateTimedEvent("3c764a20-629c-4be9-b19b-5f87bddd60d5", new TimedEventHandler(testDT, () => {
            UIWindow timerOverNotification = UIManager.Instance.OpenWindow(WindowType.Header_OneButtonAndClose, stackerName: "Time Over Notification", isNotification: true);
            (timerOverNotification.components["Title"] as TextMeshPro).text = "Time Over";
            (timerOverNotification.components["Description"] as TextMeshPro).text = "Yay, time over";
            (timerOverNotification.components["ActionButtonText"] as TextMeshPro).text = "Locate Pacient";
            (timerOverNotification.components["ActionButton"] as Interactable).OnClick.AddListener(() => {
                Debugger.AddText("Ok im calling");
                MLManager.AnalyseFrame();

            });
            (timerOverNotification.components["CloseButton"] as Interactable).OnClick.AddListener(() => {
                UIManager.Instance.CloseWindow(timerOverNotification.stacker, (timerOverNotification.components["CloseButton"] as Interactable).gameObject.GetComponent<AudioSource>());

            });


        }));

        Debug.Log(TimedEventManager.GetTimedEventTimeLeft("TEST"));


    }

    private static void StartApplication() {
        UIWindow loginWindow = UIManager.Instance.OpenWindow(WindowType.Header_TwoButtons_00, stackerName: "Login Window");

        (loginWindow.components["Title"] as TextMeshPro).text = "Welcome Caregiver";
        (loginWindow.components["Subtitle"] as TextMeshPro).text = "Select Login Method";
        (loginWindow.components["TopButtonText"] as TextMeshPro).text = "Keyboard";
        (loginWindow.components["BotButtonText"] as TextMeshPro).text = "QR Code";

        AccountManager.loginWindow = loginWindow;

        (loginWindow.components["BotButton"] as Interactable).OnClick.AddListener(() => { 
            System.Threading.Tasks.Task<bool> task = AccountManager.LoginQR(); 
        });

    }
    
    private void SetDebugger() {
        Debugger.SetCubeForTest(_cubeForTest);
        Debugger.SetSphereForTest(_sphereForTest);
        Debugger.SetDebugText(_debugText);
        LineDrawer.SetDrawLine(_lineForTest);

    }

    void Update() {
        if (!justStop) {

#if ENABLE_WINMD_SUPPORT
            bool wasUpdated = TrackerManager.UpdateTrackers();
            if (wasUpdated) {
                timeToStop++;
                if (timeToStop >= 20)
                    justStop = true;
            }
#endif

        }

    }

    private void OnDestroy() {
        APIManager.CloseAllWebSockets();
        StopAllCoroutines();

    }

}
