using System;
using System.Collections.Generic;
using Game.Content.Json;
using Game.Presentation.Json;
using UnityEngine;

namespace Game.Presentation
{
    public static class FixturePresentationKitCatalog
    {
        public static IReadOnlyList<PresentationFixtureDefinition> Create()
        {
            var data = JsonContentFile.Load<PresentationFixtureData[]>("Content/Presentation/FixturePresentationKit");
            var definitions = new List<PresentationFixtureDefinition>();
            var seen = new HashSet<string>();
            foreach (var d in data ?? throw new InvalidOperationException("Missing fixture kit."))
            {
                if (d == null || !d.Role.HasValue || !d.X.HasValue || !d.Y.HasValue || !d.BodyMotion.HasValue ||
                    d.Color == null || d.Color.Length != 4 || !seen.Add(d.Id))
                    throw new InvalidOperationException("Invalid/duplicate presentation fixture entry.");
                definitions.Add(new PresentationFixtureDefinition(d.Id, d.Role.Value, new Vector2(d.X.Value, d.Y.Value),
                    new Vector2(d.Width, d.Height), new Color(d.Color[0], d.Color[1], d.Color[2], d.Color[3]), d.BodyMotion.Value));
            }
            return definitions;
        }
    }
}
