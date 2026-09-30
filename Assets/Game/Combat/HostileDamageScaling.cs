using Game.Content;

namespace Game.Combat
{
    /// <summary>
    /// Run-wide balance coefficient on damage that hostile entities (ordinary enemies, bosses, Travelers)
    /// deal to the player: GDD run setup `hostileDamageMultiplier`, DECISION-0075 / DECISION-0107.
    /// It scales the hostile hit itself, before the player's own damage reduction, and is never a
    /// character stat, so it is not shown as protection and does not stack through the reduction channel.
    /// </summary>
    public static class HostileDamageScaling
    {
        public static bool IsHostile(CombatEntityCategory category) =>
            category == CombatEntityCategory.OrdinaryEnemy || category == CombatEntityCategory.Boss ||
            category == CombatEntityCategory.Traveler;

        public static CombatDamageRequest Apply(CombatDamageRequest request, float multiplier)
        {
            NumericValidation.ValidatePositive(multiplier, nameof(multiplier));
            return IsHostile(request.Source.Owner.Category) && multiplier != 1f
                ? request.WithAmount(request.Amount * multiplier)
                : request;
        }
    }
}
