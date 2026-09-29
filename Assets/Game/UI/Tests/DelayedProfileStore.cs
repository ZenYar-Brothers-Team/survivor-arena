using System.Threading.Tasks;
using Game.Meta;

namespace Game.UI.Tests
{
    public sealed class DelayedProfileStore : IProfileStore
    {
        private string _json;
        private string _pending;
        private readonly TaskCompletionSource<bool> _writeGate = new TaskCompletionSource<bool>();

        public DelayedProfileStore(string json) => _json = json;

        public Task<string> ReadAsync(bool backup) => Task.FromResult(_json);

        public async Task WriteAsync(string json)
        {
            _pending = json;
            await _writeGate.Task;
            _json = _pending;
        }

        public Task PreserveAndResetAsync(string json) => WriteAsync(json);

        public void CompleteWrite() => _writeGate.TrySetResult(true);
    }
}
