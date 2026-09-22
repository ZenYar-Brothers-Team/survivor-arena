# Миграция плана, scope revisions и новая очередь исполнения

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующий план](../../README.md), очередь и статусы — только в [STATUS](../../STATUS.md). Этот файл не является текущим implementation packet.

Это согласованный проект миграции, применённый по DECISION-0015. WORKFLOW, STATUS, действующие IP и design docs синхронизированы; код не изменён. Рабочая очередь находится в STATUS, таблица ниже — исторический материал review.

## M-01 — Регистрация переработанного плана и полная замена трёх документов

Это подготовительный документационный этап, а не ещё один игровой IP после старого плана.

1. Использовать пять approved sources. По поправке пользователя не создавать архивные копии, backup-файлы или отдельный snapshot трёх старых документов; заменить их содержимое напрямую.
2. Полностью заменить `Game_design.md`, `Content_design.md`, `art/ART_DIRECTION.md` соответствующими approved v2. ASSET_PIPELINE остаётся техническим источником. UI/UX и Art Production — новые утверждённые источники.
3. Нормализовать escaped Markdown отдельно от смысловых правок. Для отсутствующих/противоречащих edge cases внести только явно одобренные уточнения; остальные оставить именованными gaps с владельцами. Замена целых документов не означает молчаливое удаление защиты empty draft в коде.
4. Перенести 30 reviewable target packets в действующие `modules/` с сохранением IDs/filename, зарегистрировать пять новых. Каждому materially changed scope присвоить revision `design-sync-R2`; номер модуля не означает новый implementation.
5. Синхронизировать README, стабильный Implementation_plan entry point, WORKFLOW, STATUS и AGENTS.md. В AGENTS.md заменить «first numerically ordered Ready» ссылкой на Execution order в STATUS; перечислить UI/UX и art sources в authority map. CG-01 отражает уже полученное approval 121 cards; CG-02/03 и оставшиеся numerical gaps сохраняются. Offer count 3 больше не TBD.
6. В STATUS назначить новую Execution order и пересчитать готовность по целевой ревизии/настоящим gates. Не оставлять IP-15 Ready только потому, что зависимости старого scope когда-то были Verified.
7. В исторических evidence сохранить даты/коммиты/test counts и scope, который они проверяли. Изменившиеся правила DECISION-0004/0014 и semantic content migrations описать новыми/дополняющими decisions; прошлую историю не переписывать.
8. Проверить двусторонние зависимости, ссылки, IDs, полноту каждого packet и authority источников. В согласованный M-01 scope включить точечную синхронизацию .claude/rules/content-json.md: approved semantic replacements SKILL-001, PASSIVE-002/007, SET-001…008 требуют versioned migration и различимой истории, а общий запрет переиспользования ID для других сущностей сохраняется. Уточнить source links, где они устарели; остальные skills/hooks не переписывать заодно.

**Acceptance M-01:** три документа заменены без отдельных копий старых версий; target bodies совпадают с approved sources плюс отдельно принятые deltas; active implementation packets используют только новый design; unresolved gaps видимы; ни один новый behavior не считается verified по старому test count; selector выбирает из новой очереди.

## Изменение WORKFLOW: ID не определяет порядок

Предлагаемая замена §1 WORKFLOW и соответствующего пункта 2 AGENTS.md:

> Если пользователь явно назвал IP, выбрать его и проверить зависимости/gates. Иначе читать Execution order из STATUS.md и выбирать первый в этой очереди модуль со статусом Ready. Пропускать завершённые и Blocked; не обходить невыполненный prerequisite ради позиции в очереди. Если Ready нет — назвать конкретные blockers и предложить следующий reviewable packet, не начинать по устаревшему scope.

Сохранить остальные ограничения: зависимость допускается только с Implemented/Verified **для требуемого target scope**; Draft/TBD не превращаются в правила; status/evidence хранится только в STATUS. Нельзя трактовать историческое Verified внутри evidence как текущий status или выдумывать новый статус «Partially Verified».

После регистрации очередь хранится **только в STATUS**; README показывает milestones и ссылку, module specs — зависимости, без второй operational очереди. В этом proposal таблица ниже — проект её первоначального содержимого.

## Предлагаемая Execution order

Номера сохраняют ссылочную стабильность. Ниже явное изменение порядка, в том числе для ещё не реализованных задач. При Blocked очередь выбирает следующий Ready, если его собственные prerequisites и gates выполнены.

| Приоритет | IP | Назначение |
|---:|---|---|
| 1 | [IP-00](modules/IP-00-content-contract.md) | Контракт контента, стабильные ID и конфигурация |
| 2 | [IP-01](modules/IP-01-run-lifecycle.md) | Run lifecycle, pause ownership и результат забега |
| 3 | [IP-02](modules/IP-02-player-movement.md) | Перемещение игрока, камера и базовая геометрия |
| 4 | [IP-03](modules/IP-03-character-stats.md) | Character stats, Health и новые stat channels |
| 5 | [IP-04](modules/IP-04-enemy-core.md) | Enemy lifecycle, contact damage и per-life identity |
| 6 | [IP-05](modules/IP-05-active-skill-runtime.md) | Общий combat pipeline, control effects и target contract |
| 7 | [IP-06](modules/IP-06-xp-progression.md) | XP lifecycle, effective pickup radius и progression |
| 8 | [IP-07](modules/IP-07-level-up-draft.md) | Трёхслотовый драфт, request queue и build progression |
| 9 | [IP-08](modules/IP-08-active-skill-framework.md) | Active-skill levels, targeting и effect families |
| 10 | [IP-09](modules/IP-09-passive-framework.md) | Passive modifiers и новые stat effects |
| 11 | [IP-10](modules/IP-10-reroll-banish.md) | Reroll/banish для обновлённого драфта |
| 12 | [IP-10A](modules/IP-10A-ui-foundation.md) | UI Foundation, reusable cards, HUD и test harness |
| 13 | [IP-31](modules/IP-31-manual-run-telemetry.md) | Локальная телеметрия ручных прогонов |
| 14 | [IP-32](modules/IP-32-manual-ai-balance.md) | Ручные прогоны и AI-assisted balance review |
| 15 | [IP-11](modules/IP-11-set-framework.md) | Set recipes, priority draft policy и effect families |
| 16 | [IP-12](modules/IP-12-character-framework.md) | Character definitions, weighted draft и selection presentation |
| 17 | [IP-12A](modules/IP-12A-visual-presentation-foundation.md) | Visual Presentation Foundation и asset production pipeline |
| 18 | [IP-13](modules/IP-13-enemy-patterns.md) | Enemy movement/attack patterns и control integration |
| 19 | [IP-14](modules/IP-14-wave-director.md) | Wave Director: continuous и burst timeline |
| 20 | [IP-15](modules/IP-15-boss-framework.md) | Boss/mid-boss encounter framework |
| 21 | [IP-16](modules/IP-16-field-framework.md) | Field definitions, selection и run configuration |
| 22 | [IP-28](modules/IP-28-world-pickups.md) | World pickup framework: зелье лечения и Book |
| 23 | [IP-29](modules/IP-29-traveler-framework.md) | Traveler encounter framework |
| 24 | [IP-25](modules/IP-25-meta-progression.md) | Persistent profile, meta currency, unlocks и permanent progression |
| 25 | [IP-26](modules/IP-26-functional-ui.md) | Functional UI и полный player flow |
| 26 | [IP-17](modules/IP-17-production-skills.md) | Production Active Skills SKILL-001…016 |
| 27 | [IP-18](modules/IP-18-production-passives.md) | Production Passive Items PASSIVE-001…014 |
| 28 | [IP-19](modules/IP-19-production-sets.md) | Production Sets SET-001…020 |
| 29 | [IP-20](modules/IP-20-production-enemies.md) | Production Enemies ENEMY-001…020 и зелье PICKUP-001 |
| 30 | [IP-21](modules/IP-21-production-bosses.md) | Production Final Bosses и Mid-bosses |
| 31 | [IP-22](modules/IP-22-production-characters.md) | Production Characters CHAR-001…010 |
| 32 | [IP-23](modules/IP-23-production-fields.md) | Production Fields FIELD-001…010 |
| 33 | [IP-30](modules/IP-30-production-travelers.md) | Production Travelers TRAVELER-001…010 и Book |
| 34 | [IP-24](modules/IP-24-production-waves.md) | Canonical Wave / Encounter Content и field bindings |
| 35 | [IP-27](modules/IP-27-integration.md) | End-to-end integration, regression и content validation |

Прежний relative order выполненной основы сохраняется; её целевые дельты проверяются по зависимостям. IP-31/IP-32 поставлены до expanded encounters/production. IP-28/IP-29 — до full player flow; IP-25/IP-26 подняты перед production catalogs, чтобы проверить полный playable fixture flow раньше. IP-30 предшествует IP-24. IP-27 замыкает фактически объявленный полный scope.

Это priority queue, а не обещание выполнять заблокированные задачи. Например, если numeric production data IP-19 не готовы, можно выполнить другой Ready packet с полными данными. Если требуется общий missing control rule IP-05, зависимые modules не обходят его.

## Как пересчитывается STATUS

Каждая запись изменённого IP получает поля, например:

```text
Scope revision: design-sync-R2
Status: Ready | Blocked | In progress | Implemented | Verified
Current packet: конкретная feature/batch и критерии завершения
Blocked by / remaining gates: конкретные dependencies, G-xx, missing data/assets
Historical implementation/verification evidence: прежний scope, commits, tests, даты
Target implementation evidence: только фактически выполненные дельты
Target verification evidence: только проверки целевой ревизии
Remaining acceptance / catalog IDs: всё ещё не закрытое
Deviations / Documentation impact: фактические изменения
```

Это шаблон полей, не установленный сейчас статус. Допустимые статусы workflow остаются прежними, включая Superseded только для действительно заменённого утверждённого scope.

- **Без behavioral delta:** IP-00/IP-02 не становятся незавершёнными из-за новых ссылок. Сопоставить прежнее evidence и API compatibility; сохранить status, если требования те же. При выявленной реальной несовместимости добавить точный delta/recheck, а не понизить всё автоматически.
- **Изменённый уже выполненный scope:** IP-01/IP-03…IP-14 с IP-10A/IP-12A (кроме IP-02) теряют право называться Verified для *новой* ревизии. Старые результаты остаются historical evidence. Target становится Ready при выполненных prerequisites/gates, иначе Blocked; затем In progress → Implemented → Verified по реальным checks.
- **Ещё не реализованный scope:** IP-15…IP-27 получает полный target packet. Первое исполнение сразу соответствует новым designs; двойная реализация «old then new» исключена.
- **Новые IP-28…32:** готовность вычисляется по тому же правилу, без автоматического Ready по факту создания Markdown.
- **Прямые dependants:** после изменения status/API/acceptance пересчитать их готовность. Не объявлять все transitive historical tests недействительными; проверить реально используемые contracts, но при планировании требовать актуальный prerequisite scope.
- **Текущее Next Ready:** вычислять после миграции/закрытия gaps, не сохранять прежнее IP-15 вручную. При регистрации STATUS пересчитан; его текущее содержимое имеет приоритет над примерами этого материала.

### Пример с IP-14 и IP-15

Прежняя запись IP-14 подтверждает continuous timeline, но target включает burst/cap/catch-up. После M-01 она не даёт IP-15 разрешение зависеть от «нового проверенного Wave Director». Сохранить old evidence, поставить target IP-14 Ready/Blocked по его IP-13 и W-01; после реализации и checks обновить target evidence. IP-15 затем начинает сразу по обновлённому boss packet. Старую рабочую continuous реализацию при этом сохраняют и расширяют.

### Catalog batches без ложного завершения

Полный scope IP-17/18/19/20/21/22/23/30 — все соответствующие approved IDs, не только пилот. Для работы выбирать concrete packet по ID с полными данными и доступными module prerequisites; оставшиеся IDs перечислены в STATUS.

Ready означает, что существует согласованный текущий packet с выполненными gates; после старта — In progress. Внутренний checklist может содержать completed/remaining acceptance, но не отдельные конкурирующие module statuses. Ни успешный пилот, ни хорошие import tests не переводят весь module в Implemented/Verified, пока остаётся обязательный catalog scope. Когда ни один оставшийся packet нельзя продолжить из-за конкретного gate — Blocked с объяснением.

Framework может завершаться на synthetic fixtures, когда это его явный полный Scope (например IP-28); production pickup bindings при этом принадлежат IP-20/IP-30. Это разделение deliverables, не обход production approval.

## Предохранители от dependency cycles

- IP-07 предоставляет draft request/provider API; IP-11 реализует set provider. IP-07 не зависит от IP-11.
- IP-10 предоставляет reroll/banish policy hook; IP-11 подключает set policy. Нет IP-10 → IP-11 → IP-10.
- IP-10A проверяет common cards на fake ViewState; semantic recipe/character data поставляют IP-11/IP-12. Общая UI foundation не зависит от полного feature.
- IP-05 принимает target/control interfaces и fake categories; IP-15/IP-29 интегрируют конкретные boss/Traveler targets. Общий combat не зависит от ещё не созданного Traveler.
- IP-16 — field framework; IP-29 его потребляет. Production field/Traveler bindings IP-24/IP-30 не делают IP-16 зависимым от IP-29/IP-30.
- IP-11 использует typed reward events через fake producer; фактические potion bindings проверяют IP-19/IP-28. Set framework не зависит от своего production catalog.
- IP-01 задаёт result identity и принимает contributions; IP-04/06/07/11 — факты, IP-25 — rewards. IP-31 — дополнительный recorder; gameplay/Results не зависят от exporter.
- IP-12A — общие profiles/adapters, production assets у владельцев content. Нет требования завершить все production картинки до завершения generic pipeline.

## Соответствие первого draft и переработанного проекта

Первоначальные IP-28…41 существовали только в proposal, не в STATUS, поэтому их номера можно пересмотреть. Эта таблица — история правки проекта; её строки **не** являются действующими IP-ссылками или scope registry.

| Номер в первом draft | Где теперь находится работа |
|---|---|
| прежний IP-28: migration | M-01 этого файла, до дальнейшей реализации |
| прежний IP-29: combat/stats | IP-03, IP-05, IP-09, IP-13 |
| прежний IP-30: draft policy | IP-07, IP-10, IP-11 |
| прежний IP-31: set effects | IP-11 |
| прежний IP-32: burst | IP-14 |
| прежний IP-33: pickups | новый IP-28 framework; production potion IP-20, Book IP-30 |
| прежний IP-34: Traveler framework | новый IP-29 |
| прежний IP-35: production Travelers | новый IP-30 |
| прежний IP-36: telemetry | новый IP-31; gameplay source contracts у IP-01/IP-04/IP-05 |
| прежний IP-37: balance loop | новый IP-32 |
| прежний IP-38: common UI | IP-10A и feature-owned semantic slices |
| прежний IP-39: Settings | IP-26 |
| прежний IP-40: asset pipeline | IP-12A; catalog art у production owners |
| прежний IP-41: active effects | IP-08 |

## Проверки перед принятием и после применения

Для proposal: у всех 35 packets полный набор sections; все IP links существуют; graph acyclic и reverse consumers совпадают с dependencies; Execution order содержит каждый ID ровно раз и уважает prerequisites; requirements/gaps не потеряны при переносе.

Для M-01: проверить diff замены design docs, нормализованные links/anchors, отсутствие лишних архивных копий, отсутствие outdated numeric selector, consistency active scopes↔STATUS revision/evidence/gates. На documentation-only migration не приписывать Unity pass.

Для последующей реализации: targeted tests на delta, regression затронутых APIs, безопасный smoke-check и честное manual evidence. Final IP-27 разделяет framework/fixture integration и content-complete verification; второй результат нельзя получить только коротким fixture smoke.
