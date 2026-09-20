# IP-17 — Production Active Skills SKILL-001…016

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-08](IP-08-active-skill-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

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

## Документационные изменения

расширение title/range до 016, card→runtime/test/asset matrix и полный numeric payload. Балансные изменения только через IP-32 approval loop.

## Gates и недостающие решения

G-08/G-09 закрыты DECISION-0017; нужны полные параметры 16 skills; G-04 только если решение меняет SKILL-008; images проходят asset gates. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-22](IP-22-production-characters.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.
