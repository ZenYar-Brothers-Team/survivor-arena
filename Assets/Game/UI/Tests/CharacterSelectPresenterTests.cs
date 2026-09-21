using System;
using System.Linq;
using Game.Character;
using Game.Character.Json;
using Game.Content;
using Game.Presentation;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;
namespace Game.UI.Tests
{
    public sealed class CharacterSelectPresenterTests
    {
        private Texture2D _texture;
        private Sprite _sprite;
        private CharacterDefinition _a, _b;
        private CharacterComparisonBaseline _baseline;
        private ContentRegistry _registry;
        private CharacterSelectTestHarness _harness;
        private CharacterSelectionSession _session;
        private CharacterSelectPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _texture = new Texture2D(2, 2);
            _sprite = Sprite.Create(_texture, new Rect(0, 0, 2, 2), Vector2.zero);
            var visual = new SpriteDefinition("FIXTURE-VISUAL", _sprite);
            var skill = new BuildEntryDefinition("FIXTURE-SKILL", BuildEntryKind.ActiveSkill, "Test skill");
            _baseline = new CharacterComparisonBaseline("FIXTURE-BASELINE", new CharacterBaseStats(100, 4));
            _a = Make("FIXTURE-A", new CharacterBaseStats(100, 4, activeSkillDamageMultiplier: 9), Array.Empty<CharacterStatField>());
            _b = Make("FIXTURE-B", new CharacterBaseStats(125, 3, disappearingXpRecovery: .15f),
                new[] { CharacterStatField.MovementSpeed, CharacterStatField.MaxHealth, CharacterStatField.DisappearingXpRecovery });
            _registry = ContentRegistry.BuildFrom(new IContentDefinition[] { skill, visual, _baseline, _a, _b });
            _harness = new CharacterSelectTestHarness();
            _session = new CharacterSelectionSession(new CharacterRoster(new[] { _a, _b }, _harness), _a.Id, _harness);
            _presenter = new CharacterSelectPresenter(_session, _registry, _harness);
        }
        private CharacterDefinition Make(string id, CharacterBaseStats stats, CharacterStatField[] fields) =>
            new CharacterDefinition(id, id, stats, "FIXTURE-SKILL", default, default,
                new CharacterPresentation("Test role", _baseline.Id, "FIXTURE-VISUAL", "FIXTURE-VISUAL", fields));
        [TearDown]
        public void TearDown()
        {
            _presenter?.Dispose();
            UnityEngine.Object.DestroyImmediate(_sprite);
            UnityEngine.Object.DestroyImmediate(_texture);
        }
        [Test]
        public void Presenter_ShowsRoleSkillResolvedArtAndLockReason()
        {
            Assert.AreEqual(2, _harness.Cards.Count);
            var card = _harness.Cards[1];
            Assert.IsTrue(card.Card.IsLocked);
            StringAssert.Contains("Finish the fixture challenge", card.Card.Summary);
            StringAssert.Contains("Test role", card.Card.Summary);
            StringAssert.Contains("Starts with Test skill", card.Card.Summary);
            Assert.AreSame(_sprite, card.Card.Icon);
            Assert.AreSame(_sprite, card.Crop);
            _harness.Select(_b.Id);
            Assert.AreEqual(_a.Id, _session.SelectedId);
        }
        [Test]
        public void Highlights_KeepOrderAndDeriveNumbersFromExplicitBaseline()
        {
            var text = _harness.Cards[1].Card.Summary;
            StringAssert.Contains("Movement Speed: -25%", text);
            StringAssert.Contains("Max Health: +25%", text);
            StringAssert.Contains("Disappearing Xp Recovery: +15 pp", text);
            Assert.Less(text.IndexOf("Movement Speed", StringComparison.Ordinal), text.IndexOf("Max Health", StringComparison.Ordinal));
            Assert.IsFalse(text.Contains("Damage"));
        }
        [Test]
        public void EmptyHighlights_DoNotAutoFillEvenWithLargeDeviation()
        {
            Assert.AreEqual("Test role\nStarts with Test skill", _harness.Cards[0].Card.Summary);
        }
        [Test]
        public void ReorderAndSelect_DoNotChangeComparisonBaseline()
        {
            _harness.LockSecond = false;
            _presenter.Refresh();
            var original = _harness.Cards[1].Card.Summary;
            var other = new CharacterSelectTestHarness { LockSecond = false };
            var session = new CharacterSelectionSession(new CharacterRoster(new[] { _b, _a }, other), _b.Id, other);
            using var presenter = new CharacterSelectPresenter(session, _registry, other);
            Assert.AreEqual(original, other.Cards[0].Card.Summary);
            other.Select(_a.Id);
            Assert.AreEqual(original, other.Cards[0].Card.Summary);
        }
        [Test]
        public void Start_RechecksAccessAfterSelectionAndNeverLaunchesLockedCharacter()
        {
            _harness.LockSecond = false;
            _harness.Select(_b.Id);
            _harness.LockSecond = true;
            _harness.Start();
            Assert.AreEqual(0, _harness.Starts);
            Assert.IsFalse(_harness.CanStart);
            _harness.LockSecond = false;
            _harness.Start();
            Assert.AreEqual(_b.Id, _harness.StartedId);
            _harness.Start();
            Assert.AreEqual(1, _harness.Starts);
        }
        [Test]
        public void FailedLaunch_KeepsSelectionAvailableForRetry()
        {
            _harness.LaunchSucceeds = false;
            _harness.Start();
            Assert.IsFalse(_session.Started);
            Assert.IsTrue(_harness.CanStart);
            _harness.LaunchSucceeds = true;
            _harness.Start();
            Assert.IsTrue(_session.Started);
        }
        [Test]
        public void Dispose_UnsubscribesViewIntents()
        {
            _presenter.Dispose();
            _harness.Start();
            Assert.AreEqual(0, _harness.Starts);
        }
        [TestCase("UnknownStat")]
        [TestCase("999")]
        [TestCase("0")]
        public void Presentation_InvalidFieldRejected(string field)
        {
            Assert.Throws<InvalidOperationException>(() => FixtureCharacterDefinitionCatalog.MapPresentation(new CharacterPresentationData
            {
                Role = "Role", BaselineId = "FIXTURE-BASELINE", CropId = "FIXTURE-VISUAL", IconId = "FIXTURE-VISUAL", Highlights = new[] { field }
            }));
        }
        [Test]
        public void Presentation_MissingBaselineAndDuplicateFieldsRejected()
        {
            Assert.Throws<ArgumentException>(() => new CharacterPresentation("Role", default, "FIXTURE-VISUAL", "FIXTURE-VISUAL", Array.Empty<CharacterStatField>()));
            Assert.Throws<ArgumentException>(() => new CharacterPresentation("Role", _baseline.Id, "FIXTURE-VISUAL", "FIXTURE-VISUAL",
                new[] { CharacterStatField.MaxHealth, CharacterStatField.MaxHealth }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterComparisonBaseline("FIXTURE-INVALID", default));
        }
        [Test]
        public void Registry_RejectsMissingBaselineReference()
        {
            var skill = new BuildEntryDefinition("FIXTURE-SKILL", BuildEntryKind.ActiveSkill, "Test skill");
            Assert.Throws<ContentValidationException>(() => ContentRegistry.BuildFrom(new IContentDefinition[]
                { skill, new SpriteDefinition("FIXTURE-VISUAL", _sprite), _a }));
        }
        [Test]
        public void Roster_AllLockedDoesNotInventUnlockedCharacter()
        {
            var roster = new CharacterRoster(new[] { _b }, _harness);
            Assert.AreEqual(0, roster.UnlockedCharacters.Count);
            var session = new CharacterSelectionSession(roster, _b.Id, _harness);
            Assert.IsFalse(session.TryStart());
            Assert.AreEqual(0, _harness.Starts);
        }
    }
}
