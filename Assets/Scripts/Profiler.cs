using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

/**
* ProfilerRecorder example (base of code) from https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
*/
public class Profiler : MonoBehaviour
{
    string statsText;
    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;

    ProfilerRecorder fpsRecorder;

    private int frameCount;
    private double fpsSum, avgFPS, minFPS, maxFPS;
    private double drawTimeSum, avgDrawTime, minDrawTime, maxDrawTime;

    // void Start() {
    //     frameCount = 0;
    //     fpsSum = 0;
    //     avgFPS = -1;
    //     minFPS = 10000;
    //     maxFPS = 0;
    // }

    /*
    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0) {
            //Debug.Log("Zero samples");
            return 0;
        } else {
            //Debug.Log("Recorder capacity: " + recorder.Capacity);
        }

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i) {
                r += samples[i].Value;
                //Debug.Log(samples[i].Value);
            }
            r /= samplesCount;
        }

        return r;
    }
    */

    /*
    private unsafe double getAverage(ProfilerRecorder recorder) 
    {
        // frameCount++;
        // double sum = 0;
        // var samples = stackalloc ProfilerRecorderSample[frameCount];
        // recorder.CopyTo(samples, frameCount);

        // for (int i = 0; i < frameCount; i++) {
        //     sum += samples[i].Value;
        // }
        // return sum / frameCount;

        return -1;

    }
    */

    void OnEnable()
    {
        frameCount = 0;
        fpsSum = 0;
        avgFPS = -1;
        minFPS = 10000;
        maxFPS = -1;
        drawTimeSum = 0;
        avgDrawTime = -1;
        minDrawTime = 10000;
        maxDrawTime = -1;
        Debug.Log("FPS sum = " + fpsSum);

        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        //mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        fpsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread");      
    }

    void OnDisable()
    {
        systemMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        fpsRecorder.Dispose();
    }

    void Update()
    {
        frameCount++;
        var sampleInterval = 50;
        var avgInterval = 500;

        // Get stats for current frame
        var fps = calculateFPS(fpsRecorder.LastValue);
        var drawTime = fpsRecorder.LastValue / 1000000.0;

        // Sample FPS every 50 frames
        if (frameCount % sampleInterval == 0) {
            // Add to average
            fpsSum = fpsSum + fps;
            Debug.Log("FPS: " + fps + ", sum: " + fpsSum );

            // Check min
            if (fps < minFPS) {
                minFPS = fps;
            }
            
            // Check max
            if (fps > maxFPS) {
                maxFPS = fps;
            }
        }

        // Sample draw time every 50 frames
        if (frameCount % sampleInterval == 0) {
            // Add to average
            drawTimeSum = drawTimeSum + drawTime;
            Debug.Log("Draw Time: " + drawTime + ", sum: " + drawTimeSum );

            // Check min
            if (drawTime < minDrawTime) {
                minDrawTime = drawTime;
            }
            
            // Check max
            if (drawTime > maxDrawTime) {
                maxDrawTime = drawTime;
            }
        }

        // Average FPS and draw time every 500 frames, then reset avereage to prevent overflow
        if (frameCount >= avgInterval) {
            // Calculate averages
            avgFPS = fpsSum / (avgInterval / sampleInterval);
            avgDrawTime = drawTimeSum / (avgInterval / sampleInterval);

            // Debug
            Debug.Log("~~~~~Got average~~~~");
            Debug.Log("Avg FPS = " + fpsSum + " / " + (avgInterval / sampleInterval) + " = " + avgFPS);
            Debug.Log("Avg Draw Time = " + drawTimeSum + " / " + (avgInterval / sampleInterval) + " = " + avgDrawTime);

            // Reset averages
            frameCount = 0;
            fpsSum = 0;
            drawTimeSum = 0;
            
        }

        var sb = new StringBuilder(500);
        // I think these are format strings for the StringBuilder?
        //sb.AppendLine($"Frame Time: {GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms");
        //sb.AppendLine($"Frame Time 2: {getAverage(fpsRecorder) / 1000000.0 :F1} ms");
        sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
        
        sb.AppendLine($"Draw Time (Render Thread): {drawTime:F2} ms");        // Render Thread returns time in nanoseconds, convert to milliseconds
        sb.AppendLine($"Avg. Draw Time: {avgDrawTime:F2} ms");
        sb.AppendLine($"Min. Draw Time: {minDrawTime:F2} ms");
        sb.AppendLine($"Max. Draw Time: {maxDrawTime:F2} ms");

        sb.AppendLine($"FPS: {fps:F0}");
        sb.AppendLine($"Avg. FPS: {avgFPS:F0}");
        sb.AppendLine($"Min FPS: {minFPS:F0}");
        sb.AppendLine($"Max FPS: {maxFPS:F0}");

        statsText = sb.ToString();

    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, 200), statsText);
    }

    /**
    * Calculates frames per second. Takes the time between frames in nanoseconds and returns FPS
    */
    private double calculateFPS(double ns) {
        double ms = ns / 1000000.0;
        return 1000.0 / ms;
    }
}