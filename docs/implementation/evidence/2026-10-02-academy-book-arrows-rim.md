# FIELD-006 — книжка опыта, стрелки отталкивания и середина контура

Contract: [DECISION-0152](../../decisions/0152-academy-rim-midpoint-and-effect-glyphs.md).
Текущий execution status хранится только в STATUS.md.

## Changes and approval

Пользователь уточнил символ опыта: «Опыт мы обозначаем книжкой»; оба показанных
PNG принял прямым ответом «Да, подключить оба». Встроенный OpenAI image generation
создал отдельные прозрачные книжку и outward arrows. Мастера и дословные prompts
хранятся в `Art/Source/VFX/field-006-zone-experience/telegraph/` и
`Art/Source/VFX/field-006-zone-knockback/telegraph/`.
Пакет `Art/Packets/field006-new-effect-glyphs-v1.json` сначала PLAN, затем APPLIED (11 files).
Обе runtime textures 256×256, полные RGBA canvas, импорт Single/FullRect, PPU 128;
у книжки alphaNoiseCutoff=12 при техническом fit. Runtime PNG осмотрены отдельно.

Profile ссылки разрешают новые Telegraph assets; отдельный SpriteRenderer заменяет
mesh только для Experience/Knockback. Их orientation остаётся постоянной,
Y projection равна 0.8, видимость следует общей фазе и active window.
Shutdown/reinitialize очищает дочерние renderers вместе с прежними layers.
Mesh fallback опыта также использует открытый книжный символ.

Rim midpoint задан profile.rimReferenceRadius=0.72 по радиальной медиане alpha>128
утверждённого растра (0.71994). Масштаб оправы учитывает activeRadiusFraction текущих
данных, без изменения actual containment, payload, расписания или размера glyph.

## Checks

- `--scope art`: Unity 6000.6.0f1, EditMode 114/114, zero failed/skipped;
  generation UP TO DATE; manifest 333 records PASS.
  `TestResults/checks/20261002T184642-756836Z/summary.json`.
- Итоговые seal EditMode: 13/13, zero failed/skipped, graphics enabled;
  `TestResults/checks/20261002T184759-186910Z/EditMode.xml`.
  Первое PlayMode в этом запуске: 1/3; два stale assertions (7 scheduled vs 50,
  8 kinds vs 9). Исправлены под DECISION-0151; visual grid расширен до 3×3.
- Предварительный full graphics остановился в EditMode: 1429/1433, four failures
  вне данного art/presentation scope. PlayMode full не запускался после EditMode FAIL.
  `TestResults/checks/20261002T184259-007401Z/EditMode.xml`.
- Итоговый graphics PlayMode: 3/3, zero failed/skipped,
  `TestResults/checks/20261002T184943-716714Z/summary.json`.
  Пройдены реальный UI launch FIELD-006, legacy test field и все девять печатей.
  Осмотрены свежие `TestResults/academy-seals-active.png` и
  `TestResults/academy-field006-gameplay.png`: книжка и стрелки различимы,
  центральные glyphs не перекрываются, наземные печати сжаты.

Предварительные full failures:

1. `Game.Bootstrap.Tests.FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`:
   Vector2 array index 1 differs, printed expected/actual both `(24.00, -2.75)` (precision).
2. `Game.Meta.Tests.MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`:
   after FIELD-001 expected `(11, 11, 6)`, actual `(11, 11, 11)`.
3. `Game.Meta.Tests.MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`:
   expected 5 strings, actual 10, extra `SET-023`, `SET-024`, `SET-026` etc.
4. `Game.UI.Tests.MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`:
   expected SET-001/004/006/010/017; actual also SET-023/024/026/030/034.

Эти проверки не менялись ради art scope. Полный PASS не заявляется.
