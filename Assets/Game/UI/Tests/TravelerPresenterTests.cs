using System;
using Game.Traveler;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class TravelerPresenterTests
    {
        private TravelerSnapshot Item(Vector2 position) => new TravelerSnapshot(Guid.NewGuid(),Guid.NewGuid(),"FIXTURE-TEST","Fixture","TEST",TravelerRole.Wanderer,position,50,100,0,60,1,0);
        [TestCase(true,1)] [TestCase(false,0)]
        public void Presenter_ProjectsMultiplePointers_HidesVisibleArrow_GatesCommands(bool development,int expected)
        {
            var harness=new TravelerUiHarness { Snapshot=new[] {Item(new Vector2(2,.5f)), Item(new Vector2(2,.5f)), Item(new Vector2(.5f,.5f))} };
            using var presenter=new TravelerPresenter(harness,harness,p=>new Vector3(p.x,p.y,1),development);
            Assert.AreEqual(3,harness.Rendered.Count); Assert.IsTrue(harness.Rendered[0].Offscreen); Assert.IsFalse(harness.Rendered[2].Offscreen);
            Assert.AreEqual("→",harness.Rendered[0].Arrow); Assert.AreNotEqual(harness.Rendered[0].Position,harness.Rendered[1].Position);
            Assert.AreEqual(.5f,harness.Rendered[2].HealthFraction);
            foreach(var item in harness.Rendered) { Assert.That(item.Position.x,Is.InRange(0,1)); Assert.That(item.Position.y,Is.InRange(.15f,.85f)); }
            harness.RequestSpawn(); Assert.AreEqual(expected,harness.SpawnCount);
            harness.Snapshot=Array.Empty<TravelerSnapshot>(); presenter.Refresh(); Assert.AreEqual(0,harness.Rendered.Count);
            presenter.Dispose(); harness.RequestSpawn(); Assert.AreEqual(expected,harness.SpawnCount);
        }
        [Test]
        public void View_UsesSemanticIdsAndHealthBars_RemovesExpiredActors()
        {
            var root=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view=new UiToolkitTravelerView(root);
            var id=Guid.NewGuid(); view.Render(new[] {new TravelerHudItem(id,"TEST",.4f,new Vector2(.9f,.5f),true,"→")},"Debug",false);
            Assert.AreEqual(1,root.Q(GameplayUiElementIds.TravelerOverlay).childCount);
            Assert.AreEqual(.4f,root.Q<ProgressBar>("traveler-health").value);
            Assert.AreEqual(DisplayStyle.None,root.Q<Button>(GameplayUiElementIds.SpawnTraveler).style.display.value);
            view.Render(Array.Empty<TravelerHudItem>(),"",true);
            Assert.AreEqual(0,root.Q(GameplayUiElementIds.TravelerOverlay).childCount);
            Assert.AreEqual(DisplayStyle.Flex,root.Q<Button>(GameplayUiElementIds.SpawnTraveler).style.display.value);
        }
    }
}
