using System;
using Game.Enemy;
using Game.Presentation;
using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class SlowStatusPresenterTests
    {
        [Test]
        public void Development_StyleAndSlowAllRequests_ReachModelAndRender()
        {
            var model = new FakePreview();
            var view = new FakeView();
            using var presenter = new SlowStatusPresenter(model, view, true);
            Assert.IsTrue(view.Development);
            Assert.AreEqual(SlowStatusStyle.Off, view.Style);

            view.RequestStyle(SlowStatusStyle.Outline);
            Assert.AreEqual(SlowStatusStyle.Outline, model.Style);
            Assert.AreEqual(SlowStatusStyle.Outline, view.Style);
            view.RequestSlowAll();
            Assert.AreEqual(1, model.SlowCalls);
            Assert.AreEqual("Замедлено врагов: 7", view.Summary);
        }

        [Test]
        public void NonDevelopment_HidesSectionAndIgnoresRequests()
        {
            var model = new FakePreview();
            var view = new FakeView();
            using var presenter = new SlowStatusPresenter(model, view, false);
            view.RequestStyle(SlowStatusStyle.All);
            view.RequestSlowAll();
            Assert.IsFalse(view.Development);
            Assert.AreEqual(SlowStatusStyle.Off, model.Style);
            Assert.AreEqual(0, model.SlowCalls);
        }

        private sealed class FakePreview : ISlowStatusPreview
        {
            public SlowStatusStyle Style { get; private set; }
            public int ShownCount => 0;
            public int SlowCalls;
            public event Action Changed;
            public void SetStyle(SlowStatusStyle style) { Style = style; Changed?.Invoke(); }
            public int SlowAllForPreview() { SlowCalls++; return 7; }
        }

        private sealed class FakeView : ISlowStatusView
        {
            public SlowStatusStyle Style;
            public bool Development;
            public string Summary;
            public event Action<SlowStatusStyle> StyleRequested;
            public event Action SlowAllRequested;
            public void Render(SlowStatusStyle style, bool development, string summary) { Style = style; Development = development; Summary = summary; }
            public void RequestStyle(SlowStatusStyle style) => StyleRequested?.Invoke(style);
            public void RequestSlowAll() => SlowAllRequested?.Invoke();
        }
    }
}
