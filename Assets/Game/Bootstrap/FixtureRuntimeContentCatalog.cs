using Game.Traveler;
using System.Collections.Generic;
using System.Linq;
using Game.ActiveSkill;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Field;
using Game.Pickup;
using Game.Presentation;
using Game.Progression;
using Game.Content.Json;
using System.Collections.ObjectModel;

namespace Game.Bootstrap
{
    public sealed class FixtureRuntimeContentCatalog
    {
        // All content here is immutable once built, so it only needs parsing once
        // per game session (this static field resets on domain reload, i.e. once
        // per Editor Play session or once per real process launch) rather than
        // once per run/scene reload.
        private static FixtureRuntimeContentCatalog _cached;

        public ContentRegistry Registry { get; }
        public IReadOnlyList<BuildEntryDefinition> BuildEntries { get; }
        public IReadOnlyList<ActiveSkillProgressionDefinition> ActiveSkills { get; }
        public IReadOnlyList<PassiveProgressionDefinition> Passives { get; }
        public IReadOnlyList<SetDefinition> Sets { get; }
        public IReadOnlyList<EnemyDefinition> Enemies { get; }
        public IReadOnlyList<BossEncounterDefinition> Bosses { get; }
        public WaveTimelineDefinition WaveTimeline { get; }
        public RunSetupConfig RunSetup { get; }
        public CharacterRoster Characters { get; }
        public FixtureFieldCatalog Fields { get; }
        public FixturePickupCatalog Pickups { get; }
        public FixtureTravelerCatalog Travelers { get; }
        public IReadOnlyList<SpriteMotionProfile> SpriteMotionProfiles { get; }
        /// <summary>Exact resource bytes retained with the cached catalog, not re-read on later runs.</summary>
        public IReadOnlyDictionary<string, string> SourceSnapshot { get; }

        private FixtureRuntimeContentCatalog(
            ContentRegistry registry,
            IReadOnlyList<BuildEntryDefinition> buildEntries,
            IReadOnlyList<ActiveSkillProgressionDefinition> activeSkills,
            IReadOnlyList<PassiveProgressionDefinition> passives,
            IReadOnlyList<SetDefinition> sets,
            IReadOnlyList<EnemyDefinition> enemies,
            WaveTimelineDefinition waveTimeline,
            RunSetupConfig runSetup,
            CharacterRoster characters,
            IReadOnlyList<SpriteMotionProfile> spriteMotionProfiles,
            IReadOnlyList<BossEncounterDefinition> bosses, FixtureFieldCatalog fields, FixturePickupCatalog pickups, FixtureTravelerCatalog travelers)
        {
            RunSetup = runSetup;
            Registry = registry;
            BuildEntries = buildEntries;
            ActiveSkills = activeSkills;
            Passives = passives;
            Sets = sets;
            Enemies = enemies;
            Bosses = bosses;
            WaveTimeline = waveTimeline;
            Characters = characters;
            Fields = fields;
            Pickups = pickups; Travelers = travelers;
            SpriteMotionProfiles = spriteMotionProfiles;
            var sources = new Dictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var path in new[] { "Content/ActiveSkills/FixtureActiveSkills", "Content/Passives/FixturePassives",
                "Content/Sets/FixtureSets", "Content/Enemies/FixtureEnemies", "Content/Bosses/FixtureBosses", "Content/Waves/FixtureWaveTimeline",
                "Content/Run/FixtureRunSetup", "Content/Characters/FixtureCharacters", "Content/Characters/FixtureCharacterBaseline",
                "Content/Presentation/FixtureSpriteMotionProfiles", "Content/Presentation/FixtureSprites",
                "Content/Fields/FixtureFields", "Content/Waves/FixtureFieldWaveTimeline", "Content/Pickups/FixturePickups", "Content/Travelers/FixtureTravelers", "Content/Fields/FixtureArenaGeometry" })
                sources.Add(path, JsonContentFile.ReadText(path));
            SourceSnapshot = new ReadOnlyDictionary<string, string>(sources);
        }

        public static FixtureRuntimeContentCatalog Create()
        {
            if (_cached != null)
                return _cached;

            var activeSkills = FixtureActiveSkillCatalog.Create();
            var passives = FixturePassiveCatalog.Create();
            var sets = FixtureSetCatalog.Create();
            var enemies = FixtureEnemyCatalog.Create();
            var bosses = FixtureBossCatalog.Create(enemies);
            var waveTimeline = FixtureWaveTimelineCatalog.Create();
            var runSetup = FixtureRunSetupCatalog.Create();
            var characters = FixtureCharacterDefinitionCatalog.Create();
            var fields = FixtureFieldCatalog.Create();
            var pickups = FixturePickupCatalog.Create();
            var travelers = FixtureTravelerCatalog.Create();
            var fieldTimeline = FixtureWaveTimelineCatalog.FromJson(JsonContentFile.ReadText("Content/Waves/FixtureFieldWaveTimeline"));
            var spriteMotionProfiles = FixtureSpriteMotionProfileCatalog.Create();

            var buildEntries = new List<BuildEntryDefinition>(activeSkills.Count + passives.Count + sets.Count);
            var allDefinitions = new List<IContentDefinition>(buildEntries.Capacity + enemies.Count);
            for (var i = 0; i < activeSkills.Count; i++)
            {
                buildEntries.Add(activeSkills[i]);
                allDefinitions.Add(activeSkills[i]);
            }
            for (var i = 0; i < passives.Count; i++)
            {
                buildEntries.Add(passives[i]);
                allDefinitions.Add(passives[i]);
            }
            for (var i = 0; i < sets.Count; i++)
            {
                buildEntries.Add(sets[i]);
                allDefinitions.Add(sets[i]);
            }
            for (var i = 0; i < enemies.Count; i++)
                allDefinitions.Add(enemies[i]);
            allDefinitions.Add(waveTimeline);
            allDefinitions.Add(fieldTimeline);
            allDefinitions.AddRange(fields.Environments);
            allDefinitions.AddRange(travelers.Definitions.Values);
            allDefinitions.AddRange(travelers.Schedules);
            allDefinitions.AddRange(fields.Roster.AllFields);
            allDefinitions.Add(pickups);
            allDefinitions.AddRange(pickups.Definitions);
            allDefinitions.AddRange(bosses);
            allDefinitions.Add(FixtureCharacterDefinitionCatalog.CreateBaseline());
            for (var i = 0; i < characters.AllCharacters.Count; i++)
                allDefinitions.Add(characters.AllCharacters[i]);
            for (var i = 0; i < spriteMotionProfiles.Count; i++)
                allDefinitions.Add(spriteMotionProfiles[i]);

            // Backfill a placeholder sprite for every visual reference declared above,
            // so fixture content never has to remember to register one by hand; real
            // content (IP-17+) registers actual art here instead and the registry's
            // own reference validation catches anything still missing.
            var visualIds = allDefinitions
                .OfType<IReferencesContent>()
                .SelectMany(definition => definition.GetReferencedContent())
                .Where(reference => reference.ExpectedType == typeof(SpriteDefinition))
                .Select(reference => reference.Id);
            allDefinitions.AddRange(FixtureSpriteCatalog.CreateFor(visualIds));

            var registry = ContentRegistry.BuildFrom(allDefinitions);
            foreach (var field in fields.Roster.AllFields) field.Resolve(registry);
            for (var i = 0; i < characters.AllCharacters.Count; i++)
            {
                characters.AllCharacters[i].ResolveStartingActiveSkill(registry);
                characters.AllCharacters[i].ValidateDraftSkillReferences(registry);
            }

            _cached = new FixtureRuntimeContentCatalog(
                registry,
                buildEntries,
                activeSkills,
                passives,
                sets,
                enemies,
                waveTimeline,
                runSetup,
                characters,
                spriteMotionProfiles,
                bosses, fields, pickups, travelers);
            return _cached;
        }
    }
}
