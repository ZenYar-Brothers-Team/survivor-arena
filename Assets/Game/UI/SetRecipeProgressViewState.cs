using UnityEngine;

namespace Game.UI
{
    public readonly struct SetRecipeProgressViewState
    {
        public string Title { get; }
        public string Detail { get; }
        public int FulfilledComponents { get; }
        public int RequiredComponents { get; }
        public bool IsEligible { get; }
        public bool IsAcquired { get; }
        public bool HasProgress { get; }
        /// <summary>Components present in the build at any level (playtest 2026-09-24_9ae3826e OBS-01).</summary>
        public int OwnedComponents { get; }
        /// <summary>One visible line per component, owned ones marked; level requirement shown alongside.</summary>
        public string Components { get; }
        /// <summary>Approved set icon, also before acquisition (playtest 2026-09-25_5233a664 OBS-03).</summary>
        public Sprite Icon { get; }

        public SetRecipeProgressViewState(
            string title,
            int fulfilledComponents,
            int requiredComponents,
            bool isEligible,
            bool isAcquired, string detail = "", bool? hasProgress = null, int? ownedComponents = null, string components = "",
            Sprite icon = null)
        {
            Detail = detail ?? string.Empty;
            Title = title ?? string.Empty;
            FulfilledComponents = fulfilledComponents;
            RequiredComponents = requiredComponents;
            IsEligible = isEligible;
            IsAcquired = isAcquired;
            // Legacy producer fallback; IP-11 supplies partial-threshold progress explicitly.
            HasProgress = hasProgress ?? fulfilledComponents > 0;
            OwnedComponents = ownedComponents ?? fulfilledComponents;
            Components = components ?? string.Empty;
            Icon = icon;
        }
    }
}
