using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    /// <summary>Everything the map overlay draws; <see cref="Obstacles"/> is shared, never modified by the view.</summary>
    public sealed class MapPreviewViewState
    {
        public static readonly MapPreviewViewState Hidden = new MapPreviewViewState(false, false, default, null, default, "");

        public bool Available { get; }
        public bool Visible { get; }
        public Rect Arena { get; }
        public IReadOnlyList<Vector2[]> Obstacles { get; }
        public Rect View { get; }
        public string Summary { get; }

        public MapPreviewViewState(bool available, bool visible, Rect arena, IReadOnlyList<Vector2[]> obstacles, Rect view,
            string summary)
        {
            Available = available; Visible = visible; Arena = arena; Obstacles = obstacles; View = view; Summary = summary ?? "";
        }
    }
}
