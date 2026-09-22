using System;
using System.Threading.Tasks;
namespace Game.Settings.Tests
{
    public sealed class FaultSettingsStore : ISettingsStore
    {
        public string Text;
        public bool FailPreserve, FailWrite;
        public int Writes, Preserves;
        public TaskCompletionSource<bool> Pending;
        public Task<string> ReadAsync() => Task.FromResult(Text);
        public Task PreserveAsync() { Preserves++; if(FailPreserve)throw new InvalidOperationException("preserve failed"); return Task.CompletedTask; }
        public async Task WriteAsync(string text) { if(FailWrite)throw new InvalidOperationException("write failed"); var wait=Pending; Pending=null; if(wait!=null)await wait.Task; Text=text; Writes++; }
    }
}
