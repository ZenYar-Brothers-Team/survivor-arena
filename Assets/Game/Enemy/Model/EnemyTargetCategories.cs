using System;

namespace Game.Enemy
{
    [Flags]
    public enum EnemyTargetCategories
    {
        None = 0,
        Ordinary = 1,
        Boss = 2,
        Traveler = 4,
        All = Ordinary | Boss | Traveler
    }
}
