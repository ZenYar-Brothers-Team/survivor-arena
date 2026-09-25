using System;

namespace Game.Progression
{
    /// <summary>Fresh per-run seed (baseline v1 randomness.newRunSeedPolicy; DECISION-0057,
    /// playtest 2026-09-25_5233a664 OBS-02). Reference seeds in content JSON are only for
    /// reproducible comparisons; the resolved seed is recorded in playtest provenance.</summary>
    public static class FreshRunSeed
    {
        public static int Next() => Guid.NewGuid().GetHashCode();
    }
}
