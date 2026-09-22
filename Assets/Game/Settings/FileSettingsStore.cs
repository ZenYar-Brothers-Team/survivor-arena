using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace Game.Settings
{
    /// <summary>App-scoped settings only. Never opens a progression profile.</summary>
    public sealed class FileSettingsStore : ISettingsStore
    {
        private readonly string _path;
        public FileSettingsStore(string path) { _path=Path.GetFullPath(path); }
        public Task<string> ReadAsync() => Task.Run(()=>File.Exists(_path)?File.ReadAllText(_path):null);
        public Task PreserveAsync() => Task.Run(()=> { if(File.Exists(_path))File.Copy(_path,_path+".invalid-"+Guid.NewGuid().ToString("N")); });
        public Task WriteAsync(string text) => Task.Run(()=>
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));var temp=_path+".tmp";
            try
            {
                using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None))
                { var bytes=Encoding.UTF8.GetBytes(text);stream.Write(bytes,0,bytes.Length);stream.Flush(true); }
                if(File.Exists(_path))File.Replace(temp,_path,null);else File.Move(temp,_path);
            }
            finally { if(File.Exists(temp))File.Delete(temp); }
        });
    }
}
