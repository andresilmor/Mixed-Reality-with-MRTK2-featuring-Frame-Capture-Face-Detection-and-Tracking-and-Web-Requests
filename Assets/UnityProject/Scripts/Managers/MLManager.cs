using Newtonsoft.Json;
using OpenCVForUnity.CoreModule;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class MLManager
{/*
    private static async void MapPredictions(string predictions) {
        Debugger.AddText("HERE");
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

                        TrackerManager.CreateTracker(detection.faceRect, tempFrameMat, personMarker, facePos, out newTracker, "PacientTracker");
                        (newTracker as PacientTracker).gameObject.name = detection.id.ToString();

                        (newTracker as PacientTracker).id = detection.id;

                        if (newTracker is PacientTracker)
                            (newTracker as PacientTracker).UpdateActiveEmotion(detection.emotions.categorical[0].ToString());


                        GameObject detectionTooltip = UnityEngine.Object.Instantiate(_detectionName, facePos + new Vector3(0, 0.10f, 0), Quaternion.identity);

                        detectionTooltip.GetComponent<TextMeshPro>().SetText(detection.id.ToString());

                        pacientsMemory.Add(detection.id, newTracker);

                        GameObject three = UnityEngine.Object.Instantiate(Debugger.GetCubeForTest(), facePos, Quaternion.identity);
                        three.GetComponent<Renderer>().material.color = Color.red;

                       

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
    }*/

}
