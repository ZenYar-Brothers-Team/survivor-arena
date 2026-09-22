using Game.Content;

namespace Game.Progression
{
    /// <summary>IP-25 boundary. Null means unlocked; a nonempty reason means locked.</summary>
    public interface ICharacterAccessProvider
    {
        string GetLockReason(ContentId characterId);
    }
}
