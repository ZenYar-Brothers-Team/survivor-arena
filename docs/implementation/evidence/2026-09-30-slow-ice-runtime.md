# Лёд замедления: утверждённый raster и runtime — 2026-09-30

Решение: [DECISION-0108](../../decisions/0108-slow-status-look-preview.md). Пользователь выбрал сочетание полоски и льда, затем утвердил показанный `TestResults/visual-previews/slow-ice-candidate-v1.png` словами «Подтверждаю.»

## Ассет

- `TestResults/slow-ice-art-packet.json` привязал approval к SHA-256 `eedc9202dfe5a084d73af30618bd1bb369eed78099b03bf464542080345217ae`.
- `python scripts/art_pipeline.py TestResults/slow-ice-art-packet.json` — PLAN 6 files; `--apply` — APPLIED 6 files.
- Master и provenance: `Art/Source/VFX/slow-status/ice/`; runtime: `Assets/Resources/Art/VFX/slow-status-ice-mask.png`; visual ID `SLOW-STATUS-VISUAL-MASK`, роль `Mask`, размер 512×512. Исходный генератор и точный prompt записаны в `asset-record.json`.
- Повторный `python scripts/art_pipeline.py TestResults/slow-ice-art-packet.json` после установки: PLAN 0 files; manifest stage `Integrated`. Открыт только игровой просмотр масштаба.
- Подготовленный runtime PNG сохраняет прозрачный центр, 8-pixel padding и крупные ледяные грани. Доля полностью прозрачных пикселей master — 80.2%; runtime — 83.8%. Художественное approval относится к выбранному master; игровой масштаб проверяется отдельно.

## Runtime

- `FixtureSlowStatusPresentation.json` ссылается на `SLOW-STATUS-VISUAL-MASK`; `FixtureSlowStatusPresentationCatalog` разрешает ID через `FixtureSpriteCatalog.CreateOne` с проверкой роли `Mask`. `SurvivorArena/SpriteIceMask` читает общую ледяную текстуру и альфу текущего body sprite. `SlowIce` остаётся одним visual-only renderer под `BodyRoot`, следует за позой и отражением персонажа. UV нормализуются по `textureRect` body sprite; материал и mask texture переиспользуются.
- `SlowStatusStyle.Ice` включает лёд и полоску времени. Этот стиль выбран по умолчанию для замедленных enemies с body art; DEV сохраняет сравнение прежних вариантов. Пауза, окончание замедления, смерть и pool return сохраняют прежние правила очистки.
- Gameplay root, collider, source/timer замедления, kills и rewards не изменены.

## Проверки

- `python scripts/check_project.py --scope art` — Unity 6000.6.0f1 batch, **63/63 Game.* EditMode PASS**, 270 manifest owner/role records PASS; повтор после catalog binding: `TestResults/checks/20260930T095755-979964Z/summary.json`.
- Targeted `SlowStatusPresentationTests` после catalog binding: **6/6 EditMode PASS**, `TestResults/checks/20260930T095723-466411Z/summary.json`.
- После финального catalog binding graphics EditMode **432/432 PASS** (`TestResults/checks/20260930T100414-040986Z/summary.json`), slow graphics PlayMode **1/1 PASS** (`TestResults/checks/20260930T100341-149001Z/summary.json`) и весь graphics PlayMode **59/59 PASS** (`TestResults/checks/20260930T100507-222378Z/summary.json`); failed/skipped 0.
- Полный `check_project` пока заблокирован двумя balance validators на параллельных изменениях ENEMY-001 movementSpeed и boss teleport rule; generation `UP TO DATE`.

## Замена v002 после просмотра маленьких врагов

- Пользователь сообщил, что v001 почти не видна на маленьких врагах, и выбрал первую из показанных текстур. Первый output исходной генерации скопирован в `TestResults/visual-previews/slow-ice-candidate-first.png`; SHA-256 `1bd0e493462b7a99691528650d84d767ff37aded51eb503a6e568feb68a4df3f`. Исходный prompt сохранён дословно в пакете и обновлённом `asset-record.json`.
- `TestResults/slow-ice-first-replacement-packet.json`: PLAN/APPLIED 5 files; повторный PLAN 0. Предыдущий `v001/concept-01.png` сохранён, новый master — `v002/concept-01.png`. Runtime path, visual ID и `.meta` сохранены. Новый 512×512 runtime PNG имеет 62.7% полностью прозрачных пикселей против 83.8% у v001, поэтому крупные грани занимают больше площади маленького body sprite.
- Шейдер использует нормализованные UV каждого body sprite; оверлей копирует его sprite и следует масштабу `BodyRoot`/`VisualRoot`. Полоска позиционируется ниже нижней границы `SpriteRenderer.bounds` с `barOffsetY` как зазором до верхней границы полоски. Это устраняет перекрытие нижнего льда независимо от размера врага.
- `python scripts/check_project.py --scope art` — **64/64 EditMode PASS**, 270 manifest owner/role records PASS (`TestResults/checks/20260930T103829-321006Z/summary.json`). `SlowStatusSmokeTests` — **1/1 graphics PlayMode PASS** (`TestResults/checks/20260930T104037-139644Z/summary.json`). Новый `Apply_IceOnCompactBody_ScalesWithSpriteAndBarClearsItsBottom` проверяет размер оверлея, UV и зазор полоски на уменьшенном body sprite.
- Полный `check_project.py --scope full`: generation/audio прошли и **1089/1089 EditMode PASS**, но Unity batch crash в PlayMode до XML (`TestResults/checks/20260930T104511-449907Z/PlayMode.log`, exit 3221225477), поэтому общий результат **NOT RUN / INCOMPLETE**, не PASS. Следующая попытка graphics PlayMode остановилась до Unity: параллельная правка content sources сделала `ProductionCharacters.json` и `ProductionCharacterBaseline.json` stale. Эти файлы не входят в замену льда.
- Последующий полный graphics PlayMode выполнил 59 тестов: **58 passed, 1 failed, 0 skipped** (`TestResults/checks/20260930T105123-110906Z/PlayMode.xml`). Падение вне scope льда: `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions` — `Expected: 70; But was: 61`. Общий PASS для этого прогона не заявлен.

## Открыто

Показать v002 в игровом масштабе на маленьких ordinary, boss и Traveler, включая плотную волну. Отдельно оценить frame time при 300 ordinary, когда массовое замедление включает один дополнительный ice draw и два bar draw на каждого видимого замедленного врага.
