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

public class AppCommandCenter : MonoBehaviour
{
    //
    BinaryTree pacientsMemory;

    [Header("World Markers:")]
    [SerializeField] GameObject personMarker;

    [Header("Debugger:")]
    [SerializeField] TextMeshPro debugText;
    [SerializeField] GameObject cubeForTest;
    [SerializeField] GameObject sphereForTest;
    [SerializeField] GameObject lineForTest;
    [SerializeField] GameObject detectionName;

    // Controllers
    APIController apiController;


    // Attr's for the Machine Learning and detections
    private static FrameHandler _frameHandler = null;
    public static FrameHandler frameHandler
    {
        get { return AppCommandCenter._frameHandler; } set { if (AppCommandCenter.frameHandler == null) AppCommandCenter._frameHandler = value; }  
    }
    private Mat tempFrameMat;



    private bool justStop = false;
    private byte timeToStop = 0;


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

    public PersonProfile personMaker;

#if ENABLE_WINMD_SUPPORT
        async void Start()
#else
    void Start()
#endif

    {
        SetDebugger();
        Debugger.AddText("1");
        Debug.Log(SystemInfo.processorCount);
        LoadSavedData();
        Debugger.AddText("2");
        apiController = FindObjectOfType<APIController>();
        pacientsMemory = new BinaryTree();
        Debugger.AddText("3"); /*
        APIController.Field queryOperation = new APIController.Field(
            "medicationToTake", new APIController.FieldParams[] { 
                new APIController.FieldParams("id", "\"923fe860496a11eda8fd00c0caaaf470\""),
            });

        Debugger.AddText("4");
       
        apiController.ExecuteQuery("3466fab4975481651940ed328aa990e4", queryOperation,
            ( message) => {
                Debug.Log(message);
                try
                {
                    dynamic response = JObject.Parse(@message);
                    Debug.Log(JObject.Parse(@message)["data"]["medicationToTake"][0]["timeMeasure"]);

                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
                Debug.Log("yo");
            },
            new APIController.Field[] { 
                new APIController.Field("timeMeasure")
                /*,
                new APIController.Field("medication", new APIController.Field[] { 
                    new APIController.Field("name")
            })
        });

        */
        Debugger.AddText("5");

#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.frameHandler = await FrameHandler.CreateAsync();
        Debugger.AddText("6");
#endif

        apiController.CreateWebSocketConnection(apiController.pacientsDetection, MapPredictions);

        Debugger.AddText("7");

        /*
        GameObject newVisualTracker = UnityEngine.Object.Instantiate(personProfile, Vector3.zero, Quaternion.LookRotation(Camera.main.transform.position, Vector3.up));
        Pacient newPerson = new Pacient(newVisualTracker.GetComponent<PersonProfile>(), legacy_TrackerCSRT.create());
        newPerson.UpdateEmotion("Anger");
        newPerson.UpdateEmotion("Affection");
        */
    }


    private async void MapPredictions(string predictions)
    {
        
        try { 
            var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
            Debugger.AddText(results.ToString());

      
        
        /*
        int y1 = results[0].list[0].faceRect.y1;
        int y2 = results[0].list[0].faceRect.y2;
        int bodyY = results[0].list[0].bodyCenter.y;
        MRWorld.pixelPointRatio.distPixel = bodyY - (y1 + ((y2 - y1) * 0.5f));
        */
        Vector3 facePos = Vector3.zero;
        Vector3 bodyPos = Vector3.zero;

        Debugger.AddText(">>>>>");

        

        foreach (Detection detection in results[0].list)
        {

            FaceRect faceRect = detection.faceRect;

            Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
            bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1));

            unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
            facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1), null, 31, true, detectionName);


            //MRWorld.pixelPointRatio.distPoint = facePos.y - bodyPos.y;

           

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

                    Debugger.AddText("NO PERSON IN BINARY TREE");
                    Person newPerson;
                   
                    TrackingManager.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newPerson, "Pacient");
                    (newPerson as Pacient).personProfile.gameObject.name = detection.id.ToString();
                 
                    newPerson.id = detection.id;

                    if (newPerson is Pacient)
                        (newPerson as Pacient).UpdateEmotion(detection.emotions.categorical[0].ToString());


                    GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

                    detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                    pacientsMemory.Add(newPerson.id, newPerson);

                    GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                    three.GetComponent<Renderer>().material.color = Color.red;

                    Debugger.AddText((newPerson as Pacient).personProfile.gameObject.name);

                }
                else
                {
                    Debugger.AddText("PERSON FINDED IN BINARY TREE: " + node.GetType().ToString());
             
                    
                    if (node.data is Pacient)
                    {
                        (node.data as Pacient).UpdateEmotion(detection.emotions.categorical[0]);
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
                        WebSocket wsTemp = apiController.GetWebSocket(apiController.pacientsDetection);
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
            bool wasUpdated = TrackingManager.UpdateTrackers();
            if (wasUpdated) {
                timeToStop++;
                if (timeToStop >= 20)
                    justStop = true;
            }
#endif

        }

    }



}
