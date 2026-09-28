using System.Collections.Generic;
namespace Game.UI
{
    public sealed class MetaShopViewState
    {
        public long Currency { get; }
        public long Invested { get; }
        public long RefundFee { get; }
        public string RefundReason { get; }
        public bool ConfirmRefund { get; }
        public IReadOnlyList<ResultContentViewState> Heroes { get; }
        public MetaShopViewState(long currency, long invested, long fee, string reason, bool confirm,
            IEnumerable<ResultContentViewState> heroes)
        { Currency = currency; Invested = invested; RefundFee = fee; RefundReason = reason;
            ConfirmRefund = confirm; Heroes = new List<ResultContentViewState>(heroes).AsReadOnly(); }
    }
}
