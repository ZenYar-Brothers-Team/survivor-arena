using System;
using System.Threading.Tasks;
namespace Game.Meta.Tests
{
    public sealed class FailingProfileStore : IProfileStore
    {
        public string Main, Backup;
        public bool Fail;
        public int Writes, Preserved;
        public TaskCompletionSource<bool> Gate;
        public Task<string> ReadAsync(bool backup) => Task.FromResult(backup ? Backup : Main);
        public async Task WriteAsync(string json)
        {
            if (Gate != null) await Gate.Task;
            if (Fail) throw new InvalidOperationException("disk unavailable");
            Writes++; Backup = Main; Main = json;
        }
        public async Task PreserveAndResetAsync(string json) { Preserved++; await WriteAsync(json); }
    }
}
