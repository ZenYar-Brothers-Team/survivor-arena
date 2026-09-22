using Game.Content;
namespace Game.Progression
{
    public interface ICharacterRunLauncher
    {
        bool TryStartCharacter(ContentId id);
    }
}
