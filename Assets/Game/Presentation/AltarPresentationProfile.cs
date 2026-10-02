using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation.Json;

namespace Game.Presentation
{
    /// <summary>Effect-independent altar sprites and world-space visual tuning; DECISION-0145.</summary>
    public sealed class AltarPresentationProfile
    {
        public ContentRef<SpriteDefinition> Positive { get; }
        public ContentRef<SpriteDefinition> Negative { get; }
        public ContentRef<SpriteDefinition> Shrine { get; }
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
            Positive = new ContentRef<SpriteDefinition>(data.PositiveVisualId);
            Negative = new ContentRef<SpriteDefinition>(data.NegativeVisualId);
            Shrine = new ContentRef<SpriteDefinition>(data.ShrineVisualId);
            if (!Positive.Id.IsValid || !Negative.Id.IsValid || !Shrine.Id.IsValid) throw new ArgumentException("Altar sprite IDs required.");
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
            yield return Positive.ToReference(); yield return Negative.ToReference(); yield return Shrine.ToReference();
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
