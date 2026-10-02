# Печати зон — проверка 2026-10-02

Контракт: [DECISION-0142](../../decisions/0142-academy-zone-seal-presentation.md).
Execution status и оставшиеся gates — только в [STATUS](../STATUS.md).

## Область

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
