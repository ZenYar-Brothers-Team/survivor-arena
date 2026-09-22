using Game.Content;
using Game.Run;
namespace Game.Meta.Tests
{
    public static class MetaTestData
    {
        public static RunModel Run(int level = 20, long books = 100, string field = "FIELD-001", float duration = 900)
        {
            var run = new RunModel(duration);
            run.ConfigureSelection(new RunSelectionSnapshot(new ContentId("CHAR-001"), new ContentId(field),
                new ContentId("FIXTURE-ENV"), new ContentId("FIXTURE-WAVE")));
            run.RegisterOutcomeContributor(new MetaOutcomeContributor("experience", new RunOutcomeContribution(level: level)));
            run.RegisterOutcomeContributor(new MetaOutcomeContributor("draft", new RunOutcomeContribution(draftTotals: new RunDraftSnapshot(2,0,2,0,books))));
            return run;
        }
    }
}
