using System;
using Game.Content;

namespace Game.Progression
{
    // Everything a run needs that used to live in serialized scene fields: which
    // character starts, the draft parameters and the XP settings.
    public sealed class RunSetupConfig
    {
        public ContentId StartingCharacterId { get; }
        public DraftSettings Draft { get; }
        public ExperienceSettings Experience { get; }

        public RunSetupConfig(ContentId startingCharacterId, DraftSettings draft, ExperienceSettings experience)
        {
            if (!startingCharacterId.IsValid)
                throw new ArgumentException("Starting character id must be a valid content id.", nameof(startingCharacterId));

            StartingCharacterId = startingCharacterId;
            Draft = draft ?? throw new ArgumentNullException(nameof(draft));
            Experience = experience ?? throw new ArgumentNullException(nameof(experience));
        }
    }
}
