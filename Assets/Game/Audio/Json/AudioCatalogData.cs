namespace Game.Audio.Json
{
    public sealed class AudioCatalogData
    {
        public string MenuMusic;
        public string BattleMusic;
        public string BossMusic;
        public string VictoryMusic;
        public string DefeatMusic;
        public AudioAmbienceData[] FieldAmbiences;
        public float? RoutineGlobalCooldownSeconds;
        public AudioCueData[] Cues;
    }
}
