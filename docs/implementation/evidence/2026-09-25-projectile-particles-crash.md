# Bugfix 2026-09-25: исключение в FixedUpdate при взрыве после попадания

Статус — только [STATUS](../STATUS.md#field001-execution).

## Причина

`ProjectileImpactRuntime` и `ExplosionBurstRuntime` живут на одном пуловом root снаряда
(`FixtureProjectileRuntime.EnsurePresentationObjects`) и оба при первом `Play` делали
`gameObject.AddComponent<ParticleSystem>()`. Unity допускает один `ParticleSystem` на объект: после
попадания второй `AddComponent` (взрыв) логировал ошибку и возвращал null, а `ParticlePresentationMaterial.Apply`
(до DECISION-0055 — обращение к `_particles.main`) бросал NRE внутри `FixedUpdate → Simulate/TryImpact →
Explode → PlayExplosion`. Путь срабатывает на каждом попадании сферы SKILL-014 с визуалом взрыва; повторяющиеся
исключения со стеком на каждом физическом шаге обваливали FPS ниже 1.

## Исправление

- Каждый presenter создаёт при первом `Play` собственный дочерний объект (`ImpactParticles` /
  `ExplosionParticles`) и добавляет `ParticleSystem` на него; root снаряда частиц не несёт. Объект создаётся
  один раз и переживает возврат в пул; между жизнями — прежний `Stop(StopEmittingAndClear)`.
- Material-источник не изменён: `SpriteRenderer` root снаряда.
- Safety net: `ParticlePresentationMaterial.Apply(null, …)` бросает `ArgumentNullException` с объяснением
  вместо безымянного NRE.
- Отдельная DECISION не заводилась: это применение уже утверждённой изоляции presentation из
  [DECISION-0013](../../decisions/0013-procedural-sprite-presentation.md) (так же уже устроена пыль смерти —
  child `DeathDust` в `EnemyDeathPresentationRuntime`), без изменения контрактов и gameplay-иерархии.

## Проверки

| Проверка | Результат |
|---|---|
| .NET harness compile | 0 errors |
| .NET harness NUnit | 431/754 PASS, регрессий 0 относительно `1cea1f5`; новый тест требует Unity (ParticleSystem) — не исполним в harness |
| `smoke-check` (`check_project.py --scope full`) | **NOT RUN** — в облачной среде нет Unity |
| Регрессионный тест | `ProjectileLifecycleTests.ImpactThenExplosion_SamePooledProjectile_UsesSeparateParticleSystemsAcrossReuse` |

Первый Unity-прогон пользователя (2026-09-25): EditMode 726/727 — упала только новая проверка
`particleCount` сразу после `Emit`: у вручную симулируемой системы счётчик обновляется на следующем шаге
симуляции (в игре — tail-тик `FixedUpdate`). Исключений и ошибок `AddComponent` не было (иначе тест упал бы
раньше, на `TryImpact`). Тест теперь делает один шаг `Simulate(0.01)` перед подсчётом; нужен повторный прогон.
