using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    public sealed class DraftSession
    {
        private readonly PlayerBuild _build;

        public Guid Revision { get; } = Guid.NewGuid();
        public IReadOnlyList<DraftOption> Options { get; }
        public bool IsOpen { get; private set; } = true;

        public DraftSession(PlayerBuild build, IReadOnlyList<DraftOption> options)
        {
            _build = build ?? throw new ArgumentNullException(nameof(build));
            if (options == null) throw new ArgumentNullException(nameof(options));
            Options = new List<DraftOption>(options).AsReadOnly();
            if (options.Count == 0)
                throw new ArgumentException("An open draft requires at least one option.", nameof(options));
        }

        public void Cancel() => IsOpen = false;

        public bool TrySelect(ContentId id, out BuildSelectionResult result)
        {
            result = default;
            if (!IsOpen)
                return false;

            for (var i = 0; i < Options.Count; i++)
            {
                var option = Options[i];
                if (option.Definition.Id != id || !_build.IsEligible(option.Definition))
                    continue;

                var level = _build.TryGetEntry(id, out var current) ? current.Level : 0;
                if (level != option.Preview.CurrentLevel) return false;
                result = _build.Apply(option.Definition);
                IsOpen = false;
                return true;
            }

            return false;
        }
    }
}
