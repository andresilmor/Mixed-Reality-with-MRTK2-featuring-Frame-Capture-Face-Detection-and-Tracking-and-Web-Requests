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
    BinaryTree pacients;

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
    private FrameHandler frameHandler;
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
                Debugger.AddText("~cc I was called ?_?");
                Destroy(value);
            }
        }
    }

    public PersonMarker personMaker;


    async void Start()
    {
        LoadSavedData();
        SetDebugger();

        apiController = FindObjectOfType<APIController>();

#if ENABLE_WINMD_SUPPORT
        frameHandler = await FrameHandler.CreateAsync();
#endif

        Debugger.AddText("Buld 1");

        apiController.CreateWebSocketConnection(apiController.pacientsDetection, this.MapPredictions);

        /*
        GameObject newVisualTracker = UnityEngine.Object.Instantiate(personMarker, Vector3.zero, Quaternion.LookRotation(Camera.main.transform.position, Vector3.up));
        Pacient newPerson = new Pacient(newVisualTracker.GetComponent<PersonMarker>(), legacy_TrackerCSRT.create());
        newPerson.UpdateEmotion("Anger");
        newPerson.UpdateEmotion("Affection");
        */
    }


    private async void MapPredictions(string predictions)
    {

        Debugger.AddText("MapPredctions Called");


        var results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());


        int y1 = results[0].list[0].faceRect.y1;
        int y2 = results[0].list[0].faceRect.y2;
        int bodyY = results[0].list[0].bodyCenter.y;
        Debugger.AddText("Center Body: " + bodyY);
        Debugger.AddText("Center Face: " + (y1 + ((y2 - y1) * 0.5f)).ToString("F9"));
        MRWorld.pixelPointRatio.distPixel = bodyY - (y1 + ((y2 - y1) * 0.5f));
        Debugger.AddText("Dist Pixel: " + MRWorld.pixelPointRatio.distPixel.ToString("F9"));

        Vector3 facePos = Vector3.zero;
        Vector3 bodyPos = Vector3.zero;



        Debugger.SetFieldView();

        foreach (Detection detection in results[0].list)
        {
            FaceRect faceRect = detection.faceRect;

            Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
            bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, Debugger.GetCubeForTest());

            unprojectionOffset = MRWorld.GetUnprojectionOffset(faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f));
            facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, results[0].list[0].faceRect), unprojectionOffset, Debugger.GetCubeForTest(), 31, true, detectionName);


            Debugger.AddText("Ok");

            Person newPerson;
            Debugger.AddText("Im here yup");

           





            // ------------------------------------ DANGER ZONE --------------------------------------------- //





            Debugger.AddText("Pixels " + (detection.bodyCenter.y - (faceRect.y1 + ((faceRect.y2 - faceRect.y1) * 0.5f))).ToString("F9"));

            Debugger.AddText("Center Body: " + bodyPos.y);
            Debugger.AddText("Center Face: " + facePos.y.ToString("F9"));
            MRWorld.pixelPointRatio.distPoint = facePos.y - bodyPos.y;
            Debugger.AddText("Dist : " + MRWorld.pixelPointRatio.distPoint.ToString("F9"));
            Debugger.AddText("Distance : " + Vector3.Distance(bodyPos, facePos));

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




            Debugger.AddText("Four cubes created");
            TrackingManager.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newPerson, "Pacient");

            Debugger.AddText(newPerson.GetType().ToString());
            // two = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), bodyPos, Quaternion.identity);
            //two.GetComponent<Renderer>().material.color = Color.blue;




            // ---------------------------------------------------------------------------------------------- //
            Debugger.AddText("Second cube was created");
            Debugger.AddText(detection.emotions.categorical[0].ToString());

            if (newPerson is Pacient)
                (newPerson as Pacient).UpdateEmotion(detection.emotions.categorical[0].ToString());


            GameObject detectionTooltip = UnityEngine.Object.Instantiate(detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

            Debugger.AddText("tOOL");
            detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id);







            GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
            three.GetComponent<Renderer>().material.color = Color.red;

            Debugger.AddText("ITS OVER")


            ;

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

    public static void Strech(GameObject _sprite, Vector3 _initialPosition, Vector3 _finalPosition, bool _mirrorZ)
    {
        Vector3 centerPos = (_initialPosition + _finalPosition) / 2f;
        _sprite.transform.position = centerPos;
        Vector3 direction = _finalPosition - _initialPosition;
        direction = Vector3.Normalize(direction);
        _sprite.transform.right = direction;
        if (_mirrorZ) _sprite.transform.right *= -1f;
        Vector3 scale = new Vector3(1, 1, 1);
        scale.x = Vector3.Distance(_initialPosition, _finalPosition);
        _sprite.transform.localScale = scale;
        _sprite.transform.localRotation = new Quaternion(_sprite.transform.localRotation.x, _sprite.transform.localRotation.y, 0, _sprite.transform.localRotation.w);
    }

    private void LoadSavedData()
    {
        if (pacients == null)
            pacients = new BinaryTree();

    }

    public async void DetectPacients()
    {

        Debugger.AddText("So it starts");

        Debugger.AddText("Height: " + Camera.main.pixelHeight);
        Debugger.AddText("Width: " + Camera.main.pixelWidth);
        Debugger.AddText("Camera Rect: " + Camera.main.pixelRect.ToString());
#if ENABLE_WINMD_SUPPORT
        var lastFrame = frameHandler.LastFrame;
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
                        Debugger.AddText("Till the frame is ok");
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
                        Debugger.AddText("And keeps ok XD");

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
            bool wasUpdated = TrackingManager.UpdateTrackers(frameHandler.LastFrame.frameMat);
            if (wasUpdated) {
                timeToStop++;
                if (timeToStop >= 20)
                    justStop = true;
            }
#endif

        }

    }



}
