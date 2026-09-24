using Game.Combat;

namespace Game.Enemy
{
    /// <summary>Movement-control-only effects (aura slows) that deal no damage and create no hit feedback.</summary>
    public interface IEnemyControlReceiver
    {
        void ApplyControl(CombatDamageRequest request);
    }
}
