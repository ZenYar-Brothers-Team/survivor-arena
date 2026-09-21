using System;
using System.IO;
using System.Text;

namespace Game.Telemetry
{
    /// <summary>Local-only output under the supplied root. No deletion/retention or network side effects.</summary>
    public sealed class LocalPlaytestExportSink : IPlaytestExportSink
    {
        private readonly string _root;
        public LocalPlaytestExportSink(string root) { _root = Path.GetFullPath(root); }
        public string Write(PlaytestReport report)
        {
            if (!Guid.TryParseExact(report.ReportId, "N", out _)) throw new ArgumentException("Invalid report ID.");
            var folder = Path.Combine(_root, report.ReportId);
            Directory.CreateDirectory(folder);
            // Publish JSON last: its existence denotes a complete three-file packet.
            WriteFile(Path.Combine(folder, "summary.md"), report.Summary);
            var feedback = Path.Combine(folder, "feedback.md");
            if (!File.Exists(feedback)) File.WriteAllText(feedback, report.Feedback, new UTF8Encoding(false));
            WriteFile(Path.Combine(folder, "run.json"), report.Json);
            return folder;
        }
        private static void WriteFile(string path, string value)
        {
            var pending = path + ".pending";
            File.WriteAllText(pending, value, new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(pending, path, null);
            else File.Move(pending, path);
        }
    }
}
