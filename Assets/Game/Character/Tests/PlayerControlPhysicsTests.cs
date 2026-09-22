using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace Game.Character.Tests
{
    public sealed class PlayerControlPhysicsTests
    {
        [Test]
        public void Knockback_UsesCollisionVelocity_AndBlockedDistanceDoesNotAccumulate()
        {
            var previous = Physics2D.simulationMode;
            var previousScenes = EditorSceneManager.GetSceneManagerSetup();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("player-control-physics");
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var run = root.AddComponent<RunController>();
                if (!run.IsInitialized) run.Initialize();
                run.Model.Start();
                var playerObject = new GameObject("player");
                playerObject.transform.SetParent(root.transform);
                playerObject.layer = LayerMask.NameToLayer("Player");
                var body = playerObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                playerObject.AddComponent<BoxCollider2D>().size = Vector2.one * 0.5f;
                var player = playerObject.AddComponent<PlayerCharacterRuntime>();
                player.Initialize(new CharacterBaseStats(100f, 1f), run);
                var wall = new GameObject("player-only-wall");
                wall.transform.SetParent(root.transform);
                wall.transform.position = Vector3.right * 2f;
                var collider = wall.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(1f, 4f);
                collider.excludeLayers = ~(1 << playerObject.layer);
                Physics2D.SyncTransforms();
                player.ApplyDamage(new CombatDamageRequest(default, 0f, new CombatControlProfile(5f, 1f), 1f, 0f));
                for (var i = 0; i < 51; i++)
                {
                    body.linearVelocity = Vector2.right + player.TickAdditionalMovement(0.02f, true);
                    Physics2D.Simulate(0.02f);
                }
                Assert.Less(body.position.x, 1.3f);
                Assert.AreEqual(0f, player.Controls.KnockbackRemaining);
                Object.DestroyImmediate(wall);
                Assert.AreEqual(Vector2.zero, player.TickAdditionalMovement(0.02f, true));
                body.linearVelocity = Vector2.right;
                var before = body.position;
                Physics2D.Simulate(0.02f);
                Assert.AreEqual(0.02f, body.position.x - before.x, 0.002f);
            }
            finally
            {
                Object.DestroyImmediate(root);
                Physics2D.simulationMode = previous;
                EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
            }
        }
    }
}
