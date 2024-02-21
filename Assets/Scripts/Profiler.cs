using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using TMPro;

/**
* ProfilerRecorder example (base of code) from https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
*/
public class Profiler : MonoBehaviour
{
    string statsText;
    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder fpsRecorder;
    ProfilerRecorder mainFPSRecorder;

    private int frameCount;
    private double fpsSum, avgFPS, minFPS, maxFPS;
    private double mainFPSSum, avgMainFPSSum, minMainFPS, maxMainFPS;
    private double drawTimeSum, avgDrawTime, minDrawTime, maxDrawTime;
    private double combinedTimeSum, avgCombinedTime, minCombinedTime, maxCombinedTime;

  
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
        combinedTimeSum = 0;
        avgCombinedTime = 0;
        minCombinedTime = 10000;
        maxCombinedTime = -1;
        //Debug.Log("FPS sum = " + fpsSum);

        // System Used Memory is recorded in kibibytes
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

        // Render Thread is recorded in nanoseconds
        fpsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread"); 

        // Main Thread is recorded in deciseconds (divide by 100 to get milliseconds)   
        mainFPSRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");     
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
        var sampleInterval = 10;
        var avgInterval = 500;

        if (frameCount % sampleInterval == 0) {

            // Get stats for current frame
            var fps = calculateFPS(nsToMs(fpsRecorder.LastValue));
            var drawTime = nsToMs(fpsRecorder.LastValue);
            var mainFPS = 1000.0 / (mainFPSRecorder.LastValue / 100.0);

            var combinedTime = Time.unscaledDeltaTime * 1000;       // Convert to ms

            if (fps < 0) {
                fps = -1;
            }
            if (drawTime < 0) {
                drawTime = -1;
            }
            if (mainFPS < 0) {
                mainFPS = -1;
            }
            if (combinedTime < 0) {
                combinedTime = -1;
            }

            // Sample FPS every 50 frames
            if (fps > 0) {
                // Add to average
                fpsSum = fpsSum + fps;
                Debug.Log("FPS: " + fps + ", sum: " + fpsSum );

                // Check min
                if (fps < minFPS && fps >= 0) {
                    minFPS = fps;
                }
                
                // Check max
                if (fps > maxFPS) {
                    maxFPS = fps;
                }
            }

            // Sample draw time every 50 frames
            if (drawTime > 0) {
                // Add to average
                drawTimeSum = drawTimeSum + drawTime;
                Debug.Log("Draw Time: " + drawTime + ", sum: " + drawTimeSum );

                // Check min
                if (drawTime < minDrawTime && drawTime >= 0) {
                    minDrawTime = drawTime;
                }
                
                // Check max
                if (drawTime > maxDrawTime) {
                    maxDrawTime = drawTime;
                }
            }

            /*
            if (mainFPS > 0) {
                // Add to average
                mainFPSSum = mainFPSSum + mainFPS;
                Debug.Log("Main FPS: " + mainFPS + ", sum: " + mainFPSSum );

                // Check min
                if (mainFPS < minMainFPS) {
                    minMainFPS = mainFPS;
                }
                
                // Check max
                if (mainFPS > maxMainFPS) {
                    maxMainFPS = mainFPS;
                }
            }
            */

            if (combinedTime > 0) {
                // Add to average
                combinedTimeSum = combinedTimeSum + combinedTime;
                Debug.Log("Combined time: " + combinedTime + ", sum: " + combinedTimeSum );

                // Check min
                if (combinedTime < minCombinedTime) {
                    minCombinedTime = combinedTime;
                }
                
                // Check max
                if (combinedTime > maxCombinedTime) {
                    maxCombinedTime = combinedTime;
                }
            }

            // Average FPS and draw time every 500 frames, then reset avereage to prevent overflow
            if (frameCount >= avgInterval) {
                // Calculate averages
                avgFPS = fpsSum / (avgInterval / sampleInterval);
                avgDrawTime = drawTimeSum / (avgInterval / sampleInterval);
                avgMainFPSSum = mainFPSSum / (avgInterval / sampleInterval);
                avgCombinedTime = combinedTimeSum / (avgInterval / sampleInterval);

                // Debug
                Debug.Log("~~~~~Got average~~~~");
                Debug.Log("Avg FPS = " + fpsSum + " / " + (avgInterval / sampleInterval) + " = " + avgFPS);
                Debug.Log("Avg Draw Time = " + drawTimeSum + " / " + (avgInterval / sampleInterval) + " = " + avgDrawTime);
                Debug.Log("Avg Combined Time = " + combinedTimeSum + " / " + (avgInterval / sampleInterval) + " = " + avgCombinedTime);

                // Reset averages
                frameCount = 0;
                fpsSum = 0;
                drawTimeSum = 0;
                mainFPSSum = 0;
                combinedTimeSum = 0;
            }


        // Update GUI readings at every sample interval
            var sb = new StringBuilder(500);
            // I think these are format strings for the StringBuilder?
            sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
            sb.AppendLine();
            sb.AppendLine($"Draw Time (Render Thread): {drawTime:F2} ms");        // Render Thread returns time in nanoseconds, convert to milliseconds
            sb.AppendLine($"Avg. Draw Time: {avgDrawTime:F2} ms");
            sb.AppendLine($"Min. Draw Time: {minDrawTime:F2} ms");
            sb.AppendLine($"Max. Draw Time: {maxDrawTime:F2} ms");
            sb.AppendLine();
            sb.AppendLine($"Draw Time (Combined Threads): {combinedTime:F2} ms");        // Render Thread returns time in nanoseconds, convert to milliseconds
            sb.AppendLine($"Avg. Draw Time: {avgCombinedTime:F2} ms");
            sb.AppendLine($"Min. Draw Time: {minCombinedTime:F2} ms");
            sb.AppendLine($"Max. Draw Time: {maxCombinedTime:F2} ms");
            sb.AppendLine();
            sb.AppendLine($"FPS (Render Thread): {fps:F0}");
            sb.AppendLine($"Avg. FPS: {avgFPS:F0}");
            sb.AppendLine($"Min FPS: {minFPS:F0}");
            sb.AppendLine($"Max FPS: {maxFPS:F0}");
            sb.AppendLine();
            sb.AppendLine($"FPS (Combined Threads): {calculateFPS(combinedTime):F0}");
            sb.AppendLine($"Avg. FPS: {calculateFPS(avgCombinedTime):F0}");
            sb.AppendLine($"Min FPS: {calculateFPS(minCombinedTime):F0}");
            sb.AppendLine($"Max FPS: {calculateFPS(maxCombinedTime):F0}");
            sb.AppendLine();
            sb.AppendLine($"Main Thread FPS: {mainFPS:F0}");
            sb.AppendLine($"Draw Time (Main Thread): {mainFPSRecorder.LastValue / 100.0 :F0} ms");
            //sb.AppendLine($"Avg. Draw Time (Main Thread): {mainFPSRecorder.LastValue / 100.0 :F0} ms");

            statsText = sb.ToString();
        }

    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, 300), statsText);
    }

    /**
    * Calculates frames per second. Takes the time between frames in nanoseconds and returns FPS
    */
    private double calculateFPS(double ms) {
        // double ms = ns / 1000000.0;

        // // Profiler can sometimes return outliers that are near-zero
        // if (ms < 1) {
        //     return -1;
        // } 

        var framerate = 1000.0 / ms;

        if (framerate <= 0) {
            return -1;
        } else {
            return framerate;
        }
    }

    /**
    * Converts nanoseconds to milliseconds. Returns -1 if ms is near-zero
    */
    private double nsToMs(double ns) {
        double ms = ns / 1000000.0;

        // Profiler can sometimes return outliers that are near-zero
        if (ms < 1) {
            return -1;
        } else {
            return ms;
        }

    }

    /*
    private void getStats(double time, double sum, double min, double max) {
        if (time > 0) {
            // Add to average
            sum += time;
            Debug.Log("Main FPS: " + time + ", sum: " + sum );

            // Check min
            if (time < min) {
                min = time;
            }
            
            // Check max
            if (time > max) {
                max = time;
            }
        }
    }
    */
}