using System.Collections;
using Game.Character;
using Game.Run;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class SettingsPresentationSmokeTests
    {
        [UnityTest] public IEnumerator Shake_Damage_PreservesCameraAnchorAndResetsOnPauseOffAndEnd()
        {
            ProfileSmokeScene.Load();yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            try
            {
                CharacterSelectionSmokeDriver.StartDefault(root);yield return null;
                var player=Object.FindAnyObjectByType<PlayerCharacterRuntime>();var run=Object.FindAnyObjectByType<RunController>();
                var shake=root.GetComponent<CameraShakeRuntime>();var camera=Camera.main;var anchor=camera.transform.position;
                player.Health.TakeDamage(1);yield return null;
                Assert.Greater(shake.Offset.magnitude,0);Assert.LessOrEqual(shake.Offset.magnitude,.005f*2*camera.orthographicSize);
                Assert.AreEqual(anchor,camera.transform.position);run.Model.Pause();Assert.AreEqual(Vector2.zero,shake.Offset);
                run.Model.Resume();yield return null;Assert.AreEqual(Vector2.zero,shake.Offset);
                shake.enabled=false;player.Health.TakeDamage(1);shake.enabled=true;yield return null;
                Assert.AreEqual(Vector2.zero,shake.Offset);
                player.Health.TakeDamage(1);root.Settings.SetShake(false);Assert.AreEqual(Vector2.zero,shake.Offset);
                root.Settings.SetShake(true);player.Health.TakeDamage(player.Health.MaxHealth*10);yield return null;
                Assert.AreEqual(Vector2.zero,shake.Offset);Assert.AreEqual(RunState.Lost,run.Model.State);
            }
            finally { root.Shutdown(); }
        }
        [UnityTest] public IEnumerator Audio_Settings_RouteMasterOnceWithIndependentChannels()
        {
            ProfileSmokeScene.Load(false);yield return null;yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var sources=root.GetComponentsInChildren<AudioSource>();Assert.AreEqual(4,sources.Length);
            root.Settings.SetAudio(.5f,.2f,.8f);
            Assert.AreEqual(.02f,sources[0].volume,.0001f);Assert.AreEqual(.08f,sources[1].volume,.0001f);
            Assert.AreEqual(.1f,sources[2].volume,.0001f);Assert.AreEqual(.4f,sources[3].volume,.0001f);
            Assert.Greater(sources[0].clip.samples,0);Assert.Greater(sources[1].clip.samples,0);
            root.Settings.SetAudio(0,1,1);foreach(var source in sources)Assert.AreEqual(0,source.volume);
            root.Shutdown();
        }
    }
}
