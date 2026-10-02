# FIELD-007 — review образов и плетёного контура

Контракт: [DECISION-0150](../../decisions/0150-field007-altar-type-visuals.md).
Текущий статус и оставшиеся gates: [STATUS](../STATUS.md).

## Изменения

Все 14 показанных PNG прямо выбраны пользователем: «Хорошо, подтверждаю».
[Approved packet](../../../Art/Packets/field007-altar-types-v1.json) фиксирует
bytes/hashes/prompts/approval; PLAN/APPLY — 59 записей. Подготовлены 14 Prop,
masters/provenance, registry/manifest/import profiles. Unity создал .meta;
substantive raster правок после выбора нет. `effectVisualIds` покрывает все
14 effect IDs, missing/extra mapping отклоняется; общего fallback нет.
Runtime PNG ветра и святыни опыта просмотрены: штатный fit/alpha preparation
снял остаточный ореол, сохранил выбранные силуэты.

По последующему уточнению пользователя нити отцентрированы на actual radius:
`r1/r2 = R × (1 ± d × cos(nθ) / 2)`; радиальная и векторная середина всегда R.
Containment чуть внутри середины истинен, чуть снаружи ложен; outer pixels
декоративны и не расширяют gameplay, collider или мини-карту.

`AltarPresentationRuntime` рисует две противоположные плавные восьмилопастные
линии по обе стороны actual radius. 192 сегмента на линию, без аллокаций геометрии в Apply;
small state arc и crown mesh сохраняют 64 сегмента. Цвет берётся из эффекта,
форма от эффекта/полярности не зависит. Closure копирует первую точку в последнюю.
Near visibility, paused run clock и shutdown распространяются на вторую линию.
Authoring: ring alpha 0.85, width 0.09 wu, rest alpha 0.4, state width 0.15 wu;
boundary lobes 8 / inset fraction 0.06 / second-strand alpha multiplier 0.65.
DTO/domain проверяют presence, finite/ranges; generator передаёт source profile.

Добавлен `AltarBoundary_AllTypesShareNeutralBraid_MidpointIsExactReachAndFrozenOnPause`:
все 14 эффектов, нормализованное совпадение формы, радиус, замыкание, цвет,
различимость нитей, pause, near hiding и cleanup. Existing state/contact checks сохранены.

14 независимых imagegen-запросов, byte-identical copies вне Assets,
[галерея](../../../Art/Candidates/field007-altar-types-2026-10-02/index.html),
дословные prompts/references, hashes и первоначальный pending art packet.
После выбора используется approved packet; различия пар ветра/силы и
рельеф святынь сохранены по выбору пользователя. Крупные реликвии
различаются в уменьшенном просмотре; runtime derivatives — штатный fit 512,
padding 24, alphaNoiseCutoff 32, PPU 256, без изменения игровых контактов.

## Свежие проверки после approval, Unity 6000.6.0f1

- `art_pipeline.py Art/Packets/field007-altar-types-v1.json`: PLAN / APPLIED, 59 записей.
- `check_project.py --scope full --graphics`: EditMode 1423/1427,
  4 failed / 0 skipped / third-party 0. XML:
  `TestResults/checks/20261002T183458-614901Z/EditMode.xml`.
  Generation UP TO DATE и audio PASS 28 files / 15 cues.
  Собственные altar tests 11/11, Game.Presentation.Tests 109/109 и
  RuntimeContentCatalogTests 3/3 (presentation/catalog вместе 112/112) PASS.
  Полный PlayMode не начался из-за четырёх EditMode failures вне этой правки.
- Отдельный graphics PlayMode, filter
  `^Game\.Bootstrap\.PlayModeTests\.ProductionMonasterySmokeTests\.`: 1/1 PASS,
  0 failed/skipped, third-party 0, безопасный batch при закрытом Editor.
  `TestResults/checks/20261002T183848-973559Z/summary.json`.
  Positive/negative/shrine captures просмотрены AI и сохранены в той же папке:
  контур различим, body не обрезан, прозрачность чистая; плотный бой остаётся
  отдельным пользовательским visual review. Dev unlock/card, 36 объектов,
  actual radii, pause/shutdown прошли.
- Manifest PASS 331 owner/role records; generation `--check` UP TO DATE;
  `git diff --check` затронутых paths без whitespace errors.

Четыре failures full вне этой правки (эти файлы не исправлялись здесь):

1. `Game.Bootstrap.Tests.FieldPlatformSurfaceTests.VeilBridge_LocalWeaveCoordinates_NoMasonryOutsidePlazas_ContinuousSafeWidth(45.0f)`:
   Vector2[4] index 1 не совпал exact comparison; печатные значения обоих (24.00, −2.75).
2. `Game.Meta.Tests.MetaProfileTests.FieldClears_OnlyGrantTheFieldBasedPartOfTheMixedCatalog`:
   after FIELD-001 expected (11,11,6), actual (11,11,11).
3. `Game.Meta.Tests.MetaProfileTests.NewProductionProfile_StartsWithExactlyTheStartupSet`:
   expected 5 sets, actual 10; дополнительные SET-023/024/026/030/034.
4. `Game.UI.Tests.MetaShopTests.Unlocks_FreshProfile_IncludesInitialAndKeepsOnlyCharactersHidden`:
   те же пять дополнительных sets отсутствуют в expected startup unlocks.

Full PASS не заявляется. Первая попытка batch обнаружила CS1501 в новом тесте:
Contains с тремя аргументами заменён на actual `ZonePlacement.Contains`, затем
тест прошёл. Другой runner занимал lock — собственный повтор дождался освобождения;
batch поверх interactive Editor не запускался, grants/modes не менялись.

## Первоначальные попытки до approval (исторические результаты)

- `python scripts/content/generate.py`: обновлён ProductionFieldEnvironmentPresentation.
- `python scripts/art_pipeline.py Art/Candidates/field007-altar-types-2026-10-02/art-packet.pending.json`:
  `FAIL: Packet needs schemaVersion 1 and recorded user approval` — ожидаемый
  отказ pending packet; PLAN/APPLY для approved art не заявляются.
- `python scripts/check_project.py --scope full --graphics`: generation UP TO DATE,
  audio PASS (28 files / 15 cues), затем WinError 10061 на локальном Editor REST.
  Unity tests не запускались; fresh runtime PASS отсутствует.
- Процессы проверены с разрешённым чтением: interactive Unity 6000.6.0f1 на
  survivor-arena открыт. После завершения ранее существовавшего batch остался Editor.
  Health discovery 8090–8100 не нашёл отвечающего API. Batch поверх Editor не запускался,
  Editor не закрывался, режимы/grants не изменялись.

- `python scripts/check_project.py --scope art`: NOT RUN / INCOMPLETE,
  другой check runner уже владеет project lock; параллельный Unity run не запущен.
- `python scripts/validate-art-manifest.py`: PASS, 317 owner/role records;
  pending candidates не вошли в runtime manifest.
- `python scripts/content/generate.py --check`: UP TO DATE.
- `git diff --check` по затронутым presentation/test/docs/candidate paths:
  без ошибок whitespace; уведомления о CRLF normalization не являются ошибками.
