using System.Collections.Generic;
using System.Threading.Tasks;
namespace Game.Settings
{
    public interface IVideoDevice
    {
        VideoMode Current { get; }
        VideoMode Desktop { get; }
        VideoMode SafeWindow { get; }
        IReadOnlyList<VideoMode> WindowModes { get; }
        bool Supports(VideoMode mode);
        Task<bool> TryApplyAsync(VideoMode mode);
        void SetPreviewBackground(bool active);
    }
}
