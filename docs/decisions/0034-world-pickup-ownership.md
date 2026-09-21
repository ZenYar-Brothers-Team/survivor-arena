# DECISION-0034 — World pickup ownership и fixture geometry

Status: Proposed

Date: 2026-09-21

Related IP: IP-06, IP-07, IP-09, IP-11, IP-18, IP-19, IP-20, IP-26, IP-27, IP-28, IP-29, IP-30, IP-31

## Context

Approved правила pickup находятся в [DECISION-0033](0033-world-pickup-rules.md).
Эта запись документирует техническую реализацию между combat, progression,
field geometry, UI и telemetry; не добавляет продуктовых правил.

## Implementation record

`Game.Pickup` владеет definitions, life identity/state machine и pooled world objects.
Bootstrap разветвляет ordinary death в XP и potion sinks; оба сохраняют собственную
дедупликацию. Pickup reward target использует реальный PlayerCharacterRuntime.Heal,
общий RequestBook и SetRewardEvent с CombatSourceOrigin.Pickup. UI и telemetry
читают immutable snapshots/events, без обратной зависимости gameplay на UI.

Стабильный порядок — spawn sequence в пределах run. Tick проверяет expiry перед
contact каждого объекта; Book pause останавливает остаток snapshot. Claim защищает
от повторного входа, drop ID — от stale pooled callback. Shutdown отменяет оставшиеся
жизни и отписывает внешний run event; composition root выполняет rollback.

Fixture geometry adapter работает с axis-aligned прямоугольниками существующей
Gameplay arena. Он учитывает footprint игрока и связность со spawn, а не только
clamp внешней границы. `IPickupPlacement` позволяет заменить его для production
геометрии. FIXTURE-POTION/FIXTURE-BOOK, seed/chance/heal/visual параметры находятся
в JSON. Текстовые маркеры не являются production sprites или art approval.

## Consequences and approval boundary

Потребители используют единый world reward contract; Traveler не должен создавать
параллельный Book/draft lifecycle. Production PICKUP-001/data/art остаются IP-20,
production Book card/ID/data/art — IP-30. Это Proposed technical record для review,
не утверждение архитектуры пользователем и не снятие production gates.
