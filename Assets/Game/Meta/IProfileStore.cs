using System.Threading.Tasks;
namespace Game.Meta
{
    /// <summary>Only IO; callers validate/version documents before publishing a transaction.</summary>
    public interface IProfileStore
    {
        Task<string> ReadAsync(bool backup);
        Task WriteAsync(string json);
        Task PreserveAndResetAsync(string json);
    }
}
