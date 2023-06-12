using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using Microsoft.MixedReality.Toolkit.Utilities;
using OpenCVForUnity.UtilsModule;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;

#if ENABLE_WINMD_SUPPORT
using Windows.Media.Capture;
using Windows.Graphics.Imaging;
using Windows.Media.Devices.Core;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Media;
using Windows.Media.Devices;
using Windows.Devices.Enumeration;
using Windows.System;
using Windows.Perception.Spatial;
#endif

using Debug = MRDebug;

public class FrameArrivedEventArgs {
    public CameraFrame Frame;

    public FrameArrivedEventArgs(CameraFrame frame) {
        Frame = frame;
    }
}

public static class MediaCaptureManager
{
	public struct Frame
	{
#if ENABLE_WINMD_SUPPORT
		public MediaFrameReference mediaFrameReference;
#endif
		/// <summary>
		/// Frame image Data in format OpenCV
		/// </summary>
		//public Mat frameMat;
		public CameraExtrinsic extrinsic;
		public CameraIntrinsic intrinsic;
	}

    public static event EventHandler<FrameArrivedEventArgs> FrameArrived;

    public static int FrameCount;
    public static int FrameHeight { get; private set; }
    public static int FrameWidth { get; private set; }

	public static bool IsCapturing = false;

    public static byte PassiveServicesUsing = 0;

#if ENABLE_WINMD_SUPPORT
	private static MediaCapture _mediaCapture;
	private static MediaFrameReader _mediaFrameReader;
	private static MediaFrameSource _mediaFrameSource;

    private static CameraFrame _cameraFrame = null;



    // ---------------------------------------------------------------------------------------------------------------------------------------------------------

    /*
	private static  Frame _lastFrame;

	public static Frame LastFrame
	{
		get
		{
			lock (this)
			{
				_lastFrame.extrinsic = new CameraExtrinsic(_lastFrame.mediaFrameReference.CoordinateSystem, MRWorld.WorldOrigin);
				_lastFrame.intrinsic = new CameraIntrinsic(_lastFrame.mediaFrameReference.VideoMediaFrame);
				return _lastFrame;
			}
		}
		private set
		{
			lock (this)
			{
				//_lastFrame.frameMat = GenerateCVMat(value.mediaFrameReference);
				_lastFrame = value;
			}
		}
	}
    */
	// ---------------------------------------------------------------------------------------------------------------------------------------------------------

	public static async Task InitializeMediaFrameReaderAsync()
    {
        // Check state of media capture object 
        if (_mediaCapture == null || _mediaCapture.CameraStreamState == CameraStreamState.Shutdown || _mediaCapture.CameraStreamState == CameraStreamState.NotStreaming)
        {
            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
            }

            // Find right camera settings and prefer back camera
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
            var allCameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            var selectedCamera = allCameras.FirstOrDefault(c => c.EnclosureLocation?.Panel == Panel.Back) ?? allCameras.FirstOrDefault();
            //Debug.Log($"InitializeMediaFrameReaderAsync: selectedCamera: {selectedCamera}");

            if (selectedCamera != null)
            {
                settings.VideoDeviceId = selectedCamera.Id;
                //Debug.Log($"InitializeMediaFrameReaderAsync: settings.VideoDeviceId: {settings.VideoDeviceId}");

            }

            // Init capturer and Frame reader
            _mediaCapture = new MediaCapture();
            Debug.Log("InitializeMediaFrameReaderAsync: Successfully created media capture object.");

            await _mediaCapture.InitializeAsync(settings);
            Debug.Log("InitializeMediaFrameReaderAsync: Successfully initialized media capture object.");

            var frameSourcePair = _mediaCapture.FrameSources.Where(source => source.Value.Info.SourceKind == MediaFrameSourceKind.Color).First();
            //Debug.Log($"InitializeMediaFrameReaderAsync: frameSourcePair: {frameSourcePair}.");

            // Convert the pixel formats
            var subtype = MediaEncodingSubtypes.Bgra8;
            //var subtype = MediaEncodingSubtypes.Rgb32;

            // The overloads of CreateFrameReaderAsync with the format arguments will actually make a copy in FrameArrived

            FrameWidth = 1504;
            FrameHeight = 846;

            BitmapSize outputSize = new BitmapSize { Width = (uint)FrameWidth, Height = (uint)FrameHeight};

            _mediaFrameReader = await _mediaCapture.CreateFrameReaderAsync(frameSourcePair.Value, subtype, outputSize);
            Debug.Log("InitializeMediaFrameReaderAsync: Successfully created media frame reader.");
            _mediaFrameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Realtime;

            _mediaFrameReader.FrameArrived += onFrameArrived;

            await _mediaFrameReader.StartAsync();
            Debug.Log("InitializeMediaFrameReaderAsync: Successfully started media frame reader.");

            IsCapturing = true;
        }
    }

     /// <summary>
    /// Retrieve the latest video frame from the media frame reader
    /// </summary>
    /// <returns>VideoFrame object with current frame's software bitmap</returns>
    public static async Task<CameraFrame> GetLatestFrame(Action<object, FrameArrivedEventArgs> action = null)
    {
        Debug.Log("Getting frame");
        SoftwareBitmap bitmap;
        try{
            // The overloads of CreateFrameReaderAsync with the format arguments will actually return a copy so we don't have to copy again
            Debug.Log("A");
            var mediaFrameReference = _mediaFrameReader.TryAcquireLatestFrame();
            if (mediaFrameReference != null){ Debug.Log("Frame not null"); }
            Debug.Log("B");
            VideoFrame videoFrame = mediaFrameReference?.VideoMediaFrame?.GetVideoFrame();
            Debug.Log("C");
            var spatialCoordinateSystem = mediaFrameReference?.CoordinateSystem;
            Debug.Log("D");
            var cameraIntrinsics = mediaFrameReference?.VideoMediaFrame?.CameraIntrinsics;
            Debug.Log("E");

             // Sometimes on HL RS4 the D3D surface returned is null, so simply skip those frames
            if (videoFrame == null || (videoFrame.Direct3DSurface == null && videoFrame.SoftwareBitmap == null))
            {
                Debug.Log("Frame thrown out");
                //UnityEngine.Debug.Log("Frame thrown out");
                return _cameraFrame;
            }


            if (videoFrame.Direct3DSurface != null && videoFrame.SoftwareBitmap == null)
            {
                Debug.Log("F.1");
                bitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(videoFrame.Direct3DSurface);
            }
            else
            {
                Debug.Log("F.2");
                bitmap = videoFrame.SoftwareBitmap;
            }

            /*
            Frame returnFrame = new Frame{
                spatialCoordinateSystem = spatialCoordinateSystem,
                cameraIntrinsics = cameraIntrinsics,
                bitmap = bitmap,
                };
            */

            Debug.Log("G");
            CameraExtrinsic extrinsic = new CameraExtrinsic(spatialCoordinateSystem, MRWorld.WorldOrigin);
            Debug.Log("H");
            CameraIntrinsic intrinsic = new CameraIntrinsic(mediaFrameReference?.VideoMediaFrame);
            Debug.Log("I");
            CameraFrame cameraFrame = new CameraFrame(
                null, 
				intrinsic, 
                extrinsic,
                FrameWidth, 
                FrameHeight, 
                (uint)0, 
                mediaFrameReference,
                bitmap
            );
            Debug.Log("Inside frame reader");
            FrameArrivedEventArgs eventArgs = new FrameArrivedEventArgs(cameraFrame);
            action?.Invoke(null, eventArgs);

            return cameraFrame;

        } catch (Exception ex){
            Debug.Log("Caught exception grabbing frame");
            Debug.Log(ex.Message);
            return _cameraFrame;

        }

    }

    /// <summary>
    /// Asynchronously stop media capture and dispose of resources
    /// </summary>
    /// <returns></returns>
    public static async Task StopMediaFrameReaderAsync()
    {
#if ENABLE_WINMD_SUPPORT
        if (_mediaCapture != null && _mediaCapture.CameraStreamState != CameraStreamState.Shutdown)
        {
            await _mediaFrameReader.StopAsync();
            _mediaFrameReader.Dispose();
            _mediaCapture.Dispose();
            _mediaCapture = null;
            Debug.Log("StopMediaFrameReaderAsync: Successfully stopped.");
        }
        IsCapturing = false;
        Debug.Log("StopMediaFrameReaderAsync: Not capturing");
#endif
    }


	// ---------------------------------------------------------------------------------------------------------------------------------------------------------

#endif

    /// <summary>
    /// In order to do pixel manipulation on SoftwareBitmap images, the native memory buffer is accessed using <see cref="IMemoryBufferByteAccess"/> COM interface.
    /// The project needs to be configured to allow compilation of unsafe code. <see href="https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader"/>.
    /// </summary>
    [ComImport]
        [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private unsafe interface IMemoryBufferByteAccess
        {
            /// <summary>
            /// Gets a buffer as an array of bytes. <see href="https://docs.microsoft.com/de-de/windows/win32/winrt/imemorybufferbyteaccess-getbuffer"/>.
            /// </summary>
            /// <param Name="value">A pointer to a byte array containing the buffer Data</param>
            /// <param Name="capacity">The number of bytes in the returned array</param>
			
            void GetBuffer(out byte* value, out uint capacity);
		
        }

    private static int PadTo64(int frameWidth) {
        if (frameWidth % 64 == 0) return frameWidth;
        int paddedFrameWidth = ((frameWidth >> 6) + 1) << 6;
        return paddedFrameWidth;
    }

#if ENABLE_WINMD_SUPPORT
    /// <summary>
    /// Extracts the image according to the <see cref="ColorFormat"/> and invokes the <see cref="FrameArrived"/> event containing a <see cref="MediaCaptureManager"/>.
    /// </summary>
    public static unsafe Mat GenerateCVMat(MediaFrameReference frameReference,  bool toDispose = false, int frameWidth = 1504, int frameHeight = 846) {
        //Debug.Log("GenerateCVMat Called");
        
        SoftwareBitmap softwareBitmap = frameReference.VideoMediaFrame?.GetVideoFrame().SoftwareBitmap;
        

        //Mat _bitmap = new Mat((int)(frameHeight * 3 * 0.5f) , frameWidth, CvType.CV_8UC1);

        Mat bitmap = new Mat((int)frameHeight , (int)frameWidth, CvType.CV_8UC4);
        if (softwareBitmap != null)
        {
            
            using (var input = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (var inputReference = input.CreateReference())
            {
                
                byte* inputBytes;
                uint inputCapacity;
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out inputBytes, out inputCapacity);
                
                MatUtils.copyToMat((IntPtr)inputBytes, bitmap); // Copies Pixel Data Array to OpenCV Mat data.
                //int thisFrameCount = Interlocked.Increment(ref FrameCount);
                
                //MediaCaptureManager MediaCaptureManager = new MediaCaptureManager(bitmap, intrinsic, extrinsic, FrameWidth, FrameHeight, (uint)thisFrameCount, _format);
                //FrameArrivedEventArgs eventArgs = new FrameArrivedEventArgs(MediaCaptureManager);
                //FrameArrived?.Invoke(this, eventArgs);
            }
            
			
			if (toDispose)
				softwareBitmap.Dispose();

        }
        
        return bitmap;
    }

	private static bool testing = true;
	private static int testingCount = 0;
    
    private static void onFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
	{
    
		return;


		if (FrameArrived.GetInvocationList().Length <= 0)
			return;
		
        return;


		MediaFrameReference frame = sender.TryAcquireLatestFrame();

        if (frame != null){
				if (testing)
					Debug.Log("Frame not null");
				// Works
				/*LastFrame = new Frame
				{mediaFrameReference = frame, extrinsic = null, intrinsic = null};
				_lastFrameCapturedTimestamp = DateTime.Now;
			    */
				//Debug.Log("Frame Not Null");

				// ---------------------	DANGER ZONE		-------------------------
				
				try {
				

				CameraExtrinsic extrinsic = new CameraExtrinsic(frame.CoordinateSystem, MRWorld.WorldOrigin);
                CameraIntrinsic intrinsic = new CameraIntrinsic(frame.VideoMediaFrame);

				//int thisFrameCount = Interlocked.Increment(ref FrameCount);

				CameraFrame cameraFrame = new CameraFrame(
                    null, 
					intrinsic, 
                    extrinsic, 
                    FrameWidth, 
                    FrameHeight, 
                    0, 
                    frame,
                    null
                );

                FrameArrivedEventArgs eventArgs = new FrameArrivedEventArgs(cameraFrame);
				if (testing) {
					Debug.Log("Invoquing");
					}
                FrameArrived?.Invoke(null, eventArgs);
				if (testing) {
					Debug.Log("Invoked");
					testing = false;
					}
				} catch (Exception ex) {
					Debug.Log("Error: {ex.Message}");
				}

				

				// ---------------------	DANGER ZONE		-------------------------



        } else {
			//Debug.Log("Frame ISSSS Null");
		}
	}
 
	
#endif

}
