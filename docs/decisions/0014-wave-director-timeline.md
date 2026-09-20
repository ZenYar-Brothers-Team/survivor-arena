# DECISION-0014 — Wave Director: таймлайн как контент, директор решает, спавнер исполняет

Status: Approved

Date: 2026-09-20

Related IP: IP-14; контракт хуков потребляет IP-15; production-расписания остаются IP-24 (CG-02)

Related content IDs: FIXTURE-WAVE-TIMELINE, FIXTURE-WAVE-* — только fixture/compatibility, не production

## Context

IP-14 требует, чтобы continuous spawn управлялся последовательностью ordinary / pressure / hard-or-elite / rest фаз, с per-wave overrides, phase tags, hooks для mid-boss/final phase и pooled-жизненным циклом врагов. Раздел «Wave / Encounter Content» в Content Design пуст (CG-02), поэтому канонические расписания создавать нельзя — нужен фреймворк и явно помеченный fixture.

## Decision

- Расписание — контент: `WaveTimelineDefinition` (`IContentDefinition`, `IReferencesContent`) загружается из `Assets/Resources/Content/Waves/FixtureWaveTimeline.json` через `FixtureWaveTimelineCatalog` (правило «значения в конфиге, не в коде», DECISION-0009). Ссылки на врагов валидирует общий `ContentRegistry`.
- `WaveDirector` — чистая C#-модель без GameObject: по времени забега выбирает фазу, считает, сколько врагов пора создать (переиспользует `ContinuousSpawnTimer` на интервал текущей фазы, ограничивает `maxAliveEnemies`), выбирает тип врага взвешенно и детерминированно (`seed` и `spawnRadius` задаются в JSON таймлайна, а не в коде или сериализованных полях). `ContinuousFixtureEnemySpawner` только исполняет решения директора и владеет пулами.
- Время таймлайна = `RunModel.Elapsed`. Оно уже pause-aware, поэтому пауза и завершение забега не сдвигают фазы и не спавнят врагов; отдельного таймера у директора нет.
- Последняя фаза удерживается до конца забега, поэтому расписание короче забега имеет определённое поведение.
- Per-wave overrides — независимые множители `WaveEnemyModifiers` (HP, скорость, contact damage, damage атаки). `WaveEnemyScaler` строит scaled-копию `EnemyDefinition` с тем же id один раз на фазу при создании директора; identity-модификаторы возвращают оригинал без аллокаций. Независимость множителей позволяет более поздней волне быть быстрее, но слабее.
- Хуки (`MidBoss`, `FinalBoss`) — маркеры времени в расписании; директор лишь один раз объявляет их событием `HookTriggered`, пока забег идёт. Спавн боссов — зона IP-15; точное время финального босса остаётся конфигурируемым TBD (CG-04), поэтому composition root/директор отвергает хук позже конца забега.
- Наблюдаемость: HUD показывает `WAVE n/N · <phase>` с tag-классом (ordinary/pressure/elite/rest), dev-панель Run — время таймлайна, фазу, интервал/cap/alive, состав, множители и ближайший хук; всё через immutable ViewState и presenter (`WaveViewState`, `WaveObservabilityViewState`).

## Consequences

- Game Design и Content Design не затронуты: базовый ритм и «не монотонная» сложность уже описаны, мы лишь реализуем фреймворк. Fixture-расписание — не канон и не заменяет IP-24.
- `spawnIntervalSeconds`/`maxAliveEnemies`/round-robin по fixture-врагам удалены из `ContinuousFixtureEnemySpawner`; каденс и состав живут в JSON.
- IP-15 подписывается на `WaveDirector.HookTriggered`, ничего не меняя в директоре. IP-24 заменяет fixture-JSON утверждёнными расписаниями по полям, сохраняя формат.

## Approval

Пользователь явно поручил реализовать IP-14 2026-09-20 («да, бери IP-14»); фреймворк реализуется по спецификации модуля, продуктовые правила не изобретались.
