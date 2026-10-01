using Game.Content;
using Game.Field;
using Game.Progression;
namespace Game.Meta
{
    public sealed class ProfileAccessProvider : ICharacterAccessProvider, IFieldAccessProvider
    {
        private readonly IProfileService _profile;
        public ProfileAccessProvider(IProfileService profile) { _profile = profile ?? throw new System.ArgumentNullException(nameof(profile)); }
        public string GetLockReason(ContentId id)
        {
            var key = id.ToString();
            if (_profile.IsUnlocked(key)) return null;
            if (!_profile.Catalog.Unlocks.TryGetValue(key, out var rule)) return "Unavailable content";
            // Development-only fields are available regardless of profile progress.
            if (rule.Condition == "dev") return null;
            if (rule.Kind == "field" && rule.Condition == "fieldClearOrAchievement" && rule.Metric == "ordinaryKills")
            {
                var previous = _profile.Catalog.Unlocks.TryGetValue(rule.RequiredId, out var field)
                    ? field.Name : rule.RequiredId;
                var progress = System.Math.Min(_profile.UnlockProgress(key), rule.TargetCount);
                return "Выжить 15:00 на «" + previous + "» или убить обычных врагов · " +
                    progress + "/" + rule.TargetCount;
            }
            return rule.Description + (rule.Price > 0 ? " · " + rule.Price + " currency" : "");
        }
    }
}
