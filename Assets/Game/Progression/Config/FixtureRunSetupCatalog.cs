using System;
using Game.Content;
using Game.Content.Json;
using Game.Progression.Json;

namespace Game.Progression
{
    // Non-production run setup, config-driven instead of serialized scene fields: see
    // Resources/Content/Run/FixtureRunSetup.json and AGENTS.md's content-config rule.
    public static class FixtureRunSetupCatalog
    {
        private const string ResourcePath = "Content/Run/FixtureRunSetup";

        public static RunSetupConfig Create()
        {
            var data = JsonContentFile.Load<RunSetupConfigData>(ResourcePath);
            if (data == null)
                throw new InvalidOperationException("Run setup config is empty.");

            var draft = Require(data.Draft, "draft");
            var experience = Require(data.Experience, "experience");
            if (string.IsNullOrWhiteSpace(data.StartingCharacterId))
                throw new InvalidOperationException("Run setup 'startingCharacterId' must be set in config.");

            return new RunSetupConfig(
                new ContentId(data.StartingCharacterId),
                new DraftSettings(
                    Require(draft.OfferCount, "draft.offerCount"),
                    Require(draft.Seed, "draft.seed"),
                    Require(draft.InitialRerolls, "draft.initialRerolls"),
                    Require(draft.InitialBanishes, "draft.initialBanishes")),
                new ExperienceSettings(
                    Require(experience.BaseDropLifetimeSeconds, "experience.baseDropLifetimeSeconds"),
                    Require(experience.LevelThresholds, "experience.levelThresholds")));
        }

        private static T Require<T>(T? value, string description) where T : struct
        {
            if (!value.HasValue)
                throw new InvalidOperationException($"Run setup '{description}' must be set in config.");
            return value.Value;
        }

        private static T Require<T>(T value, string description) where T : class
        {
            if (value == null)
                throw new InvalidOperationException($"Run setup '{description}' must be set in config.");
            return value;
        }
    }
}
