# IP-21 — Production Final Bosses и Mid-bosses

Действующая спецификация принятого плана, ревизия scope `design-sync-R2`. Текущий статус, очередь исполнения и evidence — только в [STATUS.md](../STATUS.md). Основание миграции — [DECISION-0015](../../decisions/0015-design-sync-r2.md).

## Существующая база и характер изменения

Модуль ещё не реализован. Эта спецификация полностью заменяет прежний packet перед началом работы; сначала реализовывать старый scope и затем догонять target не предлагается.

## Зависимости

[IP-15](IP-15-boss-framework.md), [IP-12A](IP-12A-visual-presentation-foundation.md).

Это зависимости целевой ревизии, а не разрешение использовать прежний Verified для нового scope. UI/effect extension points, которые поставляются позже, проверяются fake implementations; они не создают обратных зависимостей.

## Context

Источники GDD/CD/Art Direction ниже — действующие канонические документы из [реестра источников](../README.md). Читать только перечисленные секции и полные карточки используемых ID. Обозначение v2 в исходном review относится к уже перенесённому содержимому, а не к параллельному канону.

новые GDD boss/run sections; полные выбранные BOSS-001…010/MIDBOSS-001…010; UI §13; Art Production §§3–4.

## Scope

двадцать encounters с phases/HP/attacks/movement/resistance, body assets и нужными telegraph/projectile/impact references. Утверждённая field correspondence переносится как данные; отсутствующие времена/числа не придумываются.

## Out of Scope

invention boss phase rules, cutscenes, самостоятельные production timings.

## Acceptance criteria

каждая фаза воспроизводит карточку, telegraph соответствует реальному effect timing/geometry; final death никогда не вызывает victory; HP/name корректны; end прекращает phase actions; visual size не меняет gameplay geometry.

Общие runtime/JSON/UI/art инварианты и условия verification — [общий контракт](../ASSET_PRODUCTION.md#общий-контракт). Они не заменяют перечисленные здесь feature checks.

## UI / observability

production final-boss bar/name, incoming notification, attack warning; midboss phase feedback по owning encounter; phase/source diagnostics для отчёта IP-31.

## Проверки

per-ID phase transitions, threshold/zero-HP/end, defeat и timer interactions; PlayMode UI cleanup, pool/lifecycle; manual light/dark field contrast.

## Документационные изменения

карточка→phases/assets/tests, missing attack-number gaps, links на authoritative field bindings.

## Gates и недостающие решения

G-14: точные attack timings/phase payload, rewards и required fields каждой карточки. Ссылки G-xx/W-01 — [матрица различий](../DESIGN_SYNC.md); AG-01/BG-01 — [правила поставки](../README.md). Уже утверждённые designs не требуют повторного approval.

## Потребители

[IP-23](IP-23-production-fields.md), [IP-24](IP-24-production-waves.md), [IP-27](IP-27-integration.md). Полный порядок и готовность определяет STATUS, не расположение файлов.

## Enemy pattern integration

Использовать [IP-13 schema/runtime contract](IP-13-enemy-patterns.md#schema-и-runtime-contract): explicit per-attack controls/wind-up, отдельный dash contact, immutable source/life snapshots и reset-safe projectile pool. Encounter owner задаёт category; новые комбинации profiles не меняют draft/wave models. Phase ordering/support/escape mechanics остаются scope этого owning packet, а fixture numbers не являются production balance.

IP-15 предоставляет [boss phase/UI/lifecycle contract](IP-15-boss-framework.md#fixture-schema-и-phase-contract)
и [missing-rule list](IP-15-boss-framework.md#missing-rule-list-для-production).
Полный цикл repeated attack нельзя выдавать за delayed repeat или post-dash payload
карточки. Для каждого production ID заполнить отдельные attack refs, нужную
sequence semantics, rewards и asset bindings; не переименовывать fixture ID.
