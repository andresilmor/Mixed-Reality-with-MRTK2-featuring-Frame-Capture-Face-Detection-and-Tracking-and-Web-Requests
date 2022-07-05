using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.MixedReality.Toolkit.Utilities;

using WebSocketSharp;

#if ENABLE_WINMD_SUPPORT
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;
#endif

using TMPro;


public class APIController : MonoBehaviour
{

    public string IP = "127.0.0.1";


    WebSocket ws;

    private FrameCapture frameCapture;

    private FrameGrabber frameGrabber;

    public TextMeshPro debugText;

    async void Start()
    {
        frameCapture = FindObjectOfType<FrameCapture>();


#if ENABLE_WINMD_SUPPORT
        frameGrabber = await FrameGrabber.CreateAsync(1504, 846);
#endif
    }

    private void ConnectWebsocker()
    {
        debugText.text += "Connect Websocket \n";
        Debug.Log("Connect Websocket ");
        ws = new WebSocket("ws://193.137.107.7:8000/test");
        debugText.text += "Var \n";
        Debug.Log("Connect Websocket ");
        ws.Connect();
        debugText.text += "Connected \n";
        Debug.Log("Connected  ");
        
        ws.Send("Connected");
        ws.OnMessage += (sender, e) =>
        {
            Debug.Log("Message Received from " + ((WebSocket)sender).Url + ", Data : " + e.Data);
        };
        ws.Send("Connected 2");

    }


    public async void ObjectPrediction()
    {
        debugText.text += "Object Prediction Call \n";
        Debug.Log("Object Prediction Call ");
        if (ws == null || !ws.IsAlive) { 
            debugText.text += "Func Call \n";
            Debug.Log("Func Call  ");
            ConnectWebsocker();
        }
        //frameCapture.CaptureFrame(ws);
        debugText.text += "Send \n";
        Debug.Log("Send");
        ws.Send("CHECKINF");
        debugText.text += "Sended \n";
        Debug.Log("Sended");

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
                        ws.Send(Convert.ToBase64String(byteArray));
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


    private void OnDestroy()
    {
        ws.Close();
    }
}
