using System.Collections.Generic;
using Game.Character;
using Game.Content;
using Game.Movement;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Progression.Tests
{
    public sealed class CharacterFrameworkTests
    {
        [Test]
        public void FixtureRoster_HasTwoDistinctCharactersAndRejectsLockedSelection()
        {
            var roster = FixtureCharacterDefinitionCatalog.Create();

            Assert.AreEqual(2, roster.AllCharacters.Count);
            Assert.AreEqual(1, roster.UnlockedCharacters.Count);
            var agile = roster.AllCharacters[0];
            var sturdy = roster.AllCharacters[1];
            Assert.AreNotEqual(agile.BaseStats.MaxHealth, sturdy.BaseStats.MaxHealth);
            Assert.AreNotEqual(agile.BaseStats.MovementSpeed, sturdy.BaseStats.MovementSpeed);
            Assert.AreNotEqual(agile.BaseStats.DisappearingXpRecovery, sturdy.BaseStats.DisappearingXpRecovery);
            Assert.AreNotEqual(agile.StartingActiveSkill.Id, sturdy.StartingActiveSkill.Id);
            Assert.AreNotEqual(
                agile.GetDraftWeight("FIXTURE-SKILL-BOLT"),
                sturdy.GetDraftWeight("FIXTURE-SKILL-BOLT"));
            Assert.IsTrue(roster.TrySelect(agile.Id, out var selected));
            Assert.AreSame(agile, selected);
            Assert.IsFalse(roster.TrySelect(sturdy.Id, out _));
            Assert.IsTrue(agile.Visual.Id.IsValid);
            Assert.IsTrue(agile.MotionProfile.Id.IsValid);
            Assert.IsTrue(sturdy.Visual.Id.IsValid);
            Assert.IsTrue(sturdy.MotionProfile.Id.IsValid);
        }

        [Test]
        public void CharacterVisualAndMotionReferences_AreTypeValidatedByRegistry()
        {
            var starting = Active("FIXTURE-ACTIVE-START");
            var motion = Motion("FIXTURE-MOTION");
            var character = new CharacterDefinition(
                "FIXTURE-CHARACTER",
                "Fixture Character",
                new CharacterBaseStats(100f, 3f),
                starting.Id,
                new ContentRef<SpriteDefinition>(motion.Id),
                new ContentRef<SpriteMotionProfile>(motion.Id));

            Assert.Throws<ContentValidationException>(() =>
                ContentRegistry.BuildFrom(new IContentDefinition[] { starting, motion, character }));

            var visual = new SpriteDefinition("FIXTURE-VISUAL", PlaceholderSprite.Shared);
            var valid = new CharacterDefinition(
                "FIXTURE-CHARACTER-VALID",
                "Fixture Character Valid",
                new CharacterBaseStats(100f, 3f),
                starting.Id,
                new ContentRef<SpriteDefinition>(visual.Id),
                new ContentRef<SpriteMotionProfile>(motion.Id));

            Assert.DoesNotThrow(() =>
                ContentRegistry.BuildFrom(new IContentDefinition[] { starting, visual, motion, valid }));
        }

        [Test]
        public void CharacterStartingSkill_ResolvesIntoFirstOccupiedActiveSlot()
        {
            var starting = Active("FIXTURE-ACTIVE-START");
            var other = Active("FIXTURE-ACTIVE-OTHER");
            var character = Character("FIXTURE-CHARACTER", starting.Id);
            var registry = ContentRegistry.BuildFrom(new IContentDefinition[] { starting, other, character });

            var build = new PlayerBuild(character.ResolveStartingActiveSkill(registry));

            Assert.AreEqual(1, build.ActiveCount);
            Assert.IsTrue(build.TryGetEntry(starting.Id, out var entry));
            Assert.AreEqual(1, entry.Level);
        }

        [Test]
        public void SeededWeightedDraft_IsReproducibleAndZeroWeightNeverAppears()
        {
            var starting = Active("FIXTURE-ACTIVE-START");
            var excluded = Active("FIXTURE-ACTIVE-ZERO");
            var favored = Active("FIXTURE-ACTIVE-FAVORED");
            var passive = Passive("FIXTURE-PASSIVE");
            var character = Character(
                "FIXTURE-CHARACTER",
                starting.Id,
                new CharacterDraftWeight(starting.Id, 0.25f),
                new CharacterDraftWeight(excluded.Id, 0f),
                new CharacterDraftWeight(favored.Id, 5f));
            var pool = new DraftPool(
                new BuildEntryDefinition[] { starting, excluded, favored, passive },
                character);
            var build = new PlayerBuild(starting);

            var first = pool.CreateOptions(build, 2, new SeededDraftRandom(12345));
            var second = pool.CreateOptions(build, 2, new SeededDraftRandom(12345));

            Assert.AreEqual(first.Count, second.Count);
            for (var i = 0; i < first.Count; i++)
            {
                Assert.AreEqual(first[i].Definition.Id, second[i].Definition.Id);
                Assert.AreNotEqual(excluded.Id, first[i].Definition.Id);
            }
        }

        [Test]
        public void DifferentCharacterWeights_ChangeDeterministicDraftResult()
        {
            var starting = Active("FIXTURE-ACTIVE-START");
            var alternate = Active("FIXTURE-ACTIVE-ALTERNATE");
            var definitions = new BuildEntryDefinition[] { starting, alternate };
            var startingFavored = Character(
                "FIXTURE-CHARACTER-START",
                starting.Id,
                new CharacterDraftWeight(starting.Id, 10f),
                new CharacterDraftWeight(alternate.Id, 1f));
            var alternateFavored = Character(
                "FIXTURE-CHARACTER-ALTERNATE",
                starting.Id,
                new CharacterDraftWeight(starting.Id, 1f),
                new CharacterDraftWeight(alternate.Id, 10f));
            var build = new PlayerBuild(starting);

            var first = new DraftPool(definitions, startingFavored)
                .CreateOptions(build, 1, new FixedDraftRandom(0.4f));
            var second = new DraftPool(definitions, alternateFavored)
                .CreateOptions(build, 1, new FixedDraftRandom(0.4f));

            Assert.AreEqual(starting.Id, first[0].Definition.Id);
            Assert.AreEqual(alternate.Id, second[0].Definition.Id);
        }

        [Test]
        public void PassiveModifier_ComposesOnSelectedCharacterBaseStats()
        {
            var character = Character(
                "FIXTURE-CHARACTER",
                "FIXTURE-ACTIVE-START",
                maxHealth: 120f,
                movementSpeed: 2.5f,
                recovery: 0.15f);
            var stats = new CharacterStats(character.BaseStats);

            stats.SetModifier("FIXTURE-PASSIVE", new CharacterStatModifier(
                maxHealthMultiplierBonus: 0.25f,
                movementSpeedMultiplierBonus: 0.2f,
                disappearingXpRecoveryBonus: 0.1f));

            Assert.AreEqual(150f, stats.MaxHealth, 0.0001f);
            Assert.AreEqual(3f, stats.MovementSpeed, 0.0001f);
            Assert.AreEqual(0.25f, stats.DisappearingXpRecovery, 0.0001f);
        }

        private static CharacterDefinition Character(
            string id,
            ContentId startingSkillId,
            params CharacterDraftWeight[] weights)
        {
            return Character(id, startingSkillId, 100f, 3f, 0f, weights);
        }

        private static CharacterDefinition Character(
            string id,
            ContentId startingSkillId,
            float maxHealth,
            float movementSpeed,
            float recovery,
            params CharacterDraftWeight[] weights)
        {
            return new CharacterDefinition(
                id,
                id,
                new CharacterBaseStats(maxHealth, movementSpeed, disappearingXpRecovery: recovery),
                startingSkillId,
                weights);
        }

        private static BuildEntryDefinition Active(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.ActiveSkill, id);
        }

        private static BuildEntryDefinition Passive(string id)
        {
            return new BuildEntryDefinition(id, BuildEntryKind.PassiveItem, id);
        }

        private static SpriteMotionProfile Motion(string id)
        {
            return new SpriteMotionProfile(
                id,
                3f,
                0.025f,
                0.675f,
                0.035f,
                1.4f,
                0.05f,
                3f,
                0.06f,
                6f,
                0.14f,
                0.1f,
                7f,
                0.08f,
                Color.red,
                0.2f,
                0.75f);
        }
    }
}
