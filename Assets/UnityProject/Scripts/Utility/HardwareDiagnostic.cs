using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Unity.Profiling;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Media.Capture;
using Windows.System;
#else
using UnityEngine.Profiling;
#endif

using Debug = MRDebug;

public class DiagnosticData {
    public int cpuFrameRate;
    public int gpuFrameRate;
    public float memoryPeak;
    public float memoryUsage;
    public float memoryLimit;

    public DiagnosticData(int cpuFrameRate, int gpuFrameRate, float memoryPeak, float memoryUsage, float memoryLimit) {
        this.cpuFrameRate = cpuFrameRate;
        this.gpuFrameRate = gpuFrameRate;
        this.memoryPeak = memoryPeak;
        this.memoryUsage = memoryUsage;
        this.memoryLimit = memoryLimit;

    }

}


public class HardwareDiagnostic : MonoBehaviour
{
    private static readonly int maxStringLength = 32;
    private static readonly int maxTargetFrameRate = 120;
    private static readonly int maxFrameTimings = 128;
    private static readonly int frameRange = 30;

    private int frameCount;
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    private float frameSampleRate = 0.1f;
    private FrameTiming[] frameTimings = new FrameTiming[maxFrameTimings];

    private static readonly ProfilerMarker LateUpdatePerfMarker = new ProfilerMarker("[MRTK] MixedRealityToolkitVisualProfiler.LateUpdate");
    private static readonly ProfilerMarker AverageFrameTimingPerfMarker = new ProfilerMarker("[MRTK] MixedRealityToolkitVisualProfiler.AverageFrameTiming");
    private static readonly ProfilerMarker WillDisplayedMemoryUsageDifferPerfMarker = new ProfilerMarker("[MRTK] MixedRealityToolkitVisualProfiler.WillDisplayedMemoryUsageDiffer");
    private static readonly ProfilerMarker MemoryItoAPerfMarker = new ProfilerMarker("[MRTK] MixedRealityToolkitVisualProfiler.MemoryItoA");

    private ulong memoryUsage;
    private ulong peakMemoryUsage;
    private ulong limitMemoryUsage;

    int cpuFrameRate = 0;
    int gpuFrameRate = 0;

    int cpuFrameRateMessage = 0;
    int gpuFrameRateMessage = 0;
    float memoryUsageMessage = 0;
    float memoryPeakMessage = 0;
    float memoryLimitMessage = 0;

    private int displayedDecimalDigits = 1;

    private static ulong AppMemoryUsageLimit {
        get {
#if WINDOWS_UWP
                return MemoryManager.AppMemoryUsageLimit;
#else
            return ConvertMegabytesToBytes(SystemInfo.systemMemorySize);
#endif
        }
    }

    private static ulong AppMemoryUsage {
        get {
#if WINDOWS_UWP
                return MemoryManager.AppMemoryUsage;
#else
            return (ulong)Profiler.GetTotalAllocatedMemoryLong();
#endif
        }
    }

    public event EventHandler<DiagnosticData> OnNewDiagnostic;



    // Start is called before the first frame update
    void Start()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    float memoryMB;
    int memoryIntegerDigits, memoryFractionalDigits;

    private void LateUpdate() {

        using (LateUpdatePerfMarker.Auto()) {
            FrameTimingManager.CaptureFrameTimings();
            ++frameCount;
            float elapsedSeconds = stopwatch.ElapsedMilliseconds * 0.001f;
            if (elapsedSeconds >= frameSampleRate) {

                // FPS -----------------------------------------------

                cpuFrameRate = (int)(1.0f / (elapsedSeconds / frameCount));
                gpuFrameRate = 0;

                // Many platforms do not yet support the FrameTimingManager. When timing Data is returned from the FrameTimingManager we will use
                // its timing Data, else we will depend on the stopwatch.
                uint frameTimingsCount = FrameTimingManager.GetLatestTimings((uint)Mathf.Min(frameCount, maxFrameTimings), frameTimings);

                if (frameTimingsCount != 0) {
                    float cpuFrameTime, gpuFrameTime;
                    AverageFrameTiming(frameTimings, frameTimingsCount, out cpuFrameTime, out gpuFrameTime);
                    cpuFrameRate = (int)(1.0f / (cpuFrameTime / frameCount));
                    gpuFrameRate = (int)(1.0f / (gpuFrameTime / frameCount));

                }

                // Update frame rate text.
                cpuFrameRateMessage = Mathf.Clamp(cpuFrameRate, 0, maxTargetFrameRate);

                if (gpuFrameRate != 0) {
                    gpuFrameRateMessage = Mathf.Clamp(gpuFrameRate, 0, maxTargetFrameRate);

                }

                // Memory Limit -----------------------------------------------

                ulong limit = AppMemoryUsageLimit;

                if (limit != limitMemoryUsage) {
                    if (WillDisplayedMemoryUsageDiffer(limitMemoryUsage, limit, displayedDecimalDigits)) {
                        //MemoryUsageToString(stringBuffer, displayedDecimalDigits, limitMemoryText, limitMemoryString, limit);

                        memoryMB = ConvertBytesToMegabytes(limit);
                        memoryIntegerDigits = (int)memoryMB;
                        memoryFractionalDigits = (int)((memoryMB - memoryIntegerDigits) * Mathf.Pow(10.0f, displayedDecimalDigits));
                       
                        memoryLimitMessage = memoryMB;

                    }

                    limitMemoryUsage = limit;

                }

                // Memory Usage -----------------------------------------------

                ulong usage = AppMemoryUsage;

                if (usage != memoryUsage) {

                    if (WillDisplayedMemoryUsageDiffer(memoryUsage, usage, displayedDecimalDigits)) {
                        //MemoryUsageToString(stringBuffer, displayedDecimalDigits, usedMemoryText, usedMemoryString, usage);

                        memoryMB = ConvertBytesToMegabytes(usage);
                        memoryIntegerDigits = (int)memoryMB;
                        memoryFractionalDigits = (int)((memoryMB - memoryIntegerDigits) * Mathf.Pow(10.0f, displayedDecimalDigits));

                        memoryUsageMessage = memoryMB;

                    }

                    memoryUsage = usage;

                }

                // Memory Peak -----------------------------------------------

                if (memoryUsage > peakMemoryUsage) {

                    if (WillDisplayedMemoryUsageDiffer(peakMemoryUsage, memoryUsage, displayedDecimalDigits)) {
                        memoryMB = ConvertBytesToMegabytes(memoryUsage);
                        memoryIntegerDigits = (int)memoryMB;
                        memoryFractionalDigits = (int)((memoryMB - memoryIntegerDigits) * Mathf.Pow(10.0f, displayedDecimalDigits));

                        memoryPeakMessage = memoryMB;

                    }

                    peakMemoryUsage = memoryUsage;

                }

                // -----------------------------------------------

                // Reset timers.
                frameCount = 0;
                stopwatch.Reset();
                stopwatch.Start();

                OnNewDiagnostic?.Invoke(this, new DiagnosticData(
                        cpuFrameRate,
                        gpuFrameRate,
                        memoryPeakMessage,
                        memoryUsageMessage,
                        memoryLimitMessage

                    ));

            }

        }

    }

    private static void AverageFrameTiming(FrameTiming[] frameTimings, uint frameTimingsCount, out float cpuFrameTime, out float gpuFrameTime) {
        using (AverageFrameTimingPerfMarker.Auto()) {
            double cpuTime = 0.0f;
            double gpuTime = 0.0f;

            for (int i = 0; i < frameTimingsCount; ++i) {
                cpuTime += frameTimings[i].cpuFrameTime;
                gpuTime += frameTimings[i].gpuFrameTime;
            }

            cpuTime /= frameTimingsCount;
            gpuTime /= frameTimingsCount;

            cpuFrameTime = (float)(cpuTime * 0.001);
            gpuFrameTime = (float)(gpuTime * 0.001);
        }
    }

    private static bool WillDisplayedMemoryUsageDiffer(ulong oldUsage, ulong newUsage, int displayedDecimalDigits) {
        using (WillDisplayedMemoryUsageDifferPerfMarker.Auto()) {
            float oldUsageMBs = ConvertBytesToMegabytes(oldUsage);
            float newUsageMBs = ConvertBytesToMegabytes(newUsage);
            float decimalPower = Mathf.Pow(10.0f, displayedDecimalDigits);

            return (int)(oldUsageMBs * decimalPower) != (int)(newUsageMBs * decimalPower);
        }
    }

    private static ulong ConvertMegabytesToBytes(int megabytes) {
        return ((ulong)megabytes * 1024UL) * 1024UL;
    }

    private static float ConvertBytesToMegabytes(ulong bytes) {
        return (bytes / 1024.0f) / 1024.0f;
    }

    private static int MemoryItoA(int value, char[] stringBuffer, int bufferIndex) {
        using (MemoryItoAPerfMarker.Auto()) {
            int startIndex = bufferIndex;

            for (; value != 0; value /= 10) {
                stringBuffer[bufferIndex++] = (char)((char)(value % 10) + '0');
            }

            char temp;
            for (int endIndex = bufferIndex - 1; startIndex < endIndex; ++startIndex, --endIndex) {
                temp = stringBuffer[startIndex];
                stringBuffer[startIndex] = stringBuffer[endIndex];
                stringBuffer[endIndex] = temp;
            }

            return bufferIndex;
        }
    }

}
