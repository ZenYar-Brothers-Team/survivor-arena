using System;
using Game.Content.Json;
using Game.Presentation.Json;

namespace Game.Presentation
{
    public static class FixturePresentationFeedbackCatalog
    {
        public static PresentationFeedbackProfile Create()
        {
            var d = JsonContentFile.Load<PresentationFeedbackProfileData>("Content/Presentation/FixturePresentationFeedback");
            if (d == null || !d.FadeSeconds.HasValue || !d.ProcSeconds.HasValue || !d.ProcScale.HasValue)
                throw new InvalidOperationException("Presentation feedback requires fadeSeconds, procSeconds and procScale.");
            return new PresentationFeedbackProfile(d.FadeSeconds.Value, d.ProcSeconds.Value, d.ProcScale.Value);
        }
    }
}
