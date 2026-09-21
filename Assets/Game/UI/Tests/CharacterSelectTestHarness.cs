using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
namespace Game.UI.Tests
{
    public sealed class CharacterSelectTestHarness : ICharacterSelectView, ICharacterRunLauncher, ICharacterAccessProvider
    {
        public event Action<ContentId> Selected;
        public event Action StartRequested;
        public IReadOnlyList<CharacterSelectCardViewState> Cards { get; private set; }
        public bool CanStart { get; private set; }
        public int Starts { get; private set; }
        public ContentId StartedId { get; private set; }
        public bool LockSecond { get; set; } = true;
        public bool LaunchSucceeds { get; set; } = true;
        public string GetLockReason(ContentId id) => LockSecond && id == new ContentId("FIXTURE-B") ? "Finish the fixture challenge" : null;
        public bool TryStartCharacter(ContentId id) { Starts++; StartedId = id; return LaunchSucceeds; }
        public void Render(IReadOnlyList<CharacterSelectCardViewState> cards, bool canStart) { Cards = cards; CanStart = canStart; }
        public void Select(ContentId id) => Selected?.Invoke(id);
        public void Start() => StartRequested?.Invoke();
    }
}
