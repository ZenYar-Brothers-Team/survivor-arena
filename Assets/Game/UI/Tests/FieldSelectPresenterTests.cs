using Game.Content;
using Game.Field;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class FieldSelectPresenterTests
    {
        private FieldSelectTestHarness _harness;
        private FieldSelectionSession _session;
        private FieldSelectPresenter _presenter;
        [SetUp]
        public void SetUp()
        {
            _harness = new FieldSelectTestHarness();
            var fields = new[] { Make("FIXTURE-A", 2), Make("FIXTURE-B", 1) };
            _session = new FieldSelectionSession(new FieldRoster(fields, _harness), fields[0].Id, _harness);
            _presenter = new FieldSelectPresenter(_session, _harness);
        }
        private static FieldDefinition Make(string id, int difficulty) => new FieldDefinition(id, id,
            "Authored description", "Preview pending", difficulty, "Fixture unlock", "ENV", "WAVE", "BOSS", new ContentId[] { "ENEMY" });
        [TearDown] public void TearDown() => _presenter.Dispose();
        [Test]
        public void Presenter_AuthoredMetadataAndLockReason_AreVisible()
        {
            StringAssert.Contains("Authored description", _harness.Cards[0].Card.Summary);
            StringAssert.Contains("Difficulty: 2/", _harness.Cards[0].Card.Summary);
            StringAssert.Contains("Difficulty: 1/", _harness.Cards[1].Card.Summary);
            StringAssert.Contains("Fixture access required", _harness.Cards[1].Card.Summary);
            Assert.AreEqual("Preview pending", _harness.Cards[0].ThumbnailPlaceholder);
            Assert.IsTrue(_harness.Cards[1].Card.IsLocked);
            _harness.Select("FIXTURE-B");
            Assert.AreEqual(new ContentId("FIXTURE-A"), _session.SelectedId);
        }
        [Test]
        public void Start_AccessRevokedAfterSelection_DoesNotLaunch()
        {
            _harness.Locked = false; _harness.Select("FIXTURE-B");
            _harness.Locked = true; _harness.Start();
            Assert.AreEqual(0, _harness.Starts);
            Assert.IsFalse(_harness.CanStart);
            _harness.Locked = false; _harness.Start(); _harness.Start();
            Assert.AreEqual(1, _harness.Starts);
            Assert.AreEqual(new ContentId("FIXTURE-B"), _harness.StartedId);
        }
        [Test]
        public void Back_PreservesValidSelection_AndDisposeUnsubscribes()
        {
            _harness.Locked = false; _harness.Select("FIXTURE-B"); _harness.Back();
            Assert.AreEqual(1, _harness.Backs);
            Assert.AreEqual(new ContentId("FIXTURE-B"), _session.SelectedId);
            _presenter.Dispose(); _harness.Start(); _harness.Back(); _harness.Select("FIXTURE-A");
            Assert.AreEqual(0, _harness.Starts);
            Assert.AreEqual(1, _harness.Backs);
            Assert.AreEqual(new ContentId("FIXTURE-B"), _session.SelectedId);
        }
        [Test]
        public void Start_FailedLaunch_RemainsRetryable()
        {
            _harness.LaunchSucceeds = false; _harness.Start();
            Assert.IsFalse(_session.Started); Assert.IsTrue(_harness.CanStart);
            _harness.LaunchSucceeds = true; _harness.Start();
            Assert.IsTrue(_session.Started);
        }
        [Test]
        public void Uxml_SelectionActionsHaveSemanticIds()
        {
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/FieldSelect.uxml");
            Assert.IsNotNull(asset);
            var root = asset.CloneTree();
            Assert.IsNotNull(root.Q<VisualElement>(GameplayUiElementIds.FieldSelectCards));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.FieldSelectStart));
            Assert.IsNotNull(root.Q<Button>(GameplayUiElementIds.FieldSelectBack));
            Assert.IsNotNull(root.Q<ScrollView>());
        }
    }
}
