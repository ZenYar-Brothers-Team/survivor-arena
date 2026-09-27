using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066: procedural hazard shapes follow the domain visuals and are hidden on reset.</summary>
    public sealed class BossHazardPresentationTests
    {
        [Test]
        public void Render_ShowsOneShapePerVisual_SizesZonesAndBeams_AndResetHidesAll()
        {
            var host = new GameObject("hazard view");
            try
            {
                var view = host.AddComponent<BossHazardPresentation>();
                var visuals = new[]
                {
                    new BossHazardVisual(BossHazardVisualKind.ZoneEdge, new Vector2(1f, 2f), 1.5f, .5f, BossHazardTestData.Color),
                    new BossHazardVisual(BossHazardVisualKind.BeamActive, Vector2.zero, 0f, .5f, BossHazardTestData.Color, 90f, 24f, 1.2f),
                    new BossHazardVisual(BossHazardVisualKind.DangerWash, Vector2.zero, BossHazardField.FullViewRadius, 1f,
                        BossHazardTestData.Color)
                };
                view.Render(visuals, new Vector2(5f, 5f), 12f);
                Assert.AreEqual(3, view.VisibleShapes);
                var shapes = host.GetComponentsInChildren<SpriteRenderer>(true);
                Assert.AreEqual(3, shapes.Count(r => r.enabled));
                Assert.AreEqual(3f, shapes[0].transform.localScale.x, 1e-4f, "Edge diameter = 2 × radius.");
                Assert.AreEqual(24f, shapes[1].transform.localScale.x, 1e-4f, "Beam length.");
                Assert.AreEqual(1.2f, shapes[1].transform.localScale.y, 1e-4f, "Active beam has its full hit width.");
                Assert.AreEqual(90f, shapes[1].transform.eulerAngles.z, 1e-3f);
                Assert.AreEqual(new Vector3(5f, 5f, 0f), shapes[2].transform.position, "The danger wash covers the view.");
                Assert.AreEqual(24f, shapes[2].transform.localScale.x, 1e-4f);
                view.ResetPresentation();
                Assert.AreEqual(0, view.VisibleShapes);
                Assert.IsTrue(host.GetComponentsInChildren<SpriteRenderer>(true).All(r => !r.enabled));
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
