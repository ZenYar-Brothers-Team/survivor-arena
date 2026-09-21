using System.Threading.Tasks;
namespace Game.Settings
{
    public interface ISettingsStore
    {
        Task<string> ReadAsync();
        Task PreserveAsync();
        Task WriteAsync(string text);
    }
}
