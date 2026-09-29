using System;
using System.IO;
using System.Text;
using Game.Telemetry;

namespace Game.Bootstrap.Automation
{
    /// <summary>Worker-only export into the run's isolated folder; never touches player data.</summary>
    public sealed class AutomationPlaytestSink : IPlaytestExportSink
    {
        private readonly string _folder;
        public AutomationPlaytestSink(string folder) { _folder = Path.GetFullPath(folder); }
        public string Write(PlaytestReport report)
        {
            if (report == null || !Guid.TryParseExact(report.ReportId, "N", out _))
                throw new ArgumentException("Invalid playtest report.", nameof(report));
            Directory.CreateDirectory(_folder);
            WriteAtomic(Path.Combine(_folder, "summary.md"), report.Summary);
            var feedback = Path.Combine(_folder, "feedback.md");
            if (!File.Exists(feedback)) WriteAtomic(feedback, report.Feedback);
            WriteAtomic(Path.Combine(_folder, "run.json"), report.Json);
            return _folder;
        }
        public static void WriteAtomic(string path, string value)
        {
            var pending = path + ".pending";
            File.WriteAllText(pending, value, new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(pending, path, null);
            else File.Move(pending, path);
        }
    }
}
