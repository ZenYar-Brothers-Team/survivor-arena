using Game.Content;

namespace Game.Run
{
    /// <summary>Accepted draft rewards and currency already earned during this run.</summary>
    public sealed class RunDraftSnapshot
    {
        public int AcceptedBooks { get; }
        public int Selections { get; }
        public int EmptyRequests { get; }
        public int CancelledRequests { get; }
        public long BookCurrency { get; }

        public RunDraftSnapshot(int acceptedBooks, int selections, int emptyRequests, int cancelledRequests, long bookCurrency)
        {
            NumericValidation.ValidateNonNegative(acceptedBooks, nameof(acceptedBooks));
            NumericValidation.ValidateNonNegative(selections, nameof(selections));
            NumericValidation.ValidateNonNegative(emptyRequests, nameof(emptyRequests));
            NumericValidation.ValidateNonNegative(cancelledRequests, nameof(cancelledRequests));
            NumericValidation.ValidateNonNegative(bookCurrency, nameof(bookCurrency));
            AcceptedBooks = acceptedBooks;
            Selections = selections;
            EmptyRequests = emptyRequests;
            CancelledRequests = cancelledRequests;
            BookCurrency = bookCurrency;
        }
    }
}
