using System.IO;
using System.Threading.Tasks;
using Game.Meta;

namespace Game.Bootstrap.PlayModeTests
{
    /// <summary>Fixture store: initial profile save succeeds, terminal receipt save fails.</summary>
    public sealed class FailAfterFirstProfileStore : IProfileStore
    {
        private string _main;
        private int _writes;
        public Task<string> ReadAsync(bool backup) => Task.FromResult(backup ? null : _main);
        public Task WriteAsync(string json)
        {
            if (++_writes > 1) return Task.FromException(new IOException("Fixture save failure"));
            _main = json;
            return Task.CompletedTask;
        }
        public Task PreserveAndResetAsync(string json) => WriteAsync(json);
    }
}
