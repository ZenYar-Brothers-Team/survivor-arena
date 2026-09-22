using System;
using System.Collections.Generic;
using Game.Content;
using Game.Field;
namespace Game.UI.Tests
{
    public sealed class FieldSelectTestHarness : IFieldAccessProvider, IFieldRunLauncher, IFieldSelectView
    {
        public bool Locked = true;
        public bool LaunchSucceeds = true;
        public int Starts, Backs;
        public ContentId StartedId;
        public IReadOnlyList<FieldSelectCardViewState> Cards;
        public bool CanStart;
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public event Action BackRequested;
        public string GetLockReason(ContentId id) => Locked && id.ToString() == "FIXTURE-B" ? "Fixture access required" : null;
        public bool TryStartField(ContentId id) { Starts++; StartedId = id; return LaunchSucceeds; }
        public void BackToCharacters() => Backs++;
        public void Render(IReadOnlyList<FieldSelectCardViewState> cards, bool canStart) { Cards = cards; CanStart = canStart; }
        public void Select(ContentId id) => Selected?.Invoke(id);
        public void Start() => StartRequested?.Invoke();
        public void Back() => BackRequested?.Invoke();
    }
}
