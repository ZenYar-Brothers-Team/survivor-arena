# DECISION-0012 — Единый валидатор числовых аргументов

Status: Approved

Date: 2026-09-15

Related IP: cross-cutting (Game.Content, Game.Combat, Game.Character, Game.Enemy, Game.Progression, Game.ActiveSkill)

## Context

Ревью PR #6 (комментарий на `CharacterBaseStats.cs:62`) отметило, что
`ValidatePositive`/`ValidateNonNegative`/`ValidateFinite`-подобные приватные
статические методы скопированы дословно в нескольких доменных типах.
Повторный аудит перед фиксом показал, что дублирование хуже, чем в
исходном комментарии: после более раннего разбиения на один-класс-на-файл
такой паттерн независимо встречался уже в 8 файлах
(`Health.cs`, `ExperienceProgression.cs`, `PlayerExperienceRuntime.cs`,
`CharacterBaseStats.cs`, `CharacterStatModifier.cs`, `EnemyDefinition.cs`,
`ActiveSkillCooldown.cs`, `ActiveSkillProjectile.cs`), плюс частично
консолидированная, но всё равно локальная копия внутри
`ProjectileBurstEffect.cs`, от которой зависели ещё 8 файлов эффектов через
`internal static`-методы одного класса.

## Decision

- Новый статический класс `Game.Content.NumericValidation` —
  `ValidateFinite`/`ValidatePositive`/`ValidateNonNegative`/
  `ValidateNonNegativeFinite`/`ValidateCount`/`ValidateRange`, каждый с
  опциональным параметром `message`, чтобы сохранить единственное
  расхождение в тексте исключения (`ExperienceProgression`: "Thresholds must
  be greater than zero.") без спецкейсов на стороне вызова.
- `Game.Content` уже не имеет собственных зависимостей — естественное место
  для сквозной утилиты, используемой из `Game.Combat`, `Game.Character`,
  `Game.Enemy`, `Game.Progression`, `Game.ActiveSkill`.
- `Game.Character` и `Game.Combat` ранее не ссылались на `Game.Content` —
  добавлена ссылка в оба `.asmdef` (без риска цикла, так как у `Game.Content`
  нет собственных зависимостей).
- `ProjectileBurstEffect`'s `internal static` валидаторы удалены полностью;
  все 8 его прежних вызывающих сторон (`MineEffect`, `AreaEffect`,
  `ChainEffect`, `BoomerangEffect`, `OrbitEffect`, `BeamEffect`,
  `ActiveSkillLevelDefinition`, `ActiveSkillActivationWave`, а также
  `ProjectileDirectionGenerator`) переключены на `NumericValidation` напрямую.
- Поведение сохранено 1-в-1: тот же тип исключения (`ArgumentOutOfRangeException`),
  тот же текст сообщений — подтверждено полным прогоном тестов, а не только
  компиляцией.

## Consequences

- Новое правило и правило будущей полиморфной валидации записаны в
  `AGENTS.md` — любой новый доменный тип обязан переиспользовать
  `NumericValidation` вместо копирования приватного метода.
- Полностью удалён источник расхождения сообщений об ошибках между похожими
  проверками в разных модулях.

## Approval

Пользователь явно утвердил 2026-09-15 в рамках повторного триажа комментариев
PR #6 («нужен какой-то общий валидатор» напротив пункта о дублировании
Validate*-хелперов), после согласования общего плана из пяти пунктов через
`EnterPlanMode`/`ExitPlanMode`.
