using System;
using System.Collections.Generic;
using Game.Content;
using Game.Progression;
using Game.ActiveSkill;
namespace Game.UI
{
    public sealed class CharacterSelectPresenter : IDisposable
    {
        private readonly CharacterSelectionSession _session;
        private readonly ContentRegistry _registry;
        private readonly ICharacterSelectView _view;
        private readonly Func<ContentId, string> _permanentSummary;
        private readonly Func<ContentId, IReadOnlyList<PermanentBonusRow>> _permanentRows;
        private ContentId _inspected;
        public CharacterSelectPresenter(CharacterSelectionSession session, ContentRegistry registry, ICharacterSelectView view, Func<ContentId, string> permanentSummary = null,
            Func<ContentId, IReadOnlyList<PermanentBonusRow>> permanentRows = null)
        {
            _permanentSummary = permanentSummary;
            _permanentRows = permanentRows;
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _inspected = session.SelectedId;
            _view.Selected += Select;
            _view.StartRequested += Start;
            Refresh();
        }
        public void Refresh()
        {
            var cards = new List<CharacterSelectCardViewState>();
            foreach (var character in _session.Roster.AllCharacters)
            {
                var presentation = character.Presentation ?? throw new InvalidOperationException("Selection requires presentation metadata.");
                var baseline = presentation.Baseline.Resolve(_registry);
                var reason = _session.Roster.GetLockReason(character.Id);
                var starting = character.ResolveStartingActiveSkill(_registry);
                var icon = starting is ActiveSkillProgressionDefinition active && active.Icon.Id.IsValid ? active.Icon.Resolve(_registry).Sprite : null;
                var highlights = new List<string>();
                foreach (var field in presentation.Highlights)
                    highlights.Add(CharacterHighlightFormatter.Format(character.BaseStats, baseline.Stats, field));
                var summary = presentation.Role + "\nStarts with " + character.ResolveStartingActiveSkill(_registry).DisplayName +
                    StartingSkillBoostText.Describe(character.StartingSkillBoost, character.StartingSkillMechanic);
                foreach (var field in presentation.Highlights)
                    summary += "\n" + CharacterHighlightFormatter.Format(character.BaseStats, baseline.Stats, field);
                if (_permanentSummary != null) summary += "\n" + _permanentSummary(character.Id);
                if (reason != null) summary += "\n" + reason;
                cards.Add(new CharacterSelectCardViewState(character.Id, new ContentCardViewState(
                    character.DisplayName, summary, summary, presentation.Icon.Resolve(_registry).Sprite,
                    !_session.Started, character.Id == _inspected, reason != null), presentation.Crop.Resolve(_registry).Sprite,
                    presentation.Role, character.ResolveStartingActiveSkill(_registry).DisplayName,
                    StartingSkillBoostText.Describe(character.StartingSkillBoost, character.StartingSkillMechanic).Trim(), string.Join("\n", highlights),
                    _permanentSummary?.Invoke(character.Id) ?? "", reason, icon, _permanentRows?.Invoke(character.Id)));
            }
            _view.Render(cards.AsReadOnly(), _session.CanStart && _inspected == _session.SelectedId);
        }
        private void Select(ContentId id)
        {
            if (_session.Started) return;
            foreach (var character in _session.Roster.AllCharacters)
                if (character.Id == id) { _inspected = id; _session.Select(id); break; }
            Refresh();
        }
        private void Start()
        {
            if (_inspected != _session.SelectedId || !_session.TryStart()) Refresh();
        }
        public void Dispose()
        {
            _view.Selected -= Select;
            _view.StartRequested -= Start;
        }
    }
}
