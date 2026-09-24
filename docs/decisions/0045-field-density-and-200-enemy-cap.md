# DECISION-0045 — Плотность FIELD-001 и предел 200 обычных врагов

Status: Approved
Date: 2026-09-22
Related IP: IP-14, IP-16, IP-23
Supersedes: visual-only obstacle limitation in DECISION-0044

## Основание

После gameplay review пользователь сообщил, что декор почти не встречается, плетень не виден вовсе, а пень существует только на месте единственного прежнего fixture obstacle. Пользователь также поручил поднять максимальное число обычных врагов до 200.

## Решение

`FIXTURE-WAVE-FINAL-RUSH.maxAliveEnemies` равен 200. Это предел ordinary-enemy spawner в финальной continuous-фазе; boss и Traveler lifecycles по-прежнему не расходуют regular capacity.

FIELD-001 fixture presentation создаёт 64 дополнительных статических препятствия: 16 размещаются в радиусе 22 world units от старта, остальные распределяются по полю. Seed, число, доля плетней, размеры коллайдеров, минимальная дистанция и число попыток задаются presentation JSON. Плетни используют axis-aligned `BoxCollider2D`, пни — `CircleCollider2D`; оба collision mask блокируют только Player. Враги, снаряды, эффекты и XP проходят через них согласно общему правилу поля.

Pickup и Traveler placement учитывают bounds созданных препятствий. Spawn point, исходный `Obstacle_Fixture` и соседние препятствия имеют authored clearance. Decoration grid уплотняется с шага 10 до 6 world units и с вероятности 0.45 до 0.75; декор остаётся visual-only и не накрывает препятствия.

## Следствия

- стартовая область показывает несколько объектов в пределах первых экранов;
- общая arena geometry остаётся 200×200, но DECISION-0044 больше не описывает внутренние props как полностью visual-only;
- 200 — regular cap, а не общий лимит всех GameObjects, projectiles, pickups, bosses и Travelers;
- spawn/pool benchmark проверяется на 200 врагах; реальная frame-time оценка при 200 живых врагах остаётся отдельным gameplay/performance review.
