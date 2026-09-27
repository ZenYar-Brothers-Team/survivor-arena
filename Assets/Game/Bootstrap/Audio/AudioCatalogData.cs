namespace Game.Bootstrap.Audio
{
    public sealed class AudioCatalogData
    {
        public string MenuMusic;
        public string BattleMusic;
        public string BossMusic;
        public string VictoryMusic;
        public string DefeatMusic;
        public string FieldAmbience;
        public float? AmbienceGain;
        public float? RoutineGlobalCooldownSeconds;
        public AudioCueData[] Cues;
    }
}
