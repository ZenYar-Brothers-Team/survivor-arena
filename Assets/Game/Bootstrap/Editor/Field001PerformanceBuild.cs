using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Game.Bootstrap.Editor
{
    /// <summary>Builds the isolated FIELD-001 performance player used by F1-09 acceptance.</summary>
    public static class Field001PerformanceBuild
    {
        public static void Build()
        {
            var output = Argument("--benchmark-build-output=");
            if (string.IsNullOrWhiteSpace(output))
                throw new ArgumentException("--benchmark-build-output=<absolute exe path> is required.");
            Directory.CreateDirectory(Path.GetDirectoryName(output) ?? throw new ArgumentException("Output directory is required."));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Gameplay.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
                extraScriptingDefines = new[] { "FIELD001_PERFORMANCE_BENCHMARK" }
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Performance player build failed: {report.summary.result}.");
        }

        private static string Argument(string prefix)
        {
            foreach (var value in Environment.GetCommandLineArgs())
                if (value.StartsWith(prefix, StringComparison.Ordinal)) return value.Substring(prefix.Length);
            return null;
        }
    }
}
