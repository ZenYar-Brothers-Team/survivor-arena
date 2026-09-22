# DECISION-0016 — Per-life enemy events и XP adapter

Status: Proposed

Date: 2026-09-20

Related IP: IP-01, IP-04, IP-05, IP-06, IP-28, IP-31

## Context

Принятый scope IP-04 требует immutable death payload после pool return, различение kill/cleanup/escape и независимость lifecycle от конкретных pickups. Ранее EnemyRuntime непосредственно создавал XP через Progression, а AoE использовал общий изменяемый буфер.

## Implementation proposal

- Каждый Initialize создаёт новый Guid LifeId. `IEnemyLifeTarget` расширяет damage target стабильными для одной жизни content/category/life полями. Run identity фиксируется при spawn.
- `EnemyLifeEvent` содержит immutable content ID, life ID, run ID (если модель доступна), category, kind, reason, position, nullable damage source и configured XP reward. Только kind Died — смерть; Despawned с reason Killed — последующая уборка того же life, не второе убийство.
- `IEnemyLifecycleSink` задаётся до Spawn. Legacy Died/Despawned callbacks сохраняются, но per-life подписки очищаются при окончании жизни. Возврат в пул происходит после callbacks; повторная Initialize внутри callback отклоняется.
- `EnemyExperienceDropSink` находится в Progression и соединяется composition root. Enemy больше не ссылается на Progression; Progression получает зависимость на Enemy contract. Drop amount/lifetime/position/pool policy сохранены. Future world-pickup policy здесь не выбирается.
- Spawner публикует ordinary-enemy kill contribution в RunOutcome; не выдаёт количество boss/Traveler deaths за уже поддержанные данные.
- Health устанавливает IsDead до HealthChanged/Damaged callbacks. Это предотвращает reentrant healing/duplicate death; порядок самих событий HealthChanged → Damaged → Died сохранён.
- Каждый AoE вызов арендует собственные collection buffers, возвращая их в finally. Вложенный damage callback не портит внешний обход.

## Boundaries / review

Это техническая реализация уже принятого IP-04, а не новая игровая или reward policy. Запись остаётся Proposed для архитектурного ревью; она не заменяет Game/Content Design. G-06…G-09 не закрываются этим решением. TD-001/TD-003 затронуты указанными конкретными изменениями; остальной debt register не пересматривается.

## Checks

Проверки pool reuse, source/position retention, cleanup/escape, live reinitialize, synchronous lethal callbacks, nested AoE и прежней XP integration находятся в Game.* test assemblies. Оперативные результаты запусков — только в [STATUS](../implementation/STATUS.md).
