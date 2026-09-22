# IP-17 — Production Active Skills SKILL-001…016

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-08](IP-08-active-skill-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

F1-00 review input: [baseline v1](../../balance/field001-baseline-v1.md) содержит
60 уровней и матрицу continuous orbit / expanding wave / targeting.
Это Proposed packet по [DECISION-0053](../../decisions/0053-field001-difficulty-and-baseline.md);
использовать как production data только после approval, затем выполнить проверки этого IP.

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новый GDD skills/combat; полные выбранные SKILL-001…016 cards и непосредственно влияющие passive/set references; UI §§6–10; Art Production §6 и соответствующие generic VFX §10.

## Scope

шестнадцать data-driven skill definitions с L1…6, qualitative changes и численными параметрами; каждый отсутствующий pattern реализуется через IP-08 contract. На каждый ID — icon, краткое upgrade delta/level text и необходимые world roles. Общие projectile/impact assets переиспользуются по явным references.

## Out of Scope

set transformations, неописанные новые skills, окончательный rebalance, generation unused frames.

## Acceptance criteria

per-ID behavior matrix покрывает L1…6 и различия уровней; 6 concurrent skills работают без ручного aim. Collider/damage geometry не выводится из картинки; actual damage сохраняет source ID для telemetry. Production visual references валидируются; отсутствие обязательного финального арта не скрывается fixture fallback.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HUD/build icon+level, reusable draft card, current→new delta и set-related detail; trigger/hit/miss и qualitative pattern наблюдаемы в harness.

## Проверки

numeric/pattern per-ID cases, level transitions, capacity/concurrency, pause/pool/reset; PlayMode representative mechanics + manual actual-speed projectile/telegraph readability.


Контракт потребления skill stats и JSON L1…L6: [IP-08 parameter mapping](IP-08-active-skill-framework.md#контракт-параметров-для-потребителей), [DECISION-0021](../../decisions/0021-additive-skill-level-bonuses.md). Size/range применяются один раз executor-ом из activation snapshot; passive definitions не переписывают skill level data. Повторные проценты skill upgrades складываются к базе.

## Документационные изменения

расширение title/range до 016, card→runtime/test/asset matrix и полный numeric payload. Балансные изменения только через IP-32 approval loop.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017; нужны полные параметры 16 skills; G-04 только если решение меняет SKILL-008; images проходят asset gates. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-22](IP-22-production-characters.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Мета-доступ

IP-25 владеет persistent access по [DECISION-0037](../../decisions/0037-meta-economy-and-persistence.md)
и разделу «Мета-экономика» CD. SKILL-016/SET-020 требуют прохождения FIELD-001;
остальные active skills/sets исходно открыты. Production definitions сохраняют этот
mapping; runtime проверяет access дополнительно к прочим требованиям draft/recipe.

## Стартовый packet FIELD-001 — field-001-start-R1

[DECISION-0050](../../decisions/0050-starting-content-and-unlocks.md) утверждён
2026-09-22; [DECISION-0051](../../decisions/0051-field001-initial-slice.md) ограничивает
этот этап исходно открытым контентом. Packet [F1-01](../milestones/FIELD-001-start.md#f1-01):
SKILL-001…007/010/013/014. Packet prerequisites: F1-00; framework prerequisites из раздела
«Зависимости» проверяются для требуемого scope. Каталожная dependency здесь
означает конкретный проверенный поднабор из milestone, не весь каталог владельца.

Scope/приёмка/checks пакета — [спецификация этапа](../milestones/FIELD-001-start.md).
Точный состав и unlocks — [Content Design](../../Content_design.md#starting-content-0050).
Все обязательные проверки этого IP сохраняются для выбранных IDs; полный scope
выше и поздние IDs не удаляются. Потребители пакета и обратные связи перечислены
в milestone; итоговый consumer — F1-08/F1-09. Текущие статусы, completed/remaining IDs,
evidence и единственная очередь находятся в [STATUS](../STATUS.md#field001-execution).
