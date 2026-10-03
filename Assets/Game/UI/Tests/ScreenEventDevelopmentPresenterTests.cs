using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class ScreenEventDevelopmentPresenterTests
    {
        private sealed class FakeModel : IScreenEventDevelopmentControl
        {
            public IReadOnlyList<ScreenEventDevelopmentEntry> Entries { get; set; } = new[]
            {
                new ScreenEventDevelopmentEntry("E-1", "Копьё", false), new ScreenEventDevelopmentEntry("E-2", "Суд", true)
            };
            public float LaunchDelaySeconds => 2f;
            public bool Busy { get; set; }
            public string ActiveName { get; set; }
            public float ActiveRemainingSeconds { get; set; }
            public string QueuedName { get; set; }
            public float QueuedInSeconds { get; set; }
            public float Intensity { get; set; } = .5f;
            public float SecondsUntilNext { get; set; } = 17f;
            public int PlayerHitCount { get; set; } = 3;
            public int UnfairStartCount { get; set; }
            public readonly List<string> Queued = new List<string>();

            public bool TryQueue(string eventId)
            {
                Queued.Add(eventId);
                Busy = true;
                QueuedName = eventId;
                QueuedInSeconds = LaunchDelaySeconds;
                return true;
            }
        }

        private sealed class FakeView : IScreenEventDevelopmentView
        {
            public event Action<string> StartRequested;
            public IReadOnlyList<ScreenEventDevelopmentEntry> Entries;
            public string Summary = "";
            public bool LaunchEnabled;
            public bool Visible;
            public int EntryCalls;

            public void SetEntries(IReadOnlyList<ScreenEventDevelopmentEntry> entries) { Entries = entries; EntryCalls++; }
            public void Render(string summary, bool launchEnabled, bool visible) { Summary = summary; LaunchEnabled = launchEnabled; Visible = visible; }
            public void Press(string id) => StartRequested?.Invoke(id);
        }

        [Test]
        public void Refresh_ShowsButtonsOnceAndEnablesThemWhenIdle()
        {
            var view = new FakeView();
            var presenter = new ScreenEventDevelopmentPresenter(new FakeModel(), view, true);
            presenter.Refresh();
            presenter.Refresh();
            Assert.AreEqual(1, view.EntryCalls);
            Assert.AreEqual(2, view.Entries.Count);
            Assert.IsTrue(view.Visible);
            Assert.IsTrue(view.LaunchEnabled);
            StringAssert.Contains("до следующего по расписанию 17 с", view.Summary);
        }

        [Test]
        public void Press_QueuesThatEvent_AndDisablesTheButtonsWhileBusy()
        {
            var model = new FakeModel();
            var view = new FakeView();
            using var presenter = new ScreenEventDevelopmentPresenter(model, view, true);
            view.Press("E-2");
            CollectionAssert.AreEqual(new[] { "E-2" }, model.Queued);
            Assert.IsFalse(view.LaunchEnabled);
            StringAssert.Contains("Запуск через 2.0 с: E-2", view.Summary);
        }

        [Test]
        public void Summary_ShowsTheRunningEventAndTheCounters()
        {
            var model = new FakeModel { Busy = true, ActiveName = "Копьё", ActiveRemainingSeconds = 3.5f, UnfairStartCount = 1 };
            var view = new FakeView();
            using var presenter = new ScreenEventDevelopmentPresenter(model, view, true);
            StringAssert.Contains("Событие идёт: Копьё · ещё 3.5 с", view.Summary);
            StringAssert.Contains("Интенсивность волны: 0.50", view.Summary);
            StringAssert.Contains("Попаданий по игроку: 3", view.Summary);
            StringAssert.Contains("Нечестных стартов: 1", view.Summary);
        }

        [Test]
        public void NoModelOrNotDevelopment_HidesThePanelAndIgnoresPresses()
        {
            var view = new FakeView();
            using (new ScreenEventDevelopmentPresenter(null, view, true)) Assert.IsFalse(view.Visible);
            var model = new FakeModel();
            using (new ScreenEventDevelopmentPresenter(model, view, false))
            {
                Assert.IsFalse(view.Visible);
                view.Press("E-1");
            }
            Assert.IsEmpty(model.Queued);
        }

        [Test]
        public void Dispose_UnsubscribesFromTheView()
        {
            var model = new FakeModel();
            var view = new FakeView();
            new ScreenEventDevelopmentPresenter(model, view, true).Dispose();
            view.Press("E-1");
            Assert.IsEmpty(model.Queued);
        }
    }
}
