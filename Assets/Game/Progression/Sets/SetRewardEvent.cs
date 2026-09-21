using Game.Combat;
namespace Game.Progression
{
    /// <summary>Typed adapter boundary; real potion binding belongs to IP-28/IP-19.</summary>
    public readonly struct SetRewardEvent
    {
        public CombatSource Source { get; }
        public SetRewardEvent(CombatSource source) { Source = source; }
    }
}
