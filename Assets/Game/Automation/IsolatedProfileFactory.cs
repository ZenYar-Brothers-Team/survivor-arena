using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Game.Meta;
using Game.Settings;

namespace Game.Automation
{
    /// <summary>One immutable template, cloned into independent memory stores for each campaign chain.</summary>
    public sealed class IsolatedProfileFactory
    {
        private readonly MetaCatalog _catalog;
        private readonly ProfileCodec _codec;
        private readonly string _initialJson;
        public string InitialProfileSha256 { get; }

        public IsolatedProfileFactory(MetaCatalog catalog, ExperimentConfig config)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (config == null) throw new ArgumentNullException(nameof(config));
            _codec = new ProfileCodec(catalog);
            _initialJson = _codec.Encode(_codec.Decode(config.InitialProfileJson));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(_initialJson));
                InitialProfileSha256 = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task<MemoryProfileStore> CreateChainStoreAsync()
        {
            var store = new MemoryProfileStore();
            await store.WriteAsync(_initialJson);
            return store;
        }

        public async Task<ProfileService> CreateChainProfileAsync()
        {
            var service = new ProfileService(_catalog, await CreateChainStoreAsync());
            await service.LoadAsync();
            if (!service.CanStart) throw new InvalidOperationException("Isolated chain profile failed to load: " + service.Message);
            return service;
        }

        /// <summary>Never reads or writes the production settings path.</summary>
        public MemorySettingsStore CreateSettingsStore() => new MemorySettingsStore();
    }
}
