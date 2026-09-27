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
        /// <summary>
        /// DECISION-0075: global scale of all hostile damage the player takes (enemies, bosses, hostile Travelers),
        /// applied to the player's base incoming-damage multiplier; 1 leaves card values unchanged.
        /// </summary>
        public float HostileDamageMultiplier { get; }

        public RunSetupConfig(ContentId startingCharacterId, DraftSettings draft, ExperienceSettings experience,
            float hostileDamageMultiplier = 1f)
        {
            NumericValidation.ValidatePositive(hostileDamageMultiplier, nameof(hostileDamageMultiplier));
            if (!startingCharacterId.IsValid)
                throw new ArgumentException("Starting character id must be a valid content id.", nameof(startingCharacterId));

            StartingCharacterId = startingCharacterId;
            Draft = draft ?? throw new ArgumentNullException(nameof(draft));
            Experience = experience ?? throw new ArgumentNullException(nameof(experience));
            HostileDamageMultiplier = hostileDamageMultiplier;
        }
    }
}
