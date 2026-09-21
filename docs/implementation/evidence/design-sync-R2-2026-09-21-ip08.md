# design-sync-R2 — IP-08 evidence, 2026-09-21

Датированная запись реализации и фактических проверок. Текущий execution status и pause boundary определяет только [STATUS](../STATUS.md).

## IP-08

### Реализация и approved semantics

- [DECISION-0021](../../decisions/0021-additive-skill-level-bonuses.md) фиксирует подтверждённое пользователем сложение процентов level upgrades к базе. `ActiveSkillLevelResolver` разворачивает `baseLevel` и шесть `levelChanges` при загрузке JSON; legacy resolved `levels` сохранены. Каждый результат проходит validating domain constructors, неизвестный путь/невалидный результат отклоняется. Повторный `SetLevel` не усиливает параметры заново.
- Targeting поддерживает current/last movement с configured initial direction, nearest, uniform random valid world targets в radius, fixed axes и independent random directions. Delayed random waves фиксируют мировые точки и выбирают разные доступные цели. Seed хранится в fixture JSON; viewport не участвует в выборе.
- `ActiveSkillActivation` хранит damage/source/level/size/range/knockback snapshot. Executor отдельно масштабирует travel/search и hit geometry. Chain идёт от последней поражённой цели к ближайшей ещё не поражённой. Бумеранг использует общий для skill instance ledger по жизни цели, в том числе между снарядами, активациями и фазами возврата.
- Projectile runtime поддерживает ricochet/falloff, unlimited pierce option, отдельный impact/explosion, линейное замедление и despawn при остановке. `SceneProjectileLauncher` владеет явным pool; collider выключается при возврате, повторная инициализация очищает предыдущую жизнь. Damage/explosion callbacks не могут завершить следующую жизнь переиспользованного снаряда.
- 13 FIXTURE definitions сохраняют старые effect families и добавляют movement, random world target, deceleration, ricochet и spheres. Production SKILL IDs и art не регистрировались. Skill observability добавлена в существующий bounded scrolling Build debug tab; UI presenter tests и UXML contract обновлены. Upgrade preview показывает resolved spatial deltas.
- [IP-08](../modules/IP-08-active-skill-framework.md#контракт-параметров-для-потребителей) содержит effect→parameter applicability и trace SKILL-001…016 до code/tests. IP-03/IP-09/IP-17 ссылаются на этот контракт. Content Design и proposal синхронизированы с approved arithmetic.

### Проверки

Runner: `.claude/skills/smoke-check/SKILL.md` → `scripts/Test-Unity.ps1 -TestFilter '^Game\.'`. Перед каждым запуском свежий `Win32_Process` check подтвердил отсутствие Unity Editor; batch не запускался поверх interactive Editor.

Итоговый запуск: Unity **6000.6.0f1**, 2026-09-21:

| Platform | Passed / total | Failed | Skipped |
| --- | --- | --- | --- |
| EditMode | 364 / 364 | 0 | 0 |
| PlayMode | 2 / 2 | 0 | 0 |

Только `Game.*`; third-party tests не включены. Локальные результаты: `TestResults/EditMode.xml`, `TestResults/PlayMode.xml`, соответствующие `.log`; runner exit 0. Эти файлы не хранятся в Git.

Новые checks: `SkillLevelResolutionTests`, `SkillTargetingTests`, `SkillEffectMappingTests`, `ProjectileLifecycleTests`; расширены `PlayerActiveSkillSetRuntimeTests`, `ActiveSkillProgressionFrameworkTests`, UI tests. Они проверяют additive L1→L6, seeded uniform choice, movement fallback, delayed captured positions, separate size/range, action-speed cadence, chain falloff, boomerang ledger/return controls/pooled target identity, deceleration integral, pause/end/pool reuse, explosion reinitialization callbacks и шесть concurrent skills с shutdown/reinit.

PlayMode: прежний `GameplaySmokeTests` и новый `ActiveSkillPatternSmokeTests` прошли. Новый smoke запускает реальные independent decelerating projectiles, проверяет движение через FixedUpdate, freeze при pause и возврат в pool после окончания run. Полный EditMode прогон также покрывает lifecycle, damage/death, XP/draft, active skills, wave/spawn/pool, composition rollback, content loading и UI presenter.

Промежуточный targeted run выявил две ошибки тестового fixture/evidence: shared JSON wave object давал обеим волнам одинаковую задержку, а проверка позиции выполнялась после возврата body в pool. Исправлены JSON delays и проверка расстояния до recycling. Последующие полные прогоны подтвердили исправления. После финального code run менялись только документы.

`git diff --check` и синтаксическая проверка всех content JSON выполнены. Новые C# файлы имеют `.meta`, существующие GUID сохранены.

### Границы результата

Это framework compatibility, не per-ID production certification. Числа fixtures, seeds и placeholder sprites не утверждают баланс/арт. Production skill payload, tracking tuning, визуальная читаемость и actual-speed art review относятся к IP-17; G-04 return-disc policy остаётся его affected gate. Новых правил set amplification нет. IP-09 не начат: пользователь потребовал паузу после IP-08.
