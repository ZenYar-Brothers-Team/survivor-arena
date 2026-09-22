using System.Threading.Tasks;
namespace Game.Settings
{
    public sealed class MemorySettingsStore : ISettingsStore
    {
        public string Text { get; private set; }
        public Task<string> ReadAsync() => Task.FromResult(Text);
        public Task PreserveAsync() => Task.CompletedTask;
        public Task WriteAsync(string text) { Text=text;return Task.CompletedTask; }
    }
}
