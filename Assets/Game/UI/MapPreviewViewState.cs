using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    /// <summary>Everything the map overlay draws; <see cref="Obstacles"/> and <see cref="Roads"/> are shared, never modified by the view.</summary>
    public sealed class MapPreviewViewState
    {
        public static readonly MapPreviewViewState Hidden = new MapPreviewViewState(false, false, default, null, null, default, "");

        public bool Available { get; }
        public bool Visible { get; }
        public Rect Arena { get; }
        public IReadOnlyList<Vector2[]> Obstacles { get; }
        public IReadOnlyList<MapPreviewRoad> Roads { get; }
        public Rect View { get; }
        public string Summary { get; }

        public MapPreviewViewState(bool available, bool visible, Rect arena, IReadOnlyList<Vector2[]> obstacles,
            IReadOnlyList<MapPreviewRoad> roads, Rect view, string summary)
        {
            Available = available; Visible = visible; Arena = arena; Obstacles = obstacles; Roads = roads; View = view;
            Summary = summary ?? "";
        }
    }
}
