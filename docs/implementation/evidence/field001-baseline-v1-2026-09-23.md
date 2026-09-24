# F1-00 — evidence подготовки baseline v1

Дата: 2026-09-23. Scope: `field-001-start-R1`, balance proposal
`field001-baseline-v1`. Основание: пользователь поручил заполнить баланс первой
карты, уточнил достижимость победы без meta и разрешил корректировать параметры
врагов. Цель и граница численного approval —
[DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md).
Текущее исполнение/очередь: [STATUS](../STATUS.md#field001-execution).

## Артефакты и audit

- [Review и матрица полей](../../balance/field001-baseline-v1.md): значения/units,
  основания, formula examples, consumers и named API/art gates.
- [Численные таблицы](../../balance/field001-baseline-v1.json): 60 skill levels,
  60 passive levels, 5 sets, 6 ordinary definitions с before→proposed delta,
  bosses/Travelers/pickups, 24 phases, 64 obstacle rects, seeds и art snapshot.
- [Read-only validator](../../balance/validate_field001_baseline.py): проверяет
  proposal, не импортирует его в Unity и не меняет runtime config.

Проверены реальные DTO/validators и потребители: ActiveSkill level/effect/targeting,
ActiveSkillInstance/executor, CharacterBaseStats mapper, ExperienceProgression,
set effect host/ability, Enemy movement/attack controllers, WaveDirector,
boss/field/pickup/Traveler schemas и TravelerDefinition/runtime.
Технические отличия зафиксированы в матрице вместо объявления data-only совместимости:
orbit lifetime/hits, wave expansion, targeting/telegraphs, conditional set effects,
SET-017 size/range opt-in, archer reposition, enemy/boss cadence, double dash,
production Traveler body binding. Gameplay code не изменялся.

Read-only manifest inventory: 40 relevant records; разные stages Imported и
Image approved сохранены. Approved art не объявляется новым production binding.
Для performance proposal прочитаны CPU/GPU/RAM текущего компьютера; 1080p и
frame/load budgets — предложенная цель, не результат замеров.

## Воспроизводимые проверки

`python -X utf8 docs/balance/validate_field001_baseline.py`:

- 60+60 level rows; ровно 5 рецептов из всех 20 canonical recipes доступны из
  начального набора; thresholds 1…6; 4-set stress build помещается в 6+6 slots.
- 17 required character stat channels соответствуют фактическому DTO; finite
  JSON numbers, без duplicate keys; projectile range/speed/lifetime согласованы.
- XP array соответствует формуле; L40 требует 1258 XP, хвостовая цена 92 XP.
- 24 последовательные фазы =900 s, веса каждой смеси =100, только 6 ordinary IDs,
  ввод ranged/dash типов по расписанию, boss hooks согласованы с definitions.
- Номинально 1735 запросов спавна без cap, математическое ожидание 2882.9 ordinary
  XP при всех kills, 116 burst enemies; консервативная alive bound 228 ниже stress250.
- Вероятности Travelers суммируются в 1, peaceful roles без attacks/contact damage,
  mandatory support sentinels валидны; pickup lifetime null либо >0.
- 64 obstacle rects внутри границ, ≥3 units между AABB и свободный старт;
  все 40 указанных runtime art paths существуют.

`scripts/check_project.py --scope docs --paths <изменённые Markdown>`:
**STATIC PASS**, 19 документов; Unity NOT RUN. `git diff --check`: **PASS**.
Evidence не подменяет Unity run.

## Ограничения вывода

Никаких новых Unity tests, игровых прогонов или FPS измерений в этом packet нет:
изменились документация и отдельный review artifact вне Assets. Проверки JSON
подтверждают внутреннюю согласованность предложения, а не совместимость с runtime
loader. Победа без meta, сложность первых попыток, точки смерти, drop economy и
читаемость в толпе ещё требуют production integration и реальных reports F1-09.
Существующие findings IP-12A/OBS-01 не объявлены закрытыми. Runtime content,
save files, art и Unity scenes не менялись.

<a id="approval-2026-09-24"></a>
## Approval — 2026-09-24

По делегированию пользователя baseline v1 принят **без изменений**
([DECISION-0053 Approval](../../decisions/0053-field001-difficulty-and-baseline.md#approval--2026-09-24)).
JSON `approval: Approved`; `runtimeImportable` остаётся false — перенос в production
JSON выполняют F1-01…08. Content Design синхронизирован: ENEMY-001…005/007
(HP/speed/contact, contact interval 1 s, ranged/dash timing), PICKUP-001 (18 HP, 1.5%),
PICKUP-002 Книга, BOSS-001/MIDBOSS-001 (XP, timings), TRAVELER-001/002/005
(XP/presence/support).

Проверки: `python -X utf8 docs/balance/validate_field001_baseline.py` — PASS
(те же counts, что выше). Unity не запускался: изменились только документы и
review JSON вне Assets.
