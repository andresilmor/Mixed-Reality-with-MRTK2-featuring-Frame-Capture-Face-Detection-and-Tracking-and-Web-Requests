using UnityEngine;
using System.Collections;

public static class PixelConverter 
{

    public static Camera camera;

    


    //Taking Your Camera Location And Is Off Setting For Position And For Amount Of World Units In Camera
    public static Vector3 ToWorldUnits(Vector2 pixelCoord, Vector3 cameraPos)
    {
        if (camera == null)
            camera = Camera.main;
        Vector2 WorldUnitsInCamera;
        Vector2 WorldToPixelAmount;
        WorldUnitsInCamera.y = camera.orthographicSize * 2;
        WorldUnitsInCamera.x = WorldUnitsInCamera.y * Screen.width / Screen.height;

        WorldToPixelAmount.x = Screen.width / WorldUnitsInCamera.x;
        WorldToPixelAmount.y = Screen.height / WorldUnitsInCamera.y;

        Vector3 returnVec3 = new Vector3(0,0, cameraPos.z);

        returnVec3.x = ((pixelCoord.x / WorldToPixelAmount.x) - (WorldUnitsInCamera.x / 2)) + cameraPos.x;
        returnVec3.y = ((pixelCoord.y / WorldToPixelAmount.y) - (WorldUnitsInCamera.y / 2)) + cameraPos.y;

        return returnVec3;
    }
}
