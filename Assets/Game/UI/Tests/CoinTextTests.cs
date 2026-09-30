using NUnit.Framework;

namespace Game.UI.Tests
{
    public sealed class CoinTextTests
    {
        [TestCase(0, "монет")]
        [TestCase(1, "монета")]
        [TestCase(2, "монеты")]
        [TestCase(4, "монеты")]
        [TestCase(5, "монет")]
        [TestCase(11, "монет")]
        [TestCase(14, "монет")]
        [TestCase(21, "монета")]
        [TestCase(22, "монеты")]
        [TestCase(111, "монет")]
        [TestCase(175, "монет")]
        [TestCase(1001, "монета")]
        public void Noun_Amount_AgreesWithRussianPlural(long amount, string expected)
        {
            Assert.AreEqual(expected, CoinText.Noun(amount));
        }
    }
}
