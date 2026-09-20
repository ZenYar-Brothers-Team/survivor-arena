# IP-17 — Production Active Skills SKILL-001…016

Материал ревью: план принят пользователем 2026-09-20 и зарегистрирован. [Действующая спецификация](../../../modules/IP-17-production-skills.md). Этот файл не является текущим implementation packet.

Ревизия согласованного проекта: `design-sync-R2`. Спецификация перенесена в действующий каталог; дальнейшие изменения выполняются там.

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-08](IP-08-active-skill-framework.md), [IP-10A](IP-10A-ui-foundation.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — пять утверждённых новых документов из [реестра источников](../README.md), после M-01 — их canonical destinations. Читать только перечисленные секции и полные карточки используемых ID.

новый GDD skills/combat; полные выбранные SKILL-001…016 cards и непосредственно влияющие passive/set references; UI §§6–10; Art Production §6 и соответствующие generic VFX §10.

## Scope

шестнадцать data-driven skill definitions с L1…6, qualitative changes и численными параметрами; каждый отсутствующий pattern реализуется через IP-08 contract. На каждый ID — icon, краткое upgrade delta/level text и необходимые world roles. Общие projectile/impact assets переиспользуются по явным references.

## Out of Scope

set transformations, неописанные новые skills, окончательный rebalance, generation unused frames.

## Acceptance criteria

per-ID behavior matrix покрывает L1…6 и различия уровней; 6 concurrent skills работают без ручного aim. Collider/damage geometry не выводится из картинки; actual damage сохраняет source ID для telemetry. Production visual references валидируются; отсутствие обязательного финального арта не скрывается fixture fallback.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../03-existing-modules-and-art.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

HUD/build icon+level, reusable draft card, current→new delta и set-related detail; trigger/hit/miss и qualitative pattern наблюдаемы в harness.

## Проверки

numeric/pattern per-ID cases, level transitions, capacity/concurrency, pause/pool/reset; PlayMode representative mechanics + manual actual-speed projectile/telegraph readability.

## Документационные изменения

расширение title/range до 016, card→runtime/test/asset matrix и полный numeric payload. Балансные изменения только через IP-32 approval loop.

## Gates и недостающие решения

G-08/G-09 и полные параметры 16 skills; G-04 только если решение меняет SKILL-008; images проходят asset gates. Ссылки G-xx/W-01 — [матрица различий](../01-reconciliation.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-19](IP-19-production-sets.md), [IP-22](IP-22-production-characters.md), [IP-27](IP-27-integration.md). Полный порядок и готовность после регистрации определяет STATUS, не расположение файлов.
