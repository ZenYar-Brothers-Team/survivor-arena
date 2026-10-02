using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class FieldRoadSurfaceTests
    {
        [TestCase(true, 5f)]
        [TestCase(true, 500f)]
        [TestCase(false, 5f)]
        public void GrassSurface_OnlyPlayerBlocked_IncludingFastMotion(bool isPlayer, float speed)
        {
            var previous = Physics2D.simulationMode;
            var root = new GameObject("Road physics test"); var bodyObject = new GameObject("Body");
            var runtime = new FieldRoadSurfaceRuntime();
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var d = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
                runtime.Initialize(d.RoadFallbackLayouts[0],root.transform);
                bodyObject.layer = isPlayer ? LayerMask.NameToLayer("Player") : 0;
                // Perimeter ring axis: arena half 100 minus ringInset 7; its outer road edge is 2 units from the border.
                bodyObject.transform.position = new Vector2(-40,-93);
                bodyObject.AddComponent<CircleCollider2D>().radius = .5f;
                var body = bodyObject.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                Physics2D.SyncTransforms();
                for (var i = 0; i < 80; i++) { body.linearVelocity = Vector2.down*speed; Physics2D.Simulate(.02f); }
                if (isPlayer) Assert.Greater(body.position.y,-98f, "Player must remain inside the rendered road at walking and dash/knockback speed.");
                else Assert.Less(body.position.y,-99f, "Grass must not constrain other entities.");
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(bodyObject); Object.DestroyImmediate(root); Physics2D.simulationMode = previous; }
        }

        [Test]
        public void BranchMouthAndRoundEnd_AreTraversableAndCleanupRemovesBlockers()
        {
            var previous = Physics2D.simulationMode;
            var root = new GameObject("Road join test"); var bodyObject = new GameObject("Player");
            var runtime = new FieldRoadSurfaceRuntime();
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var d = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
                var layout = d.RoadFallbackLayouts[0];
                var build = System.Diagnostics.Stopwatch.StartNew();
                runtime.Initialize(layout,root.transform);
                Assert.Less(build.Elapsed.TotalSeconds,1, "Road boundary construction must not rebuild thousands of grass polygon paths.");
                bodyObject.layer = LayerMask.NameToLayer("Player"); bodyObject.AddComponent<CircleCollider2D>().radius = 1f;
                var body = bodyObject.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                foreach (var branch in layout.DeadEnds)
                {
                    body.position = branch.Entrance; Physics2D.SyncTransforms();
                    var delta = branch.EndCenter-branch.Entrance;
                    for (var i = 0; i < 100; i++) { body.linearVelocity = delta/2f; Physics2D.Simulate(.02f); }
                    Assert.Less(Vector2.Distance(body.position,branch.EndCenter),.1f, "Book corridor has a blocking seam.");
                    for (var i = 0; i < 100; i++) { body.linearVelocity = -delta/2f; Physics2D.Simulate(.02f); }
                    Assert.Less(Vector2.Distance(body.position,branch.Entrance),.1f, "Branch return has a blocking seam.");
                }
                runtime.Dispose(); Assert.IsEmpty(runtime.BoundaryColliders);
                Assert.AreEqual(0,root.GetComponentsInChildren<Collider2D>().Length);
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(bodyObject); Object.DestroyImmediate(root); Physics2D.simulationMode = previous; }
        }
    }
}
