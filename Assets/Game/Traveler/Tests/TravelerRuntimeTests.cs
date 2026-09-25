using System;
using System.Collections.Generic;
using System.Linq;
using Game.Character;
using Game.Content;
using Game.Enemy;
using Game.Pickup;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Game.Traveler.Tests
{
    public sealed class TravelerRuntimeTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private TravelerEncounterRuntime _travelers;
        private WorldPickupRuntime _pickups;
        private FixtureTravelerCatalog _catalog;
        private TravelerPlacement _placement;
        private Camera _camera;
        private FixturePickupCatalog _pickupCatalog;
        private readonly List<TravelerEvent> _events = new List<TravelerEvent>();
        private int _baseline;
        [SetUp]
        public void SetUp()
        {
            _baseline=EnemyRegistry.Count; _events.Clear();
            _root=new GameObject("Traveler test"); _run=_root.AddComponent<RunController>(); _run.Initialize();
            var target=new GameObject("player"); target.transform.SetParent(_root.transform); target.AddComponent<BoxCollider2D>();
            _player=target.AddComponent<PlayerCharacterRuntime>(); _player.Initialize(new CharacterBaseStats(100,3),_run);
            var camera=new GameObject("camera"); camera.transform.SetParent(_root.transform); _camera=camera.AddComponent<Camera>(); _camera.orthographic=true; _camera.orthographicSize=5;
            var reachable=new BoxPickupPlacement(new Rect(-100,-100,200,200),Array.Empty<Rect>(),Vector2.one*.7f,Vector2.zero,.01f);
            _placement=new TravelerPlacement(reachable); _pickupCatalog=FixturePickupCatalog.Create();
            _pickups=_root.AddComponent<WorldPickupRuntime>(); _pickups.Initialize(_pickupCatalog,_run.Model,_player,new TravelerTestRewardTarget(),reachable,"FIXTURE-FIELD");
            _catalog=FixtureTravelerCatalog.Create(); _travelers=_root.AddComponent<TravelerEncounterRuntime>(); Initialize();
            _travelers.LifeEvent+=_events.Add; _run.Model.Start();
        }
        private void Initialize() => _travelers.Initialize(_catalog.Schedules[0],_catalog,_run,_player.transform,_camera,_placement,_pickups,_pickupCatalog.Book,null);
        [TearDown]
        public void TearDown() { _travelers.Shutdown(); _pickups.Shutdown(); _player.Shutdown(); _run.Shutdown(); Object.DestroyImmediate(_root); Assert.AreEqual(_baseline,EnemyRegistry.Count); }
        private EnemyRuntime Spawn(string suffix="WANDER") => _travelers.Spawn("FIXTURE-TRAVELER-"+suffix,_run.Model.Elapsed,1);
        [Test]
        public void PeacefulContact_DoesNotDispatchPlayerCombat_ExpiredAttackerCannotHit()
        {
            var hits=0; _player.CombatResolved += result => hits++;
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var target=typeof(EnemyRuntime).GetField("_contactTarget",flags);
            var contact=typeof(EnemyRuntime).GetMethod("ApplyContactHits",flags);
            var peaceful=Spawn(); target.SetValue(peaceful,_player); contact.Invoke(peaceful,new object[]{1});
            Assert.AreEqual(0,hits);
            var attacker=Spawn("BRUISER"); target.SetValue(attacker,_player);
            _run.Model.Tick(60); contact.Invoke(attacker,new object[]{1});
            Assert.AreEqual(0,hits); Assert.IsFalse(attacker.IsAlive); Assert.AreEqual(0,_pickups.Snapshot.Spawned);
        }
        [Test]
        public void Spawn_TwoScreenHeights_KillDropsOneBook_TimeoutNone()
        {
            var first=Spawn(); Assert.AreEqual(20,Vector2.Distance(first.Position,_player.transform.position),.001f);
            var life=first.LifeId; first.TakeDamage(10000);
            Assert.AreEqual(1,_pickups.Snapshot.Spawned); Assert.AreEqual(0,_travelers.Snapshot.Count);
            var second=Spawn(); Assert.AreSame(first,second); Assert.AreNotEqual(life,second.LifeId);
            _run.Model.Tick(60); second.TakeDamage(10000);
            Assert.AreEqual(1,_pickups.Snapshot.Spawned); Assert.AreEqual("Escaped",_events.Last().Outcome);
        }
        [Test]
        public void Pause_FreezesEncounterAndDamage_ResumeExpires_TerminalClears()
        {
            var actor=Spawn(); var health=actor.Health.CurrentHealth;
            _run.Model.Pause(); _run.Model.Tick(100); _travelers.Tick(); actor.TakeDamage(10000);
            Assert.AreEqual(health,actor.Health.CurrentHealth); Assert.IsFalse(actor.GetComponent<Rigidbody2D>().simulated);
            Assert.AreEqual(1,_travelers.Snapshot.Count);
            _run.Model.Resume(); Assert.IsTrue(actor.GetComponent<Rigidbody2D>().simulated);
            _run.Model.Tick(60); _travelers.Tick(); Assert.AreEqual(0,_travelers.Snapshot.Count);
            Spawn(); Spawn("GUARD"); _run.Model.Kill(); Assert.AreEqual(0,_travelers.Snapshot.Count); Assert.AreEqual(0,_pickups.Snapshot.Spawned);
        }
        [Test]
        public void SimultaneousLives_HaveIndependentHealthAndCleanup_ReinitClearsScheduleProgress()
        {
            var a=Spawn(); var b=Spawn("REST"); var c=Spawn("GUARD");
            Assert.AreEqual(3,_travelers.Snapshot.Count); a.TakeDamage(3); Assert.AreEqual(160,b.Health.CurrentHealth);
            b.Despawn(); Assert.AreEqual(2,_travelers.Snapshot.Count); Assert.IsTrue(c.IsAlive);
            var schedule=_travelers.Schedule.Select(item=>item.Time).ToArray(); Initialize();
            Assert.AreEqual(0,_travelers.Snapshot.Count); CollectionAssert.AreEqual(schedule,_travelers.Schedule.Select(item=>item.Time));
        }
        [Test]
        public void Initialize_PerRunSeedDrivesScheduleDraw_DefaultsToReferenceSeed()
        {
            var schedule=_catalog.Schedules[0];
            Assert.AreEqual(schedule.Seed,_travelers.Seed);
            for (var seed=1; seed<=3; seed++)
            {
                _travelers.Initialize(schedule,_catalog,_run,_player.transform,_camera,_placement,_pickups,_pickupCatalog.Book,null,seed:seed);
                var expected=schedule.Draw(_run.Model.Duration,new System.Random(seed));
                Assert.AreEqual(seed,_travelers.Seed);
                CollectionAssert.AreEqual(expected.Select(item=>(item.Id,item.Time)),_travelers.Schedule.Select(item=>(item.Id,item.Time)));
            }
        }
        [Test]
        public void Protection_OnlyOrdinarySameRun_ExitAndSourceRemovalCleanUp()
        {
            var guard=Spawn("GUARD");
            var ordinary=EnemyFactory.Spawn(new EnemyDefinition("TEST-ORDINARY",100,1,0,0,1),guard.Position,_player.transform,_run,_root.transform);
            var boss=EnemyFactory.Spawn(new EnemyDefinition("TEST-BOSS",100,1,0,0,1),guard.Position,_player.transform,_run,_root.transform,category:EnemyCategory.Boss);
            _travelers.Tick(); Assert.AreEqual(.2f,ordinary.Protection.Reduction); Assert.AreEqual(0,boss.Protection.Reduction); Assert.AreEqual(0,guard.Protection.Reduction);
            Assert.AreEqual(8,ordinary.TakeDamage(10),.001f);
            ordinary.transform.position=Vector2.zero; _travelers.Tick(); Assert.AreEqual(0,ordinary.Protection.Reduction);
            ordinary.transform.position=guard.Position; _travelers.Tick(); guard.Despawn(); Assert.AreEqual(0,ordinary.Protection.Reduction);
            ordinary.Despawn(); boss.Despawn();
        }
        [Test]
        public void Shield_SourceDeathAndPoolReuse_RemoveOwnedShield()
        {
            var protector=Spawn("SHIELD");
            var enemy=EnemyFactory.Spawn(new EnemyDefinition("TEST",100,1,0,0,1),protector.Position,_player.transform,_run,_root.transform);
            _travelers.Tick(); Assert.AreEqual(25,enemy.Protection.ShieldRemaining);
            enemy.TakeDamage(10); Assert.AreEqual(100,enemy.Health.CurrentHealth); Assert.AreEqual(15,enemy.Protection.ShieldRemaining);
            protector.TakeDamage(10000); Assert.AreEqual(0,enemy.Protection.ShieldRemaining); enemy.Despawn();
        }
        [Test]
        public void WanderAvoidance_IsPauseSafe_AndStaysInsideReachableArena()
        {
            var definition=_catalog.Definitions["FIXTURE-TRAVELER-WANDER"];
            var driver=new TravelerMovementDriver(definition,_placement,_run.Model.RunId,1);
            var motion=driver.Tick(Vector2.right,Vector2.zero,2,.1f,true);
            Assert.Greater(motion.Velocity.x,0); Assert.AreEqual(Vector2.zero,driver.Tick(Vector2.right,Vector2.zero,2,10,false).Velocity);
            for(var i=0;i<100;i++) Assert.IsTrue(_placement.Contains(Vector2.right+driver.Tick(Vector2.right,Vector2.zero,2,.1f,true).Velocity*.1f));
        }
    }
}
