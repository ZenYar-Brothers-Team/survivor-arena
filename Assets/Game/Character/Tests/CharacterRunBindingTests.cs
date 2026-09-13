using NUnit.Framework;

namespace Game.Character.Tests
{
    public class CharacterRunBindingTests
    {
        [Test]
        public void CharacterDeath_ChangesRunningRunToLost()
        {
            var run = new Run.RunModel();
            run.Start();
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));

            using (var health = new CharacterHealth(stats))
            using (new CharacterRunBinding(health, run))
            {
                health.TakeDamage(100f);

                Assert.AreEqual(Run.RunState.Lost, run.State);
            }
        }
    }
}
