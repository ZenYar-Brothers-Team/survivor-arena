using System;
using Game.ScreenEvents.Json;

namespace Game.ScreenEvents
{
    /// <summary>One weighted candidate of a schedule stage; the event id is resolved by <see cref="ScreenEventsDefinition"/>.</summary>
    public sealed class ScreenEventPoolEntry
    {
        public ScreenEventDefinition Event { get; }
        public float Weight { get; }

        public ScreenEventPoolEntry(ScreenEventDefinition definition, ScreenEventPoolEntryData data)
        {
            Event = definition ?? throw new ArgumentNullException(nameof(definition));
            Weight = data?.Weight ?? throw new ArgumentException($"{definition.Id}: pool weight is required.");
            Game.Content.NumericValidation.ValidatePositive(Weight, nameof(Weight));
        }
    }
}
