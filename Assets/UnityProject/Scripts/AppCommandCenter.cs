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

    [Header("World Markers:")]
    [SerializeField] public GameObject personMarker;

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

    }


    //Test start code
    private void MineField() {
        DateTime testDT = DateTime.Now;
        testDT  = testDT.Add(new TimeSpan(0, 0, 5));
        Debug.Log("=> " + testDT.ToString());
        TimedEventManager.AddUpdateTimedEvent("3c764a20-629c-4be9-b19b-5f87bddd60d5", new TimedEventHandler(testDT, () => {
            UIWindow timerOverNotification = UIManager.Instance.OpenWindow("Header_OneButtonAndClose", stackerName: "Time Over Notification");
            (timerOverNotification.components["Title"] as TextMeshPro).text = "Time Over";
            (timerOverNotification.components["Description"] as TextMeshPro).text = "Yay, time over";
            (timerOverNotification.components["ActionButtonText"] as TextMeshPro).text = "Locate Pacient";
            (timerOverNotification.components["ActionButton"] as Interactable).OnClick.AddListener(() => {
                Debugger.AddText("Ok im calling");
                MLManager.AnalyseFrame();

            });
            (timerOverNotification.components["CloseButton"] as Interactable).OnClick.AddListener(() => {
                UIManager.Instance.CloseWindow(timerOverNotification.stacker);

            });


        }));

        Debug.Log(TimedEventManager.GetTimedEventTimeLeft("TEST"));


    }

    public static void StartApplication() {
        UIWindow loginWindow = UIManager.Instance.OpenWindow("Header_TwoButtons_00", stackerName: "Login Window");

        (loginWindow.components["Title"] as TextMeshPro).text = "Welcome Caregiver";
        (loginWindow.components["Subtitle"] as TextMeshPro).text = "Select Login Method";
        (loginWindow.components["TopButtonText"] as TextMeshPro).text = "Keyboard";
        (loginWindow.components["BotButtonText"] as TextMeshPro).text = "QR Code";

        AccountManager.loginWindow = loginWindow;

        (loginWindow.components["BotButton"] as Interactable).OnClick.AddListener(() => { 
            System.Threading.Tasks.Task<bool> task = AccountManager.LoginQR(); 
        });

    }
    
    /*
    private async void MapPredictions(string predictions) {
        try {
            var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
            

            Vector3 facePos = Vector3.zero;
            Vector3 bodyPos = Vector3.zero;

            foreach (Detection detection in results[0].list) {

                FaceRect faceRect = detection.faceRect;

                Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
                bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1));

                unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
                facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1), null, 31, true, _detectionName);

                if (Vector3.Distance(bodyPos, MRWorld.tempExtrinsic.Position) < Vector3.Distance(facePos, MRWorld.tempExtrinsic.Position)) {
                    facePos.x = bodyPos.x;
                    facePos.z = bodyPos.z;
                }


                try {
                    BinaryTree.Node node = pacientsMemory.Find(detection.id);

                    if (node is null) {
                        Debugger.AddText("NEW ON TREE");
                        object newTracker;
                        Debugger.AddText("1");
                        TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newTracker, "PacientTracker");
                        (newTracker as PacientTracker).gameObject.name = detection.id.ToString();
                        Debugger.AddText((newTracker as PacientTracker).gameObject.name);
                        Debugger.AddText(detection.id.ToString());
                        Debugger.AddText("2");
                        (newTracker as PacientTracker).id = detection.id;
                        Debugger.AddText("3");
                        if (newTracker is PacientTracker)
                            (newTracker as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0].ToString());

                        Debugger.AddText("4");
                        GameObject detectionTooltip = UnityEngine.Object.Instantiate(_detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);
                        Debugger.AddText("5");
                        detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());
                        Debugger.AddText("6");
                        pacientsMemory.Add(detection.id, newTracker);
                        Debugger.AddText("7");
                        GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                        three.GetComponent<Renderer>().material.color = Color.red;
                        Debugger.AddText("8");

                    } else {
                        Debugger.AddText("ALREADY EXISTS ON TREE");

                        if (node.data is PacientTracker) {

                            (node.data as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0]);
                            //(node.GraphQLData as Pacient).UpdateOneTracker(detection.faceRect, tempFrameMat);

                        }


                    }
                } catch (Exception ex) {
                    Debugger.AddText(ex.Message);
                }

            }
        } catch (Exception error) {
            Debugger.AddText("Error: " + error.Message.ToString());

        }



        return;
    }
    */
    private void SetDebugger() {
        Debugger.SetCubeForTest(_cubeForTest);
        Debugger.SetSphereForTest(_sphereForTest);
        Debugger.SetDebugText(_debugText);
        LineDrawer.SetDrawLine(_lineForTest);

    }

    /*
    public async void DetectPacients() {
        Debugger.SetFieldView();
#if ENABLE_WINMD_SUPPORT
        var lastFrame = AppCommandCenter.frameHandler.LastFrame;
        if (lastFrame.mediaFrameReference != null)
        {
            try
            {
                using (var videoFrame = lastFrame.mediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {
                        byte[] byteArray = await Parser.ToByteArray(videoFrame.SoftwareBitmap);
                        
                        
                        tempFrameMat = lastFrame.frameMat;

                        videoFrame.SoftwareBitmap.Dispose();
                        //Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                      
                        Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.transform.position, Quaternion.identity);
                        Instantiate(Debugger.GetCubeForTest(), lastFrame.extrinsic.Position, Quaternion.identity);
                       
                        //this.tempExtrinsic = lastFrame.extrinsic;
                        //this.tempIntrinsic = lastFrame.intrinsic;
                        MRWorld.UpdateExtInt(lastFrame.extrinsic, lastFrame.intrinsic);
                        
                        FrameCapture frame = new FrameCapture(Parser.Base64ToJson(Convert.ToBase64String(byteArray)));
                        WebSocket wsTemp = APIManager.GetWebSocket(APIManager.mlLiveDetection);
                        if (wsTemp.IsOpen)
                        {
                        } else
                        {
                            wsTemp.Open();
                        }

                        wsTemp.Send("Sending");
                        wsTemp.Send(JsonUtility.ToJson(frame));
                        wsTemp.Send("Sended");
                        /*
                        ws.Send("Sending");
                        ws.Send(JsonUtility.ToJson(frame));
                        ws.Send("Sended");
                        */
                        
    /*
                    }
                    else
                    { Debug.Log("videoFrame or SoftwareBitmap = null"); }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[### Deebug ###] Update Error: {ex.Message}");
            }
        }
        else
        { Debug.Log("lastFrame.mediaFrameReference = null"); 
        }
#endif

    }
        */




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
