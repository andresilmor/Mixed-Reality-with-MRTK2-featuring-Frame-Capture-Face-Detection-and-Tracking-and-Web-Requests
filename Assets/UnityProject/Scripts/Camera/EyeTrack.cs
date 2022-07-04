using UnityEngine;
using System;

[RequireComponent(typeof(Camera))]
public class EyeTrack : MonoBehaviour
{

    [SerializeField] private float speed = 0.2f;
    [SerializeField] private RenderTexture renderTexture;

    private Camera cam;
    private Texture2D snapshot;
    private string base64 = "";

    private bool readPixes = true;


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if (Input.GetMouseButton(0)) {
            transform.eulerAngles += speed * new Vector3 (-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);

        }

        if (readPixes) {
            snapshot = new Texture2D(170, 170, TextureFormat.RGB24, false);
            cam.Render();
            RenderTexture.active = cam.targetTexture;
            snapshot.ReadPixels(new Rect(0, 0, 170, 170), 0, 0);
            base64 = Convert.ToBase64String(snapshot.EncodeToPNG());
            //System.IO.File.WriteAllBytes("D:\\PC\\Desktop\\testUNITY.png", snapshot.EncodeToPNG());
        }

    }


    public string GetSnapshot()
    {
        return base64;
        //System.IO.File.WriteAllBytes("D:\\PC\\Desktop\\testUNITY.png", bytes);

    }

}
