using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using Debug = XRDebug;

public static class LineDrawer
{
    public static GameObject line;
 
    public static void Draw(Vector3 startPoint, Vector3 endPoint, UnityEngine.Color color)
    {
        GameObject newLine = UnityEngine.Object.Instantiate(line, Vector3.zero, Quaternion.identity);
        LineRenderer lineRenderer = newLine.GetComponent<LineRenderer>();   
        lineRenderer.SetPosition(0, startPoint);
        lineRenderer.SetPosition(1, endPoint);
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

    }

    public static void SetDrawLine(GameObject lineGameObject)
    {
        line = lineGameObject;  
    }
}
