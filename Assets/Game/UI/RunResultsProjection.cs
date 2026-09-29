using System;
using System.Linq;
using Game.ActiveSkill;
using Game.Content;
using Game.Field;
using Game.Meta;
using Game.Presentation;
using Game.Progression;
using Game.Run;
using UnityEngine;

namespace Game.UI
{
    public static class RunResultsProjection
    {
        public static RunResultsViewState Create(RunOutcome result, IProfileService profile, ContentRegistry registry = null)
        {
            var receipt = profile.LastReceipt;
            if (receipt?.RunId != result.RunId.ToString()) receipt = null;
            result.Contributions.TryGetValue("experience", out var xp);
            var kills = result.Contributions.Values.Where(c => c.Kills.HasValue).ToArray();
            var sets = result.Contributions.Values.Where(c => c.Sets != null).SelectMany(c => c.Sets)
                .Select(c => c.ContentId).Distinct().Select(id => Content(id, profile.Catalog, registry)).ToArray();
            var unlocks = receipt == null ? Array.Empty<ResultContentViewState>() : receipt.NewUnlocks.Distinct()
                .Select(id => Content(id, profile.Catalog, registry)).ToArray();
            var selection = result.Selection == null ? "" :
                Content(result.Selection.CharacterId.ToString(), profile.Catalog, registry).Name + " · " +
                Content(result.Selection.FieldId.ToString(), profile.Catalog, registry).Name;
            var victory = result.Reason == RunCompletionReason.Victory;
            var title = victory ? "Победа" : result.Reason == RunCompletionReason.Defeat ? "Поражение" :
                result.Reason == RunCompletionReason.Error ? "Забег остановлен" : "Забег прерван";
            var failed = profile.State == ProfileState.PendingResult;
            return new RunResultsViewState(title, selection, TimeSpan.FromSeconds(result.ElapsedSeconds).ToString(@"mm\:ss"),
                xp?.Level, kills.Length == 0 ? (int?)null : kills.Sum(c => c.Kills.Value),
                receipt?.LevelReward, receipt?.BookReward, receipt?.Total, victory, failed,
                failed ? "Не удалось сохранить результат" : receipt == null ? "Сохраняем результат…" : "",
                sets, unlocks);
        }

        public static ResultContentViewState Content(string id, MetaCatalog catalog, ContentRegistry registry = null)
        {
            catalog.Unlocks.TryGetValue(id, out var rule);
            var name = rule?.Name ?? "Неизвестное содержимое";
            var kind = rule?.Kind switch { "character" => "Персонаж", "field" => "Карта", "set" => "Сет", _ => "Умение" };
            Sprite sprite = null;
            if (registry != null && registry.TryGet<IContentDefinition>(new ContentId(id), out var definition))
            {
                ContentRef<SpriteDefinition> icon = default;
                switch (definition)
                {
                    case CharacterDefinition character:
                        name = character.DisplayName; kind = "Персонаж";
                        if (character.Presentation != null) icon = character.Presentation.Icon;
                        break;
                    case FieldDefinition field:
                        name = field.DisplayName; kind = "Карта"; icon = field.Thumbnail ?? default; break;
                    case ActiveSkillProgressionDefinition active:
                        name = active.DisplayName; icon = active.Icon; break;
                    case PassiveProgressionDefinition passive:
                        name = passive.DisplayName; icon = passive.Icon; break;
                    case SetDefinition set:
                        name = set.DisplayName; kind = "Сет"; icon = set.Icon; break;
                    case BuildEntryDefinition entry: name = entry.DisplayName; break;
                }
                if (icon.Id.IsValid) sprite = icon.Resolve(registry).Sprite;
            }
            if (sprite == null && rule?.Kind == "field" && registry != null &&
                registry.TryGet<SpriteDefinition>(new ContentId(id + "-VISUAL-BACKGROUND"), out var background))
            {
                background.RequireRole(SpriteRole.Background);
                sprite = background.Sprite;
            }
            return new ResultContentViewState(id, name, kind, sprite);
        }
    }
}
