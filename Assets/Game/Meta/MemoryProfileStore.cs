using System.Threading.Tasks;
namespace Game.Meta
{
    /// <summary>Explicit isolated store for fixture integration and deterministic tests.</summary>
    public sealed class MemoryProfileStore : IProfileStore
    {
        public string Main { get; private set; }
        public string Backup { get; private set; }
        public Task<string> ReadAsync(bool backup) => Task.FromResult(backup ? Backup : Main);
        public Task WriteAsync(string json) { Backup = Main; Main = json; return Task.CompletedTask; }
        public Task PreserveAndResetAsync(string json) => WriteAsync(json);
    }
}
