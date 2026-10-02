using System.Linq;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.Bootstrap.Tests
{
    public sealed class FieldRoadSurfaceTests
    {
        [Test]
        public void CurbFacesAndGrassFeather_UseFixedViewProjection_WithoutNewColliders()
        {
            var parent = new GameObject("Road edge regression");
            var runtime = new FieldRoadSurfaceRuntime();
            try
            {
                var definition = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
                var registry = RuntimeContentCatalog.CreateProduction().Registry;
                var art = definition.RoadLayout.Art;
                runtime.Initialize(definition.RoadFallbackLayouts[0],parent.transform,art.Main.Resolve(registry).Sprite,
                    art.Branch.Resolve(registry).Sprite,art.Curb.Resolve(registry).Sprite);
                var faces = parent.transform.Find("RoadNetwork/RoadCurbFaces");
                Assert.NotNull(faces,"The baked rotating side must be replaced by a separate projected face.");
                var mesh = faces.GetComponent<MeshFilter>().sharedMesh;
                var points = mesh.vertices; var colors = mesh.colors;
                for (var i = 0; i < points.Length; i += 4)
                {
                    Assert.Less(Vector3.Distance(points[i+3]-points[i],Vector3.down*art.CurbFaceHeight),1e-4f);
                    Assert.Less(Vector3.Distance(points[i+2]-points[i+1],Vector3.down*art.CurbFaceHeight),1e-4f);
                }
                Assert.IsTrue(colors.Any(c => c.a == 0f),"Back-facing sides must be hidden.");
                Assert.IsTrue(colors.Any(c => c.a > .9f),"Front-facing sides remain visible.");
                var feather = parent.transform.Find("RoadNetwork/RoadGrassTransition");
                Assert.NotNull(feather);
                var edgeMesh = feather.GetComponent<MeshFilter>().sharedMesh;
                var band = edgeMesh.uv2;
                Assert.AreEqual(edgeMesh.vertexCount,band.Length);
                for (var i = 0; i < band.Length; i += 4)
                {
                    Assert.AreEqual(0f,band[i].x); Assert.AreEqual(0f,band[i+1].x);
                    Assert.AreEqual(1f,band[i+2].x); Assert.AreEqual(1f,band[i+3].x);
                }
                var material = feather.GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual(1f,material.GetFloat("_EdgeBand"));
                Assert.AreEqual(art.EdgeOpacity,material.GetFloat("_EdgeOpacity"));
                Assert.AreSame(art.Branch.Resolve(registry).Sprite.texture,material.mainTexture);
                Assert.IsEmpty(faces.GetComponentsInChildren<Collider2D>());
                Assert.IsEmpty(feather.GetComponentsInChildren<Collider2D>());
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(parent); }
        }

        [Test]
        public void ApprovedArt_KeepsPhysicsAndUsesWorldUv_WithOwnedMaterialCleanup()
        {
            var parent = new GameObject("Road art test");
            var plainParent = new GameObject("Road blockout test");
            var artRuntime = new FieldRoadSurfaceRuntime(); var plainRuntime = new FieldRoadSurfaceRuntime();
            Material material = null;
            try
            {
                var definition = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
                var layout = definition.RoadFallbackLayouts[0];
                var registry = RuntimeContentCatalog.CreateProduction().Registry;
                var art = definition.RoadLayout.Art;
                artRuntime.Initialize(layout,parent.transform,art.Main.Resolve(registry).Sprite,
                    art.Branch.Resolve(registry).Sprite,art.Curb.Resolve(registry).Sprite);
                plainRuntime.Initialize(layout,plainParent.transform);
                Assert.AreEqual(plainRuntime.BoundaryColliders.Count,artRuntime.BoundaryColliders.Count);
                for (var i = 0; i < plainRuntime.BoundaryColliders.Count; i++)
                    CollectionAssert.AreEqual(plainRuntime.BoundaryColliders[i].points,artRuntime.BoundaryColliders[i].points);
                var surface = parent.transform.Find("RoadNetwork/MainRoads");
                var mesh = surface.GetComponent<MeshFilter>().sharedMesh;
                var vertices = mesh.vertices; var uv = mesh.uv;
                Assert.AreEqual(mesh.vertexCount,uv.Length);
                for (var i = 0; i < mesh.vertexCount; i++)
                    Assert.Less(Vector2.Distance(uv[i],(Vector2)vertices[i]/art.SurfaceRepeat),1e-5f);
                material = surface.GetComponent<MeshRenderer>().sharedMaterial;
                Assert.AreEqual(1f,material.GetFloat("_MirrorRepeat"));
                Assert.AreEqual(TextureWrapMode.Clamp,material.mainTexture.wrapMode);
                Assert.AreSame(art.Main.Resolve(registry).Sprite.texture,material.mainTexture);
                var curb = parent.transform.Find("RoadNetwork/RoadCurb");
                Assert.NotNull(curb);
                Assert.IsEmpty(curb.GetComponents<Collider2D>());
                artRuntime.Dispose();
                Assert.IsTrue(material == null,"The private material must be released with the road visual root.");
                Assert.NotNull(art.Main.Resolve(registry).Sprite.texture);
            }
            finally
            {
                artRuntime.Dispose(); plainRuntime.Dispose();
                Object.DestroyImmediate(parent); Object.DestroyImmediate(plainParent);
            }
        }

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
        public void PlayerPressedIntoTheEdge_SlidesAlongObliqueRoadAndAroundRoundEnd()
        {
            var previous = Physics2D.simulationMode;
            var root = new GameObject("Road slide test"); var bodyObject = new GameObject("Player");
            var runtime = new FieldRoadSurfaceRuntime();
            try
            {
                Physics2D.simulationMode = SimulationMode2D.Script;
                var d = FixtureFieldEnvironmentPresentationCatalog.Load("Content/Presentation/ProductionFieldEnvironmentPresentation")["FIELD-003-ENVIRONMENT"];
                var layout = d.RoadFallbackLayouts[0]; var p = layout.Profile;
                runtime.Initialize(layout,root.transform);
                bodyObject.layer = LayerMask.NameToLayer("Player"); bodyObject.AddComponent<CircleCollider2D>().radius = .5f;
                var body = bodyObject.AddComponent<Rigidbody2D>(); body.gravityScale = 0; body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // Oblique main road: the movement direction keeps pushing into the edge, as a player holding a diagonal does.
                var (start,along,outward) = ObliqueStretch(layout);
                body.position = start; body.linearVelocity = Vector2.zero; Physics2D.SyncTransforms();
                for (var i = 0; i < 100; i++) { body.linearVelocity = along*6f+outward*2f; Physics2D.Simulate(.02f); }
                Assert.Greater(Vector2.Dot(body.position-start,along),.7f*6f*2f, "An oblique road edge must not catch the player on cell steps.");

                // Round end: keep pressing outward while circling; start opposite the corridor mouth.
                var branch = layout.DeadEnds[0];
                var axis = (branch.EndCenter-branch.Entrance).normalized;
                body.position = branch.EndCenter+axis*(p.DeadEndEndRadius-1f); body.linearVelocity = Vector2.zero; Physics2D.SyncTransforms();
                var travelled = 0f; var radial = body.position-branch.EndCenter;
                for (var i = 0; i < 100; i++)
                {
                    var outwardRadial = (body.position-branch.EndCenter).normalized;
                    body.linearVelocity = new Vector2(-outwardRadial.y,outwardRadial.x)*6f+outwardRadial*2f;
                    Physics2D.Simulate(.02f);
                    var next = body.position-branch.EndCenter;
                    travelled += Vector2.SignedAngle(radial,next)*Mathf.Deg2Rad*(p.DeadEndEndRadius-.5f); radial = next;
                }
                Assert.Greater(travelled,.7f*6f*2f, "The circle around a book must let the player slide along it.");
                Assert.Less(Vector2.Distance(body.position,branch.EndCenter),p.DeadEndEndRadius, "The player stays inside the round end.");
            }
            finally { runtime.Dispose(); Object.DestroyImmediate(bodyObject); Object.DestroyImmediate(root); Physics2D.simulationMode = previous; }
        }

        // A 16-unit stretch in the middle of a long diagonal main road whose chosen side is solid grass the whole way.
        private static (Vector2 start, Vector2 along, Vector2 outward) ObliqueStretch(FieldRoadLayout layout)
        {
            var p = layout.Profile;
            bool Grass(Vector2 q) => layout.MainDistance(q) > p.MainRoadWidth*.5f+.5f &&
                layout.DeadEnds.All(b => FieldRoadLayout.Distance(q,b.Entrance,b.EndCenter) > p.DeadEndWidth*.5f+.5f &&
                                         Vector2.Distance(q,b.EndCenter) > p.DeadEndEndRadius+.5f);
            foreach (var path in layout.Roads.Skip(1))
            for (var i = 1; i < path.Count; i++)
            {
                var a = path[i-1]; var b = path[i]; var along = (b-a).normalized;
                var angle = Mathf.Abs(Mathf.Atan2(along.y,along.x)*Mathf.Rad2Deg)%90f;
                if (Vector2.Distance(a,b) < 30f || angle < 25f || angle > 65f) continue;
                foreach (var outward in new[] { new Vector2(-along.y,along.x),new Vector2(along.y,-along.x) })
                {
                    var middle = (a+b)*.5f; var start = middle-along*8f;
                    var clear = true;
                    for (var t = -1f; t <= 18f && clear; t += .5f) clear = Grass(start+along*t+outward*(p.MainRoadWidth*.5f+1f));
                    if (clear) return (start+outward*(p.MainRoadWidth*.5f-1f),along,outward); // half a body from the edge
                }
            }
            Assert.Fail("Reference layout has no long oblique road with a clear side.");
            return default;
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
