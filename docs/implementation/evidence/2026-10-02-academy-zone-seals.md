# Печати зон — проверка 2026-10-02

Контракт: [DECISION-0142](../../decisions/0142-academy-zone-seal-presentation.md).
Execution status и оставшиеся gates — только в [STATUS](../STATUS.md).

## Область

## Уточнение v4 — общие цели и delayed portal transit

Source `field006-zones-preview-v4`: восемь видов, 14 placements, семь fixed
дополнительно к максимуму двух random occurrences. Все имеют affectsBothSides=true;
старые поля без этой опции сохраняют свой target contract. Pink HEAL удалён;
зелёный источник 3 HP/с. ARCANE ID сохранён, damage bonus=0, action speed=0.5;
новый центральный символ — песочные часы с fast-forward стрелками.
Enemy area movement/action/regen/defense применяются только внутри; speed burst
сохраняется 8 с с time bar. Enemy influence очищается при pool reuse/teardown.

По следующему уточнению пользователя показанный upright портал принят как
центральный объект внутри сохранённой большой entry area. Portal packet PLAN →
APPLIED, SHA256 master `154268c49a508e6c89ae10004e6f8b7f63864efd5671e013716481ba372ef70c`;
runtime fit512, padding32, cropAlpha/cutoff32, PPU256, pivot0.5/0.0625.
Повторный PLAN обоих packets даёт 0 changed files. Portal canvas 1.6 units,
entry radius [1.166667,3.5], ground flatten 0.8; portal sprite upright независимо
от radius. Collapse/hidden travel/appearance = 0.18/1/0.25 с; SmoothStep camera
только игроку. Enemy cooldown отдельный per life. Unit transit восстанавливает
physics, movement/attacks, health lock и presentation на выходе/teardown.

Первый scoped EditMode: 355/355 PASS, но receipt NOT RUN / INCOMPLETE из-за
параллельного изменения inputs, `TestResults/checks/20261002T130923-270693Z/EditMode.xml`.
Свежий full graphics Unity 6000.6: **1385/1385 EditMode PASS, 64/68 PlayMode**,
0 skipped, third-party 0; `TestResults/checks/20261002T131631-614011Z/{EditMode,PlayMode}.xml`.
Generation UP TO DATE, audio integrity PASS. Полный verdict FAIL:

- `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`: Expected 70, actual 85.
- `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`: DEV-ZONES card bottom 1250, allowed 1081.
- `Game.Bootstrap.PlayModeTests.ProductionMonasterySmokeTests.DevUnlock_Field007Card_StartsThirtySixVisibleAltarsAndPausesCleanly`: Player is kept outside the altar foundation; Expected False, actual True.
- `Game.Bootstrap.PlayModeTests.ProductionFieldDevZonesSmokeTests.Academy_UnlocksThroughDevButton_StartsThroughFieldCard_AndRunsSeals`: paused camera expected (15.15,44.02,-10), actual (15.17,44.02,-10).

Последняя ошибка исправлена: camera focus и camera pose публикуются вместе через
CameraFollowTarget, без дополнительного LateUpdate шага после pause. Повторные
scoped проверки фиксируются ниже. Чужие UI/altar failures здесь не исправлялись.

После camera fix: **12/12 EditMode + 3/3 PlayMode PASS**, 0 skipped,
`TestResults/checks/20261002T132334-513139Z/{EditMode,PlayMode}.xml`; receipt INCOMPLETE
только из-за изменения чужих inputs во время прогона. Подтверждены player/enemy
disappearance, physics/actor restore, paused intermediate camera, large entry
area и constant upright portal size. На отдельном showcase выявлена зависимость
новых SpriteRenderer от default material: без scene light art темнел. Оправа и
doorway теперь имеют собственный явно unlit Sprites/Default material.
Финальный art scope после этой правки: **111/111 EditMode PASS**, manifest **316/316**,
`TestResults/checks/20261002T135106-893290Z/summary.json`, graphics enabled.
Свежий стабильный graphics run после unlit fix: **12/12 EditMode + 3/3 PlayMode PASS**,
0 skipped, third-party 0, `TestResults/checks/20261002T135210-855853Z/summary.json`.
Active showcase осмотрен: цвета оправ восстановлены, action-speed hourglass виден,
upright doorway находится в центре большой наземной области. На общей панели
его character-scale размер закономерно мал относительно крупных зон; его размер
не умножается на радиус области. После этого уточнено personal cooldown presentation:
shared doorway не темнеет из-за cooldown одной цели; legacy player-only portal
сохраняет прежнюю rest presentation. Domain portal size test это проверяет.
Финальная проверка этого уточнения: **12/12 EditMode + 3/3 PlayMode без ошибок**,
0 skipped, third-party 0; `TestResults/checks/20261002T135445-265418Z/{EditMode,PlayMode}.xml`.
Receipt INCOMPLETE из-за параллельного изменения inputs; новый reusable PASS не
заявлен. Исполненные assertions и свежие graphics captures подтверждены; code/tests
Академии во время этого запуска не менялись. Active frame осмотрен повторно:
оправа цветная, часы со стрелками и центральный doorway видны.

## Выбранная оправа A и масштабируемый символ

Пользователь выбрал первый raster-концепт и попросил отдельный символ эффекта.
Packet `Art/Packets/field006-zone-seal-v001-2026-10-02.json` PLAN → APPLIED;
source SHA256 `7543670c5b7de96e1260a0ac188efc5ffdb67cd138bb7a337c83646dcd8aad6e`.
Runtime 512×512 RGBA, PPU 256, центр 0.5/0.5. Оправа подменяет рисунок mesh-обода,
символ остаётся отдельным неподвижным слоем с glyphScale=1.5. Оба следуют occurrence
radius; наземное сжатие 0.8 и вращение только relocating контура сохранены.
Portal исключён из замены A. Новый portal candidate — вертикальный вход с глубиной,
1024×1536 RGBA, SHA256 `154268c49a508e6c89ae10004e6f8b7f63864efd5671e013716481ba372ef70c`;
показан пользователю, не импортирован до утверждения конкретного изображения.

Art scope: 107/107 EditMode PASS, 0 skipped, manifest 315/315 PASS,
`TestResults/checks/20261002T121951-603085Z/summary.json`.
Первый отдельный graphics seal run выявил несовместимость MeshRenderer и
SpriteRenderer на одном GameObject (6 failures, NullReferenceException).
Оправа вынесена в отдельный дочерний объект. Повторный graphics run PASS:
8/8 EditMode + 2/2 PlayMode, 0 skipped, third-party 0,
`TestResults/checks/20261002T122253-563324Z/summary.json`. Реальный Dev unlock →
FIELD-006 launch и legacy zone launch PASS. Снимок игрового запуска осмотрен:
земля новая, масштабируемая снежинка читается, зона пока в слабой стадии подготовки.
Финальный art scope после исправления PASS: 107/107 EditMode, manifest 315/315,
`TestResults/checks/20261002T122353-785726Z/summary.json`.
Отдельный graphics showcase 1/1 PlayMode PASS:
`TestResults/checks/20261002T122454-522117Z/summary.json`;
кадры `TestResults/academy-seals-{waiting,preparing,active}.png` показывают восемь
видов и стадии. Portal в этом showcase ещё использует прежний mesh-рисунок.
Кадр active осмотрен AI: неровные оправы и разные центральные символы видны;
импульс заметно меньше длительных зон, символы не перекрывают оправу. Пользовательская
оценка игрового масштаба остаётся отдельным review.

## Отзыв v3 — occurrence radius и random scheduler

Источник FIELD-006 revision `field006-zones-preview-v3`: Burst radius 1.25,
остальные minRadius=Rmax/3; размеры выбираются на fixed placement или каждое random
appearance. `ZonePlacement.Radius` используется в rendering, Contains, proximity,
placement clearance и portal exit. Legacy catalogs без minRadius сохраняют свой radius.
Optional `randomSchedule` включает две цепочки, preview delay 8–16 с после исчезновения,
camera padding 2 units. Dormant placements повторно используются, скрыты и не занимают
пространство; предупреждение/активность/затухание целиком занимают слот. Новый центр
берётся только из текущего camera rectangle + padding, без fallback к старому центру.
Нет места/camera — новая пауза; fixed-пара порталов и остальные поля без schedule
сохраняют прежний lifecycle. RNG seeded; zero Tick держит расписание, Dispose скрывает pool.

Первый scoped graphics run v3: 107/107 EditMode + 3/3 PlayMode PASS, 0 skipped,
`TestResults/checks/20261002T115631-562719Z/summary.json`. Затем добавлен assertion
точного NearZoneCount после завершения occurrence; следующий full ниже проверяет и его.

Финальный full graphics v3: **1369/1369 EditMode PASS, 65/67 PlayMode**, 0 skipped,
third-party 0; `TestResults/checks/20261002T115933-576250Z/{EditMode,PlayMode}.xml`.
Academy Dev unlock → field card → gameplay, legacy zone smoke и seal graphics smoke PASS.
Общий FAIL: только прежние `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`
(Expected 70, actual 85) и
`Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`
(DEV-ZONES card bottom 1250, allowed 1081). Generation UP TO DATE, audio integrity PASS,
manifest 314/314 PASS. Approved ground packet PLAN 0 changed files; aura previews не импортированы.
AI осмотрел обновлённый gameplay capture: v002 ground отображается, smaller sampled
mesh footprint совпадает с игровой областью. Этот capture не утверждает новый aura art.

Новые арт-preview A/B сгенерированы OpenAI built-in imagegen как отдельные RGBA
1254×1254, референсы — ауры TRAVELER-005/007/009. Source/prompts/SHA256 сохранены
в `Art/Candidates/field006-auras-v3-2026-10-02/preview-record.json`. Они показаны
inline и ожидают конкретного выбора; ни один новый PNG ещё не находится в Assets.

## Свежая проверка v2 с контуром и вспышкой

- Scoped graphics Unity 6000.6: **361/361 EditMode + 3/3 PlayMode PASS**, 0 skipped,
  third-party 0; `TestResults/checks/20261002T102713-299217Z/summary.json`.
  Включены area-only Slow, quiet area damage, fixed/relocating placement,
  синхронные временные порталы, вращение до ground flattening и one-shot flash.
- Полный graphics: **1359/1359 EditMode + 65/67 PlayMode**, общий FAIL;
  `TestResults/checks/20261002T102159-847116Z/{EditMode,PlayMode}.xml`.
  FIELD-006 Dev UI launch, оба seal smoke и FIELD-007 smoke PASS.
- Два прежних UI failures: `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`
  — Expected 70, actual 85; `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`
  — DEV-ZONES card bottom 1250, allowed 1081. Их исправление вне этого scope.
- Generation UP TO DATE, audio integrity PASS, manifest **314 records PASS**.
  Повторный PLAN existing approved ice packet: 0 changed files.
- Предыдущий v2 scoped запуск `20261002T100424-170497Z` дал 356/356 + 3/3,
  но INCOMPLETE из-за изменения inputs; не является финальным PASS.
  Попытка full `20261002T100759-433925Z` не запустила тесты из-за временной ошибки
  компиляции параллельной работы roads; текущие прогоны выше её не воспроизводят.

### Состав v2

Отзыв v2: собственный пакет Академии теперь не содержит Permanent-зон и препятствий;
Slow/Haste/Ward/Portal fixed, Regeneration/Arcane/Rift/SpeedBurst relocating.
Пара порталов синхронна, не перемещается. Area slow — прямой movement fraction без
ControlStatus, немедленно снимаемый на следующем tick после выхода/выключения;
длительный skill slow складывается по сильнейшей доле, сохраняет свой ice/bar.
Layout `suppressAreaUnitFeedback=true` подавляет area damage flash для player/enemy
и Rift crack; обычные удары, смерть и длительные buffs сохраняют свои presentation.
`EnemyZoneSource.Dispose` убирает area slows и недоставленный накопленный DoT.
Дополнительный отзыв: контур relocating-зон вращается 12°/с до сжатия по Y;
fixed-контур и центральные символы остаются неподвижными. Burst firing и фактическое
использование обоих концов Portal дают осветление печати на 0.25 с, без unit flash.
Повторный run clock держит и вращение, и вспышку на паузе.

Ground v001 проверен: runtime и immutable source имеют один SHA256
`e834d6bbc2e44a6180a13d1b30dacc14466efaeb50421800e57b3cf7ef1e8c99`.
Это approved синие каменные плиты Академии. После нового отзыва сгенерирован более
магический candidate 1254×1254 RGB с тихими teal/violet прожилками и стёртыми узорами:
`Art/Candidates/field006-ground-2026-10-02/concept-01.png`; далее утверждён
прямым ответом «Поверхность утверждаю». Approved пакет
`Art/Packets/field006-ground-v002-2026-10-02.json`: PLAN/APPLIED 5 files.
Immutable source v002 RGB сохранён byte-identical, runtime подготовлен opaque-rgba
без изменения RGB/размеров; runtime SHA256
`19e3554a81087eacbda895012d5f4104a01124067b9710113fe48715abd1bb3f`.
GUID `2c4ac41cc9d942c46ae76a7dfffd889d` и runtime path сохранены.
После импорта v002: art scope **100/100 EditMode PASS**, 0 skipped, third-party 0,
manifest **314/314 PASS**, generation UP TO DATE;
`TestResults/checks/20261002T103808-604805Z/summary.json`.

Последующее поручение пользователя: дальнейшая работа непосредственно на FIELD-006.
Источник — `docs/balance/field006-zones-v1.json`, отдельные `FIELD-006-ZONE-*` IDs;
генератор выпускает field/environment binding, собственные ground и thumbnail.
FIELD-001 timeline, enemy pool, bosses и Traveler schedule подключены общими ссылками.
Обычное условие открытия сохранено; Dev unlock записывает открытие в профиль.
`ProductionField006ContentTests` проверяет shared reference и сохранение unlock;
новый FIELD-006 PlayMode случай нажимает реальную Dev кнопку и запускает карту
через настоящую карточку выбора. «Тест 06» остаётся регрессионной сценой данных.
Final academy enemies/bosses, узкие проходы и баланс не поставляются этим preview.

FIELD-DEV-ZONES («Тест 06»), восемь типов эффектов. Авторские данные —
`docs/balance/field-dev-zones-v1.json`, выпущены через `scripts/content/generate.py`.
Мерцающие зоны: `pulsePrepareSeconds=5`, `pulseIdleVisibility=0.14`, нелинейное наполнение,
работа только в полностью проявленном плато, затухание без эффекта.
Все зоны этого пакета задают `verticalScale=0.8`: визуальный эллипс и проверка попадания
совпадают; вращение внутренних слоёв происходит до сжатия родителя.
Постоянные зоны, Burst и циклы FIELD-DEV-ALTARS сохраняют свои механики.

`ZoneSealMeshBuilder` и `ZoneSealPresentationRuntime` создают три mesh-слоя на зону:
граница реального радиуса, символ, внутреннее движение. Тюнинг в напрямую авторском
`Assets/Resources/Content/Presentation/FixtureZoneSeals.json`.
Геометрия строится при инициализации; покадровое представление меняет дочерние transforms,
прозрачность и цвет. Root/collider не анимируются. `Zones.Presentation` охвачен PerfGuard.

Монстры: прежний bar+ice на Slow, короткая красная трещина на фактический урон Rift;
у Rift нет продолжающегося статуса или полоски времени. У игрока SpeedBurst показывает
общий amber bar+bolt по оставшейся доле баффа после выхода из зоны. Оверлеи кешируются
на visual rig, очищаются при окончании статуса/pool return/Shutdown;
пауза держит показанное состояние, не продвигая анимацию или таймер.

## Свежая проверка

Подключение постоянного FIELD-006 проверено в
`TestResults/checks/20261002T093012-062154Z/summary.json`, Unity **6000.6.0f1**,
безопасный graphics batch, **8/8 EditMode + 3/3 PlayMode PASS**, 0 failed/skipped, third-party 0.
Filter: `^Game\.Bootstrap\.(Tests\.(ProductionField006ContentTests|RuntimeContentCatalogTests|ZoneSealPresentationTests)|PlayModeTests\.(ProductionFieldDevZonesSmokeTests|ZoneSealVisualSmokeTests))`.
Проверены actual Dev button → academy card → start, сохранённый unlock, FIELD-001
reference reuse, approved ground/thumbnail и полноценные pause/Rift/Shutdown checks.
Круги и игровые снимки обновлены с данных постоянной Академии.
Approved ground packet `Art/Packets/field-ground-textures-004-010-2026-09-27.json`
проверен через art_pipeline: **PLAN, 0 changed files**; runtime raster/GUID не заменяются.

Финальный общий graphics runner после подключения FIELD-006:
`python scripts/check_project.py --scope full --graphics`, Unity **6000.6.0f1**,
`TestResults/checks/20261002T093538-672136Z/`.
**1346/1346 EditMode PASS; 64/67 PlayMode**, 3 failed, 0 skipped, third-party 0.
Academy real Dev UI → field card → start и регрессионный zones smoke, seal capture,
altar regression smoke и GameplaySmokeTests прошли. Critical paths представлены:
RunModel 10, Health 8, LevelUp 16, ActiveSkill 127, WaveDirector 16, EnemyPool 3,
RuntimeContentCatalog 3, GameplayUiPresenter 18, ProductionField006 2.
Generation UP TO DATE, audio 28 files/15 cues PASS; отдельный art manifest **311 records PASS**.
Общий PASS не заявляется; runner остановился на PlayMode failures:

1. `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`:
   ожидалось 70, получено 85 (`MetaShopSmokeTests.cs:124`), прежний UI gate.
2. `Game.Bootstrap.PlayModeTests.ProductionMonasterySmokeTests.DevUnlock_Field007Card_StartsThirtySixVisibleAltarsAndPausesCleanly`:
   ожидалось число врагов >0, получено 0 после 30 fixed ticks (`ProductionMonasterySmokeTests.cs:49`).
   Отдельный параллельно разрабатываемый FIELD-007; его source/проверка не исправлялись в scope Академии.
3. `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`:
   bottom карточки FIELD-DEV-ZONES 1250 при допустимой 1081, прежний UI gate.

Первая общая попытка `20261002T093314-209204Z` остановилась на 1343/1344 EditMode:
`ProductionFieldContentTests.ProductionComposition_ContainsProductionDefinitions_AndResolves`
содержал старый список игровых полей. Ожидаемый roster обновлён с новым FIELD-006
с сохранением параллельно добавленного FIELD-007; финальный общий EditMode прошёл.

После уточнения о стандартном сжатии выполнен новый графический scoped runner:
`python scripts/check_project.py --scope code --platforms EditMode PlayMode --filter '^Game\.(Zones\.|Bootstrap\.Tests\.ZoneSealPresentationTests|Bootstrap\.PlayModeTests\.(ProductionFieldDevZonesSmokeTests|ZoneSealVisualSmokeTests|ProductionFieldDevAltarsSmokeTests))' --graphics`.
Unity **6000.6.0f1**, безопасный batch при закрытом интерактивном Editor;
`TestResults/checks/20261002T082957-797686Z/summary.json`: **91/91 EditMode + 3/3 PlayMode PASS**,
0 failed/skipped, third-party 0. Девять новых случаев проверяют границу эллипса,
отсутствие бонуса за ней и валидацию масштаба. Presentation проверяет вращение внутри
сжатой границы и все production значения 0.8. Оба зональных smoke и altar smoke прошли;
снимки ниже обновлены этим прогоном. Generation и art manifest 308 повторно PASS.

## Общий прогон перед уточнением о сжатии

Unity **6000.6.0f1**, безопасный batch runner при закрытом интерактивном Editor,
`python scripts/check_project.py --scope full --graphics`.
Результаты: `TestResults/checks/20261002T081244-712599Z/`.

- **EditMode: 1323/1323 PASS**, 0 failed/skipped, third-party 0.
- **PlayMode: 63/65**, 2 failed, 0 skipped; общий full PASS не заявляется.
- Все 18 затронутых новых EditMode случаев прошли: фазы/кривая/валидация,
  timing → player modifier, границы mesh, active window, повторная инициализация,
  timed-buff projection и Rift feedback.
- `ProductionFieldDevZonesSmokeTests` и `ZoneSealVisualSmokeTests` PASS:
  настоящая production composition тестового поля, 15 печатей/45 mesh-renderer,
  мост фактического урона → Rift feedback, пауза, очистка и графические снимки.
- `ProductionFieldDevAltarsSmokeTests` PASS; отдельные циклы алтарей сохранены.
- Critical paths представлены среди прошедших тестов: RunModel/RunOutcome,
  Health damage/death, XP/LevelUp/Draft, ActiveSkill, WaveDirector, Enemy pool,
  GameplaySmokeTests, GameplayUiPresenterTests и RuntimeContentCatalogTests.

Два падения повторяют первый полный прогон этого же прохода
(`TestResults/checks/20261002T063136-448655Z/`: 1313/1313 EditMode, 63/65 PlayMode)
и ранее отмеченные UI gates в STATUS:

1. `Game.Bootstrap.PlayModeTests.MetaShopSmokeTests.Unlocks_FiltersAndScroll_TwoResolutions`:
   ожидалось 70 открытий, получено 85.
2. `Game.Bootstrap.PlayModeTests.UiEntrySmokeTests.EntryScreens_TwoResolutions_InspectAndConfirmRemainSeparate`:
   нижняя граница карточки `field-select-FIELD-DEV-ZONES` 1250 при допустимой 1081.

Падения не относятся к новой фазовой логике/отрисовке зон; их исправление в этот scope не включалось.
Первый scoped EditMode выявил создание MaterialPropertyBlock в инициализаторе MonoBehaviour;
создание перенесено в Initialize, финальная проверка lifecycle прошла.

## Данные и арт

- Generation `--check`: UP TO DATE.
- Audio integrity внутри полного runner: 28 files, 15 cues, hashes/licenses/references PASS.
- `python scripts/validate-art-manifest.py`: **308 records PASS**.
  Аудит выполнен отдельно, поскольку полный runner остановился после двух PlayMode failures.
- AI подготовил `TestResults/academy-zone-seals-reused-ice-packet.json` из approved v002:
  вход — immutable `Art/Source/VFX/slow-status/ice/v002/concept-01.png`, исходное approval сохранено.
  `python scripts/art_pipeline.py TestResults/academy-zone-seals-reused-ice-packet.json`:
  **PLAN, 0 changed files**. Новых растров и замен нет; применять пакет без изменений не требуется.
- Новые `.meta` созданы Unity при импорте; GUID существующего льда не менялся.

## Графические снимки

Все снимки сняты из Unity с актуальными mesh, а не из стороннего макета.
Порядок: верхний ряд Slow / Haste / Regeneration / ArcanePower;
нижний ряд Rift / Portal / SpeedBurst / Protection.
Снимки обновлены проверкой v2: все эффекты временные; разные периоды дают разные фазы.

- [Работающие печати](../../../TestResults/academy-seals-active.png).
- [Подготовка](../../../TestResults/academy-seals-preparing.png).
- [Ожидание](../../../TestResults/academy-seals-waiting.png).
- [Настоящий игровой масштаб](../../../TestResults/academy-seals-gameplay.png).
- [FIELD-006 после запуска через Dev unlock](../../../TestResults/academy-field006-gameplay.png).

AI осмотрел итоговые снимки: пол просматривается через рисунок, body игрока рисуется
выше печати, символы различаются; наземные контуры сжаты по вертикали до 0.8.
Эти снимки не заменяют пользовательский обзор
движения и плотной волны и не утверждают production-набор FIELD-006.
