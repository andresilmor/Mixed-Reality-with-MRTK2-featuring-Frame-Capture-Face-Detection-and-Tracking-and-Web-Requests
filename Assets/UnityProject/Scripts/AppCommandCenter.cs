using BestHTTP.WebSocket;
using Microsoft.MixedReality.Toolkit.UI;
using Newtonsoft.Json;
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


    async void Start()
    {
        LoadSavedData();
        SetDebugger();



        apiController = FindObjectOfType<APIController>();
        pacientsMemory = new BinaryTree();

#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.frameHandler = await FrameHandler.CreateAsync();
#endif


        apiController.CreateWebSocketConnection(apiController.pacientsDetection, this.MapPredictions);

        /*
        GameObject newVisualTracker = UnityEngine.Object.Instantiate(personProfile, Vector3.zero, Quaternion.LookRotation(Camera.main.transform.position, Vector3.up));
        Pacient newPerson = new Pacient(newVisualTracker.GetComponent<PersonProfile>(), legacy_TrackerCSRT.create());
        newPerson.UpdateEmotion("Anger");
        newPerson.UpdateEmotion("Affection");
        */
    }


    private async void MapPredictions(string predictions)
    {



        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());


        int y1 = results[0].list[0].faceRect.y1;
        int y2 = results[0].list[0].faceRect.y2;
        int bodyY = results[0].list[0].bodyCenter.y;
        MRWorld.pixelPointRatio.distPixel = bodyY - (y1 + ((y2 - y1) * 0.5f));

        Vector3 facePos = Vector3.zero;
        Vector3 bodyPos = Vector3.zero;



        Debugger.SetFieldView();

        foreach (Detection detection in results[0].list)
        {

            FaceRect faceRect = detection.faceRect;

            Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
            bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1));

            unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
            facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, (uint)(faceRect.x2 - faceRect.x1), null, 31, true, detectionName);



            // ------------------------------------ DANGER ZONE --------------------------------------------- //




            MRWorld.pixelPointRatio.distPoint = facePos.y - bodyPos.y;

            /*

            Vector3 test = facePos;
            test.x = test.x - MRWorld.ConvertPixelDistToPoint((faceRect.x1 + ((faceRect.x2 - faceRect.x1) * 0.5f)) - faceRect.x1);

            GameObject two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), test, Quaternion.identity);
            two.GetComponent<Renderer>().material.color = Color.red;

             test = facePos;
            test.x = test.x + MRWorld.ConvertPixelDistToPoint((faceRect.x1 + ((faceRect.x2 - faceRect.x1) * 0.5f)) - faceRect.x1);

             two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), test, Quaternion.identity);
            two.GetComponent<Renderer>().material.color = Color.red;

             test = facePos;
            test.y = test.y - MRWorld.ConvertPixelDistToPoint((faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f)) - faceRect.y1);

             two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), test, Quaternion.identity);
            two.GetComponent<Renderer>().material.color = Color.red;

             test = facePos;
            test.y = test.y + MRWorld.ConvertPixelDistToPoint((faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f)) - faceRect.y1);

             two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), test, Quaternion.identity);
            two.GetComponent<Renderer>().material.color = Color.red;
            */

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

                 
                    newPerson.id = detection.id;



                    

                    // ---------------------------------------------------------------------------------------------- //

                    // FIINE TILL HERE
                    if (newPerson is Pacient)
                        (newPerson as Pacient).UpdateEmotion(detection.emotions.categorical[0].ToString());


                    GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

                    detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                    pacientsMemory.Add(newPerson.id, newPerson);

                    /*

                    Person newPerson = new Pacient(null, null);
                    newPerson.id = 23;
                    Debug.Log(pacientsMemory.GetTreeDepth());

                    pacientsMemory.Add(newPerson.id, newPerson);
                    Debug.Log(pacientsMemory.GetTreeDepth());
                    Debug.Log(pacientsMemory.Find(23) != null);
                    Debug.Log((pacientsMemory.Find(23).data as Pacient));
                    */

                    GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                    three.GetComponent<Renderer>().material.color = Color.red;

                }
                else
                {
                    Debugger.AddText("PERSON FINDED IN BINARY TREE: " + node.GetType().ToString());
             
                    
                    if (node.data is Pacient)
                    {
                        (node.data as Pacient).UpdateEmotion(detection.emotions.categorical[0]);
                        //(node.data as Pacient).UpdateOneTracker(detection.faceRect, tempFrameMat);

                    }
                    

                }
            }
            catch (Exception ex) {
                Debugger.AddText(ex.Message);
            }

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
