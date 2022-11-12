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
using Realms;
using Realms.Exceptions;

using System.Net.NetworkInformation;

public class AppCommandCenter : MonoBehaviour
{

    private Realm realm;

    BinaryTree pacientsMemory;

    [Header("World Markers:")]
    [SerializeField] GameObject personMarker;

    [Header("Debugger:")]
    [SerializeField] TextMeshPro debugText;
    [SerializeField] GameObject cubeForTest;
    [SerializeField] GameObject sphereForTest;
    [SerializeField] GameObject lineForTest;
    [SerializeField] GameObject detectionName;
    [SerializeField] GameObject general;

    // Controllers


    // Attr's for the Machine Learning and detections
    private static FrameHandler _frameHandler = null;
    public static FrameHandler frameHandler
    {
        get { return AppCommandCenter._frameHandler; } set { if (AppCommandCenter.frameHandler == null) AppCommandCenter._frameHandler = value; }  
    }
    private Mat tempFrameMat;



    private bool justStop = false;
    private byte timeToStop = 0;

    public string ShowNetworkInterfaces()
    {
        IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        string info = "";
        foreach (NetworkInterface adapter in nics)
        {
            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            string mac = null;
            for (int i = 0; i < bytes.Length; i++)
            {
                mac = string.Concat(mac + (string.Format("{0}", bytes[i].ToString("X2"))));
                if (i != bytes.Length - 1)
                {
                    mac = string.Concat(mac + "-");
                }
            }
            info += mac + "\n";

            info += "\n";
        }
        return info;
    }

    private static AppCommandCenter _instance;
    public static AppCommandCenter Instance
    {
        get { return _instance; }
        set
        {
            if (_instance == null)
            {
                _instance = value;
            }
            else
            {
                Destroy(value);
            }
        }
    }

    void OnEnable()
    {
        BestHTTP.HTTPManager.Setup();

        var config = new RealmConfiguration
        {
            SchemaVersion = 1
        };
        config.ShouldDeleteIfMigrationNeeded = true;
        realm = Realm.GetInstance(config);

        Debug.Log("Persisted");
        realm.Write(() => {
            realm.RemoveAll();

        });

    }

    void OnDisable()
    {
        realm.Dispose();
    }


#if ENABLE_WINMD_SUPPORT
    async void Start()
#else
    void Start()
#endif
    {
        SetDebugger();
        Debug.Log(SystemInfo.processorCount);
        LoadSavedData();

        pacientsMemory = new BinaryTree();

        Debugger.AddText(
        ShowNetworkInterfaces());


        /*
        APIController.Field queryOperation = new APIController.Field(
            "medicationToTake", new APIController.FieldParams[] { 
                new APIController.FieldParams("id", "\"3c764a20-629c-4be9-b19b-5f87bddd60d5\""),
            });

        await APIController.ExecuteQuery("Read", queryOperation,
            (message) => {
                Debug.Log(message);
                try
                {
                    dynamic response = JObject.Parse(@message);
                    Debug.Log(JObject.Parse(@message)["data"]["medicationToTake"]);

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                Debug.Log("yo");
            },
            new APIController.Field[] { 
                new APIController.Field("atTime"),
                new APIController.Field("quantity"),
                new APIController.Field("medication", new APIController.Field[] {
                    new APIController.Field("name")
                    })
         
        });
        */

#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.frameHandler = await FrameHandler.CreateAsync();
#endif

        APIController.CreateWebSocketConnection(APIController.pacientsDetection, MapPredictions);

    }


    private async void MapPredictions(string predictions)
    {
        Debugger.AddText("HERE");
        try { 
            var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
            Debugger.AddText(results.ToString());

            Vector3 facePos = Vector3.zero;
            Vector3 bodyPos = Vector3.zero;


            foreach (Detection detection in results[0].list)
            {

                FaceRect faceRect = detection.faceRect;

                Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
                bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1));

                unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
                facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1), null, 31, true, detectionName);

                if (Vector3.Distance(bodyPos, MRWorld.tempExtrinsic.Position) < Vector3.Distance(facePos, MRWorld.tempExtrinsic.Position))
                {
                    facePos.x = bodyPos.x;
                    facePos.z = bodyPos.z;
                }


                try
                {
                    BinaryTree.Node node = pacientsMemory.Find(detection.id);

                    if (node is null)
                    {
                        Debugger.AddText("NEW ON TREE");
                        object newPerson;
                   
                        TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newPerson, "Pacient");
                        (newPerson as Pacient).pacientMark.gameObject.name = detection.id.ToString();

                        (newPerson as Pacient).id = detection.id;

                        if (newPerson is Pacient)
                            (newPerson as Pacient).UpdateEmotion(detection.emotions.categorical[0].ToString());


                        GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

                        detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                        pacientsMemory.Add(detection.id, newPerson);

                        GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                        three.GetComponent<Renderer>().material.color = Color.red;

                        Debugger.AddText((newPerson as Pacient).pacientMark.gameObject.name);

                    }
                    else
                    {
                        Debugger.AddText("ALREADY EXISTS ON TREE");

                        if (node.data is Pacient)
                        {
                            //(node.data as Pacient).UpdateEmotion(detection.emotions.categorical[0]);
                            //(node.GraphQLData as Pacient).UpdateOneTracker(detection.faceRect, tempFrameMat);

                        }
                    

                    }
                }
                catch (Exception ex) {
                    Debugger.AddText(ex.Message);
                }

            }
        }
        catch (Exception error)
        {
            Debugger.AddText("Error: " + error.Message.ToString());

        }



        return;
    }

    private void SetDebugger()
    {
        Debugger.SetCubeForTest(cubeForTest);
        Debugger.SetSphereForTest(sphereForTest);
        Debugger.SetDebugText(debugText);
        LineDrawer.SetDrawLine(lineForTest);

    }

    private void LoadSavedData()
    {
        if (pacientsMemory == null)
            pacientsMemory = new BinaryTree();

    }

    public async void DetectPacients()
    {
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
                      
                        Instantiate(Debugger.GetSphereForTest(), Camera.main.transform.position, Quaternion.identity);
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

    void Update()
    {
        if (!justStop)
        {

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



}
