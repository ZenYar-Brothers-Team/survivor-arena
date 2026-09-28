namespace Game.Character.Json
{
    public sealed class CharacterDraftWeightData
    {
        public string SkillId { get; set; }
        public float Weight { get; set; }
    }

    /// <summary>DECISION-0089: character draft weight of one passive item; omitted passives keep weight 1.</summary>
    public sealed class CharacterPassiveDraftWeightData
    {
        public string PassiveId { get; set; }
        public float Weight { get; set; }
    }
}
