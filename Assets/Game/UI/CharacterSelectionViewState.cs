using System;
using System.Collections.Generic;

namespace Game.UI
{
    public sealed class CharacterSelectionViewState
    {
        public IReadOnlyList<CharacterOptionViewState> Characters { get; }

        public CharacterSelectionViewState(IReadOnlyList<CharacterOptionViewState> characters)
        {
            if (characters == null)
                throw new ArgumentNullException(nameof(characters));

            var copy = new CharacterOptionViewState[characters.Count];
            for (var i = 0; i < characters.Count; i++)
                copy[i] = characters[i];
            Characters = copy;
        }
    }
}
