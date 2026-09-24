# IP-22 — Production Characters CHAR-001…010

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-12](IP-12-character-framework.md), [IP-17](IP-17-production-skills.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
17 baseline stats, starting skill, веса и highlights.
Packet Approved 2026-09-24 по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
используется как production data; проверки этого IP сохраняются.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD characters; полные CHAR-001…010 и starting SKILL cards; UI §§4,23; Art Direction §§2,7,9; Art Production §1.

## Scope

десять profiles, starting skill/stats/recovery/weights и заданное unlock metadata. Playable species/силуэты следуют новому roster, включая огра. Character Select использует body crop/variant; отдельный portrait только при недостаточной читаемости crop.

## Out of Scope

выдуманные цены/unlocks, обязательные десять новых портретов, relabel fixture как production без доказанного binding.

## Acceptance criteria

correct initial stats/loadout/weights, 0-weight exclusions и selection locks; no invented unique passive. Selection показывает significant baseline modifiers, role и starting skill; baseline и критерий отображения документированы. CHAR-001 concept approval сохраняется как art fact, но fixture→production mapping/source reuse фиксируется отдельно с проверкой соответствия карточке. Основная масса body совместима с одним вписанным кругом: без крайнего вытяжения и чрезмерно длинных выступов, но без требования круглой формы.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

locked silhouette/условие, выбранный character, crop и concise modifiers; не выводить полный внутренний stat table в player screen. Developer snapshot остаётся подробным.

## Проверки

per-character initial snapshot, modifiers и weighted draft, selected/locked states; motion/facing/hit/pause/reset, root invariance; manual body/crop review на реальном размере.

## Документационные изменения

roster/card completeness, baseline UI contract, asset provenance/reuse evidence; approved concept не обозначать автоматически как runtime integrated.

## Gates и недостающие решения

G-14: weights. G-15 resolved по DECISION-0037; цены и unlock metadata переносятся из CD. CHAR-001 concept = fixture goblin v002 подтверждён DECISION-0029; production runtime binding и per-image review остаются здесь. CHAR-006 огр и прочие approved roster choices не переутверждаются. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Character Select authoring

Перенести каноническую базовую точку сравнения Content Design в отдельные production baseline data и задать explicit ordered highlights каждого CHAR-ID по [DECISION-0026](../../decisions/0026-character-selection-baseline.md) и [контракту IP-12](IP-12-character-framework.md#character-select-data-contract). База отдельна от roster, порога автоматической значимости нет. Проверить соответствие числовых отличий actual stats; framework fixture values не переносятся как утверждённый production баланс. Numeric weights и unlock conditions/цены остаются собственными gates.

Использовать `CharacterPresentation`/`CharacterComparisonBaseline`, typed body/crop/icon references и `CharacterStatField` из [IP-12 API](IP-12-character-framework.md#framework-api-и-fixture-schema). Placeholder разрешён только fixture catalog; production missing art не маскируется backfill.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-03](../milestones/FIELD-001-start.md#f1-03):
CHAR-001; поздние character IDs только unlock metadata. Packet prerequisites: F1-00/01/02; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).
