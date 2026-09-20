# Тестовая телеметрия и простой AI-assisted balance loop

Принятый контракт IP-31/IP-32, ревизия `design-sync-R2`. Основание — отдельное пожелание пользователя: **ручные прогоны → комментарии и логи для AI → предложение корректировок → approval → изменения**. Пять новых документов уже утверждены; они не содержат законченного run-report/AI-review процесса. Этот файл определяет процесс. Он не утверждает новые балансные числа и не разрешает автоматическое применение предложений AI.

## Первая поставка

Достаточно локального набора из `run.json`, краткого `summary.md` и `feedback.md`. Тестировщик прикладывает их к задаче AI или указывает локальные пути. Не нужны backend, бот, online dashboard, LLM API, автоматическая загрузка, optimizer, Google Sheets или игровой чат с AI.

Сначала провести такой цикл на уже работающем fixture build. Это проверяет измерения и процесс; fixture-результат не объявляется балансом утверждённого production roster. Можно собирать комментарии ещё до IP-31, явно отмечая отсутствующие метрики. IP-31 не ждёт Travelers, meta, расширенных сетов и массового арта. Сначала нужен обновлённый core/event contract IP-01/IP-04/IP-05/IP-06/IP-07/IP-08 и UI harness; очередь исполнения ставит recorder сразу после этого среза, до encounter/production работ.

Обязательный player-facing `RunOutcome` из IP-01 и его feature-owned contributors (результат, время, уровень, kills, build/sets; впоследствии rewards/unlocks) работает даже без development recorder. Из него IP-26 строит компактные Results. Detailed report и export — дополнительная development возможность. Top-3 damage skills в UI остаётся optional, как указано в UI Design §16; advanced statistics и подробный combat log не входят в MVP.

## Один цикл

1. **Brief.** Выбрать один вопрос, например «почему после второй pressure phase перестаёт хватать XP». Зафиксировать build/config baseline, character/field, начальный meta state, seeds, действия тестировщика и цель наблюдения. Не назначать win-rate/TTK target без продуктового основания.
2. **Ручные прогоны.** Играть привычным способом, отмечать события в gameplay time: «02:10 невозможно выйти из группы», «04:20 улучшение не ощущается». Записать win/loss/abort и причины окончания теста. Первая сессия может обнаружить баг, а не дисбаланс.
3. **Пакет AI.** Передать report, notes, configs snapshot/hash и только релевантные GDD sections/полные Content cards. Если данные incomplete, AI сохраняет это ограничение в выводах.
4. **Анализ.** Разделить наблюдаемые факты, расчётные оценки, предположения и гипотезы. Сравнить поведение с design intent; проверить telemetry quality, fixture marker, разные версии и skill acquisition time. Не делать вывод «слишком слабое» по одному низкому total damage без контекста.
5. **Конкретное предложение.** Малый batch: ID, JSON path, старое→новое, единицы, причина, ожидаемое изменение, риск взаимодействий, затронутые docs/tests, способ отката и следующий ручной сценарий. Если лучше менять механику, подготовить отдельный design proposal, не скрывать его в численном patch.
6. **Approval.** Пользователь принимает конкретную revision целиком или отдельные строки. Rejected/deferred/no-change не применяются. Общая просьба «балансировать» не разрешает все будущие изменения. Если baseline уже изменился, показать актуализированный diff до применения.
7. **Применение.** Только после approval изменить JSON и нужную документацию; DTO/domain меняются, только если для утверждённого изменения действительно нужен новый contract. Выполнить проверки владельца системы. Никаких tuning literals в C# или Inspector.
8. **Повторный ручной тест.** Сопоставимые сценарии и условия; до/после report IDs и config hashes. Итог: оставить, откатить, проверить ещё одну гипотезу либо признать данных недостаточно. Совпадающий seed не превращает ручные действия и Unity physics в deterministic replay.

Число прогонов не фиксируется здесь как игровое правило. Один run годится для воспроизводимого бага или первичной гипотезы; он не доказывает распределение сложности. В выводе всегда указать, сколько сопоставимых прогонов использовано и какие факторы различались.

## Минимальный report contract

| Группа | Обязательные поля/смысл |
|---|---|
| Version | `schemaVersion`, `reportId`, `runId`, created UTC, recorder version/capabilities |
| Build | build/commit ID или honest `unknown`; dirty flag; platform/build type; fixture/production/mixed marker |
| Config provenance | использованные resource/content IDs, файлы и hashes; snapshot либо ссылка на воспроизводимую сохранённую версию; фактические overrides и resolved run config |
| Start | character, field и timeline ID; duration; initial stats/build; relevant persistent modifiers/profile version без посторонних персональных данных |
| Randomness | seeds по подсистемам, coverage/непокрытые RNG paths; отсутствие seed не записывается как `0` |
| Time | elapsed simulation time, running time, wall duration, pause duration/reasons; событие содержит gameplay timestamp |
| Outcome | won/lost и отдельный report completion reason: completed/aborted/retry/error/incomplete; final level/build/sets; один immutable final snapshot |
| Combat | applied HP loss dealt/taken by available source/level/category; attempted damage отдельно, если измерен; actual healing/regen; kills по target ID; unknown attribution отдельно |
| XP | dropped base, collected base, expired base, recovered award, pickup award после modifiers, остаток на земле; level/threshold progression |
| Draft | request origin/revision, offered IDs/levels, selected choice, successful/failed reroll/banish, exhausted-pool skip; queued sequence |
| Encounters | phase transitions, spawn request и actual spawn/cap result; hooks отдельно от созданных boss entities; later Traveler spawn/scaling/outcome/Book link |
| Feedback | local marker ID + elapsed time, scenario/skill level of tester as optional self-description, difficulty/readability comments, expected vs observed |
| Quality | unsupported/uninstrumented fields, dropped event count, buffer limits, export errors, incomplete reason; missing ≠ zero |

Пропущенные ещё не реализованные boss/Traveler/meta capabilities явно помечаются `unsupported`. Reporter не создаёт их первым и не выводит actual boss spawn из одного `HookTriggered`.

## Что писать часто, а что редко

- Частые hit/tick events агрегируются в памяти по ограниченным keys: source content ID, source level, target category, phase. Event-handler не пишет файл на каждое попадание.
- Timeline содержит редкие переходы: start/pause/resume, phase/encounter, level/draft choice, death/end, manual marker/error. Если нужны периодические samples плотности/HP, interval и capacity задаются validated diagnostic config.
- Лимиты memory/events/file size и overflow policy фиксируются в IP-31 implementation packet и проверяются нагрузкой. До измерения не объявляется произвольный «допустимый overhead» каноническим target.
- Export делает snapshot и запись вне combat hot path; корректность gameplay не зависит от наличия writable output directory. Failed export не показывает Success. Повторный export того же final snapshot не повторяет награды или события.
- Локальные outputs находятся вне `Assets` и runtime `Resources`; точный пользовательский output path/retention задаётся delivery contract. Raw сессии не коммитятся автоматически; curated anonymized examples допускаются отдельно. Ничего не отправляется AI/в сеть автоматически.
- Crash recovery не обещается в v1. Незавершённая сессия, если файл удалось получить, явно incomplete; отсутствие финального отчёта не маскируется как completed run.

## Семантика измерений

**Damage.** `effectiveDamage` — фактическая потеря HP, которую уже возвращает `Health.TakeDamage`, а не сумма requested attack damage. Враг с 10 HP, получивший 100 damage без mitigation, даёт 10 effective. Дополнительные 90 можно назвать overkill лишь когда действительно измерен post-mitigation request; если boundary его не предоставляет, overkill unavailable. Щиты впоследствии получают отдельный absorbed metric.

`HealthChanged` недостаточно: это событие включает damage, healing и proportional HP rescale. Healing считается по фактически восстановленному HP; wasted healing отдельно и только при измерении attempted amount. Stat recompute не становится лечением или уроном.

**Identity.** Content ID идентифицирует тип, `lifeId` — конкретную активацию pooled entity. Перед `TakeDamage` сохранить life/source/skill-level snapshot: синхронный lethal hit может вызвать death, unregister и pool return ещё до возврата applied amount. Projectile хранит источник даже после смерти стрелявшего врага; позднее попадание не получает автоматически новый уровень skill. Death считается один раз, cleanup/despawn/escape не kills.

**Damage origin.** Обычная activation, число projectiles, hits и damage ticks — разные counters. Set attack/proc не объявляется обычной skill activation; это сохраняет non-recursive contract. Kill attribution можно технически дать последнему lethal source, но это не меняет reward ownership и не вводит assist gameplay.

**XP.** Base collected XP и awarded XP после modifiers не суммируются как два независимых источника. Из одного `ExperienceChanged` нельзя получить lifetime XP: thresholds вычитаются при повышении уровня. Expired base XP, recovered award и XP, оставшийся на земле в конце, различаются.

**DPS.** Для `runningSeconds > 0`:

`observedRunDps = effectiveDamage / runningSeconds` (HP/секунда активной симуляции).

`observedEquippedDps(source) = effectiveDamage(source) / equippedRunningSeconds(source)`.

Пример: 300 applied damage за 60 s активного владения skill = 5 HP/s. Пауза 20 s в denominator не входит. Это наблюдение конкретного run, не «истинная мощность» skill; состав целей, density, cooldown uptime, acquisition level и synergies имеют значение. При нулевом denominator значение unavailable, не бесконечность и не 0.

Формула `.claude/skills/balance-check/SKILL.md` `damage × count / cooldown` — теоретическая оценка при обозначенных предположениях. Для AoE, chain falloff, return hits, orbit ticks, mine timing и conditional sets требуется соответствующая модель coverage. Её нельзя выдавать за observed DPS из telemetry.

**Performance.** `PerfGuard`/`PerfLog` фиксируют throttled превышения отдельных операций, не каждый медленный кадр. По количеству warnings нельзя рассчитать FPS или p95. Такие показатели доступны только при отдельном измерении, с hardware/build/scene/counts metadata. Static perf-audit не является измерением.

## Известные ограничения текущей реализации

- `EnemyDamageRequest` уже содержит SourceId, но `EnemyRuntime.ApplyDamage` теряет attribution на пути к `Health`; projectile атак врагов имеет profile без enemy ID. Source/result boundary дорабатывается в IP-05, per-life producer — в IP-04; IP-31 подключает recorder к этим событиям, а не создаёт второй combat pipeline.
- Draft и wave selection имеют seeded RNG, но позиции спавна используют `UnityEngine.Random.insideUnitCircle` (TD-024). В отчёте указать это; сохранение seed не гарантирует одинаковый run.
- `EnemyRegistry` и часть skill queries привязаны к EnemyRuntime; boss/Traveler targeting needs explicit adapter coverage в IP-05/IP-08 с конкретной интеграцией IP-15/IP-29.
- `FixtureSetExtraAbility` демонстрирует lifecycle, не новые production set effects. Метрики этих effects появляются вместе с IP-11/IP-19.
- Composition rollback/UI view coverage и один короткий Gameplay smoke не доказывают 15-minute runs, rewards/results или качество баланса.
- `Game.Diagnostics` не должен зависеть от gameplay assemblies; runtime adapters/collector сохраняют направление зависимостей. File export и AI review не являются gameplay dependencies.
- `.claude` JSON hook проверяет синтаксис и работает в своём клиенте. Нужны отдельные catalog/domain/reference checks; при документационной регистрации плана тесты Unity не запускались.

## Шаблон комментария тестировщика

```text
Run/report ID:
Build/config fingerprint:
Fixture или production; character/field; стартовые meta modifiers:
Цель прогона и стиль игры:
Время в забеге → наблюдение → ожидание:
Что было слишком легко/тяжело/непонятно:
Что хотелось выбрать в draft и что реально предлагалось:
Была ли проблема механикой, числами, управлением или читаемостью:
Победа/поражение/прерван; причина прекращения теста:
Известные вмешательства DEV-команд/ошибки/неполный лог:
```

## Шаблон предложения AI

```text
Proposal ID / revision:
Baseline report IDs + config hashes + data quality:
Scope: численная настройка / механика / исправление бага / только наблюдение.
Факты (со ссылками на события/счётчики и комментарий):
Расчётные оценки (формула, inputs, units, assumptions):
Гипотеза и альтернативные объяснения:
Предлагаемый diff: content ID, файл, JSON path, было → стало, units.
Для механики: точное до/после правило, edge cases и affected GDD/CD/IP/DECISION.
Почему это должно помочь; что может ухудшиться:
Что намеренно не меняем в этом batch:
Checks и ручной follow-up; критерий оставить/откатить:
Rollback: исходные значения/версия и затронутые файлы.
Approval: ссылка на явное решение пользователя и выбранные строки/revision.
Applied evidence (только после применения): diff/commit/hash/tests/new report IDs.
```

Шаблон не требует, чтобы AI всегда предлагал правку. «Не хватает данных», «это bug/readability issue» и «ничего не менять» — нормальные выводы. Пустые поля approval/applied evidence не создают разрешение и не считаются выполненной работой.

## Численные и механические изменения

| Тип | Пример | Путь применения |
|---|---|---|
| Tuning | HP, damage, cooldown, radius, drop chance в рамках определённой формулы | Конкретный diff → approval → JSON + Content tuning rationale + affected tests/manual comparison |
| Новая численная конкретизация TBD | Полный schedule, set threshold, reward price | Сначала конкретное предложение target data с rationale; после approval completeness gate снимается только для этих данных |
| Mechanics | Новый stacking, targeting, Book pool, set proc trigger | Proposed design/DECISION, impact на GDD/CD/IP; approval правила; отдельный code/config/test change владельца системы |
| Bug | Код не соответствует уже утверждённой карточке | Bug report и regression; исправление владельцем IP, не «балансный nerf» ради сокрытия ошибки |
| Presentation | Эффект кажется сильнее из-за VFX/плохой читаемости | Art/UI owner и gameplay-scale check; не менять damage без evidence |

Текущее authoring — единое JSON-дерево по DECISION-0009; Content Design хранит утверждённый смысл и связанные balance-data/rationale. Упоминание будущего Google Sheet не включает миграцию source of truth в начальный цикл.

## Проверка процесса

IP-05/IP-04 проверяют authoritative attribution и pool-safe lifecycle, IP-31 — известные агрегированные totals, timestamps, snapshots, bounded memory и export errors. IP-32 проверяет, что AI может связать факт с конкретным diff, не превращает incomplete в zero и не применяет непринятые изменения. Accept/apply/rollback можно отрепетировать на явно synthetic данных; реальные tuning changes появляются только при обоснованном отдельном approval.

После code/config patch: JSON parse; DTO/domain/ref validation; затронутые EditMode и PlayMode tests через безопасный `/smoke-check`; воспроизводимый manual follow-up. Не запускать batch Unity поверх открытого Editor. `Verified` в STATUS — только по реально выполненным module checks; production balance не становится «Verified» потому, что XML тестов зелёный.
