using System;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>Visible area of the orthographic gameplay camera, read on every query so it follows
    /// the player-centered camera (DECISION-0001) and any aspect ratio.</summary>
    public sealed class CameraTargetViewport : ITargetViewport
    {
        private readonly Camera _camera;

        public CameraTargetViewport(Camera camera)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            if (!camera.orthographic) throw new ArgumentException("Target viewport requires an orthographic camera.", nameof(camera));
        }

        public TargetViewportRect Current
        {
            get
            {
                var halfHeight = _camera.orthographicSize;
                return new TargetViewportRect(_camera.transform.position, halfHeight * _camera.aspect, halfHeight);
            }
        }
    }
}
