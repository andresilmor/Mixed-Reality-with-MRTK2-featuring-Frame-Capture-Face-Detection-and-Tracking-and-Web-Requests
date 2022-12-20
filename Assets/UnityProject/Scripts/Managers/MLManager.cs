using Newtonsoft.Json;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using BestHTTP.WebSocket;
using System.Threading.Tasks;

public static class MLManager
{
    private static Mat tempFrameMat;

    public static async Task<bool> ToggleLiveDetection() {
#if ENABLE_WINMD_SUPPORT
        AppCommandCenter.frameHandler = await FrameHandler.CreateAsync();
#endif

        APIManager.CreateWebSocketLiveDetection(APIManager.mlLiveDetection, DetectionType.Person, MLManager.MapDetections);

        return APIManager.wsLiveDetection != null;

    }

    public static async void AnalyseFrame() {
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
                      
                        UnityEngine.Object.Instantiate(Debugger.GetSphereForTest(), AppCommandCenter.cameraMain.transform.position, Quaternion.identity);
                        UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), lastFrame.extrinsic.Position, Quaternion.identity);
                       
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

                        wsTemp.Send(JsonUtility.ToJson(frame));
                                          

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

    public static async void MapDetections(string predictions, DetectionType detectionType) {
        Debugger.AddText("HERE");
        try {
            List<DetectionsList> results = JsonConvert.DeserializeObject<List<DetectionsList>>(
                JsonConvert.DeserializeObject(predictions).ToString());
        
            Vector3 worldPosition = Vector3.zero;

            switch (detectionType) {
                case DetectionType.Person:
                    foreach (Detection detection in results[0].list) {
                        GetWorldPosition(out worldPosition, detection);

                        try {

                            BinaryTree.Node node = AppCommandCenter.Instance.liveTrackers.Find(detection.id);
                            Debugger.AddText(AppCommandCenter.Instance.liveTrackers.GetTreeDepth().ToString());

                            if (node is null) {
                                Debugger.AddText("NEW ON TREE");
                                TrackerHandler newTracker = TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, worldPosition, TrackerType.PacientTracker);

                                //newTracker.gameObject.name = detection.id.ToString();
                                newTracker.SetIdentifier(detection.id);

                                /*if (newTracker is PacientTracker)
                                    (newTracker as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0].ToString());
                                */

                                GameObject detectionTooltip = UnityEngine.Object.Instantiate(AppCommandCenter.Instance._detectionName, worldPosition + new Vector3(0, 0.10f, 0), Quaternion.identity);

                                detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                                AppCommandCenter.Instance.liveTrackers.Add(detection.id, newTracker);


                            } else {
                                Debugger.AddText("ALREADY EXISTS ON TREE");
                                /*SEARCH BINARY TREE BY IDENTIFIER, ALSO LOCK WHILE CHANGING
                                if (node.data is PacientTracker) {
                                    (node.data as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0]);
                                    //(node.GraphQLData as Pacient).UpdateOneTracker(detection.faceRect, tempFrameMat);

                                }
                                */

                            }
                        } catch (Exception ex) {
                            Debugger.AddText(ex.Message);
                        }

                    }
                    break;
            
            }
            
        } catch (Exception error) {
            Debugger.AddText("Error: " + error.Message.ToString());

        }

    }

    private static void GetWorldPosition(out Vector3 worldPosition, Detection detection) {
        Vector3 worldPosRaycast = GetFaceWorldPositionByRaycast(detection);

        worldPosition = worldPosRaycast;
        return;
        Vector3 worldPosCalculation = GetWorldPositionCalculation(detection);

        if (Vector3.Distance(worldPosRaycast, MRWorld.tempExtrinsic.Position) < Vector3.Distance(worldPosCalculation, MRWorld.tempExtrinsic.Position)) {
            worldPosition = worldPosRaycast;
            return;

        }

        worldPosition = worldPosCalculation;

    }

    private static Vector3 GetWorldPositionCalculation(Detection detection) {
        throw new NotImplementedException();
    }

    private static Vector3 GetFaceWorldPositionByRaycast(Detection detection) {
        Vector2 unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.bodyCenter.y);
        Vector3 bodyPos = MRWorld.GetWorldPositionOfPixel(new Point(detection.bodyCenter.x, detection.bodyCenter.y), unprojectionOffset, (uint)(detection.faceRect.x2 - detection.faceRect.x1));

        unprojectionOffset = MRWorld.GetUnprojectionOffset(detection.faceRect.y1 + ((detection.faceRect.y2 - detection.faceRect.y1) * 0.5f));
        Vector3 facePos = MRWorld.GetWorldPositionOfPixel(MRWorld.GetBoundingBoxTarget(MRWorld.tempExtrinsic, detection.faceRect), unprojectionOffset, (uint)(detection.faceRect.x2 - detection.faceRect.x1), null, 31, true, AppCommandCenter.Instance._detectionName);

        if (Vector3.Distance(bodyPos, MRWorld.tempExtrinsic.Position) < Vector3.Distance(facePos, MRWorld.tempExtrinsic.Position)) {
            facePos.x = bodyPos.x;
            facePos.z = bodyPos.z;

        }


        GameObject FaceWorldPositionByRaycast = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
        FaceWorldPositionByRaycast.GetComponent<Renderer>().material.color = Color.red;

        return facePos;

    }
}
