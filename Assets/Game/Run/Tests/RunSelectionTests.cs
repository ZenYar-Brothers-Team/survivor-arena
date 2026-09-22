using System;
using NUnit.Framework;
namespace Game.Run.Tests
{
    public sealed class RunSelectionTests
    {
        [Test]
        public void Selection_BoundOnce_SurvivesTerminalSnapshot()
        {
            var run = new RunModel();
            var selection = new RunSelectionSnapshot("CHAR", "FIELD", "ENV", "WAVE");
            run.ConfigureSelection(selection);
            Assert.Throws<InvalidOperationException>(() => run.ConfigureSelection(selection));
            run.Start(); run.Kill();
            Assert.AreSame(selection, run.Outcome.Selection);
            Assert.IsNull(new RunModel().Selection);
        }
        [Test]
        public void Selection_AfterStart_Rejects()
        {
            var run = new RunModel(); run.Start();
            Assert.Throws<InvalidOperationException>(() => run.ConfigureSelection(new RunSelectionSnapshot("C", "F", "E", "W")));
        }
    }
}
