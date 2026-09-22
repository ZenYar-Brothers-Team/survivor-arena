using System;
using Game.Content;

namespace Game.Progression
{
    /// <summary>Immutable producer event; recorder and profile are optional consumers.</summary>
    public sealed class DraftResolution
    {
        public DraftRequest Request { get; }
        public DraftResolutionKind Kind { get; }
        public ContentId? SelectedId { get; }
        public int CurrencyAmount { get; }

        public DraftResolution(DraftRequest request, DraftResolutionKind kind,
            ContentId? selectedId = null, int currencyAmount = 0)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            if (!Enum.IsDefined(typeof(DraftResolutionKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidateNonNegative(currencyAmount, nameof(currencyAmount));
            Kind = kind;
            SelectedId = selectedId;
            CurrencyAmount = currencyAmount;
        }
    }
}
