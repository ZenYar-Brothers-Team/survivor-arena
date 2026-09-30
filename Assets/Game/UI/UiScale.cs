using System.Collections.Generic;
using Game.Settings;
using UnityEngine;
using UnityEngine.UIElements;
namespace Game.UI
{
    /// <summary>Player UI scale (Auto / 100 / 125 / 150%) applied to every UI panel; the layout itself stays in 1920x1080 logical pixels.</summary>
    public static class UiScale
    {
        private const float ReferenceHeight = 1080f;
        private static readonly List<PanelSettings> Panels = new List<PanelSettings>();
        private static float _preference;
        private static int _screenHeight;
        public static float Factor { get; private set; } = 1f;
        /// <summary>Auto picks the biggest step that still fits the screen height (1080p → 100%, 1440p → 125%, 2160p → 150%).</summary>
        public static float Resolve(float preference, float screenHeight)
        {
            if (preference > 0f) return preference;
            var best = 1f;
            foreach (var step in SettingsSnapshot.UiScales)
                if (step > 0f && step <= screenHeight / ReferenceHeight + 0.001f) best = step;
            return best;
        }
        public static string Label(float preference) => preference <= 0f ? "Авто" : Mathf.RoundToInt(preference * 100) + "%";
        public static void Register(PanelSettings panel)
        {
            if (panel == null) return;
            Panels.Add(panel);
            Apply(panel);
        }
        /// <summary>Cheap per-frame check; re-applies only when the setting or the screen height changed.</summary>
        public static void Update(float preference)
        {
            if (Mathf.Approximately(preference, _preference) && Screen.height == _screenHeight) return;
            _preference = preference; _screenHeight = Screen.height;
            Factor = Resolve(preference, Screen.height);
            Panels.RemoveAll(p => p == null);
            foreach (var panel in Panels) Apply(panel);
        }
        public static void ResetForTests()
        { Panels.Clear(); _preference = 0f; _screenHeight = 0; Factor = 1f; }
        private static void Apply(PanelSettings panel)
        {
            if (_screenHeight == 0) { _screenHeight = Screen.height; Factor = Resolve(_preference, _screenHeight); }
            // ConstantPixelSize uses scale; ScaleWithScreenSize uses the reference resolution.
            panel.scale = Factor;
            panel.referenceResolution = new Vector2Int(Mathf.RoundToInt(1920f / Factor), Mathf.RoundToInt(ReferenceHeight / Factor));
        }
    }
}
