using System;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Visual-only duration bars under the player for the timed effects that outlast their zone: shield, picked-up experience
    /// multiplier and skill/action power. Rows stack below the speed bar with the same dimensions; each bar drains to empty.
    /// All children stay under the sprite rig; pause freezes them because the remaining fractions come from the paused zone clock.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TimedEffectBarsPresentationRuntime : MonoBehaviour
    {
        public const int Capacity = 3;
        private readonly SpriteRenderer[] _back = new SpriteRenderer[Capacity];
        private readonly SpriteRenderer[] _fill = new SpriteRenderer[Capacity];
        private Transform _bar;
        private bool _built;

        /// <summary>How many bars are visible after the last <see cref="Apply"/>.</summary>
        public int ShowingCount { get; private set; }

        /// <param name="firstRow">Row index of the first bar: 0 when nothing above, 1 below the speed bar.</param>
        public void Apply(SpritePresentationRuntime presentation, SlowStatusPresentationProfile profile,
            float shield01, float experience01, float power01, int firstRow)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (presentation == null || !presentation.IsInitialized || (shield01 <= 0f && experience01 <= 0f && power01 <= 0f))
            {
                Clear();
                return;
            }
            var rig = presentation.Rig;
            Build(rig);
            var body = rig.BodyRenderer;
            var scale = _bar.parent.lossyScale;
            var width = profile.BarWidth / Mathf.Max(1e-4f, Mathf.Abs(scale.x));
            var height = profile.BarHeight / Mathf.Max(1e-4f, Mathf.Abs(scale.y));
            var row = firstRow;
            ShowingCount = 0;
            for (var i = 0; i < Capacity; i++)
            {
                var remaining = Mathf.Clamp01(i == 0 ? shield01 : i == 1 ? experience01 : power01);
                var shown = remaining > 0f;
                _back[i].enabled = _fill[i].enabled = shown;
                if (!shown) continue;
                var fillColor = i == 0 ? profile.ShieldBarFillColor : i == 1 ? profile.ExperienceBarFillColor : profile.PowerBarFillColor;
                var center = new Vector3(body.bounds.center.x,
                    body.bounds.min.y - profile.BarOffsetY - profile.BarHeight * (row + .5f), body.transform.position.z);
                var local = _bar.parent.InverseTransformPoint(center);
                _back[i].transform.parent.localPosition = new Vector3(local.x, local.y, 0f);
                Style(_back[i], body, profile.BarBackColor, body.sortingOrder + 2);
                Style(_fill[i], body, fillColor, body.sortingOrder + 3);
                _back[i].transform.localScale = new Vector3(width, height, 1f);
                _fill[i].transform.localPosition = new Vector3(-width * (1f - remaining) * .5f, 0f, 0f);
                _fill[i].transform.localScale = new Vector3(width * remaining, height, 1f);
                row++; ShowingCount++;
            }
            _bar.gameObject.SetActive(true);
        }

        public void Clear()
        {
            if (_built) _bar.gameObject.SetActive(false);
            ShowingCount = 0;
        }

        private void OnDisable() => Clear();

        private void Build(SpritePresentationRig rig)
        {
            if (_built) return;
            _bar = new GameObject("TimedEffectBars").transform;
            _bar.SetParent(rig.transform, false);
            for (var i = 0; i < Capacity; i++)
            {
                var row = new GameObject("Bar" + i).transform;
                row.SetParent(_bar, false);
                _back[i] = Create("Back", row);
                _fill[i] = Create("Fill", row);
            }
            _built = true;
        }

        private static SpriteRenderer Create(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = SpeedStatusPresentationRuntime.Pixel();
            renderer.enabled = false;
            return renderer;
        }

        private static void Style(SpriteRenderer renderer, SpriteRenderer body, Color color, int order)
        {
            renderer.sharedMaterial = SlowStatusPresentationRuntime.MaterialFor(color);
            renderer.color = Color.white;
            renderer.sortingLayerID = body.sortingLayerID;
            renderer.sortingOrder = order;
        }
    }
}
