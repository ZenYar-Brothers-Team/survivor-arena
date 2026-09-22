# Implementation Plan

Принятый план `design-sync-R2`, зарегистрирован 2026-09-20 по [DECISION-0015](../decisions/0015-design-sync-r2.md). Три design-документа полностью заменены утверждёнными версиями без архивных копий прежних файлов. Новые требования внесены в существующие IP до дальнейшей реализации; отдельно добавлены пять самостоятельных модулей.

## Текущий ограниченный этап

[FIELD-001 на исходно открытом наборе](milestones/FIELD-001-start.md), ревизия
`field-001-start-R1`: 10 active / 10 passive / 5 sets, CHAR-001 и encounters первой
карты. Дизайн утверждён DECISION-0050; граница — DECISION-0051. Это packets
существующих IP, полный каталог сохраняется. Единственная текущая очередь и
готовность — [STATUS](STATUS.md#field001-execution); после этапа backlog не
возобновляется автоматически. Проверка поздних unlocks в gameplay вне этого этапа.

## Источники и authority

- [Game Design](../Game_design.md) — общие игровые правила.
- [Content Design](../Content_design.md) — конкретные сущности, behavior и balance data; 121 target card уже утверждена.
- [UI / UX Design](../UI%20%20UX%20Design.md) — пользовательские экраны, состояния и взаимодействия.
- [Art Direction](../art/ART_DIRECTION.md) — утверждённое визуальное направление.
- [Art Production](../art/Art%20Production.md) — inventory и роли ассетов.
- [Asset Pipeline](../art/ASSET_PIPELINE.md) — технические пути, provenance, подготовка, импорт и image approval.
- [Модули](modules/) — scope, зависимости, критерии и проверки.
- [STATUS](STATUS.md) — единственная Execution order, текущие статусы, готовность, краткое evidence и ссылки на подробности.
- [WORKFLOW](WORKFLOW.md) — процесс работы над выбранным IP.

Repository/code показывает фактически реализованное поведение. IP и код не переопределяют дизайн автоматически. Исходные файлы с `v2` в имени — материалы импорта, а не параллельные каноны; дальнейшие изменения делаются по canonical paths выше.

## Исполнение и ревизии

Если пользователь не назвал модуль, выбирать **первый Ready в Execution order из STATUS**, а не первый по номеру. Зависимости должны быть Implemented/Verified для требуемого целевого scope. ID остаётся стабильным и не задаёт очередность.

30 существующих IP сохранены под прежними IDs/filenames; 28 получают изменения scope, IP-00/IP-02 сохраняют behavioral contracts. Пять новых — IP-28 pickup framework, IP-29 Traveler framework, IP-30 production Travelers/Book, IP-31 локальная телеметрия и IP-32 ручной AI balance workflow. Все 35 спецификаций полные; отдельное слияние со старым текстом не требуется.

Историческое Verified подтверждает только прежний scope. STATUS ссылается на [архив прежнего scope](evidence/pre-design-sync-R2.md) и [подробные проверки целевой ревизии](evidence/design-sync-R2-2026-09-21.md); эти записи не содержат текущих статусов. При старте IP читать его запись и Context, а evidence — только по необходимости. Fixture, готовый production поднабор и весь каталог имеют разные критерии завершения; первый поднабор не закрывает catalog IP.

## Milestones

1. Обновление core/event/draft/UI contracts и ранний ручной цикл IP-31/IP-32.
2. Расширенные build/presentation/encounter systems, boss/field frameworks, pickups и Travelers.
3. Мета-прогрессия и сквозной UI с рабочими Settings до массового наполнения каталогов.
4. Production по подготовленным ID: behavior, данные, per-ID art, UI и проверки в одной поставке; затем полные расписания полей.
5. IP-27: end-to-end, полный предусмотренный каталог, manual runs, readability/performance и прослеживаемый balance review.

Это описание результатов, не вторая очередь. Порядок, текущие packets и readiness находятся только в STATUS.

## Content и delivery gates

| Gate | Правило |
|---|---|
| CG-01 | Approval пяти документов и 121 существующей target-карточки получен. Старый blanket Draft blocker снят; новые непредусмотренные сущности и будущие proposals не получают approval автоматически. |
| CG-02 | Production Wave / Encounter Content отсутствует; расписания каждого поля требуют полных согласованных данных. |
| CG-03 | Resolved — DECISION-0037: rewards/prices/upgrades/unlocks/exit policy определены; поставка JSON и profile runtime — IP-25. Прочие production data/art gates сохраняются. |
| CG-04 | Оставшиеся tuning/timing значения, вероятности, thresholds и конкретные duration/intervals проверяются по используемым полям. Draft offer count уже равен 3 и больше не TBD. |
| G-01…G-18, W-01 | Конкретные конфликты/пропуски и владельцы находятся в [DESIGN_SYNC](DESIGN_SYNC.md). Принятие плана не выбирает автоматически вариант решения открытого вопроса. |
| AG-01 | Конкретный image/replacement approval, provenance и технические gates Asset Pipeline. Approval дизайна не означает approval ещё не созданного изображения. |
| BG-01 | Approval конкретной revision balance proposal и выбранного diff до применения; затем checks и повторный ручной прогон. |

Gate относится только к зависимому packet/ID. Не требуется заполнить весь каталог для начала независимой подготовленной работы. Book-механика утверждена; production card/ID ещё нужно заполнить в IP-30. IP-28 проверяется на fixtures, production potion PICKUP-001 принадлежит IP-20.

## Общие контракты и рабочие материалы

- [DESIGN_SYNC](DESIGN_SYNC.md) — историческая сверка новых требований с базой и реестр открытых вопросов.
- [ASSET_PRODUCTION](ASSET_PRODUCTION.md) — общий runtime/JSON/UI/art контракт, ownership и стадии производства.
- [BALANCE_WORKFLOW](BALANCE_WORKFLOW.md) — ручные прогоны → комментарии/логи → предложение AI → approval → изменения/проверки/повторный прогон.
- [DECISION-0015](../decisions/0015-design-sync-r2.md) — принятое решение о миграции, семантических заменах ID и границах изменения.
- [Материалы ревью](proposals/2026-09-20-design-expansion/README.md) — принятый проект до регистрации, не текущий implementation packet.

## Каталог модулей

Каталог отсортирован по ID только для поиска. Execution order — в STATUS.

- [IP-00 — Контракт контента, стабильные ID и конфигурация](modules/IP-00-content-contract.md)
- [IP-01 — Run lifecycle, pause ownership и результат забега](modules/IP-01-run-lifecycle.md)
- [IP-02 — Перемещение игрока, камера и базовая геометрия](modules/IP-02-player-movement.md)
- [IP-03 — Character stats, Health и новые stat channels](modules/IP-03-character-stats.md)
- [IP-04 — Enemy lifecycle, contact damage и per-life identity](modules/IP-04-enemy-core.md)
- [IP-05 — Общий combat pipeline, control effects и target contract](modules/IP-05-active-skill-runtime.md)
- [IP-06 — XP lifecycle, effective pickup radius и progression](modules/IP-06-xp-progression.md)
- [IP-07 — Трёхслотовый драфт, request queue и build progression](modules/IP-07-level-up-draft.md)
- [IP-08 — Active-skill levels, targeting и effect families](modules/IP-08-active-skill-framework.md)
- [IP-09 — Passive modifiers и новые stat effects](modules/IP-09-passive-framework.md)
- [IP-10 — Reroll/banish для обновлённого драфта](modules/IP-10-reroll-banish.md)
- [IP-10A — UI Foundation, reusable cards, HUD и test harness](modules/IP-10A-ui-foundation.md)
- [IP-11 — Set recipes, priority draft policy и effect families](modules/IP-11-set-framework.md)
- [IP-12 — Character definitions, weighted draft и selection presentation](modules/IP-12-character-framework.md)
- [IP-12A — Visual Presentation Foundation и asset production pipeline](modules/IP-12A-visual-presentation-foundation.md)
- [IP-13 — Enemy movement/attack patterns и control integration](modules/IP-13-enemy-patterns.md)
- [IP-14 — Wave Director: continuous и burst timeline](modules/IP-14-wave-director.md)
- [IP-15 — Boss/mid-boss encounter framework](modules/IP-15-boss-framework.md)
- [IP-16 — Field definitions, selection и run configuration](modules/IP-16-field-framework.md)
- [IP-17 — Production Active Skills SKILL-001…016](modules/IP-17-production-skills.md)
- [IP-18 — Production Passive Items PASSIVE-001…014](modules/IP-18-production-passives.md)
- [IP-19 — Production Sets SET-001…020](modules/IP-19-production-sets.md)
- [IP-20 — Production Enemies ENEMY-001…020 и зелье PICKUP-001](modules/IP-20-production-enemies.md)
- [IP-21 — Production Final Bosses и Mid-bosses](modules/IP-21-production-bosses.md)
- [IP-22 — Production Characters CHAR-001…010](modules/IP-22-production-characters.md)
- [IP-23 — Production Fields FIELD-001…010](modules/IP-23-production-fields.md)
- [IP-24 — Canonical Wave / Encounter Content и field bindings](modules/IP-24-production-waves.md)
- [IP-25 — Persistent profile, meta currency, unlocks и permanent progression](modules/IP-25-meta-progression.md)
- [IP-26 — Functional UI и полный player flow](modules/IP-26-functional-ui.md)
- [IP-27 — End-to-end integration, regression и content validation](modules/IP-27-integration.md)
- [IP-28 — World pickup framework: зелье лечения и Book](modules/IP-28-world-pickups.md)
- [IP-29 — Traveler encounter framework](modules/IP-29-traveler-framework.md)
- [IP-30 — Production Travelers TRAVELER-001…010 и Book](modules/IP-30-production-travelers.md)
- [IP-31 — Локальная телеметрия ручных прогонов](modules/IP-31-manual-run-telemetry.md)
- [IP-32 — Ручные прогоны и AI-assisted balance review](modules/IP-32-manual-ai-balance.md)

## Проверка и документация

Каждый IP содержит Context, зависимости, Scope/Out of Scope, acceptance criteria, UI/observability, проверки, документационные изменения и gates. При реализации применять repository conventions и подходящие .claude helpers; code/config и affected design/evidence синхронизируются в одном change. Миграция документации сама по себе не является проверкой нового gameplay.
