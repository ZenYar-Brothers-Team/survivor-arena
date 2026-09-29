using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Game.Bootstrap.Editor
{
    /// <summary>Separate opt-in development player; ordinary builds never define BALANCE_AUTOMATION.</summary>
    public static class AutomationBuild
    {
        public static void Build()
        {
            var output = Argument("--balance-build-output=");
            if (string.IsNullOrWhiteSpace(output) || !Path.IsPathRooted(output) ||
                !output.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("--balance-build-output=<absolute exe path> required.");
            if (File.Exists(output)) throw new IOException("Balance executable already exists: " + output);
            var dataFolder = Path.Combine(Path.GetDirectoryName(output), Path.GetFileNameWithoutExtension(output) + "_Data");
            if (Directory.Exists(dataFolder)) throw new IOException("Balance data folder already exists: " + dataFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Gameplay.unity" },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
                extraScriptingDefines = new[] { "BALANCE_AUTOMATION" }
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Balance player build failed: " + report.summary.result);
            var manifest = new JObject
            {
                ["schemaVersion"] = 1, ["builtUtc"] = DateTime.UtcNow.ToString("O"),
                ["unityVersion"] = UnityEngine.Application.unityVersion,
                ["commit"] = Git("rev-parse HEAD") ?? "unknown",
                ["dirty"] = !string.IsNullOrEmpty(Git("status --porcelain")),
                ["executableSha256"] = HashFile(output), ["dataSha256"] = HashTree(dataFolder),
                ["buildType"] = "Development+BALANCE_AUTOMATION"
            };
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(output), "build-manifest.json"),
                manifest.ToString(), new UTF8Encoding(false));
        }

        private static string HashFile(string path)
        {
            using var sha = SHA256.Create();
            using var file = File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", "").ToLowerInvariant();
        }
        private static string HashTree(string folder)
        {
            if (!Directory.Exists(folder)) throw new IOException("Build data folder missing: " + folder);
            var text = new StringBuilder();
            foreach (var file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories).OrderBy(item => item, StringComparer.Ordinal))
                text.Append(Path.GetRelativePath(folder, file).Replace('\\', '/')).Append(':').Append(HashFile(file)).Append('\n');
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "").ToLowerInvariant();
        }
        private static string Argument(string prefix)
        {
            foreach (var value in Environment.GetCommandLineArgs())
                if (value.StartsWith(prefix, StringComparison.Ordinal)) return value.Substring(prefix.Length);
            return null;
        }
        private static string Git(string arguments)
        {
            try
            {
                using var process = new Process { StartInfo = new ProcessStartInfo("git", arguments)
                { WorkingDirectory = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..")),
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true } };
                process.Start();
                var output = process.StandardOutput.ReadToEndAsync();
                var errors = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(1500)) { process.Kill(); return null; }
                return process.ExitCode == 0 ? output.GetAwaiter().GetResult().Trim() : null;
            }
            catch { return null; }
        }
    }
}
