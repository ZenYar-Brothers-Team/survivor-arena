using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Game.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Game.Automation
{
    /// <summary>
    /// AB-14: single main-thread producer, bounded non-blocking queue, one background writer.
    /// Call Complete once and await Completion before reporting success. No Unity objects cross threads.
    /// </summary>
    public sealed class DemonstrationWriter : IDisposable
    {
        private const int FooterReserveBytes = 4096;
        private static readonly Encoding Utf8 = new UTF8Encoding(false);
        private readonly BlockingCollection<string> _queue;
        private readonly Func<TextWriter> _open;
        private readonly Action<bool> _commit;
        private readonly string _header;
        private readonly int _maxSamples;
        private readonly long _maxBytes;
        private long _queuedBytes;
        private long _writtenBytes;
        private int _writtenSamples;
        private string _error;
        private string _terminal;
        private bool _closing;
        private bool _complete;

        public int AcceptedSamples { get; private set; }
        public int RejectedSamples { get; private set; }
        public int WrittenSamples => Volatile.Read(ref _writtenSamples);
        public long WrittenBytes => Interlocked.Read(ref _writtenBytes);
        public string Error => Volatile.Read(ref _error);
        public bool RecordingComplete => Volatile.Read(ref _complete);
        public Task Completion { get; }

        public DemonstrationWriter(DemonstrationConfigData settings, JObject header,
            Func<TextWriter> open, Action<bool> commit)
        {
            (settings ?? throw new ArgumentNullException(nameof(settings))).Validate();
            _open = open ?? throw new ArgumentNullException(nameof(open));
            _commit = commit ?? throw new ArgumentNullException(nameof(commit));
            _header = (header ?? throw new ArgumentNullException(nameof(header))).ToString(Formatting.None);
            _queuedBytes = Utf8.GetByteCount(_header) + 1L;
            _maxBytes = settings.MaxFileMegabytes.Value * 1024L * 1024L;
            if (_queuedBytes > _maxBytes - FooterReserveBytes) throw new ArgumentException("Demonstration header exceeds byte budget.");
            _maxSamples = settings.MaxSamples.Value;
            _queue = new BlockingCollection<string>(settings.QueueCapacity.Value);
            Completion = Task.Run(Write);
        }

        /// <summary>Creates a new isolated file. An incomplete recording keeps the .partial suffix.</summary>
        public static DemonstrationWriter CreateFile(string path, DemonstrationConfigData settings, JObject header)
        {
            var full = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path)));
            var partial = full + ".partial";
            if (File.Exists(full) || File.Exists(partial)) throw new IOException("Demonstration output already exists.");
            return new DemonstrationWriter(settings, header, () =>
            {
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                return new StreamWriter(new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.Read), Utf8);
            }, complete => { if (complete) File.Move(partial, full); });
        }

        public bool TryWriteSample(JObject sample)
        {
            if (_closing || Error != null) return false;
            using var guard = PerfGuard.Measure("DemonstrationWriter.SerializeAndQueue", 8f);
            if (AcceptedSamples >= _maxSamples) return Reject("sampleLimit");
            sample["type"] = "sample";
            sample["sampleIndex"] = AcceptedSamples;
            var line = sample.ToString(Formatting.None);
            var bytes = Utf8.GetByteCount(line) + 1L;
            if (_queuedBytes + bytes > _maxBytes - FooterReserveBytes) return Reject("byteLimit");
            if (!_queue.TryAdd(line)) return Reject("queueFull");
            _queuedBytes += bytes;
            AcceptedSamples++;
            return true;
        }

        public void SetError(string error)
        {
            if (string.IsNullOrEmpty(error)) return;
            Interlocked.CompareExchange(ref _error, error.Length <= 512 ? error : error.Substring(0, 512), null);
        }

        public Task Complete(JObject terminal)
        {
            if (_closing) return Completion;
            _closing = true;
            _terminal = (terminal ?? new JObject()).ToString(Formatting.None);
            _queue.CompleteAdding();
            // No producer can use the queue after _closing. Dispose synchronization resources after draining.
            _ = Completion.ContinueWith(_ => _queue.Dispose(), TaskScheduler.Default);
            return Completion;
        }

        private bool Reject(string reason)
        {
            RejectedSamples++;
            SetError(reason);
            return false;
        }

        private void Write()
        {
            try
            {
                var complete = false;
                using (var writer = _open())
                {
                    writer.NewLine = "\n";
                    WriteLine(writer, _header);
                    foreach (var line in _queue.GetConsumingEnumerable())
                    {
                        WriteLine(writer, line);
                        Interlocked.Increment(ref _writtenSamples);
                    }
                    complete = Error == null && WrittenSamples > 0 && WrittenSamples == AcceptedSamples;
                    var footer = JObject.Parse(_terminal ?? "{}");
                    footer["type"] = "end";
                    footer["recordingComplete"] = complete;
                    footer["samples"] = WrittenSamples;
                    footer["rejectedSamples"] = RejectedSamples;
                    footer["error"] = Error;
                    var end = footer.ToString(Formatting.None);
                    if (WrittenBytes + Utf8.GetByteCount(end) + 1L > _maxBytes)
                        throw new IOException("Demonstration footer exceeds byte budget.");
                    WriteLine(writer, end);
                }
                _commit(complete);
                Volatile.Write(ref _complete, complete);
            }
            catch (Exception error) { SetError("writeFailure: " + error.Message); }
        }

        private void WriteLine(TextWriter writer, string value)
        {
            writer.WriteLine(value);
            writer.Flush(); // Off the gameplay thread; completed lines remain readable after a hard process stop.
            Interlocked.Add(ref _writtenBytes, Utf8.GetByteCount(value) + 1L);
        }

        public void Dispose()
        {
            if (_closing) return;
            SetError("disposedBeforeCompletion");
            Complete(new JObject { ["reason"] = "disposed" });
        }
    }
}
