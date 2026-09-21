using System;
using System.Collections.Generic;
using Game.Character;
using Game.Enemy;
using Game.Run;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillFrameworkTestContext : IDisposable
    {
        public GameObject RunObject { get; }
        public RunController Run { get; }
        public GameObject Owner { get; }
        public PlayerCharacterRuntime Player { get; }
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
        public SkillFrameworkTestContext()
        {
            RunObject = new GameObject("Skill framework run");
            Run = RunObject.AddComponent<RunController>();
            TestLifecycle.InvokeAwake(Run);
            Run.Model.Start();
            Owner = new GameObject("Skill framework owner");
            Player = Owner.AddComponent<PlayerCharacterRuntime>();
            Player.Initialize(new CharacterBaseStats(100f, 3f), Run);
        }
        public EnemyRuntime Enemy(Vector2 position)
        {
            var enemy = EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-IP08-ENEMY", 100f, 1f, 0f, 0f, 1f),
                position, Owner.transform, Run);
            _enemies.Add(enemy);
            return enemy;
        }
        public void Dispose()
        {
            foreach (var enemy in _enemies) if (enemy != null) enemy.Despawn();
            UnityEngine.Object.DestroyImmediate(Owner);
            UnityEngine.Object.DestroyImmediate(RunObject);
        }
    }
}
