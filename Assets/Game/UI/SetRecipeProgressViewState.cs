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

        public SetRecipeProgressViewState(
            string title,
            int fulfilledComponents,
            int requiredComponents,
            bool isEligible,
            bool isAcquired, string detail = "", bool? hasProgress = null)
        {
            Detail = detail ?? string.Empty;
            Title = title ?? string.Empty;
            FulfilledComponents = fulfilledComponents;
            RequiredComponents = requiredComponents;
            IsEligible = isEligible;
            IsAcquired = isAcquired;
            // Legacy producer fallback; IP-11 supplies partial-threshold progress explicitly.
            HasProgress = hasProgress ?? fulfilledComponents > 0;
        }
    }
}
