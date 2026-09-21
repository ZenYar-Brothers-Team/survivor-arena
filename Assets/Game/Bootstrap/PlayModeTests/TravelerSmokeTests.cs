using System.Collections;
using System.Linq;
using Game.Character;
using Game.Enemy;
using Game.Pickup;
using Game.Progression;
using Game.Run;
using Game.Telemetry;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace Game.Bootstrap.PlayModeTests
{
    public sealed class TravelerSmokeTests
    {
        [UnityTest]
        public IEnumerator Traveler_FindKillCollectBook_PausePointersAndReset()
        {
            SceneManager.LoadScene("Gameplay",LoadSceneMode.Single);
            yield return null; yield return null;
            var root=Object.FindAnyObjectByType<GameplayCompositionRoot>();
            CharacterSelectionSmokeDriver.StartDefault(root);
            var run=Object.FindAnyObjectByType<RunController>();
            var player=Object.FindAnyObjectByType<PlayerCharacterRuntime>();
            var draft=Object.FindAnyObjectByType<LevelUpDraftRuntime>();
            var ui=Object.FindAnyObjectByType<GameplayUiRoot>().Document.rootVisualElement;
            try
            {
                var first=root.Travelers.Spawn("FIXTURE-TRAVELER-WANDER",run.Model.Elapsed,1);
                var second=root.Travelers.Spawn("FIXTURE-TRAVELER-SHIELD",run.Model.Elapsed,1);
                var firstId=first.LifeId;
                Assert.AreEqual(20,Vector2.Distance(first.Position,player.transform.position),.05f);
                yield return null; yield return null;
                var overlay=ui.Q(GameplayUiElementIds.TravelerOverlay);
                Assert.AreEqual(2,overlay.childCount);
                Assert.IsTrue(overlay.Children().All(item=>item.ClassListContains("traveler-pointer")));
                Assert.IsTrue(overlay.Children().All(item=>item.resolvedStyle.width>0 && item.resolvedStyle.height>0));
                run.Model.Pause(); var position=first.Position; var elapsed=run.Model.Elapsed;
                yield return new WaitForSecondsRealtime(.08f);
                Assert.AreEqual(position,first.Position); Assert.AreEqual(elapsed,run.Model.Elapsed);
                run.Model.Resume();
                player.GetComponent<Rigidbody2D>().position=first.Position;
                player.transform.position=first.Position; Physics2D.SyncTransforms();
                yield return null; yield return null;
                Assert.IsFalse(overlay.Q("traveler-"+firstId.ToString("N")).ClassListContains("traveler-pointer"));
                first.TakeDamage(100000);
                Assert.AreEqual(1,root.Pickups.Snapshot.Spawned);
                var book=Object.FindObjectsByType<WorldPickupVisual>(FindObjectsSortMode.None).Single(item=>item.Life!=null && item.Life.Definition.Kind==PickupRewardKind.Book);
                Assert.AreEqual(firstId,book.Life.Identity.SourceLifeId);
                root.Pickups.Tick(0); yield return null;
                Assert.IsTrue(draft.IsDraftOpen); Assert.AreEqual(RunState.Paused,run.Model.State);
                Assert.AreEqual(1,root.Travelers.Snapshot.Count);
                draft.Select(draft.CurrentDraft.Options[0].Definition.Id);
                second.Despawn(EnemyLifeReason.Escaped); yield return null;
                Assert.AreEqual(1,root.Pickups.Snapshot.Spawned); Assert.AreEqual(0,overlay.childCount);
                var session=(PlaytestSession)root.Playtest;
                StringAssert.Contains("traveler.Killed",session.Recorder.Snapshot(null,false).Json);
                StringAssert.Contains("traveler.Escaped",session.Recorder.Snapshot(null,false).Json);
                var oldRun=run.Model.RunId;
                root.Shutdown(); root.OpenCharacterSelection(); CharacterSelectionSmokeDriver.StartDefault(root);
                yield return null;
                Assert.AreNotEqual(oldRun,run.Model.RunId); Assert.AreEqual(0,root.Travelers.Snapshot.Count);
            }
            finally { root.Shutdown(); }
        }
    }
}
