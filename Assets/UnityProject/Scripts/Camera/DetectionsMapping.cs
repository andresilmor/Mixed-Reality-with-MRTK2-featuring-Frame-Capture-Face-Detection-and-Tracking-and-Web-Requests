using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionsMapping : MonoBehaviour
{
    [SerializeField] private Camera terminatorCamera;
    [SerializeField] private GameObject testObject;

    public DetectionsMapping(GameObject testObject)
    {
        this.testObject = testObject;
        terminatorCamera = GameObject.FindGameObjectWithTag("Terminator").GetComponent<Camera>();
    }

    public void MapDetection(Vector2 pixel, Vector3 camPos, Quaternion camRot, TMPro.TextMeshPro debugText = null)
    {
        debugText.text = debugText.text + "\nMapDetection";
        terminatorCamera.transform.position = camPos;
        terminatorCamera.transform.rotation = camRot;
        debugText.text = debugText.text + "\nTransform done";
        Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(0, 846, terminatorCamera.nearClipPlane)), Quaternion.identity);
        Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(1504, 846, terminatorCamera.nearClipPlane)), Quaternion.identity);
        Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(1504, 0, terminatorCamera.nearClipPlane)), Quaternion.identity);
        Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(0, 0, terminatorCamera.nearClipPlane)), Quaternion.identity);
        debugText.text = debugText.text + "\nCorners";
        Vector3 origin = terminatorCamera.ScreenToWorldPoint(new Vector3(pixel.x, pixel.y, terminatorCamera.nearClipPlane));
        debugText.text = debugText.text + "\nVec3";
        Instantiate(testObject, origin, Quaternion.identity);
        debugText.text = debugText.text + "\nCenter";
        RaycastHit hit;
        if (Physics.Raycast(origin, terminatorCamera.transform.forward, out hit, Mathf.Infinity, 1 << 31)) 
        {
            Instantiate(testObject, hit.point, Quaternion.identity);
            debugText.text = debugText.text + "\nDone I hope";
        }
    }


}
