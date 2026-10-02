# FIELD-009 — подключение выбранного полотна D

Пользователь выбрал D после отклонения массивных stone A/B/C и поручил
встроить его в игру. [Контракт](../../decisions/0147-field009-holy-ground-art.md#уточнение-мостов-выбран-d),
[provenance](../../../Art/Candidates/field009-bridges-2026-10-02/lightweight-v2-record.json).

## Подключение

- FieldPlatformSurface shader получает opt-in veil mode, прозрачное полотно,
  редкие антиалиасные диагональные нити и тонкий край safe width. Время не
  используется, motion/bloom/частицы отсутствуют. Textured platform/ground branch прежняя.
- Bridge mesh передаёт world-unit длину вдоль и signed across coordinate, поэтому
  рисунок следует каждому мосту и не растягивается от длины или поворота.
- FieldPlatformSurfaceRuntime не создаёт thick face/rim/inlay на части union
  contour вне круглых платформ. Circle surface/ground/material, layout,
  IsWalkable, books и damage driver не меняются. Mesh/material ownership прежний.
- JSON DTO/domain требуют finite mode-specific параметры; отсутствие opt-in
  сохраняет legacy stone mode. Authoring — field009-platforms-v1.json;
  ProductionFieldEnvironmentPresentation регенерирован штатным генератором.
- Concept sheet не импортирован в Assets; D реализован процедурно, без raster
  replacement, source pixels, новых import settings, PNG или GUID.

## Проверки этой версии

- Generation: UP TO DATE; art manifest: 317/317 PASS.
- Existing approved FIELD-009 art packet PLAN: PASS, changedFiles=0.
- C# compile-only с установленным Unity 6000.6 Roslyn и существующими Unity
  assembly response files: Game.Presentation, Game.Bootstrap,
  Game.Bootstrap.Tests, Game.Bootstrap.PlayModeTests — COMPILE PASS.
  Выходы только в `TestResults/bridge-veil-compile/`, исходные Library assemblies
  не перезаписывались. Это не Unity test execution или shader compilation.
- Регрессии добавлены: local weave UV и отсутствие bridge masonry на 0/45/90°,
  непрерывная safe width, retained plaza volume, missing/non-finite parameter
  rejection, material cleanup. Тесты скомпилированы, но НЕ ЗАПУЩЕНЫ.
- Code graphics и art runners: Unity tests НЕ ЗАПУЩЕНЫ, WinError 10061 при
  обращении к открытому Editor REST. Evidence:
  `TestResults/checks/20261002T164431-645940Z/`,
  `TestResults/checks/20261002T164437-268645Z/`.
  Первоначальный combined regex отвергнут безопасным runner до запуска;
  повтор использовал поддерживаемый anchored namespace `^Game.Bootstrap.`.
- Editor process подтверждён, endpoints 8090–8100 не ответили. Batch поверх
  interactive Editor не запускался, Editor не закрывался. Для tests/capture
  требуется включить UnitySkills REST в этом проекте или закрыть Editor для
  следующего безопасного runner. Shader/runtime/новый gameplay capture не проверены.

Текущий execution state — только STATUS. Полный PASS, импорт/визуальная приёмка
нового runtime и завершение IP-23 не заявляются. Последующим запросом пользователь
поручил отдельно закоммитить выбранный D; прежние draft packets и отклонённые
арт-кандидаты не входят в этот коммит.
