using Game.Settings;
namespace Game.UI.Tests
{
    public sealed class FakeAudioPreview : IAudioPreview
    {
        public int Stops;public void Preview(bool music){}public void StopPreviews(){Stops++;}
    }
}
