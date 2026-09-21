# IP-26 — Functional UI и полный player flow

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-01](IP-01-run-lifecycle.md), [IP-10A](IP-10A-ui-foundation.md), [IP-11](IP-11-set-framework.md), [IP-12](IP-12-character-framework.md), [IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-25](IP-25-meta-progression.md), [IP-28](IP-28-world-pickups.md), [IP-29](IP-29-traveler-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

UI §§1–5,11–23 полностью; новые GDD core loop/run/XP/build/sets/fields/characters/meta/Travelers; только отображаемые production cards и profile metadata; DECISION-0005. IP-10A owns reusable cards/HUD/build, IP-26 owns settings, feature modules own boss/Traveler/Book state.

## Scope

Main Menu Play/Meta/Settings/Exit; Character Select→Field Select→Run; level-up/Book drafts; Pause/Build→Resume/Settings/Quit; Victory/Defeat→Results; Retry немедленно с теми же character/field, Main Menu, meta purchases. Results: outcome/time/level/kills/currency/sets/unlocks; top-3 skills by damage только при доступной корректной attribution IP-31, без обязательного отдельного analytics screen. В этом же IP находятся basic Settings: persisted Master/Music/SFX, resolution/window mode, current movement keys, Screen Shake toggle. Поставить минимальные рабочие audio routing endpoints/preview и presentation consumer shake, не декоративные controls. Внутренние этапы shell→settings→full navigation являются checklist одного IP, не отдельными execution statuses.

## Out of Scope

новые game/economy rules, generation final images внутри UI, compendium/advanced analytics/controller polish/localization до отдельного scope, сложные transitions. Remap, production audio catalog и расширенный accessibility menu — отдельно; basic Settings из Scope не относятся к этому исключению.

## Acceptance criteria

полный цикл без DEV; locked choices недоступны, relevant baseline modifiers и difficulty понятны. Retry без дополнительного confirmation/selection создаёт новый run и очищает старые subscriptions/entities/UI while preserving профиль/settings. Draft completion не снимает чужие pause reasons; settings возвращает в исходный экран; результаты и награды совпадают с model. Quit reward/confirmation policy и краткий victory→results transition получают конкретный контракт, если ещё не заданы. Каждый enabled setting реально применяется и сохраняется; invalid/corrupt preferences дают документированный safe fallback, unsupported video mode не запирает пользователя; Back/Resume сохраняют pause reasons и selection. Shake off отключает только visual effect, baseline camera follow сохраняется согласно решению G-16. Обязательные Results работают в release без IP-31; optional top 3 только при доступной attribution.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

целевой screen/state/intent map, keyboard+mouse input/focus, normal/hover/pressed/disabled/selected/locked, meaningful errors; все player-facing данные отдельно от DEV. Traveler HP у каждого, arrow исчезает при видимости/death/escape; countdown ухода не добавляется.

## Проверки

fresh/existing profile happy/death/level-up/Book/pause/settings/quit/retry/unlock/purchase paths, double-click/idempotency, long text/empty lists, alternate supported resolution; PlayMode full flow плюс ручной 1920×1080 review. Preference validation/fake store, channel isolation, input binding display, manual video apply/revert/fullscreen/audio/shake; release/no-recorder Results.

## Документационные изменения

complete UI flow и semantic IDs; явно разграничить IP-10A/IP-26/feature-owned slices, обязательный art binding и отложенный polish. Старый blanket Out of Scope «audio/settings» не скрывает утверждённый Settings scope — его реализует IP-26. Settings persistence и service ownership документируются здесь; production soundtrack/SFX library не добавляется.

## Gates и недостающие решения

G-15/G-16: Quit/reward/failure ordering, real audio/shake/settings contract. Retry same character/field immediate уже утверждён. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Character selection integration

Переиспользовать `CharacterSelectionSession`, `CharacterSelectPresenter` и `ContentCard` из [IP-12 API](IP-12-character-framework.md#framework-api-и-fixture-schema). Composition создаёт выбранный loadout до запуска run clock; profile access поставляет IP-25. Selection panel имеет отдельный жизненный цикл. Новый navigation flow не должен возвращать автоматический запуск startingCharacterId или вычислять baseline из roster.


## Presentation preference boundary

IP-12A предоставляет `IScreenShakePreference` и `ScreenShakeRequestGate`: preference читается на каждом request, выключенное значение и не-running state не выпускают запрос. IP-26 реализует persistent setting и camera consumer/сброс активного offset при выключении; наличие request boundary не закрывает G-16 и не означает готовность audio/settings service. См. [IP-12A contract](IP-12A-visual-presentation-foundation.md#контракт-технического-пакета).
