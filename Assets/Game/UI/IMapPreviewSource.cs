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
        /// <summary>All stationary altars, including resting and off-screen ones; empty for other fields.</summary>
        IReadOnlyList<MapPreviewAltar> Altars { get; }
        /// <summary>Every trap turret and barrel of the field (FIELD-004), including far-away ones; empty for other fields.</summary>
        IReadOnlyList<MapPreviewTrap> Traps => System.Array.Empty<MapPreviewTrap>();
        /// <summary>World rectangle the gameplay camera currently shows.</summary>
        Rect View { get; }
    }
}
