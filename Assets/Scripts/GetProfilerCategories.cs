/**
* GetProfilerCategories.cs
*
* This class provides a method to print all available ProfilerRecorders to the console
* upon startup. This is necessary to determine what data the Unity project is able to
* collect while profiling. This class can be added to any Unity project without
* dependencies. This class is used verbatim from the Unity 2022.3 documentation.
* 
* Code References:
* Original project: https://docs.unity3d.com/ScriptReference/Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderDescription.html
*
* Kaiya Magnuson, 2024
*/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

public class GetProfilerCategories : MonoBehaviour
{
    struct StatInfo
    {
        public ProfilerCategory Cat;
        public string Name;
        public ProfilerMarkerDataUnit Unit;
    }

    void Start() {
        EnumerateProfilerStats();
    }
    
    static unsafe void EnumerateProfilerStats()
    {
        var availableStatHandles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(availableStatHandles);

        var availableStats = new List<StatInfo>(availableStatHandles.Count);
        foreach (var h in availableStatHandles)
        {
            var statDesc = ProfilerRecorderHandle.GetDescription(h);
            var statInfo = new StatInfo()
            {
                Cat = statDesc.Category,
                Name = statDesc.Name,
                Unit = statDesc.UnitType
            };
            availableStats.Add(statInfo);
        }
        availableStats.Sort((a, b) =>
        {
            var result = string.Compare(a.Cat.ToString(), b.Cat.ToString());
            if (result != 0)
                return result;

            return string.Compare(a.Name, b.Name);
        });

        var sb = new StringBuilder("Available stats:\n");
        foreach (var s in availableStats)
        {
            sb.AppendLine($"{(int)s.Cat}\t\t - {s.Name}\t\t - {s.Unit}");
        }

        Debug.Log(sb.ToString());
    }
}