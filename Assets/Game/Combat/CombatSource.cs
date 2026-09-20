using Game.Content;

namespace Game.Combat
{
    /// <summary>Immutable attribution retained by delayed attacks after the owner is gone.</summary>
    public readonly struct CombatSource
    {
        public CombatIdentity Owner { get; }
        public ContentId? ContentId { get; }
        public CombatSourceOrigin Origin { get; }
        public int? SkillLevel { get; }
        public CombatSource(CombatIdentity owner, ContentId? contentId, CombatSourceOrigin origin, int? skillLevel = null)
        {
            if (skillLevel.HasValue) NumericValidation.ValidateCount(skillLevel.Value, nameof(skillLevel));
            Owner = owner;
            ContentId = contentId;
            Origin = origin;
            SkillLevel = skillLevel;
        }
    }
}
