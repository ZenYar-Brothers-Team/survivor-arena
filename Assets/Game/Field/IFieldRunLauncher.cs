using Game.Content;
namespace Game.Field
{
    public interface IFieldRunLauncher
    {
        bool TryStartField(ContentId id);
        void BackToCharacters();
    }
}
