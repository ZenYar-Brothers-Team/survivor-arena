using System;
using System.Collections.Generic;
using Game.Enemy;
using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class RaidPresenterTests
    {
        private sealed class FakeModel : IRaidDevelopmentControl
        {
            public IReadOnlyList<RaidTemplateKind> Templates { get; set; } = new[] { RaidTemplateKind.Ring, RaidTemplateKind.Wall };
            public int TotalEnemyCount { get; set; } = 240;
            public int NearbyEnemyCount { get; set; } = 130;
            public int TriggerEnemyCount => 100;
            public bool RaidActive { get; set; }
            public RaidTemplateKind ActiveTemplate { get; set; } = RaidTemplateKind.Ring;
            public float RaidRemainingSeconds { get; set; }
            public float NextRaidSeconds { get; set; } = 75f;
            public bool CountdownRunning { get; set; } = true;
            public List<RaidTemplateKind> Started { get; } = new List<RaidTemplateKind>();

            public bool TryStartRaid(RaidTemplateKind kind)
            {
                Started.Add(kind);
                RaidActive = true;
                RaidRemainingSeconds = 20f;
                return true;
            }
        }

        private sealed class FakeView : IRaidView
        {
            public event Action<RaidTemplateKind> StartRequested;
            public IReadOnlyList<RaidTemplateKind> Templates;
            public string Summary = "";
            public bool LaunchEnabled;
            public bool Development;

            public void SetTemplates(IReadOnlyList<RaidTemplateKind> templates) => Templates = templates;

            public void Render(string summary, bool launchEnabled, bool development)
            {
                Summary = summary;
                LaunchEnabled = launchEnabled;
                Development = development;
            }

            public void Press(RaidTemplateKind kind) => StartRequested?.Invoke(kind);
        }

        [Test]
        public void Refresh_ShowsCountersAndTimerState()
        {
            var view = new FakeView();
            using var presenter = new RaidPresenter(new FakeModel(), view, true);

            StringAssert.Contains("Врагов на карте: 240", view.Summary);
            StringAssert.Contains("Врагов в радиусе экрана: 130", view.Summary);
            StringAssert.Contains("идёт", view.Summary);
            Assert.IsTrue(view.Development);
            Assert.IsTrue(view.LaunchEnabled);
            Assert.AreEqual(2, view.Templates.Count);
        }

        [Test]
        public void Press_StartsTheTemplateAndShowsRemainingTime()
        {
            var model = new FakeModel();
            var view = new FakeView();
            using var presenter = new RaidPresenter(model, view, true);

            view.Press(RaidTemplateKind.Wall);

            CollectionAssert.AreEqual(new[] { RaidTemplateKind.Wall }, model.Started);
            StringAssert.Contains("Облава: ДА", view.Summary);
            StringAssert.Contains("20.0", view.Summary);
            Assert.IsFalse(view.LaunchEnabled, "A second launch is blocked while a roundup runs.");
        }

        [Test]
        public void Refresh_WithoutDevelopmentMode_HidesThePanelAndIgnoresPresses()
        {
            var model = new FakeModel();
            var view = new FakeView();
            using var presenter = new RaidPresenter(model, view, false);

            view.Press(RaidTemplateKind.Ring);

            Assert.IsFalse(view.Development);
            Assert.IsEmpty(model.Started);
        }

        [Test]
        public void Refresh_StoppedCountdown_SaysThereIsNoCrowd()
        {
            var view = new FakeView();
            using var presenter = new RaidPresenter(new FakeModel { CountdownRunning = false }, view, true);

            StringAssert.Contains("стоит", view.Summary);
        }

        [Test]
        public void Dispose_UnsubscribesFromTheView()
        {
            var model = new FakeModel();
            var view = new FakeView();
            var presenter = new RaidPresenter(model, view, true);
            presenter.Dispose();

            view.Press(RaidTemplateKind.Ring);

            Assert.IsEmpty(model.Started);
        }
    }
}
