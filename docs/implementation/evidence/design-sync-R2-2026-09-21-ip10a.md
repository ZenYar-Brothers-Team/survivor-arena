# IP-10A evidence — 2026-09-21

## IP-10A

Scope revision: `design-sync-R2`. Источник исполнения/очереди — [STATUS](../STATUS.md).

### Реализация

- Расширена существующая UI Toolkit foundation. `ContentCard` / `DraftCard` используют immutable snapshots, resolved optional icon references и стандартные состояния normal/hover/pressed/disabled/selected/locked. Генерации, импорта и назначения production art нет; fallback — shape/текст.
- Draft сохраняет три позиции при 0/1/2/3 options, Book heading/queue, revision-aware select/reroll/Banish/cancel. Рецепты приходят готовыми ordered snapshots: current и projected отдельно, completed recipe не означает acquired set. Первые два summary + `more`, полные components/thresholds в отдельной scroll details area при hover/focus.
- HUD: elapsed timer сверху, HP/XP/level снизу, compact 6+6 slots и acquired sets. HUD labels не focusable; tooltip содержит имя/уровень. Эффекты и dynamic low-HP passive detail доступны в Pause/Build, включая grid 6+6, acquired sets и progressed unacquired recipes. `HasProgress` поддерживает partial threshold при fulfilled count=0.
- Неизменившиеся Build/character snapshots не пересоздают VisualElements; Build snapshot копирует входные коллекции. UI не изменяет build при показе projection.
- `UiNotification` — один nonblocking slot, replacement и автоматический expiry по pause-aware delta. Foundation подключает level-up и set acquisition; остальные event texts проверяются fixtures, producers принадлежат своим IP.
- DEV по-прежнему development-gated, collapsed by default, tabs/scroll, bounds проверены ≤25%×45% при reference viewport. Launcher расположен выше нижнего HUD.
- USS переименован в `GameplayUiStyles.uss` с сохранением GUID; loader использует уникальный resource path, исключающий выбор пустого inline StyleSheet subasset из одноимённого UXML. Regression: exact standalone resource check + реальная PlayMode geometry.

### Coverage и условия

- `UiFoundationTests`: 0/1/2/3 positions, disabled placeholders, defensive copies, current/projected/completes/acquired distinction, partial threshold, max-level build, unchanged element identity, locked/selected, nonblocking notification replacement/pause/expiry.
- `GameplayUiPresenterTests`: сохранён fake model/view contract, revision-aware Banish/cancel/control state, queued Book/level-up, development gating. Preview/effects строятся presenter/producer, renderer не вычисляет recipes/eligibility.
- `GameplayUiAssetTests`: semantic assets и уникальная загрузка USS. Новый namespace semantic IDs описан в [спецификации](../modules/IP-10A-ui-foundation.md#контракт-компонентов).
- `GameplaySmokeTests`: реальная сцена, movement/pause, XP→draft→apply, queued Book requests, Banish, dynamic passive effects и terminal timer 15:00. Чтение passive detail мигрировано с HUD tooltip на Pause/Build card summary.
- `UiFoundationSmokeTests`: fake-state harness в отдельных UIDocument/PanelSettings; geometry, keyboard focus, ровно один intent при submit, no overlay over choices, disabled empty, Book/set, 6+6 max-level build, locked fixture, partial recipe и DEV bounds. Target render textures 1920×1080 и 1280×720, constant pixel size — более строгая проверка доступного места, чем runtime reference scaling.
- В продуктовых документах нет утверждённого minimum resolution. 1280×720 использован как lower verification viewport, без объявления нового product constraint.

### Результаты

Unity 6000.6.0f1: **383/383 Game.* EditMode, 3/3 PlayMode passed, 0 failed, 0 skipped**, оба процесса exit 0. Third-party tests не запускались (`^Game\.` filter). Перед каждым запуском — свежая проверка `Win32_Process`; открытого interactive Editor не было. EditMode через `scripts/Test-Unity.ps1`; PlayMode через тот же Unity Test Runner в hidden batch process с graphics для capture (без `-nographics`). Критические пути run/damage/death/XP/draft/active skill/waves/enemy pool/composition/UI/content представлены passed tests.

Ограничение лога: после сохранения успешного PlayMode XML при teardown Editor остаётся `PlayerCharacterRuntime.RemoveModifier` → `PlayerPassiveSetRuntime.OnDestroy` NullReferenceException. Та же ошибка присутствовала в сохранённом до этой задачи `TestResults/PlayMode.log`; это существующий lifecycle defect вне UI foundation, не новый failed test. Не объявляется исправленным и не скрывается за успешными XML counts.

Артефакты: `TestResults/EditMode.xml`, `TestResults/EditMode.log`, `TestResults/IP10A-PlayMode.xml`, `TestResults/IP10A-PlayMode.log`; изображения `TestResults/ip10a-{draft,pause,pause-details}-{1920x1080,1280x720}.png`. Это локальные generated test artifacts, не production assets.

Визуально просмотрены реальные RenderTexture captures draft/long text и Pause/Build в обоих разрешениях. Полный длинный effect text остаётся доступен в scroll details; обязательные card choices и Resume не закрываются. Это проверка снимков и автоматизированного input/geometry harness, не утверждение о ручном игровом прогоне человеком.

### Documentation impact

IP-10A component/state/semantic contracts, consumer IP-11/IP-12 и STATUS/readiness синхронизированы. GDD/Content Design и production balance не менялись. Реальное наполнение recipe projections и baseline-relative character summaries остаётся у IP-11/IP-12. Cross-layer/product deviations отсутствуют. Граница остановки перед IP-31 сохраняется.
