namespace Game.UI
{
    public readonly struct SetRecipeProgressViewState
    {
        public string Title { get; }
        public int FulfilledComponents { get; }
        public int RequiredComponents { get; }
        public bool IsEligible { get; }
        public bool IsAcquired { get; }

        public SetRecipeProgressViewState(
            string title,
            int fulfilledComponents,
            int requiredComponents,
            bool isEligible,
            bool isAcquired)
        {
            Title = title ?? string.Empty;
            FulfilledComponents = fulfilledComponents;
            RequiredComponents = requiredComponents;
            IsEligible = isEligible;
            IsAcquired = isAcquired;
        }
    }
}
