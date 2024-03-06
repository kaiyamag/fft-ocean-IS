/*
* Profiler.cs
*
* This class samples average , minimum , and maximum frame rate (FPS), draw time (ms), and memory usage (MB)
* for any Godot project . Performance data is sampled every 10 frames (see ‘sample_rate ‘) and averages are
* calculated every 500 frames (see ‘print_rate ‘). The latest performance data and number of elapsed frames are
* displayed in a GUI overlay of the Godot project .
*
* This class can be added without dependencies to any Godot project
*
* Code References :
* Basic ProfilerRecorder example: https://docs.unity3d.com/ScriptReference/Unity.Profiling.ProfilerRecorder.html
*
* Copyright Kaiya Magnuson 2024
*/

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
    string statsText;                           // String builder for stats printout
    ProfilerRecorder systemMemoryRecorder;      // Gets current memory usage readings
    ProfilerRecorder fpsRecorder;               // Gets current frame rate readings

    private int frameCount;                                                                 // The number of elapsed frames , used for averaging
    private double fpsSum, avgFPS, minFPS, maxFPS;                                          // Accumulators for frame rate
    private double drawTimeSum, avgDrawTime, minDrawTime, maxDrawTime;                      // Accumulators for draw time (GPU only)
    private double combinedTimeSum, avgCombinedTime, minCombinedTime, maxCombinedTime;      // Accumulators for draw time (GPU and CPU)


    void OnEnable()
    {
        frameCount = 0;
        fpsSum = 0;
        avgFPS = -1;
        minFPS = 10000;     // Minimum FPS is not expected to exit the range [0 ,10000]
        maxFPS = -1;
        drawTimeSum = 0;
        avgDrawTime = -1;
        minDrawTime = 10000;    // Minimum draw time is not expected to exit the range [0 ,1000]
        maxDrawTime = -1;
        combinedTimeSum = 0;
        avgCombinedTime = -1;
        minCombinedTime = 10000;    // Minimum draw time is not expected to exit the range [0 ,1000]
        maxCombinedTime = -1;

        // System Used Memory is recorded in kibibytes
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

        // Render Thread is recorded in nanoseconds
        fpsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread"); 
    }

    void OnDisable()
    {
        systemMemoryRecorder.Dispose();
        fpsRecorder.Dispose();
    }

    void Update()
    {
        frameCount++;
        var sampleInterval = 10;    // Number of frames to wait between each profiler sample
        var avgInterval = 500;      // Number of frames to wait between calculating and printing averages

        if (frameCount % sampleInterval == 0) {

            // Get stats for current frame
            var fps = calculateFPS(nsToMs(fpsRecorder.LastValue));  // Tracks current frame rate
            var drawTime = nsToMs(fpsRecorder.LastValue);           // Tracks draw time for the GPU
            var combinedTime = Time.unscaledDeltaTime * 1000;       // Tracks draw time for the CPU and GPU combined

            if (fps < 0) {
                fps = -1;
            }
            if (drawTime < 0) {
                drawTime = -1;
            }
            if (combinedTime < 0) {
                combinedTime = -1;
            }

            // Sample FPS
            if (fps > 0) {
                // Add to average
                fpsSum = fpsSum + fps;
                // Check min
                if (fps < minFPS && fps >= 0) {
                    minFPS = fps;
                }
                // Check max
                if (fps > maxFPS) {
                    maxFPS = fps;
                }
            }

            // Sample draw time on GPU only
            if (drawTime > 0) {
                // Add to average
                drawTimeSum = drawTimeSum + drawTime;
                // Check min
                if (drawTime < minDrawTime && drawTime >= 0) {
                    minDrawTime = drawTime;
                }
                // Check max
                if (drawTime > maxDrawTime) {
                    maxDrawTime = drawTime;
                }
            }

            // Sample draw time on CPU and GPU
            if (combinedTime > 0) {
                // Add to average
                combinedTimeSum = combinedTimeSum + combinedTime;
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
                avgCombinedTime = combinedTimeSum / (avgInterval / sampleInterval);

                // Reset averages
                frameCount = 0;
                fpsSum = 0;
                drawTimeSum = 0;
                combinedTimeSum = 0;
            }

            // Update GUI readings at every sample interval
            var sb = new StringBuilder(500);
            // These are format strings for the StringBuilder
            sb.AppendLine($"Frame Count: {frameCount}");
            sb.AppendLine("--------------------");
            sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
            sb.AppendLine();
            sb.AppendLine($"FPS (Combined Threads): {calculateFPS(combinedTime):F0}");
            sb.AppendLine($"Avg. FPS: {calculateFPS(avgCombinedTime):F0}");
            sb.AppendLine($"Min FPS: {calculateFPS(maxCombinedTime):F0}");          // NOTE: Min FPS corresponds to max draw time and vice versa
            sb.AppendLine($"Max FPS: {calculateFPS(minCombinedTime):F0}");
            sb.AppendLine();
            sb.AppendLine($"Draw Time (Combined Threads): {combinedTime:F2} ms");        
            sb.AppendLine($"Avg. Draw Time: {avgCombinedTime:F2} ms");
            sb.AppendLine($"Min. Draw Time: {minCombinedTime:F2} ms");
            sb.AppendLine($"Max. Draw Time: {maxCombinedTime:F2} ms");
            sb.AppendLine();
            sb.AppendLine($"FPS (Render Thread): {fps:F0}");
            sb.AppendLine($"Avg. FPS: {avgFPS:F0}");
            sb.AppendLine($"Min FPS: {minFPS:F0}");
            sb.AppendLine($"Max FPS: {maxFPS:F0}");
            sb.AppendLine();
            sb.AppendLine($"Draw Time (Render Thread): {drawTime:F2} ms");        // Render Thread returns time in nanoseconds, convert to milliseconds
            sb.AppendLine($"Avg. Draw Time: {avgDrawTime:F2} ms");
            sb.AppendLine($"Min. Draw Time: {minDrawTime:F2} ms");
            sb.AppendLine($"Max. Draw Time: {maxDrawTime:F2} ms");
            sb.AppendLine();
            
            statsText = sb.ToString();
        }

    }

    /**
    * Prepares a text box in the scene GUI
    */
    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 300, 400), statsText);
    }

    /**
    * Calculates frames per second. Takes the time between frames in milliseconds and returns FPS. 
    * Returns -1 if framerate is negative.
    */
    private double calculateFPS(double ms) {
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
}