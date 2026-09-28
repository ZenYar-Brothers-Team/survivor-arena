using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace Game.Bootstrap.Editor
{
    /// <summary>Exports a compact CPU summary from a FIELD-001 standalone binary profiler log.</summary>
    public static class Field001ProfileAnalyzer
    {
        [Serializable]
        private sealed class ProfileSummary
        {
            public string input;
            public string thread;
            public int firstFrame;
            public int lastFrame;
            public int frameCount;
            public float frameP50Ms;
            public float frameP95Ms;
            public float frameP99Ms;
            public float frameMaxMs;
            public List<MarkerSummary> markers = new List<MarkerSummary>();
        }

        [Serializable]
        private sealed class MarkerSummary
        {
            public string name;
            public int calls;
            public double totalMs;
            public float maxSampleMs;
        }

        private sealed class MarkerAccumulator
        {
            public int Calls;
            public double TotalMs;
            public float MaxSampleMs;
        }

        public static void Analyze()
        {
            var input = Argument("--benchmark-profile-input=");
            var output = Argument("--benchmark-profile-summary=");
            if (string.IsNullOrWhiteSpace(input) || !File.Exists(input))
                throw new ArgumentException("--benchmark-profile-input=<existing raw profile> is required.");
            if (string.IsNullOrWhiteSpace(output))
                throw new ArgumentException("--benchmark-profile-summary=<output json> is required.");
            if (!ProfilerDriver.LoadProfile(input, false))
                throw new InvalidOperationException("Unity could not load the profiler log: " + input);

            var frames = new List<float>();
            var markers = new Dictionary<string, MarkerAccumulator>(StringComparer.Ordinal);
            var summary = new ProfileSummary
            {
                input = input,
                firstFrame = ProfilerDriver.firstFrameIndex,
                lastFrame = ProfilerDriver.lastFrameIndex
            };

            for (var frame = summary.firstFrame; frame <= summary.lastFrame; frame++)
            {
                using var iterator = new ProfilerFrameDataIterator();
                iterator.SetRoot(frame, 0);
                if (summary.thread == null) summary.thread = iterator.GetThreadName();
                frames.Add(iterator.frameTimeMS);
                while (iterator.Next(true))
                {
                    if (string.IsNullOrEmpty(iterator.name) || iterator.durationMS < 0.01f) continue;
                    if (!markers.TryGetValue(iterator.name, out var marker))
                    {
                        marker = new MarkerAccumulator();
                        markers.Add(iterator.name, marker);
                    }
                    marker.Calls++;
                    marker.TotalMs += iterator.durationMS;
                    marker.MaxSampleMs = Mathf.Max(marker.MaxSampleMs, iterator.durationMS);
                }
            }

            frames.Sort();
            summary.frameCount = frames.Count;
            summary.frameP50Ms = Percentile(frames, 0.50f);
            summary.frameP95Ms = Percentile(frames, 0.95f);
            summary.frameP99Ms = Percentile(frames, 0.99f);
            summary.frameMaxMs = frames.Count == 0 ? 0f : frames[frames.Count - 1];
            summary.markers = markers
                .Select(pair => new MarkerSummary
                {
                    name = pair.Key,
                    calls = pair.Value.Calls,
                    totalMs = pair.Value.TotalMs,
                    maxSampleMs = pair.Value.MaxSampleMs
                })
                .Where(marker => marker.maxSampleMs >= 0.25f)
                .OrderByDescending(marker => marker.totalMs)
                .Take(200)
                .ToList();

            var directory = Path.GetDirectoryName(output);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(output, JsonUtility.ToJson(summary, true));
            Debug.Log("FIELD-001 profiler summary: " + output);
        }

        private static float Percentile(IReadOnlyList<float> sorted, float percentile)
        {
            if (sorted.Count == 0) return 0f;
            var index = Math.Min(sorted.Count - 1, Math.Max(0, Mathf.CeilToInt(sorted.Count * percentile) - 1));
            return sorted[index];
        }

        private static string Argument(string prefix)
        {
            foreach (var value in Environment.GetCommandLineArgs())
                if (value.StartsWith(prefix, StringComparison.Ordinal)) return value.Substring(prefix.Length);
            return null;
        }
    }
}
