using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace Game.Meta
{
    /// <summary>Single-writer local store; IO runs off the Unity thread. Atomic rename is the commit point.</summary>
    public sealed class FileProfileStore : IProfileStore
    {
        private readonly string _path;
        public FileProfileStore(string path) { _path = Path.GetFullPath(path ?? throw new ArgumentNullException(nameof(path))); }
        public Task<string> ReadAsync(bool backup) => Task.Run(() =>
        { var path = backup ? _path + ".bak" : _path; return File.Exists(path) ? File.ReadAllText(path) : null; });
        public Task WriteAsync(string json) => Task.Run(() => Write(json));
        private void Write(string json)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            var temp = _path + ".tmp";
            try
            {
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                { var bytes = Encoding.UTF8.GetBytes(json); stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
                if (File.Exists(_path)) File.Replace(temp, _path, _path + ".bak");
                else File.Move(temp, _path);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
        public Task PreserveAndResetAsync(string json) => Task.Run(() =>
        {
            var suffix = ".preserved-" + Guid.NewGuid().ToString("N");
            if (File.Exists(_path)) File.Copy(_path, _path + suffix);
            if (File.Exists(_path + ".bak")) File.Copy(_path + ".bak", _path + ".bak" + suffix);
            Write(json);
            File.Copy(_path, _path + ".bak", true); // Recovery must leave a valid fallback, not the damaged main.
        });
    }
}
