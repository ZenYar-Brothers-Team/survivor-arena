# DECISION-0006 — Общая модель здоровья (Game.Combat.Health)

Status: Approved

Date: 2026-09-15

Related IP: IP-03, IP-04

Related content IDs: —

## Context

Код-ревью PR #6 (develop-evg → develop) выявил, что `Game.Character.CharacterHealth` и `Game.Enemy.EnemyHealth` независимо дублируют логику урона, смерти и событий: `EnemyHealth` — упрощённая копия `CharacterHealth` без масштабирования по статам, лечения и regen. Пользователь попросил учесть при исправлении потенциальное расширение — несколько одновременных игроков в забеге и юниты-союзники (например, наносящий урон компаньон, следующий за игроком, или союзник, только выдающий пассивку).

## Decision

- Выделен модуль `Game.Combat` с единственной реализацией `Health` и интерфейсом-точкой расширения `IHealthProfile` (`MaxHealth`, `IncomingDamageMultiplier`, `HealthRestorationMultiplier`, `HealthRegenerationPerSecond`, событие `Changed`).
- `Game.Character.CharacterStats` реализует `IHealthProfile` без изменения собственной логики.
- Добавлен `FixedHealthProfile` — статичный профиль (множители = 1, regen = 0) для сущностей без процентной композиции статов; сейчас используется врагами.
- `CharacterHealth.cs` и `EnemyHealth.cs` удалены; `PlayerCharacterRuntime.Health` и `EnemyRuntime.Health` теперь оба типа `Game.Combat.Health`.

## Consequences

- Game Design и Content Design не затронуты — изменение чисто архитектурное, системные правила здоровья (DECISION-0004) не меняются.
- IP-03 и IP-04 evidence: путь `CharacterHealth.cs`/`EnemyHealth.cs` заменён на `Assets/Game/Combat/Health.cs`.
- Будущий юнит-союзник (пока не часть Game Design) может переиспользовать `Health` напрямую через `FixedHealthProfile` или собственный stats-backed профиль, не дублируя урон/смерть заново; несколько одновременных игроков уже поддерживаются без изменений, так как каждый `PlayerCharacterRuntime` создаёт собственные `CharacterStats`+`Health`.
- Базовое поведение `Health` покрыто `Game.Combat.Tests/HealthTests.cs`; интеграция со статами — `Game.Character.Tests/CharacterHealthIntegrationTests.cs` (бывший `CharacterHealthTests.cs`).

## Approval

Пользователь явно выбрал вариант 2026-09-15: «Вариант 1: единый Health + IHealthProfile (рекомендую)» и разместить модуль в «Новый модуль Game.Combat (рекомендую)».
