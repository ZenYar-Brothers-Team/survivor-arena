using System;
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
        public MetaUnlock(MetaUnlockData data)
        {
            Id = new ContentId(data.Id).ToString();
            Name = string.IsNullOrWhiteSpace(data.Name) ? Id : data.Name;
            Kind = data.Kind;
            if (Kind != "character" && Kind != "field" && Kind != "skill" && Kind != "set" && Kind != "passive") throw new ArgumentException("Invalid unlock kind.");
            Condition = data.Condition;
            if (Condition != "initial" && Condition != "firstRun" && Condition != "fieldClear" && Condition != "access") throw new ArgumentException("Invalid unlock condition.");
            RequiredId = data.RequiredId;
            if (Condition == "fieldClear" || Condition == "access") new ContentId(RequiredId);
            else if (RequiredId != null) throw new ArgumentException("Unexpected unlock dependency.");
            Price = data.Price ?? throw new ArgumentException("price required.");
            NumericValidation.ValidateNonNegative(Price, nameof(Price));
            if (Condition == "initial" && Price != 0) throw new ArgumentException("Initial content cannot cost currency.");
        }
        public string Description => Condition == "initial" ? "Available from the start" :
            Condition == "firstRun" ? "Finish your first run (including Quit)" :
            Condition == "fieldClear" ? "Survive 15:00 on " + RequiredId : "Unlock " + RequiredId;
    }
}
