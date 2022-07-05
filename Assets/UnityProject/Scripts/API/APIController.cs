using System;
using UnityEngine;

using TMPro;
using BestHTTP;
using BestHTTP.WebSocket;

//These are needed, trust me ^-^ 
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
#endif



public class APIController : MonoBehaviour
{

    public string IP = "127.0.0.1";


    private FrameCapture frameCapture;

    private FrameGrabber frameGrabber;

    public TextMeshPro debugText;

    private WebSocket ws;

    string address = "ws://193.137.107.8:8000/ws";

    async void Start()
    {
        frameCapture = FindObjectOfType<FrameCapture>();

        
        /*ws = new WebSocket(new Uri(address));
        debugText.text = debugText.text + address;


        ws.OnMessage += (WebSocket webSocket, string message) =>
        {
            debugText.text = debugText.text + "\nMessage: " + message;
            Debug.Log("Text Message received from server: " + message);
        };

        ws.OnClosed += (WebSocket webSocket, UInt16 code, string message) =>
        {
            Debug.Log("WebSocket is now Closed!");
        };

        ws.OnError += (WebSocket ws, string error) =>
        {
            debugText.text = debugText.text + "\nErro: " + error;
            Debug.LogError("Error: " + error);
        };

        ws.OnOpen += (WebSocket ws) =>
        {
            debugText.text = debugText.text + "\nOpen (Inside)";
            ws.Send("CHECKINF 60");
        };

        ws.Open();
        ws.Send("CHECKINF 64");
        


        debugText.text = debugText.text + "\nWS Created/Opened";
        */

#if ENABLE_WINMD_SUPPORT
        frameGrabber = await FrameGrabber.CreateAsync(1504, 846);
#endif
    }



    public async void ObjectPrediction()
    {
        var request = new HTTPRequest(new Uri("http://193.137.107.8:8000/"), OnRequestFinished);
        request.Send();
        void OnRequestFinished(HTTPRequest request, HTTPResponse response)
        {
            Debug.Log("Request Finished! Text received: " + response.DataAsText);
            debugText.text = debugText.text + "\n" + response.DataAsText;
        }

        /*

        if (ws.IsOpen) { 
            debugText.text = debugText.text + "\nSend Called";
            ws.Send("CHECKINF 83");
            debugText.text = debugText.text + "\nSend Done";
        } else
        {
            ws.Open();
            debugText.text = debugText.text + "\nWS Second Try";
        }
        */

#if ENABLE_WINMD_SUPPORT
        var lastFrame = frameGrabber.LastFrame;
        ws.Send("Im Inside");
        if (lastFrame.mediaFrameReference != null)
        {
            try
            {
                using (var videoFrame = lastFrame.mediaFrameReference.VideoMediaFrame.GetVideoFrame())
                {
                    if (videoFrame != null && videoFrame.SoftwareBitmap != null)
                    {
                        byte[] byteArray = await toByteArray(videoFrame.SoftwareBitmap);
                        Debug.Log($"[### DEBUG ###] byteArray Size = {byteArray.Length}");
                       
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
        { Debug.Log("lastFrame.mediaFrameReference = null"); }
#endif

    }

#if ENABLE_WINMD_SUPPORT
    public async Task<byte[]> toByteArray(SoftwareBitmap sftBitmap_c)
    {
        SoftwareBitmap sftBitmap = SoftwareBitmap.Convert(sftBitmap_c, BitmapPixelFormat.Bgra8);
        InMemoryRandomAccessStream mss = new InMemoryRandomAccessStream();
        Windows.Graphics.Imaging.BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, mss);

        encoder.SetSoftwareBitmap(sftBitmap);
        await encoder.FlushAsync();

        IBuffer bufferr = new Windows.Storage.Streams.Buffer((uint)mss.Size);
        await mss.ReadAsync(bufferr, (uint)mss.Size, InputStreamOptions.None);

        DataReader dataReader = DataReader.FromBuffer(bufferr);
        byte[] bytes = new byte[bufferr.Length];
        dataReader.ReadBytes(bytes);
        return bytes;
    }
#endif


    void OnDestroy()
    {
        if (this.ws != null)
        {
            this.ws.Close();
            this.ws = null;
        }
    }
}
