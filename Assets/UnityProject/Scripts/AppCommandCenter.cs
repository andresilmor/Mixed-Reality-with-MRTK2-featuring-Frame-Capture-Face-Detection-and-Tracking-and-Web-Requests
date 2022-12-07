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



    BinaryTree pacientsMemory;

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
    [SerializeField] GameObject personMarker;

    //[field:SerializeField] x {get; private set;}

    [Header("Debugger:")]
    [SerializeField] TextMeshPro debugText;
    [SerializeField] GameObject cubeForTest;
    [SerializeField] GameObject sphereForTest;
    [SerializeField] GameObject lineForTest;
    [SerializeField] GameObject detectionName;
    [SerializeField] GameObject general;


    // Attr's for the Machine Learning and detections
    private static FrameHandler _frameHandler = null;
    public static FrameHandler frameHandler {
        get { return AppCommandCenter._frameHandler; }
        set { if (AppCommandCenter.frameHandler == null) AppCommandCenter._frameHandler = value; }
    }
    private Mat tempFrameMat;

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

        RealmController.BulldozeRealm();

    }

    void OnDisable() {
        RealmController.realm.Dispose();

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

        if (pacientsMemory == null)
            pacientsMemory = new BinaryTree();

        Debugger.AddText(ShowNetworkInterfaces());

        if (!SceneManager.GetSceneByName("UI").isLoaded)
            await SceneManager.LoadSceneAsync("UI", LoadSceneMode.Additive);


#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.frameHandler = await FrameHandler.CreateAsync();
#endif

        //APIController.CreateWebSocketConnection(APIController.pacientsDetection, MapPredictions);

    }

    public static void StartApplication() {
        UIWindow loginWindow = UIController.Instance.OpenWindow("Header & Two Buttons", stackerName: "Login Window");

        (loginWindow.components["Title"] as TextMeshPro).text = "Welcome Caregiver";
        (loginWindow.components["Subtitle"] as TextMeshPro).text = "Select Login Method";
        (loginWindow.components["TopButtonText"] as TextMeshPro).text = "Keyboard";
        (loginWindow.components["BotButtonText"] as TextMeshPro).text = "QR Code";

        AccountController.loginWindow = loginWindow;

        (loginWindow.components["BotButton"] as Interactable).OnClick.AddListener(() => { System.Threading.Tasks.Task<bool> task = AccountController.LoginQR(); });

    }


    private async void MapPredictions(string predictions) {
        Debugger.AddText("HERE");
        try {
            var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
            Debugger.AddText(results.ToString());

            Vector3 facePos = Vector3.zero;
            Vector3 bodyPos = Vector3.zero;


            foreach (Detection detection in results[0].list) {

                FaceRect faceRect = detection.faceRect;

                Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
                bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1));

                unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
                facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1), null, 31, true, detectionName);

                if (Vector3.Distance(bodyPos, MRWorld.tempExtrinsic.Position) < Vector3.Distance(facePos, MRWorld.tempExtrinsic.Position)) {
                    facePos.x = bodyPos.x;
                    facePos.z = bodyPos.z;
                }


                try {
                    BinaryTree.Node node = pacientsMemory.Find(detection.id);

                    if (node is null) {
                        Debugger.AddText("NEW ON TREE");
                        object newTracker;

                        TrackerController.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newTracker, "PacientTracker");
                        (newTracker as PacientTracker).gameObject.name = detection.id.ToString();

                        (newTracker as PacientTracker).id = detection.id;

                        if (newTracker is PacientTracker)
                            (newTracker as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0].ToString());


                        GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

                        detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                        pacientsMemory.Add(detection.id, newTracker);

                        GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                        three.GetComponent<Renderer>().material.color = Color.red;

                        Debugger.AddText((newTracker as PacientTracker).gameObject.name);

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

    private void SetDebugger() {
        Debugger.SetCubeForTest(cubeForTest);
        Debugger.SetSphereForTest(sphereForTest);
        Debugger.SetDebugText(debugText);
        LineDrawer.SetDrawLine(lineForTest);

    }


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
                        WebSocket wsTemp = APIController.GetWebSocket(APIController.pacientsDetection);
                        if (wsTemp.IsOpen)
                        {
                            Debugger.AddText("Is Open");
                        } else
                        {
                            Debugger.AddText("Is not open");
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

    void Update() {
        if (!justStop) {

#if ENABLE_WINMD_SUPPORT
            bool wasUpdated = TrackerController.UpdateTrackers();
            if (wasUpdated) {
                timeToStop++;
                if (timeToStop >= 20)
                    justStop = true;
            }
#endif

        }

    }

    private void OnDestroy() {
        APIController.CloseAllWebSockets();
        StopAllCoroutines();
    }

}
