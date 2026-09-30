using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.UI.Tests
{
    // DECISION-0110: mid-boss HP above its head, only while on screen, never with numbers.
    public sealed class OverheadHealthPresenterTests
    {
        [Test]
        public void Refresh_OnScreenSourcesGetBars_OffscreenAndGoneRemoved()
        {
            var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view = new UiToolkitOverheadHealthView(root);
            var visible = Guid.NewGuid();
            var offscreen = Guid.NewGuid();
            var sources = new List<OverheadHealthSource>
            {
                new OverheadHealthSource(visible, new Vector3(.25f, .75f, 0f), .4f),
                new OverheadHealthSource(offscreen, new Vector3(1.5f, .5f, 0f), 1f)
            };
            var presenter = new OverheadHealthPresenter(() => sources, p => new Vector3(p.x, p.y, 1f), view);

            var overlay = root.Q(GameplayUiElementIds.OverheadHealthOverlay);
            Assert.AreEqual(1, overlay.childCount);
            var bar = overlay.Q<ProgressBar>(GameplayUiElementIds.OverheadHealthBar(visible));
            Assert.AreEqual(.4f, bar.value, 1e-5f);
            Assert.AreEqual("", bar.title, "No HP numbers.");
            Assert.AreEqual(25f, bar.style.left.value.value, 1e-3f);
            Assert.AreEqual(25f, bar.style.top.value.value, 1e-3f, "Screen y is flipped (y down).");

            sources.Clear();
            presenter.Refresh();
            Assert.AreEqual(0, overlay.childCount);
        }
    }
}
