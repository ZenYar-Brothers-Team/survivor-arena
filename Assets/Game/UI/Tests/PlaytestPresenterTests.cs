using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class PlaytestPresenterTests
    {
        [Test]
        public void Presenter_DisabledRejectsIntents_EnabledRoutesOnce_AndDisposeUnsubscribes()
        {
            var session = new FakePlaytestSession(); var view = new FakePlaytestView();
            var presenter = new PlaytestPresenter(session, view);
            view.Export(); view.Mark("hidden"); Assert.AreEqual(0, session.Exports); Assert.IsNull(session.Marker);
            session.Enabled = true; presenter.Refresh(); view.Mark("observed"); view.Export();
            Assert.AreEqual("observed", session.Marker); Assert.AreEqual(1, session.Exports);
            var renders = view.Renders; presenter.Refresh(); Assert.AreEqual(renders, view.Renders);
            presenter.Dispose(); view.Export(); Assert.AreEqual(1, session.Exports);
        }
        [Test]
        public void Playtest_UsesSemanticScrollablePane_AndHonestDisabledState()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var gameView = new UiToolkitGameplayView(root);
            using var view = new UiToolkitPlaytestView(root);
            using var presenter = new PlaytestPresenter(null, view);
            Assert.IsNotNull(root.Q<ScrollView>(GameplayUiElementIds.DevelopmentPlaytestPane));
            Assert.AreEqual(512, root.Q<TextField>(GameplayUiElementIds.PlaytestNote).maxLength);
            Assert.AreEqual("Recording disabled", root.Q<Label>(GameplayUiElementIds.PlaytestSummary).text);
            Assert.IsFalse(root.Q<Button>(GameplayUiElementIds.PlaytestExport).enabledSelf);
            gameView.SetDevelopmentControlsVisible(false);
            Assert.AreEqual(DisplayStyle.None, root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel).style.display.value);
            gameView.SetDevelopmentControlsVisible(true);
            Assert.AreEqual(DisplayStyle.None, root.Q<VisualElement>(GameplayUiElementIds.DevelopmentPanel).style.display.value);
        }
    }
}
