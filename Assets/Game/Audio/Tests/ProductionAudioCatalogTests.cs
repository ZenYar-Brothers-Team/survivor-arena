using Game.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Audio.Tests
{
    public sealed class ProductionAudioCatalogTests
    {
        [Test]
        public void ProductionAudio_ImportsAllReferences_AndUsesSharedBossTheme()
        {
            var catalog = ProductionAudioCatalog.Load();
            Assert.IsNotNull(catalog.MenuMusic);
            Assert.IsNotNull(catalog.BattleMusic);
            Assert.IsNotNull(catalog.BossMusic);
            Assert.IsNotNull(catalog.VictoryMusic);
            Assert.IsNotNull(catalog.DefeatMusic);
            Assert.IsTrue(catalog.TryGetAmbience("FIELD-001", out var ambience, out var gain));
            Assert.IsNotNull(ambience);
            Assert.AreEqual(.12f, gain);
            Assert.IsTrue(catalog.TryGet("boss.warning", out var warning, out var clips));
            Assert.AreEqual(2, warning.Priority.Value);
            Assert.AreEqual(1, clips.Length);
            Assert.IsTrue(catalog.TryGet("xp.pickup", out var xp, out _));
            Assert.GreaterOrEqual(xp.CooldownSeconds.Value, .15f);
            Assert.IsTrue(catalog.TryGet("enemy.hit", out var hit, out _));
            Assert.LessOrEqual(hit.Gain.Value, .2f);
        }

        [Test]
        public void EverySkill_HasItsOwnSoundFamily_AndSeveralFamiliesExist()
        {
            var catalog = ProductionAudioCatalog.Load();
            var families = new System.Collections.Generic.HashSet<string>();
            for (var skill = 1; skill <= 16; skill++)
            {
                Assert.IsTrue(catalog.TryGetSkillCue($"SKILL-{skill:000}", out var cue), $"SKILL-{skill:000} needs a cue.");
                Assert.IsTrue(catalog.TryGet(cue, out var data, out _), cue);
                Assert.AreEqual(0, data.Priority.Value, "Attack sounds are routine, never important.");
                families.Add(cue);
            }
            Assert.GreaterOrEqual(families.Count, 6, "Rough, blade, air, energy, fire and ice are heard apart.");
            Assert.IsFalse(catalog.TryGetSkillCue("SKILL-999", out _));
        }

        [Test]
        public void EnemyActionsAndZoneMoments_HaveCues_AndDangerousOnesAreImportant()
        {
            var catalog = ProductionAudioCatalog.Load();
            foreach (var id in new[] { "enemy.windup", "enemy.shot", "enemy.dash.windup", "enemy.dash", "boss.zone", "boss.beam",
                "boss.summon", "boss.teleport.windup", "boss.slam", "altar.on", "altar.cursed", "zone.activate", "shrine.reward",
                "zone.strike", "zone.burst", "zone.portal" })
                Assert.IsTrue(catalog.TryGet(id, out _, out _), id);
            foreach (var id in new[] { "enemy.dash.windup", "enemy.dash", "boss.zone", "boss.beam", "boss.summon", "boss.slam",
                "altar.on", "altar.cursed", "shrine.reward", "zone.strike", "zone.portal" })
            {
                catalog.TryGet(id, out var cue, out _);
                Assert.GreaterOrEqual(cue.Priority.Value, 1, id);
            }
        }

        [Test]
        public void LongTracks_Stream_WhileShortCueIsReadyInMemory()
        {
            foreach (var name in new[] { "menu_music", "battle_music", "boss_music", "victory_music",
                "defeat_music" })
            {
                var path = $"Assets/Resources/Audio/Music/{name}.ogg";
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                Assert.IsNotNull(importer, path);
                Assert.AreEqual(AudioClipLoadType.Streaming, importer.defaultSampleSettings.loadType, path);
            }
            var ambience = AssetImporter.GetAtPath("Assets/Resources/Audio/Ambience/FIELD-001/field_ambience.ogg") as AudioImporter;
            Assert.IsNotNull(ambience);
            Assert.AreEqual(AudioClipLoadType.Streaming, ambience.defaultSampleSettings.loadType);
            var cue = AssetImporter.GetAtPath("Assets/Resources/Audio/Sfx/ui_confirm.ogg") as AudioImporter;
            Assert.IsNotNull(cue);
            Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, cue.defaultSampleSettings.loadType);
        }

        [Test]
        public void FieldWithoutAmbienceBinding_HasNoFallback_AndKeepsSharedMusic()
        {
            var catalog = ProductionAudioCatalog.Load();
            Assert.IsFalse(catalog.TryGetAmbience("FIELD-002", out var clip, out var gain));
            Assert.IsNull(clip);
            Assert.AreEqual(0, gain);
            Assert.IsNotNull(catalog.BattleMusic);
            Assert.IsNotNull(catalog.BossMusic);
        }
    }
}
