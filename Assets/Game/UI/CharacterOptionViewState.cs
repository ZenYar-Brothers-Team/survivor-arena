using Game.Content;

namespace Game.UI
{
    public readonly struct CharacterOptionViewState
    {
        public ContentId Id { get; }
        public string Title { get; }
        public string StartingSkillId { get; }
        public float MaxHealth { get; }
        public float MovementSpeed { get; }
        public float ActiveSkillDamageMultiplier { get; }
        public float ActiveSkillCooldownMultiplier { get; }
        public float DisappearingXpRecovery { get; }
        public bool IsSelected { get; }

        public CharacterOptionViewState(
            ContentId id,
            string title,
            string startingSkillId,
            float maxHealth,
            float movementSpeed,
            float activeSkillDamageMultiplier,
            float activeSkillCooldownMultiplier,
            float disappearingXpRecovery,
            bool isSelected)
        {
            Id = id;
            Title = title;
            StartingSkillId = startingSkillId;
            MaxHealth = maxHealth;
            MovementSpeed = movementSpeed;
            ActiveSkillDamageMultiplier = activeSkillDamageMultiplier;
            ActiveSkillCooldownMultiplier = activeSkillCooldownMultiplier;
            DisappearingXpRecovery = disappearingXpRecovery;
            IsSelected = isSelected;
        }
    }
}
