using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    public sealed class MapPreviewPresenterTests
    {
        [Test]
        public void Presenter_StartsHidden_ToggleShowsTheMapAndFollowsTheCamera()
        {
            var source = new FakeMapPreviewSource(); var view = new FakeMapPreviewView();
            using var presenter = new MapPreviewPresenter(source, view, true);
            Assert.IsTrue(view.State.Available); Assert.IsFalse(view.State.Visible);
            view.Toggle();
            Assert.IsTrue(view.State.Visible);
            Assert.AreEqual(source.Arena, view.State.Arena);
            Assert.AreSame(source.Obstacles, view.State.Obstacles);
            StringAssert.Contains("120×120", view.State.Summary);
            StringAssert.Contains("препятствий: 1", view.State.Summary);
            StringAssert.DoesNotContain("дорог", view.State.Summary, "Fields without roads keep the previous summary.");
            source.View = new Rect(10f, 20f, 17.8f, 10f);
            presenter.Refresh();
            Assert.AreEqual(source.View, view.State.View, "The camera frame follows the player on every refresh.");
            view.Toggle();
            Assert.IsFalse(view.State.Visible);
        }

        [Test]
        public void Presenter_WithRoads_PassesThemToTheViewAndCountsThem()
        {
            var source = new FakeMapPreviewSource
            {
                Roads = new List<MapPreviewRoad>
                {
                    new MapPreviewRoad(new[] { new Vector2(-50f, 0f), new Vector2(50f, 0f) }, 10f, Color.blue),
                    new MapPreviewRoad(new[] { new Vector2(0f, 30f) }, 14f, Color.yellow)
                }
            };
            var view = new FakeMapPreviewView();
            using var presenter = new MapPreviewPresenter(source, view, true);
            view.Toggle();
            Assert.AreSame(source.Roads, view.State.Roads);
            StringAssert.Contains("участков дорог: 2", view.State.Summary);
        }

        [Test]
        public void Presenter_WithAltars_PassesActualRadiiAndRestingOffscreenMarkers()
        {
            var source = new FakeMapPreviewSource
            {
                Altars = new[]
                {
                    new MapPreviewAltar(new Vector2(-40, 30), 1.5f, Color.cyan, false, false),
                    new MapPreviewAltar(new Vector2(40, -30), 4.5f, Color.red, true, true)
                }
            };
            var view = new FakeMapPreviewView();
            using var presenter = new MapPreviewPresenter(source, view, true);
            view.Toggle();
            Assert.AreSame(source.Altars, view.State.Altars);
            Assert.AreEqual(1.5f, view.State.Altars[0].Radius);
            Assert.IsFalse(view.State.Altars[0].Active);
            StringAssert.Contains("алтарей: 2", view.State.Summary);
        }

        [Test]
        public void Presenter_WithoutDevelopmentOrSource_IsUnavailableAndIgnoresToggle()
        {
            var view = new FakeMapPreviewView();
            using (new MapPreviewPresenter(new FakeMapPreviewSource(), view, false))
            {
                view.Toggle();
                Assert.IsFalse(view.State.Available); Assert.IsFalse(view.State.Visible);
            }
            using (new MapPreviewPresenter(null, view, true))
            {
                view.Toggle();
                Assert.IsFalse(view.State.Available); Assert.IsFalse(view.State.Visible);
            }
        }

        [Test]
        public void Presenter_DisposeUnsubscribes()
        {
            var view = new FakeMapPreviewView();
            var presenter = new MapPreviewPresenter(new FakeMapPreviewSource(), view, true);
            presenter.Dispose();
            var renders = view.Renders;
            view.Toggle();
            Assert.AreEqual(renders, view.Renders);
        }

        [Test]
        public void MapPane_UsesSemanticIds_AndTheOverlayStaysWithinTheDevelopmentBudget()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var gameView = new UiToolkitGameplayView(root);
            using var view = new UiToolkitMapPreviewView(root);
            using var presenter = new MapPreviewPresenter(new FakeMapPreviewSource(), view, true);
            Assert.IsNotNull(root.Q<ScrollView>(GameplayUiElementIds.DevelopmentMapPane));
            var toggle = root.Q<Button>(GameplayUiElementIds.MapToggle);
            Assert.AreEqual("Показать карту", toggle.text);
            Assert.IsTrue(toggle.enabledSelf);
            var overlay = root.Q(GameplayUiElementIds.MapOverlay);
            Assert.AreEqual(PickingMode.Ignore, overlay.pickingMode, "The overlay never blocks gameplay input.");
            Assert.AreEqual(DisplayStyle.None, overlay.style.display.value, "The map starts hidden.");
        }
    }
}
