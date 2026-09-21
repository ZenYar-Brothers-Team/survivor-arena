# IP-14 — Wave Director: continuous и burst timeline

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Доработать действующий директор и spawner, сохранив division Director decides/spawner executes, pooling и phase scaling.

## Зависимости

[IP-04](IP-04-enemy-core.md), [IP-13](IP-13-enemy-patterns.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

GDD «Структура забега…», «Враги…»; Content Fields/empty Wave Encounter section; DECISION-0014; WaveDirector/timeline/spawn timer/spawner.

## Scope

Data-driven continuous/burst mode, count/window/composition/cap/geometry/seed; ordinary/pressure/elite/rest rhythm, independent modifiers, one-shot mid/final boss hooks. Pause-aware run time. Actual spawn/cap/deferral outcomes observable; no separate hidden default. Traveler schedule независим и не становится обычной wave type.

## Out of Scope

Production schedules, adaptive difficulty, Traveler RNG/type selection, profiler claims from throttled warnings.

## Acceptance criteria

Burst исполняется один раз в заданном окне независимо от regular enemy cap: заполненный лимит не обрезает группу и не откладывает её появление. Боссы и Путники не занимают regular cap. Пауза замораживает время окна; завершившиеся окна и невышедший остаток не воспроизводятся после skip; terminal state прекращает спавн. Continuous behavior retained. Phase transitions/skips/last hold/hook boundaries deterministic на director level; later wave may be faster but frailer. Registry/pool stays consistent at repeated load. Spawn actual counts distinguish requested/suppressed/deferred. Production schedules не выводятся из fixture timeline.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

Phase HUD; dev timeline/composition/actual cap/burst/hook diagnostics; producer fields доступны IP-31, но gameplay не зависит от recorder.

## Проверки

Continuous regression, burst 0/1/count/cap, phase skip/catch-up/same-time hook, pause/end/restart, pool reuse; load bound with recorded hardware/counts and approved threshold. Seeded decisions ≠ full physics replay.

## Документационные изменения

Update DECISION-0014 через явное дополнение, wave schema и fixture rationale; IP-15/24 consume target contract.

## Gates и недостающие решения

W-01 утверждён [DECISION-0029](../../decisions/0029-burst-pressure-and-player-palette.md). G-11/G-14 остаются только в части shared event boundary/data; production schedules и Traveler-specific timing этим решением не определяются. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-15](IP-15-boss-framework.md), [IP-16](IP-16-field-framework.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Runtime и fixture schema

`WaveTimelineData.seed` и `WavePhaseData.spawnMode` обязательны. Mode — `Continuous`
или `Burst`, независимо от rhythm tag. Continuous сохраняет interval/cap; burst требует
объект `burst` с `count` (целое ≥0), `offsetSeconds` (≥0) и `windowSeconds` (>0).
Окно `[offset, offset + window)` находится внутри duration фазы. Например, count=18,
offset=0, window=1 в фазе с началом 45 s: первый running tick в `[45,46)` запрашивает
все 18 врагов; tick ровно в 46 s отменяет группу. Один burst — одна группа, без
растягивания выдачи и без повторных попыток. Интервал и regular cap остаются явными
положительными полями фаз; burst их не применяет.

Director использует elapsed run time для окна и transitions, delta для continuous timer.
При смене фазы continuous delta ограничен временем, проведённым в новой фазе;
пропущенные фазы не накапливают заявки. Last hold не продлевает burst window.
Время не может идти назад; новый забег создаёт новый director. `isRunning=false`
не меняет timeline/hook/consumption state. При одинаковом времени hooks упорядочены
MidBoss → FinalBoss, исполняются по одному разу перед spawn decision. Пропущенные
hooks догоняются; пропущенные burst windows отменяются. Terminal state не догоняет hooks.

Spawner владеет только ordinary enemies, включая spawned burst: они учитываются
при последующем continuous cap. Boss/Traveler owners используют отдельный lifecycle
и не входят в этот счётчик. Seeded independent RNG streams выбирают composition и
равномерный угол на окружности `spawnRadius` (world units) вокруг текущей позиции игрока.
Это воспроизводимость решений, а не физики/движения игрока.

`WaveDirector.LastDecision` — requested/allowed/suppressed/expired/deferred;
`ContinuousFixtureEnemySpawner.LastSpawnOutcome` и `SpawnResolved` — immutable producer
facts с phase/time/mode, actual, unavailable, regular alive/cap. `Tick` возвращает actual.
Suppressed = requested − allowed; unavailable = allowed − actual; deferred всегда 0.
Expired — сумма численностей пропущенных групп за этот tick, не часть requested.
При unavailable группа считается выданной и не повторяется. UI показывает последний
ненулевой outcome в существующей gated/collapsed Run drawer; phase HUD сохраняется.
IP-31 может подписаться на producer независимо от gameplay; запись в exporter не обязательна.

Fixture сохраняет ordinary/elite/rest cadence и меняет три pressure-фазы на группы
18/26/34 с окном 1 s от начала фазы: synthetic проверка uncapped pressure и faster/frailer
modifiers, не утверждённый баланс поля. Production schedules принадлежат IP-24.

## Нагрузочная проверка

Пользователь 2026-09-21 утвердил spawn-only bound на текущем ПК: 100 врагов,
10 циклов, холодное создание ≤250 ms, повторное создание из пула ≤50 ms за группу,
без роста числа объектов и registry residue после shutdown. Тест записывает CPU/RAM,
Unity version и фактические времена. Это не FPS target и не gameplay density review IP-12A.
