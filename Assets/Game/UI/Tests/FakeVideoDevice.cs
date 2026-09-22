using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Settings;
namespace Game.UI.Tests
{
    public sealed class FakeVideoDevice : IVideoDevice
    {
        public VideoMode Current { get; private set; } = new VideoMode(1920,1080,true);
        public VideoMode Desktop => new VideoMode(1920,1080,true);
        public VideoMode SafeWindow => new VideoMode(1280,720,false);
        public IReadOnlyList<VideoMode> WindowModes => new[] { SafeWindow, new VideoMode(1920,1080,false) };
        public bool Background { get; private set; }
        public bool Fail { get; set; }
        public bool Supports(VideoMode m) => m.Equals(Desktop)||m.Equals(SafeWindow)||m.Equals(WindowModes[1]);
        public Task<bool> TryApplyAsync(VideoMode m) { if(Fail||!Supports(m))return Task.FromResult(false); Current=m; return Task.FromResult(true); }
        public void SetPreviewBackground(bool active) { Background=active; }
    }
}
