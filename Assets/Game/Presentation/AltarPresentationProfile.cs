using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation.Json;

namespace Game.Presentation
{
    /// <summary>Type-specific altar sprites and shared polarity contour tuning; DECISION-0150.</summary>
    public sealed class AltarPresentationProfile
    {
        public IReadOnlyDictionary<ContentId, ContentRef<SpriteDefinition>> EffectVisuals { get; }
        public float AltarHeight { get; }
        public float ShrineHeight { get; }
        public float AltarContactRadius { get; }
        public float AltarContactOffsetY { get; }
        public float ShrineContactRadius { get; }
        public float ShrineContactOffsetY { get; }
        public float IdleBrightness { get; }
        public float ActiveBrightness { get; }
        public float RingAlpha { get; }
        public float RingThickness { get; }
        public int BoundaryLobes { get; }
        public float BoundaryInsetFraction { get; }
        public float BoundaryWeaveAlpha { get; }
        public float RestingAlpha { get; }
        public int SortingOrder { get; }
        public float StateRingRadius { get; }
        public float ShrineStateRingRadius { get; }
        public float StateRingAspect { get; }
        public float StateRingThickness { get; }
        public float GlowRadius { get; }
        public float GlowHeightFraction { get; }
        public float ReadyGlowAlpha { get; }
        public float ActiveGlowAlpha { get; }
        public float FlashSeconds { get; }
        public UnityEngine.Color GlowColor { get; }

        public AltarPresentationProfile(AltarPresentationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.EffectVisualIds == null || data.EffectVisualIds.Count == 0)
                throw new ArgumentException("Altar effectVisualIds required.");
            var visuals = new Dictionary<ContentId, ContentRef<SpriteDefinition>>();
            foreach (var entry in data.EffectVisualIds)
            {
                var effectId = new ContentId(entry.Key);
                var visual = new ContentRef<SpriteDefinition>(entry.Value);
                if (!effectId.IsValid || !visual.Id.IsValid) throw new ArgumentException("Altar effect and sprite IDs required.");
                visuals.Add(effectId, visual);
            }
            EffectVisuals = new System.Collections.ObjectModel.ReadOnlyDictionary<ContentId, ContentRef<SpriteDefinition>>(visuals);
            AltarHeight = Required(data.AltarHeight, "altarHeight");
            ShrineHeight = Required(data.ShrineHeight, "shrineHeight");
            AltarContactRadius = Required(data.AltarContactRadius, "altarContactRadius");
            AltarContactOffsetY = Required(data.AltarContactOffsetY, "altarContactOffsetY");
            ShrineContactRadius = Required(data.ShrineContactRadius, "shrineContactRadius");
            ShrineContactOffsetY = Required(data.ShrineContactOffsetY, "shrineContactOffsetY");
            NumericValidation.ValidatePositive(AltarContactRadius, nameof(AltarContactRadius));
            NumericValidation.ValidatePositive(ShrineContactRadius, nameof(ShrineContactRadius));
            NumericValidation.ValidateNonNegativeFinite(AltarContactOffsetY, nameof(AltarContactOffsetY));
            NumericValidation.ValidateNonNegativeFinite(ShrineContactOffsetY, nameof(ShrineContactOffsetY));
            RingThickness = Required(data.RingThickness, "ringThickness");
            NumericValidation.ValidatePositive(AltarHeight, nameof(AltarHeight));
            NumericValidation.ValidatePositive(ShrineHeight, nameof(ShrineHeight));
            NumericValidation.ValidatePositive(RingThickness, nameof(RingThickness));
            BoundaryLobes = data.BoundaryLobes ?? throw new ArgumentException("Altar boundaryLobes required.");
            NumericValidation.ValidateRange(BoundaryLobes, 3, 12, nameof(BoundaryLobes));
            BoundaryInsetFraction = Fraction(data.BoundaryInsetFraction, "boundaryInsetFraction");
            NumericValidation.ValidateRange(BoundaryInsetFraction, .01f, .1f, nameof(BoundaryInsetFraction));
            BoundaryWeaveAlpha = Fraction(data.BoundaryWeaveAlpha, "boundaryWeaveAlpha");
            IdleBrightness = Fraction(data.IdleBrightness, "idleBrightness");
            ActiveBrightness = Fraction(data.ActiveBrightness, "activeBrightness");
            RingAlpha = Fraction(data.RingAlpha, "ringAlpha");
            RestingAlpha = Fraction(data.RestingAlpha, "restingAlpha");
            if (ActiveBrightness < IdleBrightness) throw new ArgumentException("Active altar must be at least as bright as idle.");
            SortingOrder = data.SortingOrder ?? throw new ArgumentException("Altar sortingOrder required.");
            StateRingRadius = Required(data.StateRingRadius, "stateRingRadius");
            ShrineStateRingRadius = Required(data.ShrineStateRingRadius, "shrineStateRingRadius");
            StateRingThickness = Required(data.StateRingThickness, "stateRingThickness");
            GlowRadius = Required(data.GlowRadius, "glowRadius");
            FlashSeconds = Required(data.FlashSeconds, "flashSeconds");
            NumericValidation.ValidatePositive(StateRingRadius, nameof(StateRingRadius));
            NumericValidation.ValidatePositive(ShrineStateRingRadius, nameof(ShrineStateRingRadius));
            NumericValidation.ValidatePositive(StateRingThickness, nameof(StateRingThickness));
            NumericValidation.ValidatePositive(GlowRadius, nameof(GlowRadius));
            NumericValidation.ValidatePositive(FlashSeconds, nameof(FlashSeconds));
            StateRingAspect = Fraction(data.StateRingAspect, "stateRingAspect");
            NumericValidation.ValidatePositive(StateRingAspect, nameof(StateRingAspect));
            GlowHeightFraction = Fraction(data.GlowHeightFraction, "glowHeightFraction");
            ReadyGlowAlpha = Fraction(data.ReadyGlowAlpha, "readyGlowAlpha");
            ActiveGlowAlpha = Fraction(data.ActiveGlowAlpha, "activeGlowAlpha");
            if (ActiveGlowAlpha < ReadyGlowAlpha) throw new ArgumentException("Active glow must be brighter than ready glow.");
            if (!UnityEngine.ColorUtility.TryParseHtmlString(data.GlowColor, out var glowColor))
                throw new ArgumentException("Altar glowColor required as HTML color.");
            GlowColor = glowColor;
        }

        public IEnumerable<ContentReference> GetReferencedContent()
        {
            foreach (var visual in EffectVisuals.Values) yield return visual.ToReference();
        }

        private static float Required(float? value, string name) => value ?? throw new ArgumentException("Altar " + name + " required.");
        private static float Fraction(float? value, string name)
        {
            var number = Required(value, name);
            NumericValidation.ValidateRange(number, 0f, 1f, name);
            return number;
        }
    }
}
