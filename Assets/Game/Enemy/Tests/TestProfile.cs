namespace Game.Enemy.Tests
{
    /// <summary>Roundup profile builder shared by the raid tests; values mirror the production profile.</summary>
    internal static class TestProfile
    {
        public static RaidProfile Create(float minInterval = 60f, float maxInterval = 120f) => new RaidProfile(
            "RAID-TEST", 1f, 40, .5f, minInterval, maxInterval, 2f, .9f, 1f, 150, .8f, .6f, .15f,
            new RaidRingProfile(15f, .5f),
            new RaidWallProfile(20f, 4f, .25f, 1f),
            new RaidContractionProfile(20f, .6f, .18f),
            new RaidPincerProfile(20f, .8f, .8f, .15f));
    }
}
