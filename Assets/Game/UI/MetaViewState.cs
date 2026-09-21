using System.Collections.Generic;
namespace Game.UI
{
    public sealed class MetaViewState
    {
        public bool Visible { get; }
        public string Title { get; }
        public string Summary { get; }
        public string Message { get; }
        public bool CanContinue { get; }
        public bool CanShop { get; }
        public bool IsResults { get; }
        public bool IsError { get; }
        public bool CanReset { get; }
        public bool CanQuit { get; }
        public IReadOnlyList<MetaCardViewState> Cards { get; }
        public IReadOnlyList<string> Characters { get; }
        public string SelectedCharacter { get; }
        public MetaViewState(bool visible, string title, string summary, string message, bool canContinue,
            bool canShop, bool results, bool error, bool canReset, bool canQuit, IEnumerable<MetaCardViewState> cards,
            IEnumerable<string> characters, string selected)
        { Visible = visible; Title = title; Summary = summary; Message = message; CanContinue = canContinue;
            CanShop = canShop; IsResults = results; IsError = error; CanReset = canReset; CanQuit = canQuit;
            Cards = new List<MetaCardViewState>(cards).AsReadOnly(); Characters = new List<string>(characters).AsReadOnly(); SelectedCharacter = selected; }
    }
}
