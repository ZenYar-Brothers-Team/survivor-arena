# Pickup art and seeded drop scatter — 2026-09-22

## Scope

По поручению пользователя одним пакетом подготовлены и подключены три world pickup visuals:

- cyan crystal для physical XP drop;
- зелёное лечебное зелье `PICKUP-001`, используемое fixture potion;
- закрытая охристо-бордовая Traveler Book.

Каждый generated master сохранён неизменным в `Art/Source/Pickups/<owner>/selected-master.png` и `v001/concept-01.png`. Runtime derivatives нормализованы без искажения в прозрачный canvas 256×256 и импортированы через category `pickup`: 320 PPU, centered pivot, Full Rect, Bilinear, Clamp, no mipmaps, uncompressed. Provenance и SHA-256 записаны в asset records и manifest. Статус изображений — Review до пользовательского gameplay-scale approval.

## Runtime presentation

Все три visuals имеют `SpriteRole.Pickup`. Общий `PickupSpritePresentation` создаёт только дочерний `VisualRoot`, применяет небольшой bob и pulse в running-time и не меняет root/collider. Shutdown и pool return очищают sprite, tint, scale, position и phase. Старые text markers сохранены как fallback для synthetic definitions без resolved visual.

`FixturePickups.json` задаёт visual IDs/scales и `dropScatterRadius = 0.30`. XP использует тот же presentation через `PlayerExperienceRuntime`; Potion/Book получают resolved visuals из общего `ContentRegistry`.

## Scatter

XP, Potion и Book получают равномерное по площади диска смещение вокруг source position. Chance RNG и scatter RNG разделены. XP stream принадлежит `PlayerExperienceRuntime`, world-pickup stream — `WorldPickupRuntime`; оба пересоздаются из config seed при Initialize. Potion/Book после scatter проходят прежний reachable-point adapter. Reward identity, chance, pickup radius и stable processing order не меняются.

Основание: [DECISION-0043](../../decisions/0043-seeded-drop-scatter.md).

## Verification

- `python scripts/validate-art-manifest.py`: PASS, 20 owner/role records.
- Targeted EditMode: 2/2 — XP death scatter и WorldPickup sequential scatter.
- Full EditMode: **651/651**, 0 skipped.
- Full PlayMode after replacing obsolete text-marker assertion with sprite-role coverage: **25/25**, 0 skipped.
- Unity `6000.6.0f1`, 2026-09-22.

Critical paths present in the successful suites: content registry and imports, enemy death→XP, world Potion/Book contact and reward flow, pause/draft, pool reuse, gameplay composition and Gameplay smoke.

## Open review

Пользовательский gameplay-scale gate остаётся открытым: проверить относительный размер трёх предметов, читаемость в толпе, амплитуду bob/pulse и достаточность scatter radius `0.30`.

## Pickup-radius tuning

После первого gameplay-просмотра пользователь сообщил, что XP и Зелье подбирать слишком трудно. Базовый XP pickup radius текущих fixture characters увеличен с `0.20` до `0.50` world units. Contact radius Зелья и Книги увеличен с `0.22` до `0.40`; Книга сохраняет общий world-pickup contract. Размеры sprites, scatter radius и passive multiplier formula не менялись.

После tuning: JSON parse PASS; целевой `FixtureRuntimeContentCatalogTests.Create_BuildsOneValidatedRegistryForEveryRuntimeDefinition` — **1/1 EditMode**. Полный smoke не повторялся, поскольку runtime-код и schema не менялись.
