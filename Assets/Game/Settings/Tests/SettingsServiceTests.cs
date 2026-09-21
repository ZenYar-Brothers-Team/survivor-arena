using System;
using System.Threading.Tasks;
using NUnit.Framework;
namespace Game.Settings.Tests
{
    public sealed class SettingsServiceTests
    {
        [Test] public async Task Defaults_MissingFile_PersistsAndRoutesChannels()
        {
            var store=new MemorySettingsStore(); var s=new SettingsService(SettingsConfig.Load(),store,new FakeVideoDevice()); await s.LoadAsync();
            Assert.AreEqual(.64f,s.Current.Gain(false),.0001f); Assert.AreEqual(.48f,s.Current.Gain(true),.0001f);
            Assert.IsTrue(s.Current.Shake); Assert.IsFalse(s.Dirty); Assert.AreEqual(s.Current.Video,SettingsCodec.Decode(store.Text).Video);
            s.SetAudio(0,1,1);Assert.AreEqual(0,s.Current.Gain(true));Assert.AreEqual(0,s.Current.Gain(false));
        }
        [TestCase("{}")] [TestCase("{broken")] [TestCase("{\"schemaVersion\":99}")]
        public async Task Load_InvalidDocument_PreservesBeforeReplacing(string text)
        {
            var store=new FaultSettingsStore {Text=text,FailPreserve=true}; var s=new SettingsService(SettingsConfig.Load(),store,new FakeVideoDevice());await s.LoadAsync();
            Assert.AreEqual(text,store.Text);Assert.AreEqual(0,store.Writes);Assert.IsTrue(s.Dirty);
            store.FailPreserve=false;await s.SaveAsync();Assert.AreEqual(2,store.Preserves);Assert.AreEqual(1,store.Writes);Assert.IsFalse(s.Dirty);
        }
        [Test] public async Task Preview_Timeout_RevertsWithoutPersistingCandidate()
        {
            var store=new MemorySettingsStore();var video=new FakeVideoDevice();var s=new SettingsService(SettingsConfig.Load(),store,video);await s.LoadAsync();
            s.Tick(100);s.SetCandidate(video.SafeWindow);await s.ApplyVideoAsync();Assert.IsTrue(video.Background);Assert.AreEqual(video.SafeWindow,video.Current);
            s.SetAudio(.2f,.3f,.4f);await s.SaveAsync();Assert.AreEqual(video.Desktop,SettingsCodec.Decode(store.Text).Video);
            s.Tick(110);Assert.AreEqual(video.Desktop,video.Current);Assert.IsFalse(video.Background);Assert.AreEqual(VideoPreviewState.Idle,s.PreviewState);
        }
        [Test] public async Task Preview_Keep_PersistsConfirmedModeAndLatestAudio()
        {
            var store=new MemorySettingsStore();var video=new FakeVideoDevice();var s=new SettingsService(SettingsConfig.Load(),store,video);await s.LoadAsync();
            s.SetCandidate(video.SafeWindow);await s.ApplyVideoAsync();s.SetAudio(.2f,.3f,.4f);await s.KeepVideoAsync();
            var saved=SettingsCodec.Decode(store.Text);Assert.AreEqual(video.SafeWindow,saved.Video);Assert.AreEqual(.3f,saved.Music);Assert.IsFalse(video.Background);
        }
        [Test] public async Task Save_OverlappingWrite_PersistsNewestRevisionAndRetriesFailure()
        {
            var store=new FaultSettingsStore();var s=new SettingsService(SettingsConfig.Load(),store,new FakeVideoDevice());await s.LoadAsync();
            var pending=new TaskCompletionSource<bool>();store.Pending=pending;s.SetShake(false);var save=s.SaveAsync();s.SetAudio(.1f,.2f,.3f);
            Assert.AreSame(save,s.SaveAsync());pending.SetResult(true);await save;Assert.AreEqual(.1f,SettingsCodec.Decode(store.Text).Master);
            store.FailWrite=true;s.SetShake(true);await s.SaveAsync();Assert.IsTrue(s.Dirty);Assert.IsNotEmpty(s.Message);
            store.FailWrite=false;await s.SaveAsync();Assert.IsFalse(s.Dirty);Assert.IsTrue(SettingsCodec.Decode(store.Text).Shake);
        }
        [Test] public async Task Close_DuringVideoApply_WaitsThenRevertsAndSavesAudio()
        {
            var video=new FakeVideoDevice();var store=new MemorySettingsStore();var settings=new SettingsService(SettingsConfig.Load(),store,video);await settings.LoadAsync();
            var gate=new TaskCompletionSource<bool>();video.Pending=gate;settings.SetCandidate(video.SafeWindow);
            var apply=settings.ApplyVideoAsync();settings.SetAudio(.1f,.2f,.3f);var close=settings.CloseAsync();Assert.IsFalse(close.IsCompleted);
            gate.SetResult(true);await apply;await close;
            Assert.AreEqual(video.Desktop,video.Current);Assert.IsFalse(video.Background);Assert.AreEqual(.1f,SettingsCodec.Decode(store.Text).Master);
        }
        [Test] public async Task Load_UnsupportedSavedMode_UsesAndPersistsSafeWindow()
        {
            var video=new FakeVideoDevice();var store=new FaultSettingsStore {Text=SettingsCodec.Encode(new SettingsSnapshot(.2f,.3f,.4f,false,new VideoMode(123,456,false)))};
            var settings=new SettingsService(SettingsConfig.Load(),store,video);await settings.LoadAsync();
            Assert.AreEqual(video.SafeWindow,video.Current);Assert.AreEqual(video.SafeWindow,SettingsCodec.Decode(store.Text).Video);
            Assert.AreEqual(.2f,settings.Current.Master);Assert.IsFalse(settings.Current.Shake);
        }
        [Test] public void Values_NonFiniteOrInvalid_Reject()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>new SettingsSnapshot(float.NaN,1,1,true,new VideoMode(1,1,false)));
            Assert.Throws<ArgumentOutOfRangeException>(()=>new VideoMode(0,1,false));
        }
    }
}
