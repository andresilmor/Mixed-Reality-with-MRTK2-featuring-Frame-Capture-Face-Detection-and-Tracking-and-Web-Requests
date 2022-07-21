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

    public void MapDetection(Vector2 pixel, Vector3 camPos, Quaternion camRot, TMPro.TextMeshPro debugText = null, LineDrawer debugLine = null)
    {
        debugText.text = debugText.text + "\nMapDetection";
        terminatorCamera.transform.position = camPos;
        terminatorCamera.transform.rotation = camRot;
        debugText.text = debugText.text + "\nTransform done";
        GameObject one = Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(0, Camera.main.pixelHeight, terminatorCamera.nearClipPlane)), Quaternion.identity);
        GameObject two = Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, terminatorCamera.nearClipPlane)), Quaternion.identity);
        GameObject three = Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, 0, terminatorCamera.nearClipPlane)), Quaternion.identity);
        GameObject four = Instantiate(testObject, terminatorCamera.ScreenToWorldPoint(new Vector3(0, 0, terminatorCamera.nearClipPlane)), Quaternion.identity);
        debugText.text = debugText.text + "\nCorners";
        Vector3 origin = terminatorCamera.ScreenToWorldPoint(new Vector3(pixel.x, pixel.y, terminatorCamera.nearClipPlane));
        debugText.text = debugText.text + "\nVec3";
        GameObject center = Instantiate(testObject, origin, Quaternion.identity);
        debugText.text = debugText.text + "\nCenter";
        RaycastHit hit;
        GameObject hitPoint = null;
        if (Physics.Raycast(origin, terminatorCamera.transform.forward, out hit, Mathf.Infinity, 1 << 31)) 
        {
            hitPoint = Instantiate(testObject, hit.point, Quaternion.identity);
            debugText.text = debugText.text + "\nDone I hope";
        }

        if (debugLine != null)
        {
            debugLine.Draw(one.transform.position, center.transform.position);
            debugLine.Draw(two.transform.position, center.transform.position);
            debugLine.Draw(three.transform.position, center.transform.position);
            debugLine.Draw(four.transform.position, center.transform.position);
            if (hitPoint != null)
                debugLine.Draw(center.transform.position, hitPoint.transform.position);
        }

    }


}
