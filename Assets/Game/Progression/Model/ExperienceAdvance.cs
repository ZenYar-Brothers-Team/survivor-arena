namespace Game.Progression
{
    internal readonly struct ExperienceAdvance
    {
        public int PreviousLevel { get; }
        public int Level { get; }
        public float PreviousExperience { get; }
        public float Experience { get; }
        public bool HasAward { get; }

        public ExperienceAdvance(int previousLevel, int level, float previousExperience, float experience, bool hasAward)
        {
            PreviousLevel = previousLevel;
            Level = level;
            PreviousExperience = previousExperience;
            Experience = experience;
            HasAward = hasAward;
        }
    }
}
