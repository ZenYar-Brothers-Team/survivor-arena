using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Game.Automation
{
    /// <summary>Validated experiment snapshot. Data returns a fresh copy so callers cannot alter a running experiment.</summary>
    public sealed class ExperimentConfig
    {
        private readonly string _json;
        public string OutputDirectory { get; }
        public string InitialProfilePath { get; }
        internal string InitialProfileJson { get; }
        public string ExperimentId { get; }
        public string InitialProfileSha256 { get; }
        public ExperimentConfigData Data => JsonConvert.DeserializeObject<ExperimentConfigData>(_json, ExperimentConfigLoader.JsonSettings);

        internal ExperimentConfig(ExperimentConfigData data, string outputDirectory, string initialProfilePath, string initialProfileJson)
        {
            _json = JsonConvert.SerializeObject(data, ExperimentConfigLoader.JsonSettings);
            OutputDirectory = outputDirectory;
            InitialProfilePath = initialProfilePath;
            InitialProfileJson = initialProfileJson;
            using (var sha = SHA256.Create())
                InitialProfileSha256 = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(initialProfileJson)))
                    .Replace("-", "").ToLowerInvariant();
            ExperimentId = data.ExperimentId;
        }

        public override string ToString() => _json;
    }
}
