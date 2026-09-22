# IP-26 — Functional UI и полный player flow

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Functional shell объединяет существующие feature-owned экраны и результаты IP-25. Production content/art остаются у catalog IP; fixture flow использует те же runtime boundaries.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-25](IP-25-meta-progression.md), [IP-28](IP-28-world-pickups.md), [IP-29](IP-29-traveler-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
draft/recipe числа, highlights и отображение proposed PICKUP-002 после принятия.
Это Proposed packet по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
использовать как production data только после approval, затем выполнить проверки этого IP.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

UI §§1–5,11–23 полностью; новые GDD core loop/run/XP/build/sets/fields/characters/meta/Travelers; только отображаемые production cards и profile metadata; DECISION-0005. IP-10A owns reusable cards/HUD/build, IP-26 owns settings, feature modules own boss/Traveler/Book state.

## Scope

Main Menu Play/Meta/Settings/Exit; Character Select→Field Select→Run; level-up/Book drafts; Pause/Build→Resume/Settings/Quit; Victory/Defeat→Results; Retry немедленно с теми же character/field, Main Menu, meta purchases. Results: outcome/time/level/kills/currency/sets/unlocks; top-3 skills by damage только при доступной корректной attribution IP-31, без обязательного отдельного analytics screen. В этом же IP находятся basic Settings: persisted Master/Music/SFX, resolution/window mode, current movement keys, Screen Shake toggle. Поставить минимальные рабочие audio routing endpoints/preview и presentation consumer shake, не декоративные controls. Внутренние этапы shell→settings→full navigation являются checklist одного IP, не отдельными execution statuses.

## Out of Scope

новые game/economy rules, generation final images внутри UI, compendium/advanced analytics/controller polish/localization до отдельного scope, сложные transitions. Remap, production audio catalog и расширенный accessibility menu — отдельно; basic Settings из Scope не относятся к этому исключению.

## Acceptance criteria

полный цикл без DEV; locked choices недоступны, relevant baseline modifiers и difficulty понятны. Retry без дополнительного confirmation/selection создаёт новый run и очищает старые subscriptions/entities/UI while preserving профиль/settings. Draft completion не снимает чужие pause reasons; settings возвращает в исходный экран; результаты и награды совпадают с model. Quit без дополнительного confirmation ведёт в Results; victory/defeat открывают Results сразу. Ошибка записи оставляет pending result с Retry Save и блокирует новый run/покупки (DECISION-0037). Каждый enabled setting реально применяется и сохраняется; invalid/corrupt preferences дают документированный safe fallback, unsupported video mode не запирает пользователя; Back/Resume сохраняют pause reasons и selection. Shake off отключает только visual effect, baseline camera follow сохраняется согласно решению G-16. Обязательные Results работают в release без IP-31; optional top 3 только при доступной attribution.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

целевой screen/state/intent map, keyboard+mouse input/focus, normal/hover/pressed/disabled/selected/locked, meaningful errors; все player-facing данные отдельно от DEV. Traveler HP у каждого, arrow исчезает при видимости/death/escape; countdown ухода не добавляется.

## Проверки

fresh/existing profile happy/death/level-up/Book/pause/settings/quit/retry/unlock/purchase paths, double-click/idempotency, long text/empty lists, alternate supported resolution; PlayMode full flow плюс ручной 1920×1080 review. Preference validation/fake store, channel isolation, input binding display, manual video apply/revert/fullscreen/audio/shake; release/no-recorder Results.

## Документационные изменения

complete UI flow и semantic IDs; явно разграничить IP-10A/IP-26/feature-owned slices, обязательный art binding и отложенный polish. Старый blanket Out of Scope «audio/settings» не скрывает утверждённый Settings scope — его реализует IP-26. Settings persistence и service ownership документируются здесь; production soundtrack/SFX library не добавляется.

## Gates и недостающие решения

G-15 resolved по DECISION-0037: Quit→Results, reward/save/error ordering. G-16/G-20 resolved по [DECISION-0038](../../decisions/0038-settings-and-field-difficulty.md): defaults, persistence/failure, audio routing, video rollback, camera offset и difficulty 1–5. Retry same character/field immediate уже утверждён. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Character selection integration

Переиспользовать `CharacterSelectionSession`, `CharacterSelectPresenter` и `ContentCard` из [IP-12 API](IP-12-character-framework.md#framework-api-и-fixture-schema). Composition создаёт выбранный loadout до запуска run clock; profile access поставляет IP-25. Selection panel имеет отдельный жизненный цикл. Новый navigation flow не должен возвращать автоматический запуск startingCharacterId или вычислять baseline из roster.

Field Select использует `FieldSelectionSession`, `FieldSelectPresenter`, `FieldSelect.uxml`
и semantic IDs [IP-16](IP-16-field-framework.md#framework-api-и-fixture-schema).
Character confirm открывает Field Select, Start Run повторно проверяет оба доступа;
Back сохраняет допустимые selections. Retry должен использовать `RunOutcome.Selection`
с теми же character/field, без открытия selection и без зависимости от telemetry.
G-20 resolved по DECISION-0038: production difficulty берётся явно из CD (1–5); fixture metadata не является production mapping.


## Presentation preference boundary

IP-12A предоставляет `IScreenShakePreference` и `ScreenShakeRequestGate`: preference читается на каждом request, выключенное значение и не-running state не выпускают запрос. IP-26 реализует persistent setting и camera consumer/сброс активного offset при выключении; G-16 policy определена DECISION-0038; request boundary сам по себе не реализует audio/settings service. См. [IP-12A contract](IP-12A-visual-presentation-foundation.md#контракт-технического-пакета).

## World pickup integration boundary

Использовать [единый контракт IP-28](IP-28-world-pickups.md#framework-api-и-fixture-schema):
WorldPickupRuntime.Spawn с source identity, Health/RequestBook/SetRewardEvent через
PlayerPickupRewardTarget, immutable pickup snapshots/events для UI и telemetry.
Не дублировать collection/draft lifecycle. Chance/restoration читают текущие stats;
XP radius не влияет на contact pickup. Production definitions/data/art и Traveler
encounter semantics остаются в scope соответствующих владельцев.
## Traveler vertical slice

IP-29 поставляет ITravelerRuntime snapshots/events, TravelerPresenter и
UiToolkitTravelerView: HP всех ролей, off-screen pointer lanes и gated dev spawn
в Build tab. IP-26 интегрирует готовый slice; отдельного gameplay state в UI нет.
## Profile / result integration

IP-25 предоставляет IProfileService, ProfileRunBinding, ProfileAccessProvider и
MetaPresenter/MetaScreen. Переиспользовать commit-state и pending-result retry;
не начислять rewards в navigation/view. Quit→Results и Retry same selection уже
связаны с composition. IP-26 объединяет готовые поверхности с Main Menu/Settings,
дополняет presentation/art и production difficulty после закрытия их gates.
Profile IO исполняется вне игрового потока; Reset повреждённого профиля — явный intent.

## Settings implementation contract

Читать DECISION-0038 полностью вместе с UI §18. App-scoped service владеет settings,
UI только intents/snapshots. Основные acceptance cases решения входят в checks этого
IP: отдельный файл/defaults/preserve-invalid/retry, Master×channel routing, preview,
confirmed video с real-time rollback, bounded visual shake от actual player damage.
Production soundtrack, remap, localization, exclusive fullscreen и дополнительные
graphics options вне текущего scope. Существующая camera centering логика остаётся
baseline; spatial gameplay queries не должны читать shake offset.

## Runtime / UI contract

`Game.Settings` — app-scoped `ISettingsService`/`SettingsService`, immutable snapshots,
validated `SettingsConfig` из `Content/Settings/SettingsDefaults.json`. Файл
`Application.persistentDataPath/settings-v1.json` имеет version 1; профиль не меняется.
`FileSettingsStore` выполняет IO вне main thread, сохраняет invalid original отдельно,
затем атомарно заменяет файл. Save-loop сериализует revisions; candidate video не
попадает в persisted snapshot до Keep. `IVideoDevice` отделяет реальные Screen API
от fake-mode проверок; UI показывает фактически применённый режим.

`IAppNavigation` → `AppShellPresenter` → `IAppShellView`/`AppShellScreen`:
Main Menu → Play → Character → Field → Run; Meta → Back → Main Menu.
Manual Pause → Settings → Back сохраняет все pause owners. Quit → Results,
Retry переиспользует selection snapshot; Main Menu завершает consumers прошлого run.
Escape закрывает Settings (при pending video сначала Revert), возвращает из
Character Select либо переключает только manual pause в запущенном run.
Semantic IDs — `GameplayUiElementIds.Shell*`/`Settings*`, assets —
`UI/AppShell.uxml` и `UI/AppShellStyles.uss`. Результаты используют IP-25 Meta IDs;
`MetaSelection` теперь означает Main Menu, standalone launchers скрыты.

`SettingsAudioRuntime` владеет Music/gameplay SFX и двумя preview sources;
короткие synthetic clips — только проверка routing, не production soundtrack.
Каждый источник получает Master×channel×sourceGain один раз. Gameplay SFX
не запускается вне Running; pause сохраняет playback position, terminal останавливает.

`CameraShakeRuntime` принимает actual player damage через IP-12A request gate.
Offset ограничен JSON envelope; camera transform смещается только между URP
begin/end-camera-render callbacks и немедленно восстанавливается. Gameplay Update,
spawn/visibility и camera follow читают baseline. Disable/pause/off/end очищают effect.

`NotificationQueue` показывает одно неблокирующее сообщение вне центра; остальные
ждут в ограниченной очереди. UI-time при pause не идёт. `RunNotificationBinding`
переводит level/set/Traveler/boss events, profile observer — новые unlocks.
Этот binding также поставляет required special-kill contribution вне telemetry;
Results суммирует ordinary/special kills и показывает acquired sets.
Controls читает реальные InputAction bindings; remap не добавлен.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-03](../milestones/FIELD-001-start.md#f1-03):
startup/locks/recipe UI; Results и actual-content integration в F1-08. Packet prerequisites: F1-00/01/02; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).

Принятый новый UI-контракт — раздел «Стартовая прогрессия» UI/UX. Полный Results→расширенный повторный run остаётся вне стартового этапа; этот этап не вводит постоянный field whitelist.
