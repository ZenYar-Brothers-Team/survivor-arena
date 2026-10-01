using System;
using System.Collections.Generic;
using Game.Content;
namespace Game.Meta
{
    public sealed class MetaUnlock
    {
        public string Id { get; }
        public string Name { get; }
        public string Kind { get; }
        public string Condition { get; }
        public string RequiredId { get; }
        public long Price { get; }
        public string Metric { get; }
        public string TargetId { get; }
        public long TargetCount { get; }
        public IReadOnlyList<string> Grants { get; }
        public string ProgressKey => RequiredId + "|" + Metric + "|" + (TargetId ?? "-");
        public MetaUnlock(MetaUnlockData data)
        {
            Id = new ContentId(data.Id).ToString();
            Name = string.IsNullOrWhiteSpace(data.Name) ? Id : data.Name;
            Kind = data.Kind;
            if (Kind != "character" && Kind != "field" && Kind != "skill" && Kind != "set" && Kind != "passive") throw new ArgumentException("Invalid unlock kind.");
            Condition = data.Condition;
            if (Condition != "initial" && Condition != "firstRun" && Condition != "fieldClear" && Condition != "access" &&
                Condition != "achievement" && Condition != "fieldClearOrAchievement" && Condition != "dev") throw new ArgumentException("Invalid unlock condition.");
            RequiredId = data.RequiredId;
            if (Condition == "fieldClear" || Condition == "access" || Condition == "achievement" || Condition == "fieldClearOrAchievement") new ContentId(RequiredId);
            else if (RequiredId != null) throw new ArgumentException("Unexpected unlock dependency.");
            Metric = data.Metric;
            TargetId = data.TargetId;
            TargetCount = data.TargetCount ?? 0;
            Grants = Array.AsReadOnly(data.Grants ?? Array.Empty<string>());
            if (Grants.Count > 0 && Kind != "character") throw new ArgumentException("Only characters grant starting skills.");
            foreach (var grant in Grants) new ContentId(grant);
            if (Condition == "achievement" || Condition == "fieldClearOrAchievement")
            {
                if (Metric != "ordinaryKills" && Metric != "earnedGold" && Metric != "activeDamage" &&
                    Metric != "characterDamage" && Metric != "killsById" && Metric != "skillDamage")
                    throw new ArgumentException("Invalid achievement metric.");
                if (TargetCount <= 0) throw new ArgumentException("Achievement targetCount must be positive.");
                if (Metric == "killsById" || Metric == "skillDamage" || Metric == "characterDamage") new ContentId(TargetId);
                else if (TargetId != null) throw new ArgumentException("Unexpected achievement targetId.");
            }
            else if (Metric != null || TargetId != null || data.TargetCount.HasValue)
                throw new ArgumentException("Unexpected achievement data.");
            if (Condition == "dev" && Kind != "field") throw new ArgumentException("Only fields can be development-only.");
            Price = data.Price ?? throw new ArgumentException("price required.");
            NumericValidation.ValidateNonNegative(Price, nameof(Price));
            if ((Condition == "initial" || Condition == "dev") && Price != 0) throw new ArgumentException("Initial content cannot cost currency.");
            if ((Condition == "achievement" || Condition == "fieldClearOrAchievement") && Price != 0)
                throw new ArgumentException("Achievement unlock cannot cost currency.");
        }
        public string Description => Condition == "initial" ? "Available from the start" :
            Condition == "dev" ? "Development field, always available" :
            Condition == "firstRun" ? "Finish your first run (including Quit)" :
            Condition == "fieldClear" ? "Survive 15:00 on " + RequiredId :
            Condition == "achievement" ? "Earn achievement on " + RequiredId :
            Condition == "fieldClearOrAchievement" ? "Clear or earn achievement on " + RequiredId : "Unlock " + RequiredId;
    }
}
