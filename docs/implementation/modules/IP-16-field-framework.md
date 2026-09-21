# IP-16 — Field definitions, selection и run configuration

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Framework использует существующую Gameplay scene и независимый модуль `Game.Field`.
Production field geometry и assets поставляет IP-23; их готовность не следует из fixture packet.

## Зависимости

[IP-02](IP-02-player-movement.md), [IP-12](IP-12-character-framework.md), [IP-14](IP-14-wave-director.md), [IP-15](IP-15-boss-framework.md), [IP-10A](IP-10A-ui-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Текущая fixture geometry дополнена [DECISION-0035](../../decisions/0035-traveler-encounter-rules.md):
FixtureArenaGeometry.json задаёт внутреннюю сторону 20 полных высот reference
viewport (200 world units), reference height 10 и толщину стен 0.5. Editor baker
сохраняет positions/colliders в Gameplay scene; это не production geometry mapping.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD/CD «Поля», schema FIELD-001…010; UI §§4–5,23; player-only geometry DECISION-0003; ContentRef contract.

## Scope

selected character/field run configuration; typed environment/enemy/wave/boss/midboss references; field display name/description/thumbnail/difficulty/unlock presentation metadata, source профиля для available IDs. Difficulty 1–5 задаётся данными, а не вычисляется из номера ID. Field-defined Traveler schedule extension point не требует раннего production binding. IP-16 не зависит от IP-29/IP-30 или IP-24: это field framework с optional encounter extension points, а не готовый production timeline.

## Out of Scope

production geometry/картинки/расписания всех полей, придумывание чисел сложности или unlock conditions.

## Acceptance criteria

два fixture fields дают различимую configuration; locked нельзя запустить; Back сохраняет допустимую selection; missing/wrong refs отвергаются до run; player-only obstacles не блокируют enemies/projectiles. Недостающее конкретное difficulty/unlock значение выявлено явно.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

функциональный field selection slice с lock reason, description и difficulty; placeholder thumbnail допустим до image approval, выбранный ID виден в run snapshot.

## Проверки

selection/locked/back, invalid refs, geometry, reinitialization другого field; presenter и PlayMode selection→correct field.

## Документационные изменения

field/run-start contract и metadata units, поле для будущего Traveler binding без circular dependency; IP-23/IP-24/IP-26 Context.

## Gates и недостающие решения

G-14: конкретные geometry/encounter values; G-15 resolved по DECISION-0037, G-20 resolved по DECISION-0038. Fixture metadata отдельно. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

G-20 resolved: [DECISION-0038](../../decisions/0038-settings-and-field-difficulty.md)
задаёт единую шкалу 1–5 и explicit production mapping в CD. Fixture difficulty
сохраняется; runtime не вычисляет значение из номера ID. Traveler rank отдельно.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-25](IP-25-meta-progression.md), [IP-26](IP-26-functional-ui.md), [IP-27](IP-27-integration.md), [IP-29](IP-29-traveler-framework.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Boss encounter binding

Field configuration выбирает timeline и encounter definitions согласованно с
[IP-15](IP-15-boss-framework.md#fixture-schema-и-phase-contract): final definition/hook
обязательны, midboss hook optional; каждый используемый hook имеет definition.
BossEncounterRuntime получает fresh WaveDirector/RunModel перед началом run.
Смена field не переносит consumed hooks, phase или boss life предыдущего run.
Fixture boss schema не закрывает production G-14 schedules/rewards.

## Framework API и fixture schema

`FieldDefinition` — `IContentDefinition/IReferencesContent`, independent `Game.Field`
assembly. `Resolve(registry)` до инициализации gameplay проверяет типы/наличие
environment, enemy pool, timeline, final/midboss и optional Traveler references.
Все враги timeline должны входить в field pool. Final hook/definition обязательны;
mid hook и definition присутствуют либо оба, либо ни одного. Перепутанные boss roles
отвергаются даже при совпадающем CLR type.

`Assets/Resources/Content/Fields/FixtureFields.json` содержит:

| Поле | Контракт |
|---|---|
| `defaultFieldId`, `availableFieldIds` | Явный fixture profile; default обязан существовать и быть доступным |
| `displayName`, `description`, `thumbnailPlaceholder`, `unlockDescription` | Обязательные непустые строки; текстовая заглушка preview, без сгенерированного арта |
| `difficulty` | Обязательное целое 1–5, без единиц; fixture-only UI metadata, не множитель combat stats |
| `environmentId`, `timelineId`, `finalBossId`, `enemyIds` | Обязательные typed refs; enemy pool непустой, без повторов |
| `midBossId`, `travelerScheduleId` | Optional typed refs; отсутствие явно означает отсутствие binding |
| environment `sceneName`, `spawnPointName`, `obstacleNames` | Binding существующей scene; имена уникальны, collider active/non-trigger/player-only |

Два synthetic поля: `FIXTURE-FIELD-OPEN` (difficulty 2) использует прежние семь
wave enemy types, timeline и mid/final encounters; `FIXTURE-FIELD-FOCUSED`
(difficulty 1) — seeker-only timeline с seed 13579, radius 7 world units,
continuous interval 3 s/cap 5 и единственным final hook 840 s. Эти числа нужны
для различимого тестового запуска и не являются production balance.
Оба поля ссылаются на существующий `FIXTURE-ENVIRONMENT-ARENA` в Gameplay scene;
данный packet не добавляет новую геометрию и не меняет collision masks/art.

`IFieldAccessProvider.GetLockReason(id)`: null = available, непустая причина = locked.
`FieldRoster` проверяет доступ заново при выборе и старте. Session не выбирает
другое поле автоматически при отзыве доступа. `FieldSelectionSession`/presenter
владеют navigation intent, а composition root — запуском. Back пересоздаёт character
session с прежним character ID; field ID сохраняется при следующем переходе вперёд.
Новый отдельный вызов `OpenCharacterSelection` начинает selection с defaults профиля.

Run start сохраняет `RunSelectionSnapshot` (character/field/environment/timeline)
в `RunModel`, а terminal capture — в `RunOutcome.Selection`; optional telemetry
использует те же resolved field/timeline и actual timeline seed. Snapshot immutable,
привязывается ровно один раз до Start; core tests без composition могут иметь null.
Каждый новый run получает fresh model/director/encounter hooks и возвращает игрока
на bound spawn point. Failure после начала composition разматывает subsystems;
cancel selection оставляет uninitialized adapters выключенными до нового запуска.

Field-owned token и его конкретный consumer описаны ниже. Исходная техническая запись: [DECISION-0032](../../decisions/0032-field-run-configuration.md).

## Traveler payload consumer

FieldTravelerScheduleDefinition — базовый typed token; TravelerScheduleDefinition
из Game.Traveler добавляет validated policy/pool. Оба текущих fixture fields имеют
конкретный schedule, composition root передаёт его TravelerEncounterRuntime.
Bare token без payload по-прежнему отвергается. Field не зависит от Traveler;
production field pools/rank/schedules поставляют IP-24/IP-30.