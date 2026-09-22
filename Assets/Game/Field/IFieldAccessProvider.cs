using Game.Content;
namespace Game.Field
{
    /// <summary>Profile boundary: null means available; otherwise return a nonempty player-facing lock reason.</summary>
    public interface IFieldAccessProvider
    {
        string GetLockReason(ContentId id);
    }
}
