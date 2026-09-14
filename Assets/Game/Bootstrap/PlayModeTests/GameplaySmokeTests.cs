using System.Collections;
using Game.Progression;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Bootstrap.PlayModeTests
{
    public sealed class GameplaySmokeTests
    {
        [UnityTest]
        public IEnumerator GameplayScene_ComposesLevelsUpAndResumes()
        {
            SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
            yield return null;
            yield return null;

            var root = Object.FindAnyObjectByType<GameplayCompositionRoot>();
            var run = Object.FindAnyObjectByType<RunController>();
            var experience = Object.FindAnyObjectByType<PlayerExperienceRuntime>();
            var draft = Object.FindAnyObjectByType<LevelUpDraftRuntime>();

            Assert.IsNotNull(root);
            Assert.IsTrue(root.IsInitialized);
            Assert.AreEqual(RunState.Running, run.Model.State);

            experience.AddPickedUpExperience(5f);
            Assert.IsTrue(draft.IsDraftOpen);
            Assert.IsTrue(run.Model.IsPausedBy(RunPauseReasons.LevelUpDraft));
            DraftOption? passiveOption = null;
            for (var i = 0; i < draft.CurrentDraft.Options.Count; i++)
            {
                if (draft.CurrentDraft.Options[i].Definition.Kind != BuildEntryKind.PassiveItem)
                    continue;
                passiveOption = draft.CurrentDraft.Options[i];
                break;
            }
            Assert.IsTrue(passiveOption.HasValue);
            Assert.IsTrue(draft.Select(passiveOption.Value.Definition.Id));
            Assert.IsFalse(draft.IsDraftOpen);
            Assert.AreEqual(RunState.Running, run.Model.State);
            Assert.AreEqual(1, Object.FindAnyObjectByType<PlayerPassiveSetRuntime>().PassiveCount);
        }
    }
}
