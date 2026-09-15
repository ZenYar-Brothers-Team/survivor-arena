# DECISION-0007 — Визуалы как content-ссылки (SpriteDefinition/ContentRef)

Status: Approved

Date: 2026-09-15

Related IP: IP-00, IP-04, IP-05, IP-08; groundwork for IP-17, IP-20, IP-22

Related content IDs: —

## Context

Код-ревью PR #6 и последующий разбор кода показали, что placeholder-спрайты и цвета захардкожены прямо внутри runtime MonoBehaviour (`EnemyRuntime`, `FixtureProjectileRuntime`, `ExperienceDropRuntime`, мины скиллов), без структурной возможности задать разным врагам/умениям/будущим персонажам собственный визуал и без способа поймать отсутствующий presentation asset до рантайма. `FixtureActiveSkillCatalog.CreateRing()` также показал конкретный симптом: визуальный паттерн уровня (`Ring`/`Cross`) выбирался тем же inline-тернарником, что и механика, из-за чего `Cross` на деле повторял формулу `Ring` (отдельный баг, см. PR-ревью, не исправлен этим решением). `IP-17-production-skills.md` уже требует «production label/icon reference... отсутствующий presentation asset обнаруживается validator» — то есть решение было предвосхищено в плане, но не реализовано. Пользователь попросил заложить архитектуру заранее (не идеально, с заглушками 1×1), чтобы IP-17+ не изобретали её с нуля и чтобы текущий код было проще читать.

## Decision

- Новый модуль `Game.Presentation`: `SpriteDefinition` — визуал как полноценный `IContentDefinition` (регистрируется в том же `ContentRegistry`, что и остальной контент); `FixtureSpriteCatalog` — генерирует заглушки 1×1 для любой задекларированной, но ещё не заполненной ссылки на визуал.
- `EnemyDefinition` и `ActiveSkillLevelDefinition` получили опциональный `ContentRef<SpriteDefinition> Visual` (по умолчанию не задан — обратная совместимость с существующими тестами/контентом). `EnemyDefinition` и `ActiveSkillProgressionDefinition` реализуют `IReferencesContent`, поэтому `ContentRegistry.Build()` автоматически проверяет любой заданный визуал — отсутствующий presentation asset ловится на этапе сборки контента, как и требует IP-17.
- `FixtureRuntimeContentCatalog` сканирует все `IReferencesContent`-ссылки и сам подставляет заглушки через `FixtureSpriteCatalog`, так что fixture-контент не может забыть зарегистрировать визуал.
- Резолвинг в рантайме доведён до конца только для врагов: `EnemyDefinition.Visual` резолвится один раз в `GameplayCompositionRoot`, передаётся через `EnemyFactory`/`ContinuousFixtureEnemySpawner`, и `EnemyRuntime` рисует именно его вместо захардкоженного плейсхолдера. Для активных умений реализован только слой данных и валидации — резолвинг в реальную отрисовку снарядов/мин оставлен как следующий шаг для IP-17.
- `FixtureActiveSkillCatalog`: скилл Ring/Cross теперь явно задаёт разные `Visual`-ссылки для уровней 1–3 и 4–6 вместо неявного тернарника — демонстрация целевого паттерна per-level авторинга (сам баг с формулой `Cross`/`Ring` не исправлен и отслеживается отдельно).

## Consequences

- Game Design не затронут — арт и визуальная спецификация явно вне его области.
- Acceptance criteria IP-17 («production label/icon reference... missing presentation asset обнаруживается validator») теперь имеют конкретную техническую реализацию вместо изобретения с нуля.
- IP-04/IP-05/IP-08 evidence дополняется опциональными полями; поведение существующего fixture-контента не меняется, так как `Visual` по умолчанию не задан.
- IP-20 (Production Enemies) и IP-22 (Production Characters) наследуют ту же конвенцию бесплатно после снятия блокировки.
- Тестовые asmdef (`Game.Enemy.Tests`, `Game.ActiveSkill.Tests`, `Game.Progression.Tests`) получили ссылку на `Game.Presentation`, так как конструкторы `EnemyDefinition`/`ActiveSkillLevelDefinition` теперь требуют разрешения этого типа при any вызове, даже без явной передачи `visual`.

## Approval

Пользователь явно запросил 2026-09-15: «давай сразу заложим логику для спрайтов и для прокачек, чтобы в будущем нам было проще все добавлять... надо сразу продумать структуру кода для будущих ip-17+... не нужно сразу реализовывать идеально, можно пока только архитектуру накидать и передавать туда заглушки в виде спрайтов 1на1».
