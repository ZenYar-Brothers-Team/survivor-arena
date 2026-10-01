using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Tests
{
    internal sealed class FakeMapPreviewSource : IMapPreviewSource
    {
        public Rect Arena { get; set; } = new Rect(-60f, -60f, 120f, 120f);
        public IReadOnlyList<Vector2[]> Obstacles { get; set; } = new List<Vector2[]>
        {
            new[] { new Vector2(0, 0), new Vector2(4, 0), new Vector2(2, 3) }
        };
        public Rect View { get; set; } = new Rect(-8.9f, -5f, 17.8f, 10f);
    }
}
