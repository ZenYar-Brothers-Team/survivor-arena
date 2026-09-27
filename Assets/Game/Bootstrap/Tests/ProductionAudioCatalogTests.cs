using Game.Bootstrap.Audio;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Game.Bootstrap.Tests
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
            Assert.IsNotNull(catalog.FieldAmbience);
            Assert.IsTrue(catalog.TryGet("boss.warning", out var warning, out var clips));
            Assert.AreEqual(2, warning.Priority.Value);
            Assert.AreEqual(1, clips.Length);
            Assert.IsTrue(catalog.TryGet("xp.pickup", out var xp, out _));
            Assert.GreaterOrEqual(xp.CooldownSeconds.Value, .15f);
            Assert.IsTrue(catalog.TryGet("enemy.hit", out var hit, out _));
            Assert.LessOrEqual(hit.Gain.Value, .2f);
        }

        [Test]
        public void LongTracks_Stream_WhileShortCueIsReadyInMemory()
        {
            foreach (var name in new[] { "menu_music", "battle_music", "boss_music", "victory_music",
                "defeat_music", "field_ambience" })
            {
                var path = $"Assets/Resources/Audio/Field001/{name}.ogg";
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                Assert.IsNotNull(importer, path);
                Assert.AreEqual(AudioClipLoadType.Streaming, importer.defaultSampleSettings.loadType, path);
            }
            var cue = AssetImporter.GetAtPath("Assets/Resources/Audio/Field001/ui_confirm.ogg") as AudioImporter;
            Assert.IsNotNull(cue);
            Assert.AreEqual(AudioClipLoadType.DecompressOnLoad, cue.defaultSampleSettings.loadType);
        }
    }
}
