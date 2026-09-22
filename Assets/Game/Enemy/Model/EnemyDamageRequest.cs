using System;
using Game.Combat;
using Game.Content;

namespace Game.Enemy
{
    public readonly struct EnemyDamageRequest
    {
        public CombatDamageRequest Combat { get; }
        public ContentId SourceId => Combat.Source.ContentId ?? default;
        public float Amount => Combat.Amount;

        public EnemyDamageRequest(ContentId sourceId, float amount)
        {
            if (!sourceId.IsValid)
                throw new ArgumentException("Enemy damage requires a valid source content id.", nameof(sourceId));
            NumericValidation.ValidateNonNegative(amount, nameof(amount));

            Combat = new CombatDamageRequest(new CombatSource(default, sourceId, CombatSourceOrigin.Unknown), amount);
        }

        public EnemyDamageRequest(CombatDamageRequest combat) { Combat = combat; }
        public EnemyDamageRequest WithAmount(float amount) => new EnemyDamageRequest(Combat.WithAmount(amount));
        public EnemyDamageRequest WithDirection(float x, float y) => new EnemyDamageRequest(Combat.WithDirection(x, y));
    }
}
