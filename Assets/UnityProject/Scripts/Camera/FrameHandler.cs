using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using Microsoft.MixedReality.Toolkit.Utilities;
using OpenCVForUnity.UtilsModule;
using System.Runtime.InteropServices;

#if ENABLE_WINMD_SUPPORT
using Windows.Media.Capture;
using Windows.Graphics.Imaging;
using Windows.Media.Devices.Core;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
#endif

public class FrameHandler
{
	public struct Frame
	{
#if ENABLE_WINMD_SUPPORT
		public MediaFrameReference mediaFrameReference;
#endif
		/// <summary>
		/// Frame image data in format OpenCV
		/// </summary>
		public Mat frameMat;
		public CameraExtrinsic extrinsic;
		public CameraIntrinsic intrinsic;
	}



#if ENABLE_WINMD_SUPPORT
	MediaCapture mediaCapture;
	MediaFrameSource mediaFrameSource;
	MediaFrameReader mediaFrameReader;

	private Frame _lastFrame;

	public Frame LastFrame
	{
		get
		{
			lock (this)
			{
				_lastFrame.extrinsic = new CameraExtrinsic(_lastFrame.mediaFrameReference.CoordinateSystem, MRWorld.worldOrigin);
				_lastFrame.intrinsic = new CameraIntrinsic(_lastFrame.mediaFrameReference.VideoMediaFrame);
				_lastFrame.frameMat = GenerateCVMat(_lastFrame.mediaFrameReference);
				return _lastFrame;
			}
		}
		private set
		{
			lock (this)
			{
				_lastFrame = value;
			}
		}
	}




	private DateTime _lastFrameCapturedTimestamp = DateTime.MaxValue;

	public float ElapsedTimeSinceLastFrameCaptured
	{
		get
		{
			return (float)(DateTime.Now - DateTime.MinValue).TotalMilliseconds;
		}
	}

	public bool IsValid
	{
		get
		{
			return mediaFrameReader != null;
		}
	}

	private FrameHandler(MediaCapture mediaCapture = null, MediaFrameSource mediaFrameSource = null, MediaFrameReader mediaFrameReader = null)
	{
		this.mediaCapture = mediaCapture;
		this.mediaFrameSource = mediaFrameSource;
		this.mediaFrameReader = mediaFrameReader;

		if (this.mediaFrameReader != null)
		{
			Debug.Log("+= setting");
			this.mediaFrameReader.FrameArrived += onFrameArrived;
		}
	}

	public static async Task<FrameHandler> CreateAsync(int width = 1504, int height = 846) //Default values anyway, if not defined the "outputSize" in "mediaCapture.CreateFrameReaderAsync"
	{
		MediaCapture mediaCapture = null;
		MediaFrameReader mediaFrameReader = null;

		MediaFrameSourceGroup selectedGroup = null;
		MediaFrameSourceInfo selectedSourceInfo = null;

		// Pick first color source             
		var groups = await MediaFrameSourceGroup.FindAllAsync();
		foreach (MediaFrameSourceGroup sourceGroup in groups)
		{
			foreach (MediaFrameSourceInfo sourceInfo in sourceGroup.SourceInfos)
			{
				Debug.Log($"[### DEBUG ###] id = {sourceInfo.Id}");
				if (sourceInfo.SourceKind == MediaFrameSourceKind.Color)
				{
					selectedSourceInfo = sourceInfo;
					break;
				}
			}

			if (selectedSourceInfo != null)
			{
				selectedGroup = sourceGroup;
				break;
			}
		}

		// No valid camera was found. This will happen on the emulator.
		if (selectedGroup == null || selectedSourceInfo == null)
		{
			Debug.Log("Failed to find Group and SourceInfo");
			return new FrameHandler();
		}

		// Create settings 
		var settings = new MediaCaptureInitializationSettings
		{
			SourceGroup = selectedGroup,

			// This media capture can share streaming with other apps.
			SharingMode = MediaCaptureSharingMode.SharedReadOnly,

			// Only stream video and don't initialize audio capture devices.
			StreamingCaptureMode = StreamingCaptureMode.Video,

			// Set to CPU to ensure frames always contain CPU SoftwareBitmap images
			// instead of preferring GPU D3DSurface images.
			MemoryPreference = MediaCaptureMemoryPreference.Cpu,
		};

		// Create and initilize capture device 
		mediaCapture = new MediaCapture();

		try
		{
			await mediaCapture.InitializeAsync(settings);
		}
		catch (Exception e)
		{
			Debug.Log($"Failed to initilise mediacaptrue {e.ToString()}");
			return new FrameHandler();
		}

		
		Debug.Log($"[### DEBUG ###] mediaCapture.FrameSources Count = {mediaCapture.FrameSources.Count}");
		string ID = "";
		foreach (KeyValuePair<string, MediaFrameSource> kvp in mediaCapture.FrameSources)
		{
				Debug.Log($"[### DEBUG ###] Key = {kvp.Key}");
				ID = kvp.Key;
				break;
		}
		
		MediaFrameSource selectedSource = mediaCapture.FrameSources[ID];
		Debug.Log("OK!");
		var subtype = MediaEncodingSubtypes.Bgra8;
		BitmapSize outputSize = new BitmapSize { Width = (uint)width, Height = (uint)height };

		// create new frame reader 
		mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(selectedSource, subtype, outputSize);

		MediaFrameReaderStartStatus status = await mediaFrameReader.StartAsync();


		if (status == MediaFrameReaderStartStatus.Success)
		{
			Debug.Log("MediaFrameReaderStartStatus == Success");
			return new FrameHandler(mediaCapture, selectedSource, mediaFrameReader);
		}
		else
		{
			Debug.Log($"MediaFrameReaderStartStatus != Success; {status}");
			return new FrameHandler();
		}
	}


	public async Task StopFrameHandlerAsync()
	{
        if (mediaCapture != null && mediaCapture.CameraStreamState != Windows.Media.Devices.CameraStreamState.Shutdown)
        {
			Debug.Log("Close Camera!");
			await mediaFrameReader.StopAsync();
            mediaFrameReader.Dispose();
            mediaCapture.Dispose();
            mediaCapture = null;
        }
	}
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
            /// <param name="value">A pointer to a byte array containing the buffer data</param>
            /// <param name="capacity">The number of bytes in the returned array</param>
			
            void GetBuffer(out byte* value, out uint capacity);
		
        }

#if ENABLE_WINMD_SUPPORT
    /// <summary>
    /// Extracts the image according to the <see cref="ColorFormat"/> and invokes the <see cref="FrameArrived"/> event containing a <see cref="CameraFrame"/>.
    /// </summary>
    public unsafe Mat GenerateCVMat(MediaFrameReference frameReference, bool toDispose = false, int frameWidth = 1504, int frameHeight = 846) {
        
        
        SoftwareBitmap softwareBitmap = frameReference.VideoMediaFrame?.GetVideoFrame().SoftwareBitmap;
        

        Mat _bitmap = new Mat(frameHeight, frameWidth, CvType.CV_8UC1);
        if (softwareBitmap != null)
        {
            
            using (var input = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (var inputReference = input.CreateReference())
            {
                
                byte* inputBytes;
                uint inputCapacity;
                ((IMemoryBufferByteAccess)inputReference).GetBuffer(out inputBytes, out inputCapacity);
                
                MatUtils.copyToMat((IntPtr)inputBytes, _bitmap); // Copies Pixel Data Array to OpenCV Mat data.
                //int thisFrameCount = Interlocked.Increment(ref FrameCount);
                
                //CameraFrame cameraFrame = new CameraFrame(_bitmap, intrinsic, extrinsic, FrameWidth, FrameHeight, (uint)thisFrameCount, _format);
                //FrameArrivedEventArgs eventArgs = new FrameArrivedEventArgs(cameraFrame);
                //FrameArrived?.Invoke(this, eventArgs);
            }
            
			
			if (toDispose)
				softwareBitmap.Dispose();

        }
        
        return _bitmap;
    }


    void onFrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
	{
		MediaFrameReference frame = sender.TryAcquireLatestFrame();
        if (frame != null){
				LastFrame = new Frame
				{mediaFrameReference = frame, extrinsic = null, intrinsic = null, frameMat = null};
				_lastFrameCapturedTimestamp = DateTime.Now;

            }
	}
	
#endif

}
