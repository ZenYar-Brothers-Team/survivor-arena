using System;
using System.Linq;
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
        public void Presenter_OffscreenPointersRideFrame_VisibleKeepsHealth_GatesCommands(bool development,int expected)
        {
            // Projection is identity (x,y already viewport, y up); aspect 1 keeps angles exact.
            var harness=new TravelerUiHarness { Snapshot=new[] {Item(new Vector2(2,.5f)), Item(new Vector2(2,.5f)), Item(new Vector2(.5f,.5f)), Item(new Vector2(.5f,-3f))} };
            using var presenter=new TravelerPresenter(harness,harness,p=>new Vector3(p.x,p.y,1),development,()=>1f);
            Assert.AreEqual(4,harness.Rendered.Count);
            var right=harness.Rendered[0]; var sameSide=harness.Rendered[1]; var visible=harness.Rendered[2]; var below=harness.Rendered[3];
            Assert.IsTrue(right.Offscreen); Assert.IsTrue(sameSide.Offscreen); Assert.IsFalse(visible.Offscreen); Assert.IsTrue(below.Offscreen);
            // DECISION-0109: close to the screen edge, not pulled toward the centre.
            Assert.AreEqual(.97f,right.Position.x,1e-4f); Assert.AreEqual(.5f,right.Position.y,1e-4f);
            Assert.AreEqual(.95f,below.Position.y,1e-4f); Assert.AreEqual(.5f,below.Position.x,1e-4f);
            Assert.AreEqual(0f,right.AngleDegrees,1e-3f); Assert.AreEqual(90f,below.AngleDegrees,1e-3f, "Down on screen is +90° (y down).");
            Assert.AreEqual(.97f,sameSide.Position.x,1e-4f,"A second pointer on the same edge stays on the frame.");
            Assert.GreaterOrEqual(Mathf.Abs(sameSide.Position.y-right.Position.y),.07f-1e-4f,"…but slides along it.");
            Assert.AreEqual(.5f,visible.HealthFraction);
            foreach(var item in harness.Rendered) { Assert.That(item.Position.x,Is.InRange(0,1)); Assert.That(item.Position.y,Is.InRange(0,1)); }
            harness.RequestSpawn(); Assert.AreEqual(expected,harness.SpawnCount);
            harness.Snapshot=Array.Empty<TravelerSnapshot>(); presenter.Refresh(); Assert.AreEqual(0,harness.Rendered.Count);
            presenter.Dispose(); harness.RequestSpawn(); Assert.AreEqual(expected,harness.SpawnCount);
        }

        [Test]
        public void Presenter_WideScreenAngle_UsesAspect()
        {
            var harness=new TravelerUiHarness { Snapshot=new[] {Item(new Vector2(1.5f,-.5f))} };
            using var presenter=new TravelerPresenter(harness,harness,p=>new Vector3(p.x,p.y,1),false,()=>16f/9f);
            var item=harness.Rendered.Single();
            Assert.AreEqual(Mathf.Atan2(1f,16f/9f)*Mathf.Rad2Deg,item.AngleDegrees,1e-3f);
        }

        [Test]
        public void View_PointerHasArrowAndMutedCaptionWithoutHealth_VisibleHasOnlyHealth()
        {
            var root=AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Game/UI/Resources/UI/GameplayUi.uxml").CloneTree();
            using var view=new UiToolkitTravelerView(root);
            var pointerId=Guid.NewGuid(); var visibleId=Guid.NewGuid();
            view.Render(new[] {new TravelerHudItem(pointerId,.4f,new Vector2(.97f,.5f),true,30f), new TravelerHudItem(visibleId,.6f,new Vector2(.4f,.4f),false,0f)},"Debug",false);
            var overlay=root.Q(GameplayUiElementIds.TravelerOverlay);
            Assert.AreEqual(2,overlay.childCount);
            var pointer=overlay.Q("traveler-"+pointerId.ToString("N"));
            Assert.AreEqual(DisplayStyle.Flex,pointer.Q<TravelerPointerArrow>().style.display.value);
            Assert.AreEqual(30f,pointer.Q<TravelerPointerArrow>().style.rotate.value.angle.value,1e-4f);
            Assert.AreEqual("Путник",pointer.Q<Label>(GameplayUiElementIds.TravelerPointerCaption).text);
            Assert.AreEqual(DisplayStyle.None,pointer.Q<ProgressBar>(GameplayUiElementIds.TravelerHealth).style.display.value);
            var visible=overlay.Q("traveler-"+visibleId.ToString("N"));
            Assert.AreEqual(DisplayStyle.None,visible.Q<TravelerPointerArrow>().style.display.value);
            Assert.AreEqual(DisplayStyle.None,visible.Q<Label>(GameplayUiElementIds.TravelerPointerCaption).style.display.value);
            Assert.AreEqual(.6f,visible.Q<ProgressBar>(GameplayUiElementIds.TravelerHealth).value);
            Assert.AreEqual(DisplayStyle.None,root.Q<Button>(GameplayUiElementIds.SpawnTraveler).style.display.value);
            view.Render(Array.Empty<TravelerHudItem>(),"",true);
            Assert.AreEqual(0,overlay.childCount);
            Assert.AreEqual(DisplayStyle.Flex,root.Q<Button>(GameplayUiElementIds.SpawnTraveler).style.display.value);
        }
    }
}
