using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    /// <summary>Read-only world geometry for the development map preview: the arena, roads, player-only obstacles and the camera view.</summary>
    public interface IMapPreviewSource
    {
        /// <summary>Playable area in world units.</summary>
        Rect Arena { get; }
        /// <summary>World-space outline of every obstacle that blocks the player; static for the run.</summary>
        IReadOnlyList<Vector2[]> Obstacles { get; }
        /// <summary>Walkable road pieces in drawing order; empty for fields without roads; static for the run.</summary>
        IReadOnlyList<MapPreviewRoad> Roads { get; }
        /// <summary>World rectangle the gameplay camera currently shows.</summary>
        Rect View { get; }
    }
}
