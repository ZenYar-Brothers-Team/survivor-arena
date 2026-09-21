using Game.Content;
using Game.Field;
using Game.Progression;
namespace Game.Meta
{
    public sealed class ProfileAccessProvider : ICharacterAccessProvider, IFieldAccessProvider
    {
        private readonly IProfileService _profile;
        public ProfileAccessProvider(IProfileService profile) { _profile = profile ?? throw new System.ArgumentNullException(nameof(profile)); }
        public string GetLockReason(ContentId id) => _profile.IsUnlocked(id.ToString()) ? null :
            _profile.Catalog.Unlocks.TryGetValue(id.ToString(), out var rule) ? rule.Description + (rule.Price > 0 ? " · " + rule.Price + " currency" : "") : "Unavailable content";
    }
}
