using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// Projects overhead HP bars (DECISION-0110: every living mid-boss) onto the screen each frame.
    /// A bar exists only while its actor is on screen; there is no off-screen pointer and no HP number.
    /// </summary>
    public sealed class OverheadHealthPresenter
    {
        private readonly Func<IReadOnlyList<OverheadHealthSource>> _sources;
        private readonly Func<Vector3, Vector3> _project;
        private readonly IOverheadHealthView _view;
        private readonly List<OverheadHealthBarItem> _bars = new List<OverheadHealthBarItem>();

        public OverheadHealthPresenter(Func<IReadOnlyList<OverheadHealthSource>> sources, Func<Vector3, Vector3> project, IOverheadHealthView view)
        {
            _sources = sources ?? throw new ArgumentNullException(nameof(sources));
            _project = project ?? throw new ArgumentNullException(nameof(project));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            Refresh();
        }

        public void Refresh()
        {
            _bars.Clear();
            foreach (var source in _sources())
            {
                var viewport = _project(source.WorldTop);
                if (viewport.z <= 0 || viewport.x < 0 || viewport.x > 1 || viewport.y < 0 || viewport.y > 1) continue;
                _bars.Add(new OverheadHealthBarItem(source.LifeId, new Vector2(viewport.x, 1 - viewport.y), source.Health01));
            }
            _view.Render(_bars);
        }
    }
}
