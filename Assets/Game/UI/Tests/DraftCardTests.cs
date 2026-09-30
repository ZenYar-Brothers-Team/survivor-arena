using Game.Content;
using NUnit.Framework;
using UnityEngine.UIElements;
namespace Game.UI.Tests
{
    public sealed class DraftCardTests
    {
        [Test]
        public void SelectButton_IsNotKeyboardFocusable_SoKeysCannotMoveInspectionOrChoice()
        {
            var card = new DraftCard(new DraftOptionViewState(new ContentId("TEST-A"), "Умение A", "Описание", true), () => { });
            Assert.IsFalse(card.SelectButton.focusable);
        }
    }
}
