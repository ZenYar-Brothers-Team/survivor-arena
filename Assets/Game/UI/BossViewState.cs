using System;

namespace Game.UI
{
    public readonly struct BossViewState
    {
        public Guid LifeId { get; }
        public string Name { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public bool Visible => LifeId != Guid.Empty && CurrentHealth > 0 && MaxHealth > 0;
        public BossViewState(Guid lifeId, string name, float currentHealth, float maxHealth)
        { LifeId = lifeId; Name = name; CurrentHealth = currentHealth; MaxHealth = maxHealth; }
    }
}
