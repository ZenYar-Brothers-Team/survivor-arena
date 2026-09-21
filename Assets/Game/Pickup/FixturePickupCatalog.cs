using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Game.Content;
using Game.Content.Json;
using Game.Enemy;
using Game.Field;
using Game.Pickup.Json;
using Newtonsoft.Json;
using UnityEngine;
namespace Game.Pickup
{
    public sealed class FixturePickupCatalog : IContentDefinition, IReferencesContent
    {
        public ContentId Id => new ContentId("FIXTURE-PICKUP-CONFIG");
        public IReadOnlyList<PickupDefinition> Definitions { get; }
        public PickupDefinition Potion { get; }
        public PickupDefinition Book { get; }
        public float BaseChance { get; }
        public int Seed { get; }
        public float PlacementSkin { get; }
        public float FeedbackSeconds { get; }
        public IReadOnlyDictionary<ContentId, float> EnemyChances { get; }
        public IReadOnlyDictionary<ContentId, float> FieldChances { get; }
        private FixturePickupCatalog(PickupCatalogData data)
        {
            BaseChance = data.BaseChance ?? throw new ArgumentException("baseChance is required.");
            Seed = data.Seed ?? throw new ArgumentException("seed is required.");
            PlacementSkin = data.PlacementSkin ?? throw new ArgumentException("placementSkin is required.");
            FeedbackSeconds = data.FeedbackSeconds ?? throw new ArgumentException("feedbackSeconds is required.");
            NumericValidation.ValidateRange(BaseChance, 0, 1, nameof(BaseChance));
            NumericValidation.ValidatePositive(PlacementSkin, nameof(PlacementSkin));
            NumericValidation.ValidatePositive(FeedbackSeconds, nameof(FeedbackSeconds));
            Definitions = (data.Pickups ?? throw new ArgumentException("pickups are required.")).Select(ToDefinition).ToList().AsReadOnly();
            var byId = Definitions.ToDictionary(definition => definition.Id);
            if (!byId.TryGetValue(new ContentId(data.PotionId), out var potion) || potion.Kind != PickupRewardKind.Potion ||
                !byId.TryGetValue(new ContentId(data.BookId), out var book) || book.Kind != PickupRewardKind.Book)
                throw new ArgumentException("Potion and Book bindings must resolve to the correct reward kind.");
            Potion = potion; Book = book;
            EnemyChances = ToChances(data.EnemyChances); FieldChances = ToChances(data.FieldChances);
        }
        private static IReadOnlyDictionary<ContentId, float> ToChances(Dictionary<string, float> source)
        {
            if (source == null) throw new ArgumentException("Chance override maps must be explicit, including empty maps.");
            var result = new Dictionary<ContentId, float>();
            foreach (var pair in source) { NumericValidation.ValidateRange(pair.Value, 0, 1, pair.Key); result.Add(pair.Key, pair.Value); }
            return new ReadOnlyDictionary<ContentId, float>(result);
        }
        private static PickupDefinition ToDefinition(PickupData data)
        {
            if (data == null || data.Color == null || data.Color.Length != 4) throw new ArgumentException("Pickup RGBA requires four values.");
            return new PickupDefinition(data.Id, data.Kind ?? throw new ArgumentException("kind is required."),
                data.Healing ?? throw new ArgumentException("healing is required, including Book zero."),
                data.ContactRadius ?? throw new ArgumentException("contactRadius is required."), data.LifetimeSeconds,
                data.Marker, new Color(data.Color[0], data.Color[1], data.Color[2], data.Color[3]),
                data.MarkerSize ?? throw new ArgumentException("markerSize is required."));
        }
        public float Chance(ContentId enemy, ContentId field, float multiplier) => PotionDropPolicy.Chance(BaseChance,
            FieldChances.TryGetValue(field, out var f) ? f : (float?)null,
            EnemyChances.TryGetValue(enemy, out var e) ? e : (float?)null, multiplier);
        public IEnumerable<ContentReference> GetReferencedContent()
        {
            foreach (var definition in Definitions) yield return new ContentRef<PickupDefinition>(definition.Id).ToReference();
            foreach (var id in EnemyChances.Keys) yield return new ContentRef<EnemyDefinition>(id).ToReference();
            foreach (var id in FieldChances.Keys) yield return new ContentRef<FieldDefinition>(id).ToReference();
        }
        public static FixturePickupCatalog Create() => FromJson(JsonContentFile.ReadText("Content/Pickups/FixturePickups"));
        public static FixturePickupCatalog FromJson(string json) => new FixturePickupCatalog(
            JsonConvert.DeserializeObject<PickupCatalogData>(json, JsonContentFile.Settings) ?? throw new ArgumentException("Pickup config is required."));
    }
}
