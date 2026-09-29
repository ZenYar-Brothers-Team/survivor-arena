# IP-26 — Functional UI и полный player flow

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Functional shell объединяет существующие feature-owned экраны и результаты IP-25. Production content/art остаются у catalog IP; fixture flow использует те же runtime boundaries.

Существующие действия и переходы сохраняются при чистовом проходе. Геометрия
прототипа не является принятой композицией: layout, пропорции, плотность текста
и presentation variants пересматриваются по UI/UX и DECISION-0083.

Draft/Book используют один DraftCard IP-10A: шапку icon + type/level из макета,
крупные подписи «Активное»/«Пассивное»/«Сет» без овальных плашек и иконки
в списке связанных сетов по уточнению DECISION-0086 после просмотра игры.
Очередь показывает только число следующих выборов, не уровни забега; inspector
различает количество owned компонентов и выполненные thresholds. Проценты целые,
скорость использования пересчитывается из интервала по UI §7.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-25](IP-25-meta-progression.md), [IP-28](IP-28-world-pickups.md), [IP-29](IP-29-traveler-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
draft/recipe числа, highlights и отображение proposed PICKUP-002 после принятия.
Packet Approved 2026-09-24 по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
используется как production data; проверки этого IP сохраняются.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

UI §§1–5,11–23 полностью и §§6–10 для совместного layout review с IP-10A; новые GDD core loop/run/XP/build/sets/fields/characters/meta/Travelers; только отображаемые production cards и profile metadata; DECISION-0005/0081/0083. IP-10A owns reusable cards/HUD/build, IP-26 owns settings, feature modules own boss/Traveler/Book state.

## Meta R1 — delta 2026-09-29

[DECISION-0091](../../decisions/0091-personal-meta-upgrades-and-refund.md) и
[композиция](../proposals/2026-09-29-ui-meta-r1.md): только карточки выбранного героя,
без global-блока, расширенный список с прокруткой и подтверждаемый платный возврат.
IP-25 владеет ценами/уровнями/транзакцией и фактическими затратами; экран не вычисляет экономику.
Числа и область возврата утверждены: выбранный герой, комиссия 1000, доступность
при баланс + возврат >= 1000. Равенство даёт 0, меньшая сумма — disabled с причиной.
Проверить cap/недостаток золота/pending/
error/выключенные бонусы, смену героя, scroll/focus и 1080p/720p.
Выбор героя — прокручиваемая сетка миниатюр под портретом, не dropdown.
Карточки улучшений компактные, только текст, без иконок сетов/предметов.
Дорелизная экономика использует отдельный профиль без миграции старых покупок.

Полная вкладка «Открытия» — [DECISION-0092](../../decisions/0092-unlocks-collection-ui.md):
все определения, включая начальные; фильтры типа и состояния; общая прокрутка
сетки 3/2 колонки и закреплённые фильтры/счётчик/возврат. Проверить пустой фильтр,
весь каталог, покупку и возврат фокуса, сохранение scroll при refresh, 1080p/720p.
Силуэты остаются только у неоткрытых персонажей, условия и экономика не меняются.
Коллекция показывает готовые иллюстрации всех десяти карт независимо от наличия
playable field definition; это не открывает карту для запуска. Meta/results имеют
полноэкранную непрозрачную подложку, скрываемую вместе с экраном.
Проверка Meta должна включать первый вход из меню до Character Select: artwork
registry уже доступен. Toggle бонусов не пересоздаёт upgrade rows/roster и не
смещает scroll. Порядок типов — по UI/UX §17, перебросы/исключения последние.

## Scope

Main Menu Play/Meta/Settings/Exit; Character Select→Field Select→Run; level-up/Book drafts; Pause/Build→Resume/Settings/Quit; Victory/Defeat→Results; Retry немедленно с теми же character/field, Main Menu, meta purchases. Results: outcome/time/level/kills/currency/sets/unlocks; top-3 skills by damage только при доступной корректной attribution IP-31, без обязательного отдельного analytics screen. В этом же IP находятся basic Settings: persisted Master/Music/SFX, resolution/window mode, current movement keys, Screen Shake toggle и Mouse movement toggle (default Off). Поставить минимальные рабочие audio routing endpoints/preview и presentation consumer shake, не декоративные controls. Escape/Space/right mouse переключают только manual pause по [DECISION-0084](../../decisions/0084-mouse-movement-and-pause-shortcuts.md). Внутренние этапы shell→settings→full navigation являются checklist одного IP, не отдельными execution statuses.

## Out of Scope

новые game/economy rules, generation final images внутри UI, compendium/advanced analytics/controller polish/localization до отдельного scope, сложные transitions. Remap, production audio catalog и расширенный accessibility menu — отдельно; basic Settings из Scope не относятся к этому исключению.

## Acceptance criteria

полный цикл без DEV; locked choices недоступны, relevant baseline modifiers и difficulty понятны. Retry без дополнительного confirmation/selection создаёт новый run и очищает старые subscriptions/entities/UI while preserving профиль/settings. Draft completion не снимает чужие pause reasons; settings возвращает в исходный экран; результаты и награды совпадают с model. Quit без дополнительного confirmation ведёт в Results; victory/defeat открывают Results сразу. Ошибка записи оставляет pending result с Retry Save и блокирует новый run/покупки (DECISION-0037). Каждый enabled setting реально применяется и сохраняется; invalid/corrupt preferences дают документированный safe fallback, unsupported video mode не запирает пользователя; Back/Resume сохраняют pause reasons и selection. Shake off отключает только visual effect, baseline camera follow сохраняется согласно решению G-16. Обязательные Results работают в release без IP-31; optional top 3 только при доступной attribution.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Персональные Meta upgrades используют отдельные stat icons META-003…014-VISUAL-ICON,
разрешённые через SpriteDefinition registry в presenter. Slot 32 px перед названием
не увеличивает высоту; отсутствие registry в isolated fixtures оставляет текстовый
вариант. Production smoke проверяет все 12 sprites и отсутствие перекрытия текста.

целевой screen/state/intent map, keyboard+mouse input/focus, normal/hover/pressed/disabled/selected/locked, meaningful errors; все player-facing данные отдельно от DEV. Traveler HP у каждого, arrow исчезает при видимости/death/escape; countdown ухода не добавляется.

## Проверки

fresh/existing profile happy/death/level-up/Book/pause/settings/quit/retry/unlock/purchase paths, double-click/idempotency, long text/empty lists, alternate supported resolution; PlayMode full flow плюс ручной 1920×1080 review. Preference validation/fake store, channel isolation, input binding display, manual video apply/revert/fullscreen/audio/shake; release/no-recorder Results.

Visual acceptance первого среза: production поле/иконки/текст, HUD с HP возле
героя без speed controls, level-up и Book, Pause со всеми Resume/Settings/Quit,
начальный и заполненный билд, 1920×1080 и 1280×720. Fixture harness не заменяет
production composition. Проверять отдельные hover/focus/pressed/disabled/selected
состояния, кириллицу и читаемость; один статичный кадр не доказывает всю матрицу.
По DECISION-0085 в player-facing карточках/подробностях нет абсолютного базового
урона; damage upgrades показываются процентами без изменения gameplay values.
По [DECISION-0086](../../decisions/0086-ui-review-density-and-inspection.md):
HUD без wave/инструкций/кнопки Pause и с компактным масштабом только в 720p; Draft/Book
разделяют inspect и отдельный confirm, включая Banish. Список связанных сетов
со scroll не подтверждает карточку. Pause отводит больше места рецептам,
показывает компактные acquired/missed icon/name в общем правом scroll,
оставляет все 6+6 слотов и footer видимыми при увеличенной области персонажа.
Любой сет Pause открывает краткий эффект; тот же текст в Draft / Book рядом с
component levels. Закрытие справки через крестик/outside click/Escape/ПКМ не
вызывает Resume/Quit и не снимает чужие pause reasons; проверить возврат фокуса,
keyboard activation, bounds и cleanup при scroll/resize/смене экрана.
Рецепты Pause — 3 колонки в 1080p / 2 в 720p. В Draft недостижимые/acquired/закрытые
сеты не входят ни в список, ни в счётчик; существующая eligibility — у IP-11.
Скорость сравнивается с общим baseline, не со стартом выбранного персонажа.
Проверить 10 связанных/20 общих рецептов и быстрый стартовый герой = 120%,
без повторного pause/submit по Space и без изменений DECISION-0084 routing.

## Документационные изменения

complete UI flow и semantic IDs; явно разграничить IP-10A/IP-26/feature-owned slices, обязательный art binding и отложенный polish. Старый blanket Out of Scope «audio/settings» не скрывает утверждённый Settings scope — его реализует IP-26. Settings persistence и service ownership документируются здесь; production soundtrack/SFX library не добавляется.

## Gates и недостающие решения

G-15 resolved по DECISION-0037: Quit→Results, reward/save/error ordering. G-16/G-20 resolved по [DECISION-0038](../../decisions/0038-settings-and-field-difficulty.md): defaults, persistence/failure, audio routing, video rollback, camera offset и difficulty 1–5. Retry same character/field immediate уже утверждён. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

UI visual language resolved по [DECISION-0081](../../decisions/0081-field-folio-ui-visual-language.md):
«Полевой фолиант», умеренная декоративность, утверждённые palette/type/state/motion
defaults. Layout, DEV-only скорость и player HP уточняются
[DECISION-0083](../../decisions/0083-player-ui-layout-and-dev-boundary.md).
Замена длинного описания, отдельное подтверждение и компактные recipe states
утверждены в DECISION-0086; композиционная проработка —
[layout R2](../proposals/2026-09-28-ui-layout-r2.md).
Выполнение и применимость evidence определяет только STATUS.

### Последовательность чистового UI-прохода

Композиционный reference следующего экрана: [UI Results R1](../proposals/2026-09-28-ui-results-r1.md).
Принятый Results R1 и явный запрос +20 за Book upgrade закреплены
[DECISION-0090](../../decisions/0090-results-r1-and-book-upgrade-gold.md).
Исполнение и gates определяются STATUS. Typed Results projection получает saved
receipt данного RunId; новые открытия выше сетов, общая прокрутка коллекции,
награда и кнопки вне scroll. Никакого парсинга summary или начисления во View.

1. Определить player/DEV и primary/secondary information на реальном контенте.
2. Подготовить композицию HUD → Draft → Pause / Build в двух целевых разрешениях;
   layout не выводится из существующих размеров UXML/USS.
3. Реализовать выбранную композицию и контекстные details в границах View/presenter,
   затем применить общую тему; учесть совместный footer Pause и AppShell.
4. Проверить production screenshots и input/state/release матрицу вместе с IP-10A/IP-27.
5. После visual acceptance среза переработать Main Menu/selection, Results/Meta,
   Settings: для каждого сначала композиция, затем тема. Общие CSS-подобные селекторы
   не должны случайно менять ещё не переработанные экраны.

Это checklist scope, не вторая execution queue. Текущая работа, gates и порядок
исполнения находятся в [STATUS](../STATUS.md).

Композиционный reference — [UI entry R1](../proposals/2026-09-28-ui-entry-r1.md):
Main Menu/Character Select/Field Select, короткие детали, крупное изображение и
раздельные inspect/confirm. Пользователь принял текущий макет 2026-09-28;
Main Menu использует выбранную послойную иллюстрацию E с Шепоткой v002
([approval](../proposals/ui-entry-r1/menu-shepotka-review.md#visual-approval--2026-09-28)).
Её более взрослый образ и свитки относятся только к иллюстрации, не к canonical
CHAR-003. Approval не разрешает подключать недостающие production characters/fields
и не заменяет runtime verification. Исполнение Unity-переноса — по STATUS.

Уточнение [DECISION-0087](../../decisions/0087-character-silhouettes-and-field-grid.md):
неполученные герои — силуэты в каталоге и крупном просмотре; поля — компактная
сетка десяти карточек без описаний окружения и большой правой detail-панели.
Metadata/access owners IP-12/IP-16 не меняются. Проверить переход силуэт→body после
покупки, вместимость десяти полей в 720p/1080p и сохранение отдельного confirm.
На полях числовая дробь сложности заменяется пятью мечами: заполнены N из пяти
по actual difficulty, остальные контурные. Проверять число слотов и заполнение
в обоих разрешениях; новые raster assets для этих простых UI shapes не нужны.
Закрытые карточки доступны для просмотра, но не для запуска; UI не раскрывает
имя/роль/умение/stats/lock reason и выводит
только силуэт и «?» во всех областях Character Select (уточнение DECISION-0087).
При этом presenter хранит
inspected ID отдельно от допустимого session selection; подтверждение проверяет
их совпадение и актуальный access. Список строится только из runtime-каталога;
тест десяти/двадцати полей использует fixture cards, не новые production definitions.
Main Menu: слои Menu E имеют общий landscape canvas, интерфейс не двигается
вместе с иллюстрацией. При скрытии меню/потере фокуса часы декорации останавливаются.
Unity использует смещение двух слоёв и отдельные Painter2D rays/dust; это не
перенос CSS 3D perspective и не изменение исходной картинки. Theme entry-* локальна
этим трём экранам; Settings/Meta/Results и принятый HUD/Draft/Pause не переоформляются.
HTML фиксирует композицию, но не считается финальной отделкой кнопок: после
выбора layout отдельный тематический проход применяет Art Direction §12.2
(матовые поверхности, тонкий контур/фактура, полная state matrix), без тяжёлого bevel.
Тематический проход поручен пользователем 2026-09-29: общий процедурный фон без
новых raster assets и единая state matrix кнопок распространяются на Entry,
Settings, Meta и Results; layout, navigation и доступность контента не меняются.

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

Чистовое Settings оформление по [DECISION-0093](../../decisions/0093-settings-folio-ui.md):
`SettingsPanel` / `UI/SettingsStyles.uss` и shell UXML, две колонки, проценты,
режим через dropdown, отдельный video confirmation. Служба Settings продолжает
владеть отсчётом и rollback. Проверить обе геометрии, modal focus, Apply/Back,
mouse/shake/audio, сохранение и возврат к исходному pause owner.

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

В игровом срезе Settings/Quit принадлежат прежнему `AppShellScreen`, но его
существующий `ShellPause` reparented в `GameplayUiRoot.PauseFooter`. Дублирования
кнопок/intent нет. При shutdown/rollback host возвращается в shell до очистки
gameplay tree. Global pause routing сначала учитывает закрытие popup и Space
на отображаемом UI control; закрытие справки или Resume не переключает pause
повторно в том же кадре. Скрытые controls не удерживают игровой shortcut.

`Game.Audio.AudioRoutingRuntime` владеет Music/menu SFX и двумя preview sources
(перенос из Settings — [DECISION-0072](../../decisions/0072-project-structure-and-audio-ownership.md));
settings preferences и `IAudioPreview` остаются в Game.Settings, production run events — у [IP-33](IP-33-production-audio.md).
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
