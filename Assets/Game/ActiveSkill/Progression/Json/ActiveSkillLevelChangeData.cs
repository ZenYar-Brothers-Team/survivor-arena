using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Game.ActiveSkill.Json
{
    /// <summary>Absolute replacements and additive percentages over unmodified base values (DECISION-0021).</summary>
    public sealed class ActiveSkillLevelChangeData
    {
        public Dictionary<string, JToken> Overrides { get; set; }
        public Dictionary<string, float> Bonuses { get; set; }
    }
}
