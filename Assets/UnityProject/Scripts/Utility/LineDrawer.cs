using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public GameObject line;
 
    public void Draw(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        GameObject newLine = Instantiate(line, Vector3.zero, Quaternion.identity);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();   
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

    }
}
